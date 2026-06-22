namespace RentMat.Application.Exceptions.Users;

public class NotEnoughMoneyException (int id) : Exception($"User with id {id} doesn't have enough money");