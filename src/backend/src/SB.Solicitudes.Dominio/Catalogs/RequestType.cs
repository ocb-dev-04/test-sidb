using SB.Solicitudes.Dominio.Common;

namespace SB.Solicitudes.Dominio.Catalogs;

public sealed class RequestType : Entity<int>
{
    public const int NAME_MAXIMUM_LENGTH = 100;
    public const int DESCRIPTION_MAXIMUM_LENGTH = 500;

    private RequestType()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public static RequestType Create(string name, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new RequestType { Name = name.Trim(), Description = description.Trim(), IsActive = true };
    }

    public void Update(string name, string description, bool isActive)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        Description = description.Trim();
        IsActive = isActive;
    }
}
