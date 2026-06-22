namespace RentMat.Application.Exceptions.Users;

public class UserAlreadyExistsException() : Exception("User with this login or email already exists");