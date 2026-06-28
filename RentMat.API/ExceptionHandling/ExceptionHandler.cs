using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RentMat.Application.Exceptions.Authentication;
using RentMat.Application.Exceptions.Booking;
using RentMat.Application.Exceptions.Devices;
using RentMat.Application.Exceptions.Users;

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
        }, cancellationToken);

        return true;
    }

    private (HttpStatusCode statusCode, string message) MapException(Exception exception)
    {
        return exception switch
        {
            NotEnoughMoneyException ex => (HttpStatusCode.Conflict, ex.Message),
            UserNotFoundException ex => (HttpStatusCode.NotFound, ex.Message),
            UserAlreadyExistsException ex => (HttpStatusCode.Conflict, ex.Message),

            InvalidCredentialsException ex => (HttpStatusCode.Unauthorized, ex.Message),
            JwtKeyNotFoundException ex => (HttpStatusCode.InternalServerError, ex.Message),

            BookingAccessDeniedException ex => (HttpStatusCode.Forbidden, ex.Message),
            BookingForDeviceNotFoundException ex => (HttpStatusCode.NotFound, ex.Message),
            BookingNotFoundException ex => (HttpStatusCode.NotFound, ex.Message),

            DeviceNotFoundException ex => (HttpStatusCode.NotFound, ex.Message),
            DeviceIsBookedException ex => (HttpStatusCode.Conflict, ex.Message),
            DeviceCategoryNotFoundException ex => (HttpStatusCode.NotFound, ex.Message),
            DeviceIsNotAvailableException ex => (HttpStatusCode.Conflict, ex.Message),

            BadHttpRequestException ex => (HttpStatusCode.BadRequest, "One or more request parameters are invalid"),
            UnauthorizedAccessException ex => (HttpStatusCode.Unauthorized,
                "User is not authorized to perform this action"),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occured")
        };
    }
}