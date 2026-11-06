#!/bin/sh
# deploy/deploy.sh - test VM'de bir sürümü Compose ile çalıştırır ve kaydını tutar.
# Jenkins'in Deploy stage'i ve elle rollback aynı betiği kullanır. Betik ve compose.yaml
# ekibin dizinine (/opt/saha/ekipNN) kopyalanır; Compose proje adı dizin adıdır.
#
#   sh deploy.sh <IMG> <TAG> <WEB_PORT> [commit] [kaynak]
#   örnek (rollback): sh /opt/saha/ekip01/deploy.sh "$IMG" 1.0.1 8081 - elle
set -eu
IMG="$1"
TAG="$2"
WEB_PORT="$3"
COMMIT="${4:--}"
KAYNAK="${5:-elle}"

DIZIN="$(cd "$(dirname "$0")" && pwd)"
PROJE="$(basename "$DIZIN")"
cd "$DIZIN"
# Test VM paylaşılır: registry girişi ekibin kendi dizinindedir.
export DOCKER_CONFIG="$DIZIN/.docker" IMG TAG WEB_PORT

# 1. Hedef etiket registry'de mi? pull başarısızsa çalışan sürüme dokunulmaz.
docker compose -p "$PROJE" pull --quiet

# 2. Yeni sürüm. .env güncellenir: elle yapılan bir "up" da aynı sürümü çalıştırır.
printf 'IMG=%s\nTAG=%s\nWEB_PORT=%s\n' "$IMG" "$TAG" "$WEB_PORT" > .env
docker compose -p "$PROJE" up -d --remove-orphans

# 3. Kayıt: zaman, etiket, commit, kaynak ve çalışan imajların digest'leri
OZETLER=""
for s in product-service order-service web-ui; do
  d="$(docker image inspect --format '{{index .RepoDigests 0}}' "$IMG/$s:$TAG")"
  OZETLER="$OZETLER $s@${d#*@}"
done
SATIR="$(date -u +%Y-%m-%dT%H:%M:%SZ) tag=$TAG commit=$COMMIT kaynak=$KAYNAK$OZETLER"
echo "$SATIR" >> deploy-kayit.log
echo "KAYIT $SATIR"
