using GestaoResiduosESG.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GestaoResiduosESG.API.Middleware;

/// <summary>
/// Tratamento centralizado de exceções — ponto único de controle de erros.
/// Evita duplicação de try/catch nos controllers e garante que detalhes internos
/// não vazem para o cliente em produção.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Exceção não tratada: {Message}", exception.Message);

        var (statusCode, title, detail) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Recurso não encontrado", exception.Message),
            BusinessException => (StatusCodes.Status400BadRequest, "Regra de negócio violada", exception.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro interno do servidor",
                _environment.IsDevelopment() ? exception.Message : "Ocorreu um erro inesperado. Contate o suporte.")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
