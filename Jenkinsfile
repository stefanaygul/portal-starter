// Jenkinsfile - Saha Ekipman Talep Portalı
//
// Pipeline tanımı repoda yaşar: kodla birlikte gözden geçirilir ve sürümlenir.
//
// Hangi olay hangi stage'i çalıştırır?
//   Stage                          feature/*   develop       main          tag v*
//   Restore, Build, Test, web-ui   çalışır     çalışır       çalışır       çalışır
//   SonarQube + Quality Gate       -           çalışır       -             -
//   Docker Build & Push            -           etiket: SHA   etiket: SHA   -
//   Docker: SemVer etiketi         -           -             -             SHA imajına SemVer etiketi
//   Deploy (test VM) + smoke test  -           TAG = SHA     -             TAG = SemVer
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
    // Aynı branch'in iki build'i aynı anda deploy etmesin.
    disableConcurrentBuilds()
  }

  environment {
    // Her job kendi Docker girisini kullanir; build context'inin disinda tutulur.
    DOCKER_CONFIG     = "${WORKSPACE}@tmp/docker-config"
    REGISTRY          = 'gitlab.kurum.test:5050'
    IMG               = "${REGISTRY}/egitim/ekip-NN/saha-ekipman-portal"
    SONAR_PROJECT_KEY = 'saha-portal-ekipNN'
    // Test VM: deploy kullanıcısı, ekibin dizini ve portu (ortam kartı)
    TEST_VM           = 'deploy@test-vm.kurum.test'
    VM_DIZIN          = '/opt/saha/ekipNN'
    WEB_PORT          = 'PORT_NN'
    TEST_URL          = "http://test-vm.kurum.test:${WEB_PORT}"
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

    // Tag build'i (v1.1.0): imaj yeniden derlenmez. Tag'in gösterdiği commit'in main build'inde
    // üretilen SHA imajı alınır, SemVer etiketi eklenir. İki etiket aynı digest'i gösterir.
    // TAG_NAME tag build'inde tanımlıdır (v1.1.0); SemVer etiketi baştaki v atılarak üretilir.
    stage('Docker: SemVer etiketi') {
      when { buildingTag() }
      steps {
        withCredentials([usernamePassword(credentialsId: 'gitlab-registry',
            usernameVariable: 'U', passwordVariable: 'P')]) {
          sh '''echo "$P" | docker login $REGISTRY -u "$U" --password-stdin
                SHA=$(git rev-parse --short HEAD)
                SURUM=${TAG_NAME#v}
                for s in product-service order-service web-ui; do
                  # main build'i henüz bitmemiş olabilir: en çok 5 dakika beklenir.
                  i=0
                  until docker manifest inspect $IMG/$s:$SHA > /dev/null 2>&1; do
                    i=$((i + 1))
                    if [ $i -gt 20 ]; then
                      echo "HATA: $IMG/$s:$SHA registry'de yok. Bu tag'in commit'i ($SHA) için main build'i kırmızı ya da hiç çalışmadı; imaj yeniden derlenmez. main build'ini yeşile döndürüp bu tag job'ını yeniden çalıştırın."
                      exit 1
                    fi
                    echo "$IMG/$s:$SHA bekleniyor (main build'i) ..."
                    sleep 15
                  done
                  docker pull -q $IMG/$s:$SHA
                  docker tag $IMG/$s:$SHA $IMG/$s:$SURUM
                  docker push -q $IMG/$s:$SURUM
                done'''
        }
      }
    }

    // develop: TAG = commit SHA; tag build'i: TAG = SemVer. Hedef makinede aynı betik
    // (deploy/deploy.sh) çalışır: önce pull (etiket registry'de yoksa çalışan sürüme dokunmaz),
    // sonra up, sonra kayıt (commit, etiket, imaj digest'leri).
    stage('Deploy: test') {
      when { anyOf { branch 'develop'; buildingTag() } }
      steps {
        script {
          env.DEPLOY_TAG = env.TAG_NAME ? env.TAG_NAME.replaceFirst(/^v/, '') : sh(script: 'git rev-parse --short HEAD', returnStdout: true).trim()
          env.DEPLOY_COMMIT = sh(script: 'git rev-parse HEAD', returnStdout: true).trim()
        }
        sshagent(['test-vm-ssh']) {
          sh 'scp deploy/compose.yaml deploy/deploy.sh $TEST_VM:$VM_DIZIN/'
          script {
            def cikti = sh(returnStdout: true,
              script: 'ssh $TEST_VM "sh $VM_DIZIN/deploy.sh $IMG $DEPLOY_TAG $WEB_PORT $DEPLOY_COMMIT $BUILD_URL"').trim()
            echo cikti
            def kayit = cikti.readLines().find { it.startsWith('KAYIT ') } ?: 'KAYIT yok'
            // Aciklama duz metindir (HTML islenmez); ogeler " · " ile ayrilir.
            currentBuild.description = "test VM: TAG=${env.DEPLOY_TAG} · commit=${env.DEPLOY_COMMIT.take(7)} · " +
              kayit.tokenize(' ').findAll { it.contains('@sha256:') }.collect { it.take(it.indexOf('@sha256:') + 20) + '…' }.join(' · ')
          }
        }
      }
    }

    stage('Smoke test') {
      when { anyOf { branch 'develop'; buildingTag() } }
      steps {
        // Her istek yeniden denenir: servisler ayni anda hazir olmaz (Nginx once ayaga kalkar,
        // order-service sonra; arada 502 doner). --retry 502/503/504'u da yeniden dener.
        sh '''curl -fsS --retry 10 --retry-connrefused --retry-delay 3 "$TEST_URL/api/products" | grep -q '"code"'
              curl -fsS --retry 10 --retry-connrefused --retry-delay 3 -o /dev/null "$TEST_URL/api/orders"
              echo "smoke test: $TEST_URL yanıt veriyor (TAG=$DEPLOY_TAG)"'''
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
