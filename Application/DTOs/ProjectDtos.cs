namespace Practica_Ing_de_software_I_.Application.DTOs;

public record CreateProjectRequest(
    string Title,
    string Description,
    int StudentId
);

public record UpdateProjectStatusRequest(
    string Status
);

public record ProjectResponse(
    int Id,
    string Title,
    string Description,
    string Status,
    int StudentId,
    string? StudentEmail
);

