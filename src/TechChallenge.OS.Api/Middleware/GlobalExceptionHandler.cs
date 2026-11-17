using Microsoft.AspNetCore.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using ApplicationException = TechChallenge.OS.Application.Exceptions.ApplicationException;
using TechChallenge.OS.Application.Exceptions;

namespace TechChallenge.OS.Api.Middleware;

/// <summary>
/// Global exception handler para mapeamento centralizado de exceções para ProblemDetails.
/// 
/// Mapeia:
/// - NotFoundException → 404 Not Found
/// - InvalidOperationException → 409 Conflict
/// - ApplicationException → 400 Bad Request
/// - Exception genérica → 500 Internal Server Error
/// </summary>
[ExcludeFromCodeCoverage]
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Exceção não tratada: {ExceptionType} - {Message}", 
            exception.GetType().Name, exception.Message);

        var response = httpContext.Response;

        switch (exception)
        {
            case NotFoundException notFoundEx:
                response.StatusCode = StatusCodes.Status404NotFound;
                await WriteErrorResponseAsync(response, StatusCodes.Status404NotFound, 
                    "NOT_FOUND", notFoundEx.Message, cancellationToken);
                return true;

            case InvalidOperationException invalidOpEx:
                response.StatusCode = StatusCodes.Status409Conflict;
                await WriteErrorResponseAsync(response, StatusCodes.Status409Conflict,
                    "CONFLICT", invalidOpEx.Message, cancellationToken);
                return true;

            case ApplicationException appEx:
                response.StatusCode = StatusCodes.Status400BadRequest;
                await WriteErrorResponseAsync(response, StatusCodes.Status400BadRequest,
                    "BAD_REQUEST", appEx.Message, cancellationToken);
                return true;

            default:
                response.StatusCode = StatusCodes.Status500InternalServerError;
                await WriteErrorResponseAsync(response, StatusCodes.Status500InternalServerError,
                    "INTERNAL_SERVER_ERROR", 
                    "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.",
                    cancellationToken);
                return true;
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpResponse response,
        int statusCode,
        string errorCode,
        string message,
        CancellationToken cancellationToken)
    {
        response.ContentType = "application/json";

        var problemDetails = new
        {
            type = $"https://api.example.com/errors/{errorCode}",
            title = GetErrorTitle(statusCode),
            status = statusCode,
            detail = message,
            instance = "",
            timestamp = DateTime.UtcNow
        };

        await response.WriteAsJsonAsync(problemDetails, cancellationToken);
    }

    private static string GetErrorTitle(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Requisição Inválida",
        StatusCodes.Status404NotFound => "Recurso Não Encontrado",
        StatusCodes.Status409Conflict => "Conflito de Estado",
        StatusCodes.Status500InternalServerError => "Erro Interno do Servidor",
        _ => "Erro"
    };
}
