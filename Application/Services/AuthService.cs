using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Practica_Ing_de_software_I_.Application.DTOs;
using Practica_Ing_de_software_I_.Domain.Entities;
using Practica_Ing_de_software_I_.Infrastructure.Persistence;

namespace Practica_Ing_de_software_I_.Application.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        // Consulta de solo lectura usando AsNoTracking y CancellationToken
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null)
        {
            return null;
        }

        // Verificación de contraseña usando BCrypt (nunca en texto plano)
        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            return null;
        }

        // Generación del token JWT
        var token = GenerateJwtToken(user);

        // Retornar DTO de respuesta (nunca exponer la entidad de dominio directamente)
        return new LoginResponse(
            Token: token,
            UserId: user.Id,
            Email: user.Email,
            Role: user.Role
        );
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }

    private string GenerateJwtToken(User user)
    {

var secretKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Falta Jwt:Key en los archivos de configuración.");
var issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Falta Jwt:Issuer en los archivos de configuración.");
var audience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Falta Jwt:Audience en los archivos de configuración.");

if (!double.TryParse(_configuration["Jwt:ExpiresInHours"], out var expirationHours))
{
    expirationHours = 24;
}

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(expirationHours),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}

