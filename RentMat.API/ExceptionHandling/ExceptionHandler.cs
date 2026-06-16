using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RentMat.Application.Booking.Exceptions;

namespace RentMat.API.ExceptionHandling;

public class ExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ExceptionHandler> _logger;

    public ExceptionHandler(ILogger<ExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = MapException(exception);
        
        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception occured");
        else
            _logger.LogWarning(exception, "Handled exception: {Message}", exception.Message);

        httpContext.Response.StatusCode = (int)statusCode;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = statusCode == HttpStatusCode.InternalServerError ? null : exception.Message
        }, cancellationToken: cancellationToken);

        return true;
    }

    private (HttpStatusCode statusCode, string message) MapException(Exception exception)
        => exception switch
        {
            InvalidDateRangeException ex => (HttpStatusCode.BadRequest, ex.Message),
            NotEnoughMoneyException ex => (HttpStatusCode.BadRequest, ex.Message),
            UserNotFoundException ex => (HttpStatusCode.NotFound, ex.Message),
            DeviceNotFoundException ex => (HttpStatusCode.NotFound, ex.Message),
            DeviceAlreadyBookedException ex => (HttpStatusCode.BadRequest, ex.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occured")
        };
}