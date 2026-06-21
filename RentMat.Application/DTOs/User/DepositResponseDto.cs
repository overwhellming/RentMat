namespace RentMat.Application.DTOs.User;

public record DepositResponseDto(int Id, decimal Amount, DateTimeOffset CreatedAt, decimal CurrentBalace);