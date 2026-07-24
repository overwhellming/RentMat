namespace RentMat.Application.DTOs.Authentication;

public record TokenResponseDto(string AccessToken, string RefreshToken, DateTimeOffset AccessTokenExpires);