using RentMat.Core.Models;

namespace RentMat.Application.Services.Interfaces;

public interface IJwtTokenService
{
    string CreateToken(User user);
}