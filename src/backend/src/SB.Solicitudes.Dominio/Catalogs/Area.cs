using SB.Solicitudes.Dominio.Common;

namespace SB.Solicitudes.Dominio.Catalogs;

public sealed class Area : Entity<int>
{
    public const int NAME_MAXIMUM_LENGTH = 100;

    private Area()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public static Area Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Area { Name = name.Trim(), IsActive = true };
    }

    public void Update(string name, bool isActive)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        IsActive = isActive;
    }
}
