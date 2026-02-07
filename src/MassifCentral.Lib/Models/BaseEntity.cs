namespace MassifCentral.Lib.Models;

/// <summary>
/// Base class for domain entities that provides common properties and behavior.
/// All domain models should inherit from this class to ensure consistency.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the date and time when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the entity was last modified.
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets a value indicating whether the entity is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
