using Microsoft.EntityFrameworkCore;
using Practica_Ing_de_software_I_.Application.DTOs;
using Practica_Ing_de_software_I_.Domain.Entities;
using Practica_Ing_de_software_I_.Infrastructure.Persistence;

namespace Practica_Ing_de_software_I_.Application.Services;

public class ProjectService : IProjectService
{
    private readonly AppDbContext _context;

    public ProjectService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProjectResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .AsNoTracking()
            .Include(p => p.Student)
            .Select(p => new ProjectResponse(
                p.Id,
                p.Title,
                p.Description,
                p.Status,
                p.StudentId,
                p.Student != null ? p.Student.Email : null
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProjectResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects
            .AsNoTracking()
            .Include(p => p.Student)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (project is null)
        {
            return null;
        }

        return new ProjectResponse(
            project.Id,
            project.Title,
            project.Description,
            project.Status,
            project.StudentId,
            project.Student != null ? project.Student.Email : null
        );
    }

    public async Task<IEnumerable<ProjectResponse>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .AsNoTracking()
            .Include(p => p.Student)
            .Where(p => p.StudentId == studentId)
            .Select(p => new ProjectResponse(
                p.Id,
                p.Title,
                p.Description,
                p.Status,
                p.StudentId,
                p.Student != null ? p.Student.Email : null
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProjectResponse> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var project = new Project
        {
            Title = request.Title,
            Description = request.Description,
            StudentId = request.StudentId,
            Status = "Pending"
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync(cancellationToken);

        var student = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.StudentId, cancellationToken);

        return new ProjectResponse(
            project.Id,
            project.Title,
            project.Description,
            project.Status,
            project.StudentId,
            student?.Email
        );
    }

    public async Task<ProjectResponse?> UpdateStatusAsync(int id, UpdateProjectStatusRequest request, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects
            .Include(p => p.Student)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (project is null)
        {
            return null;
        }

        project.Status = request.Status;
        await _context.SaveChangesAsync(cancellationToken);

        return new ProjectResponse(
            project.Id,
            project.Title,
            project.Description,
            project.Status,
            project.StudentId,
            project.Student?.Email
        );
    }

    public async Task<ReviewResponse?> ReviewProjectAsync(CreateReviewRequest request, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project is null)
        {
            return null;
        }

        project.Status = request.NewStatus;
        await _context.SaveChangesAsync(cancellationToken);

        return new ReviewResponse(
            project.Id,
            request.Comments,
            project.Status,
            DateTime.UtcNow
        );
    }
}

