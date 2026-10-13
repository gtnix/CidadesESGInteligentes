# Projeto - Cidades ESGInteligentes

API **Gestão de Resíduos ESG** (ASP.NET Core 8) — continuidade do projeto ESG da Fase IV, agora com práticas DevOps: pipeline CI/CD, containerização e dois ambientes (staging e produção).

**FIAP | Graduação em Análise e Desenvolvimento de Sistemas | Navegando pelo mundo DevOps**

| Integrante | RM |
| --- | --- |
| Andre Luiz Silva | RM565836 |
| Giuliana Takara | RM562736 |

---

## Como executar localmente com Docker

Pré-requisitos: Docker Engine e Docker Compose v2.

```bash
# 1. (Opcional) variáveis locais — o compose já tem defaults seguros para a entrega
cp .env.example .env

# 2. Subir STAGING (API em http://localhost:5001 + SQL Server na porta 1434)
chmod +x scripts/deploy.sh scripts/smoke-test.sh
./scripts/deploy.sh staging
./scripts/smoke-test.sh staging

# 3. Subir PRODUÇÃO (API em http://localhost:5000; banco só na rede interna)
./scripts/deploy.sh production
./scripts/smoke-test.sh production
```

Comandos equivalentes sem o script:

```bash
docker compose -p gestao-esg-staging -f docker-compose.yml -f docker-compose.staging.yml up -d --build --wait
docker compose -p gestao-esg-prod     -f docker-compose.yml -f docker-compose.prod.yml     up -d --build --wait
```

URLs após o deploy:

| Ambiente | Swagger / API | Health | SQL Server |
| --- | --- | --- | --- |
| Staging | http://localhost:5001 | http://localhost:5001/health | localhost:1434 |
| Produção | http://localhost:5000 | http://localhost:5000/health | não publicado |

O `/health` devolve JSON com `status`, `environment` (`Staging` ou `Production`) e o check do banco. O título do Swagger também inclui o ambiente — isso diferencia os prints.

Parar os stacks:

```bash
docker compose -p gestao-esg-staging -f docker-compose.yml -f docker-compose.staging.yml down
docker compose -p gestao-esg-prod     -f docker-compose.yml -f docker-compose.prod.yml     down
```

### Sem Docker (desenvolvimento)

Pré-requisito: SQL Server em `localhost:1433` e SDK .NET 8.

```bash
dotnet ef database update \
  --project src/GestaoResiduosESG.Infrastructure \
  --startup-project src/GestaoResiduosESG.API

dotnet run --project src/GestaoResiduosESG.API
dotnet test GestaoResiduosESG.sln
```

---

## Pipeline CI/CD

Ferramenta: **GitHub Actions** (`.github/workflows/ci-cd.yml`).

O pipeline dispara em `push` e `pull_request` nas branches `main` e `develop`.

```text
push/PR
  → Build          (dotnet restore + build Release)
  → Testes         (dotnet test — testes xUnit já existentes)
  → Docker         (docker build da imagem)
  → Deploy Staging (compose staging + smoke /health)   [push em develop ou main]
  → Deploy Produção(compose produção + smoke /health)  [somente push em main]
```

Lógica dos ambientes:

- Branch `develop` → CI completo + **deploy staging**.
- Branch `main` → CI completo + **deploy staging** + **deploy produção** (produção só sobe se o staging passou).
- Pull request → apenas build e testes (não faz deploy).

Os jobs de deploy usam GitHub Environments (`staging` e `production`) e os mesmos scripts da máquina local (`scripts/deploy.sh` e `scripts/smoke-test.sh`), para o professor ver o pipeline verde e os stacks rodando com a mesma receita.

---

## Containerização

Estratégias do `Dockerfile`:

- **Multi-stage**: `sdk:8.0` para restore/publish e `aspnet:8.0` só com o runtime (imagem menor e sem SDK em produção).
- **Cache de restore**: copia os `.csproj` antes do código-fonte.
- **curl no runtime**: healthcheck HTTP em `/health`.
- **HTTP 8080** no container (sem redirecionar para HTTPS).

```dockerfile
# Estágio 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/GestaoResiduosESG.API/GestaoResiduosESG.API.csproj                             src/GestaoResiduosESG.API/
COPY src/GestaoResiduosESG.Application/GestaoResiduosESG.Application.csproj             src/GestaoResiduosESG.Application/
COPY src/GestaoResiduosESG.Domain/GestaoResiduosESG.Domain.csproj                       src/GestaoResiduosESG.Domain/
COPY src/GestaoResiduosESG.Infrastructure/GestaoResiduosESG.Infrastructure.csproj       src/GestaoResiduosESG.Infrastructure/
RUN dotnet restore src/GestaoResiduosESG.API/GestaoResiduosESG.API.csproj
COPY . .
RUN dotnet publish src/GestaoResiduosESG.API/GestaoResiduosESG.API.csproj \
    --configuration Release --output /app/publish --no-restore

# Estágio 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "GestaoResiduosESG.API.dll"]
```

Orquestração (Docker Compose):

- Serviços: **api** + **db** (SQL Server 2022).
- **Redes** explícitas (`esg-network`), isoladas por projeto Compose (`gestao-esg-staging` vs `gestao-esg-prod`).
- **Volumes** separados para os dados de staging e produção.
- **Variáveis de ambiente** para connection string, JWT e `ASPNETCORE_ENVIRONMENT`.
- Overlay [`docker-compose.staging.yml`](docker-compose.staging.yml) (porta 5001) e [`docker-compose.prod.yml`](docker-compose.prod.yml) (porta 5000; banco não publicado).
- Modelo de variáveis em [`.env.example`](.env.example).

---

## Prints do funcionamento

Evidências em [`docs/evidencias/`](docs/evidencias/):

| Evidência | Arquivo |
| --- | --- |
| Pipeline — visão geral (5 jobs verdes) | [docs/evidencias/pipeline-overview.png](docs/evidencias/pipeline-overview.png) |
| Pipeline — job Build | [docs/evidencias/pipeline-build.png](docs/evidencias/pipeline-build.png) |
| Pipeline — job Testes | [docs/evidencias/pipeline-testes.png](docs/evidencias/pipeline-testes.png) |
| Pipeline — job Docker | [docs/evidencias/pipeline-docker.png](docs/evidencias/pipeline-docker.png) |
| Pipeline — Deploy Staging | [docs/evidencias/pipeline-deploy-staging.png](docs/evidencias/pipeline-deploy-staging.png) |
| Pipeline — Deploy Produção | [docs/evidencias/pipeline-deploy-production.png](docs/evidencias/pipeline-deploy-production.png) |
| Staging no ar (Swagger) | [docs/evidencias/staging-swagger.png](docs/evidencias/staging-swagger.png) |
| Staging `/health` | [docs/evidencias/staging-health.png](docs/evidencias/staging-health.png) |
| Produção no ar (Swagger) | [docs/evidencias/production-swagger.png](docs/evidencias/production-swagger.png) |
| Produção `/health` | [docs/evidencias/production-health.png](docs/evidencias/production-health.png) |
| Containers dos dois ambientes | [docs/evidencias/docker-ps.png](docs/evidencias/docker-ps.png) |

Documentação técnica (PDF): [docs/documentacao-tecnica.pdf](docs/documentacao-tecnica.pdf)

Pipeline verde (branch `main`): [run 38441726696](https://github.com/gtnix/CidadesESGInteligentes/actions/runs/38441726696)

Repositório: [github.com/gtnix/CidadesESGInteligentes](https://github.com/gtnix/CidadesESGInteligentes)

---

## Tecnologias utilizadas

- **C# / ASP.NET Core 8** — Web API REST
- **Entity Framework Core 8** + **SQL Server 2022**
- **xUnit + Moq + WebApplicationFactory** — testes automatizados
- **Docker** (Dockerfile multi-stage) + **Docker Compose** (redes, volumes, env)
- **GitHub Actions** — CI/CD (build, testes, imagem, deploy staging e produção)
- **Swagger / OpenAPI**, **JWT Bearer**, **FluentValidation**, **AutoMapper**

---

## Autenticação

A API usa **JWT Bearer**. Para operações de escrita:

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "GestaoESG@2026"
}
```

Use o `token` no header `Authorization: Bearer {token}` (ou **Authorize** no Swagger).

---

## Endpoints principais

| Método | Rota | Auth | Descrição |
| --- | --- | --- | --- |
| POST | `/api/auth/login` | Aberto | Gera token JWT |
| GET | `/health` | Aberto | Status da API + ambiente + banco |
| GET | `/api/residuos?page=1&pageSize=10` | Aberto | Lista resíduos (paginado) |
| POST | `/api/residuos` | JWT | Cria resíduo |
| GET | `/api/pontos-coleta/{id}/status` | Aberto | Ocupação do ponto (%) |
| POST | `/api/pontos-coleta/{id}/registrar-coleta` | JWT | Registra coleta (zera quantidade) |
| POST | `/api/descartes` | JWT | Registra descarte + gera alertas |
| GET | `/api/alertas?status=Pendente` | Aberto | Lista alertas por status |
| PUT | `/api/alertas/{id}/resolver` | JWT | Resolve alerta |

---

## Arquitetura

```text
CidadesESGInteligentes/
├── Dockerfile
├── docker-compose.yml
├── docker-compose.staging.yml
├── docker-compose.prod.yml
├── .env.example
├── .github/workflows/ci-cd.yml
├── scripts/deploy.sh
├── scripts/smoke-test.sh
├── src/
│   ├── GestaoResiduosESG.Domain/
│   ├── GestaoResiduosESG.Application/
│   ├── GestaoResiduosESG.Infrastructure/
│   └── GestaoResiduosESG.API/
├── tests/GestaoResiduosESG.UnitTests/
├── docs/
└── README.md
```

---

## Checklist de entrega (obrigatório)

| Item | OK |
| --- | --- |
| Projeto compactado em .ZIP com estrutura organizada | [x] |
| Dockerfile funcional | [x] |
| docker-compose.yml ou arquivos Kubernetes | [x] |
| Pipeline com etapas de build, teste e deploy | [x] |
| README.md com instruções e prints | [x] |
| Documentação técnica com evidências (PDF ou PPT) | [x] |
| Deploy realizado nos ambientes staging e produção | [x] |
