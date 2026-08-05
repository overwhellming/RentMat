using MediatR;
using RentMat.Application.DTOs.User;

namespace RentMat.Application.Queries.Users;

public record GetUserByIdQuery(int UserId) : IRequest<UserResponseDto>;