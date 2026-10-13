# Estágio 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia apenas os .csproj primeiro para aproveitar cache de dependências
COPY src/GestaoResiduosESG.API/GestaoResiduosESG.API.csproj                             src/GestaoResiduosESG.API/
COPY src/GestaoResiduosESG.Application/GestaoResiduosESG.Application.csproj             src/GestaoResiduosESG.Application/
COPY src/GestaoResiduosESG.Domain/GestaoResiduosESG.Domain.csproj                       src/GestaoResiduosESG.Domain/
COPY src/GestaoResiduosESG.Infrastructure/GestaoResiduosESG.Infrastructure.csproj       src/GestaoResiduosESG.Infrastructure/

RUN dotnet restore src/GestaoResiduosESG.API/GestaoResiduosESG.API.csproj

# Copia o restante dos arquivos e publica
COPY . .
RUN dotnet publish src/GestaoResiduosESG.API/GestaoResiduosESG.API.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

# Estágio 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "GestaoResiduosESG.API.dll"]
