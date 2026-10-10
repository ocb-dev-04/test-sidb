using SB.Solicitudes.Dominio.Common;

namespace SB.Solicitudes.Dominio.GovernmentEntities;

public sealed class GovernmentEntity : Entity<int>
{
    public const int NAME_MAXIMUM_LENGTH = 250;
    public const int CATEGORY_MAXIMUM_LENGTH = 150;
    public const int STATE_BRANCH_MAXIMUM_LENGTH = 100;
    public const int SECTOR_MAXIMUM_LENGTH = 150;

    private GovernmentEntity()
    {
    }

    private GovernmentEntity(int id)
        : base(id)
    {
    }

    public string Name { get; private set; } = string.Empty;

    public string Category { get; private set; } = string.Empty;

    public string StateBranch { get; private set; } = string.Empty;

    public string Sector { get; private set; } = string.Empty;

    public static GovernmentEntity Create(string name, string category, string stateBranch, string sector)
        => Restore(default, name, category, stateBranch, sector);

    public static GovernmentEntity Restore(int id, string name, string category, string stateBranch, string sector)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        GovernmentEntity governmentEntity = new(id);
        governmentEntity.Update(name, category, stateBranch, sector);
        return governmentEntity;
    }

    public GovernmentEntity WithId(int id) => Restore(id, Name, Category, StateBranch, Sector);

    public void Update(string name, string category, string stateBranch, string sector)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        Category = category.Trim();
        StateBranch = stateBranch.Trim();
        Sector = sector.Trim();
    }
}
