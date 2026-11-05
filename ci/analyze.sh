#!/bin/sh
# SonarScanner for .NET: begin -> build -> test -> end.
# Scanner derlemeyi dinler; build, begin ile end arasında değilse analiz boş kalır.
#
# Sunucu adresi ve token bu dosyada yoktur: Jenkins'te withSonarQubeEnv bloğu ortam
# değişkeniyle verir. Proje anahtarı SONAR_PROJECT_KEY değişkeninden gelir (Jenkinsfile).
set -eu

: "${SONAR_PROJECT_KEY:?SONAR_PROJECT_KEY tanimli degil}"

dotnet sonarscanner begin /k:"$SONAR_PROJECT_KEY" \
  /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml"

dotnet build SahaPortal.sln -c Release --no-restore

dotnet test SahaPortal.sln -c Release --no-build \
  --logger "junit;LogFileName=junit.xml" \
  --collect:"XPlat Code Coverage" -- \
  DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

dotnet sonarscanner end
