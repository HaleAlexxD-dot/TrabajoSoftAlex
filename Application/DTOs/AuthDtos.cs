namespace Practica_Ing_de_software_I_.Application.DTOs;

public record LoginRequest(
    string Email,
    string Password
);

public record LoginResponse(
    string Token,
    int UserId,
    string Email,
    string Role
);

