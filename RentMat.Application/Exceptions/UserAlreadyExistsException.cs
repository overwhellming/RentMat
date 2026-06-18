namespace RentMat.Application.Exceptions;

public class UserAlreadyExistsException() : Exception("User with this login or email already exists");