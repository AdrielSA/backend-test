namespace BackendTest.Core.Entities;

/// <summary>
/// Clase base abstracta para todas las entidades del dominio
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Identificador de la entidad
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Fecha y hora de creación del registro en UTC
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Fecha y hora de última actualización del registro en UTC
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
