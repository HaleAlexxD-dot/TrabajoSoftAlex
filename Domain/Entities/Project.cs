namespace Practica_Ing_de_software_I_.Domain.Entities;

public class Project
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";

    // Clave foránea referenciando al estudiante
    public int StudentId { get; set; }

    // Propiedad de navegación hacia la entidad User (Estudiante)
    public User Student { get; set; } = null!;
}

