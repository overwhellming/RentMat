namespace RentMat.Application.DTOs.User;

public record UserResponseDto(int Id, string Login, string Email, string Role, decimal Balance, DateTimeOffset CreatedAt);