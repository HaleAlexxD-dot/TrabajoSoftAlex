namespace Practica_Ing_de_software_I_.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    // Relación de navegación: Un usuario/estudiante puede tener múltiples proyectos
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}

