using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Practica_Ing_de_software_I_.Application.DTOs;
using Practica_Ing_de_software_I_.Application.Services;

namespace Practica_Ing_de_software_I_.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    /// Obtiene los proyectos del estudiante autenticado o especificado por parámetro.
    /// Utiliza .AsNoTracking() en la capa de servicio para optimizar lecturas.
    /// </summary>
    [HttpGet("mine")]
    [ProducesResponseType(typeof(IEnumerable<ProjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMine([FromQuery] int? studentId, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int targetStudentId = studentId ?? (int.TryParse(userIdClaim, out var parsedId) ? parsedId : 0);

        if (targetStudentId <= 0)
        {
            return BadRequest(new { message = "Se requiere un studentId válido o iniciar sesión con token de estudiante." });
        }

        var projects = await _projectService.GetByStudentIdAsync(targetStudentId, cancellationToken);
        return Ok(projects);
    }

    /// <summary>
    /// Registra una nueva propuesta de proyecto de graduación.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.StudentId <= 0)
        {
            return BadRequest(new { message = "El título y un StudentId válido son obligatorios." });
        }

        var project = await _projectService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
    }

    /// <summary>
    /// Obtiene el detalle de un proyecto por su identificador.
    /// Utiliza .AsNoTracking() en la capa de servicio.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var project = await _projectService.GetByIdAsync(id, cancellationToken);

        if (project is null)
        {
            return NotFound(new { message = $"No se encontró el proyecto con ID {id}." });
        }

        return Ok(project);
    }

    /// <summary>
    /// Registra la revisión de un tutor para un proyecto específico.
    /// </summary>
    [HttpPost("{id}/reviews")]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReviewProject(int id, [FromBody] SubmitReviewRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NewStatus))
        {
            return BadRequest(new { message = "El nuevo estado del proyecto es obligatorio." });
        }

        var reviewRequest = new CreateReviewRequest(id, request.Comments, request.NewStatus);
        var review = await _projectService.ReviewProjectAsync(reviewRequest, cancellationToken);

        if (review is null)
        {
            return NotFound(new { message = $"No se encontró el proyecto con ID {id} para realizar la revisión." });
        }

        return Ok(review);
    }
}

