namespace RentMat.Application.DTOs.RentalBooking;

public record BookingResponseDto(
    int Id,
    string DeviceName,
    string Login,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    decimal TotalPrice);