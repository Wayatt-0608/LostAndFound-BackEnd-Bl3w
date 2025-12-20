using LostAndFound.Domain.Entities;
using System.Security.Claims;

namespace LostAndFound.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    ClaimsPrincipal? ValidateToken(string token);
}

