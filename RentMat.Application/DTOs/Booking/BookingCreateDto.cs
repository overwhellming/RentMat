namespace RentMat.Application.DTOs.RentalBooking;

public sealed record BookingCreateDto(int DeviceId, int UserId, DateTimeOffset StartDate, DateTimeOffset EndDate);