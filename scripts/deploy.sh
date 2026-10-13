#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

compose() {
  if docker compose version >/dev/null 2>&1; then
    docker compose "$@"
  else
    docker-compose "$@"
  fi
}

usage() {
  echo "Uso: $0 {staging|production}" >&2
  exit 1
}

ENV_NAME="${1:-}"
case "$ENV_NAME" in
  staging)
    PROJECT="gestao-esg-staging"
    FILES=(-f docker-compose.yml -f docker-compose.staging.yml)
    ;;
  production|prod)
    ENV_NAME="production"
    PROJECT="gestao-esg-prod"
    FILES=(-f docker-compose.yml -f docker-compose.prod.yml)
    ;;
  *)
    usage
    ;;
esac

echo "==> Deploy ${ENV_NAME} (projeto ${PROJECT})"
compose -p "$PROJECT" "${FILES[@]}" up -d --build --remove-orphans --wait --wait-timeout 180

echo "==> Aguardando healthchecks do Compose..."
if compose -p "$PROJECT" "${FILES[@]}" ps --help 2>/dev/null | grep -q -- "--format"; then
  compose -p "$PROJECT" "${FILES[@]}" ps
else
  compose -p "$PROJECT" "${FILES[@]}" ps
fi

echo "==> Stack ${ENV_NAME} publicado. Execute: ./scripts/smoke-test.sh ${ENV_NAME}"
