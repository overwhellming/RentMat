namespace RentMat.Application.Exceptions.Users;

public class UserNotFoundException(int id) : Exception($"User with id {id} was not found");