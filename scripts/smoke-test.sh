#!/usr/bin/env bash
set -euo pipefail

usage() {
  echo "Uso: $0 {staging|production}" >&2
  exit 1
}

ENV_NAME="${1:-}"
case "$ENV_NAME" in
  staging)
    BASE_URL="${STAGING_URL:-http://localhost:5001}"
    EXPECTED_ENV="Staging"
    ;;
  production|prod)
    ENV_NAME="production"
    BASE_URL="${PROD_URL:-http://localhost:5000}"
    EXPECTED_ENV="Production"
    ;;
  *)
    usage
    ;;
esac

HEALTH_URL="${BASE_URL}/health"
echo "==> Smoke test ${ENV_NAME} em ${HEALTH_URL}"

ATTEMPTS="${SMOKE_ATTEMPTS:-60}"
SLEEP_SECONDS="${SMOKE_SLEEP:-5}"

for i in $(seq 1 "$ATTEMPTS"); do
  if BODY="$(curl -fsS "$HEALTH_URL" 2>/dev/null)"; then
    echo "$BODY"
    echo "$BODY" | grep -qi "healthy" || {
      echo "Resposta de /health sem status Healthy (tentativa ${i}/${ATTEMPTS})" >&2
      sleep "$SLEEP_SECONDS"
      continue
    }
    echo "$BODY" | grep -qi "$EXPECTED_ENV" || {
      echo "AVISO: /health não contém environment=${EXPECTED_ENV}" >&2
    }
    echo "==> GET ${BASE_URL}/ (Swagger)"
    curl -fsS -o /dev/null -w "Swagger HTTP %{http_code}\n" "${BASE_URL}/"
    echo "==> Smoke test ${ENV_NAME} OK"
    exit 0
  fi
  echo "Aguardando ${ENV_NAME} ficar pronto (${i}/${ATTEMPTS})..."
  sleep "$SLEEP_SECONDS"
done

echo "Falha: ${ENV_NAME} não respondeu em ${HEALTH_URL} após ${ATTEMPTS} tentativas." >&2
exit 1
