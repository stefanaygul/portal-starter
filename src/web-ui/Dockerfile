# Build context: repo koku.  docker build -f src/web-ui/Dockerfile .
ARG HUB=docker.io
FROM ${HUB}/library/node:24-alpine AS build
WORKDIR /app
# .npmrc da kopyalanir: npm ci paketleri Nexus npm-group deposundan alir.
COPY src/web-ui/package*.json src/web-ui/.npmrc ./
RUN npm ci
COPY src/web-ui/ ./
RUN npm run build

FROM ${HUB}/nginxinc/nginx-unprivileged:stable-alpine
COPY deploy/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=3s CMD wget -q -O /dev/null http://127.0.0.1:8080/ || exit 1
