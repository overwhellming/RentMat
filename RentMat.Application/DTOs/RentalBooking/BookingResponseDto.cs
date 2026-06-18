namespace RentMat.Application.DTOs.RentalBooking;

public sealed record BookingResponseDto(
    int Id,
    string DeviceName,
    string Login,
    string StatusName,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    decimal TotalPrice);