// Jenkinsfile - Saha Ekipman Talep Portalı
//
// Pipeline tanımı repoda yaşar: kodla birlikte gözden geçirilir ve sürümlenir.
// 5. gün eklenecek stage'ler: Deploy (test VM) ve smoke test, Pack & Publish (bonus).
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
    REGISTRY = 'gitlab.kurum.test:5050'
    IMG      = "${REGISTRY}/egitim/ekip-NN/saha-ekipman-portal"
  }

  stages {
    stage('Restore') {
      steps {
        sh 'dotnet restore SahaPortal.sln'
      }
    }

    stage('Build') {
      steps {
        sh 'dotnet build SahaPortal.sln -c Release --no-restore'
      }
    }

    stage('Unit Test') {
      steps {
        sh '''dotnet test SahaPortal.sln -c Release \
          --no-build --logger "junit;LogFileName=junit.xml"'''
      }
      post {
        always { junit '**/TestResults/junit.xml' }
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
  }

  post {
    success { updateGitlabCommitStatus name: 'jenkins', state: 'success' }
    failure { updateGitlabCommitStatus name: 'jenkins', state: 'failed' }
    aborted { updateGitlabCommitStatus name: 'jenkins', state: 'canceled' }
  }
}
