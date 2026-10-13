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
        return new RequestType
        {
            Name = DomainGuard.RequiredText(name, NAME_MAXIMUM_LENGTH),
            Description = DomainGuard.BoundedText(description, DESCRIPTION_MAXIMUM_LENGTH),
            IsActive = true,
        };
    }

    public void Update(string name, string description, bool isActive)
    {
        Name = DomainGuard.RequiredText(name, NAME_MAXIMUM_LENGTH);
        Description = DomainGuard.BoundedText(description, DESCRIPTION_MAXIMUM_LENGTH);
        IsActive = isActive;
    }
}
