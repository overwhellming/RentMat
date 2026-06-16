namespace RentMat.Application.DTOs.RentalBooking;

public record BookingCreateDto(int deviceId, int userId, DateTimeOffset startDate, DateTimeOffset endDate);