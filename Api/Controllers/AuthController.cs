using Microsoft.AspNetCore.Mvc;
using Practica_Ing_de_software_I_.Application.DTOs;
using Practica_Ing_de_software_I_.Application.Services;

namespace Practica_Ing_de_software_I_.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);

        if (response is null)
        {
            return Unauthorized(new { message = "Credenciales incorrectas." });
        }

        return Ok(response);
    }
}

