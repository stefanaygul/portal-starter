// Jenkinsfile - Saha Ekipman Talep Portalı
//
// Pipeline tanımı repoda yaşar: kodla birlikte gözden geçirilir ve sürümlenir.
// 5. gün eklenecek stage'ler: Deploy (test VM) ve smoke test, Pack & Publish (bonus).
//
// SonarQube Community Build yalnızca ana branch'i analiz eder: analiz ve Quality Gate
// yalnızca develop build'inde çalışır; diğer branch'lerde build ve test yeterlidir.
pipeline {
  agent { label 'dotnet-docker' }

  options {
    // 'gitlab': Jenkins'teki GitLab connection adı. 'gitlab-api': ekip klasöründeki
    // Project Access Token; commit status GitLab'e bu token ile yazılır.
    gitLabConnection(gitLabConnection: 'gitlab', jobCredentialId: 'gitlab-api', useAlternativeCredential: true)
    // Build kuyruğa girdiği anda GitLab'de 'jenkins' durumu "pending" olur.
    gitlabBuilds(builds: ['jenkins'])
    timeout(time: 30, unit: 'MINUTES')
  }

  environment {
    // Her job kendi Docker girisini kullanir; build context'inin disinda tutulur.
    DOCKER_CONFIG     = "${WORKSPACE}@tmp/docker-config"
    REGISTRY          = 'gitlab.kurum.test:5050'
    IMG               = "${REGISTRY}/egitim/ekip-NN/saha-ekipman-portal"
    SONAR_PROJECT_KEY = 'saha-portal-ekipNN'
  }

  stages {
    stage('Restore') {
      steps {
        sh 'dotnet restore SahaPortal.sln'
      }
    }

    // develop dışındaki branch'ler: analiz yok, yalnızca build ve test.
    stage('Build') {
      when { not { branch 'develop' } }
      steps {
        sh 'dotnet build SahaPortal.sln -c Release --no-restore'
      }
    }

    stage('Unit Test') {
      when { not { branch 'develop' } }
      steps {
        sh '''dotnet test SahaPortal.sln -c Release \
          --no-build --logger "junit;LogFileName=junit.xml"'''
      }
      post {
        always { junit '**/TestResults/junit.xml' }
      }
    }

    // develop: analiz derlemeyi sarar (begin -> build -> test -> end).
    // 'sonarqube': Jenkins'teki sunucu tanımının adı. 'sonar-token': ekip klasöründeki token.
    stage('Build + Test + SonarQube') {
      when { branch 'develop' }
      steps {
        withSonarQubeEnv(installationName: 'sonarqube', credentialsId: 'sonar-token') {
          sh './ci/analyze.sh'
        }
      }
      post {
        always { junit '**/TestResults/junit.xml' }
      }
    }

    stage('Quality Gate') {
      when { branch 'develop' }
      steps {
        timeout(time: 10, unit: 'MINUTES') {
          waitForQualityGate abortPipeline: true, credentialsId: 'sonar-token'
        }
      }
    }

    stage('web-ui') {
      steps {
        dir('src/web-ui') {
          sh 'npm ci'
          sh 'npm test -- --coverage --ci'
          sh 'npm run build'
        }
      }
      post {
        always { junit 'src/web-ui/junit.xml' }
      }
    }

    // İmaj yalnızca ana branch'lerde yayınlanır; etiket commit SHA'dır.
    // MCR ve HUB (temel imajların iç aynası) Jenkins'in ortak ortam değişkenleridir.
    stage('Docker Build & Push') {
      when { anyOf { branch 'develop'; branch 'main' } }
      steps {
        withCredentials([usernamePassword(credentialsId: 'gitlab-registry',
            usernameVariable: 'U', passwordVariable: 'P')]) {
          sh '''echo "$P" | docker login $REGISTRY -u "$U" --password-stdin
                SHA=$(git rev-parse --short HEAD)
                docker build -f src/product-service/Dockerfile --build-arg MCR=$MCR -t $IMG/product-service:$SHA .
                docker build -f src/order-service/Dockerfile   --build-arg MCR=$MCR -t $IMG/order-service:$SHA .
                docker build -f src/web-ui/Dockerfile          --build-arg HUB=$HUB -t $IMG/web-ui:$SHA .
                for s in product-service order-service web-ui; do
                  docker push $IMG/$s:$SHA
                done'''
        }
      }
    }
  }

  post {
    always { sh 'rm -f "$DOCKER_CONFIG/config.json"' }
    success { updateGitlabCommitStatus name: 'jenkins', state: 'success' }
    failure { updateGitlabCommitStatus name: 'jenkins', state: 'failed' }
    aborted { updateGitlabCommitStatus name: 'jenkins', state: 'canceled' }
  }
}
