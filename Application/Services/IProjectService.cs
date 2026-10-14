using Practica_Ing_de_software_I_.Application.DTOs;

namespace Practica_Ing_de_software_I_.Application.Services;

public interface IProjectService
{
    Task<IEnumerable<ProjectResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProjectResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProjectResponse>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);
    Task<ProjectResponse> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);
    Task<ProjectResponse?> UpdateStatusAsync(int id, UpdateProjectStatusRequest request, CancellationToken cancellationToken = default);
    Task<ReviewResponse?> ReviewProjectAsync(CreateReviewRequest request, CancellationToken cancellationToken = default);
}

