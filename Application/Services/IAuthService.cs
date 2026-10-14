using Practica_Ing_de_software_I_.Application.DTOs;

namespace Practica_Ing_de_software_I_.Application.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}

