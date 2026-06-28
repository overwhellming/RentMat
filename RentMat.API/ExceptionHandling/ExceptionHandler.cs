using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RentMat.Application.Exceptions;
using RentMat.Application.Exceptions.Authentication;
using RentMat.Application.Exceptions.Booking;
using RentMat.Application.Exceptions.Devices;
using RentMat.Application.Exceptions.Users;

namespace RentMat.API.ExceptionHandling;

public class ExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetailsService;
    
    public ExceptionHandler(ILogger<ExceptionHandler> logger, IProblemDetailsService problemDetailsService)
    {
        _logger = logger;
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = MapException(exception);
        
        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception occured");
        else
            _logger.LogWarning(exception, "Handled exception: {Detail}", detail);

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = title,
                Detail = detail
            },
            Exception = exception,
        });
    }

    private (HttpStatusCode statusCode, string title, string detail) MapException(Exception exception)
    {
        if (exception is IExceptionBase baseEx)
            return (baseEx.StatusCode, baseEx.Title, exception.Message);
        return (HttpStatusCode.InternalServerError, "Internal server error", "An unexpected error occured");
    }
}