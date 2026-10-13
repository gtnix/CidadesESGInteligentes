using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using FluentValidation;
using FluentValidation.AspNetCore;
using GestaoResiduosESG.API.Middleware;
using GestaoResiduosESG.Application.Interfaces;
using GestaoResiduosESG.Application.Mappings;
using GestaoResiduosESG.Application.Services;
using GestaoResiduosESG.Application.Validators;
using GestaoResiduosESG.Domain.Interfaces;
using GestaoResiduosESG.Infrastructure.Data;
using GestaoResiduosESG.Infrastructure.Data.Context;
using GestaoResiduosESG.Infrastructure.Data.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("GestaoResiduosESG.Infrastructure")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IResiduoRepository, ResiduoRepository>();
builder.Services.AddScoped<IPontoColetaRepository, PontoColetaRepository>();
builder.Services.AddScoped<IDescarteRepository, DescarteRepository>();
builder.Services.AddScoped<IAlertaRepository, AlertaRepository>();
builder.Services.AddScoped<ILimiteArmazenamentoRepository, LimiteArmazenamentoRepository>();
builder.Services.AddScoped<IPontoResiduoRepository, PontoResiduoRepository>();

builder.Services.AddScoped<IResiduoService, ResiduoService>();
builder.Services.AddScoped<IPontoColetaService, PontoColetaService>();
builder.Services.AddScoped<IDescarteService, DescarteService>();
builder.Services.AddScoped<IAlertaService, AlertaService>();
builder.Services.AddScoped<ILimiteArmazenamentoService, LimiteArmazenamentoService>();
builder.Services.AddScoped<IPontoResiduoService, PontoResiduoService>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key não configurada. Defina 'Jwt:Key' via variável de ambiente ou User Secrets.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly));

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ResiduoCreateValidator>();

// Versionamento de API — permite evoluir a API sem quebrar clientes existentes
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new QueryStringApiVersionReader("api-version"),
        new HeaderApiVersionReader("X-Api-Version"));
}).AddApiExplorer(options =>
{
    options.GroupNameFormat           = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

// Controllers com enums como strings no JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var environmentLabel = builder.Environment.EnvironmentName.ToUpperInvariant() switch
{
    "PRODUCTION" => "PRODUÇÃO",
    "STAGING" => "STAGING",
    "DEVELOPMENT" => "DEVELOPMENT",
    _ => builder.Environment.EnvironmentName.ToUpperInvariant()
};

// Swagger com suporte a Bearer token — o nome do ambiente aparece no título (evidência de staging vs produção)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = $"Gestão de Resíduos ESG — {environmentLabel}",
        Version = "v1",
        Description = "API para rastreamento de resíduos, pontos de coleta e alertas de capacidade. " +
                      $"Ambiente: {builder.Environment.EnvironmentName}. " +
                      "Continuação da Fase II - Automatizando Processos ESG (Oracle/PL-SQL).",
        Contact = new OpenApiContact
        {
            Name = "FIAP - Graduação Ano 2 - Fase IV"
        }
    });
    c.EnableAnnotations();
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // Adiciona o botão "Authorize" com Bearer token no Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization — informe: Bearer {seu_token}",
        Name        = "Authorization",
        In          = ParameterLocation.Header,
        Type        = SecuritySchemeType.ApiKey,
        Scheme      = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Tratamento global de exceções — centraliza erros e evita vazamento de detalhes internos
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Health checks — /health
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

// Aplica migrações automaticamente na inicialização (garante funcionamento no Docker)
// IsRelational() protege de erro ao usar banco InMemory nos testes de integração
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (db.Database.IsRelational())
        db.Database.Migrate();
}

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", $"Gestão de Resíduos ESG — {environmentLabel} v1");
    c.DocumentTitle = $"Gestão de Resíduos ESG — {environmentLabel}";
    c.RoutePrefix = string.Empty;
});

if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var payload = new
        {
            status = report.Status.ToString(),
            environment = app.Environment.EnvironmentName,
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description
                })
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
});
app.Run();

// Necessário para WebApplicationFactory nos testes de integração
public partial class Program { }
