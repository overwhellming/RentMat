namespace RentMat.Application.DTOs.RentalBooking;

public sealed record BookingCreateDto(int DeviceId, DateTimeOffset StartDate, DateTimeOffset EndDate);