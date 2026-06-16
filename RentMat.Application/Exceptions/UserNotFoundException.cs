namespace RentMat.Application.Booking.Exceptions;

public class UserNotFoundException(int id) : Exception($"User with id {id} was not found");