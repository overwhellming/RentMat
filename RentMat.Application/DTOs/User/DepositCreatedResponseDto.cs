namespace RentMat.Application.DTOs.User;

public record DepositCreatedResponseDto(int Id, decimal Amount, DateTimeOffset CreatedAt, decimal CurrentBalance);