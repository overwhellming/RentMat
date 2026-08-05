using MediatR;

namespace RentMat.Application.Queries.Users;

public record GetUserBalanceQuery(int UserId) : IRequest<decimal>;