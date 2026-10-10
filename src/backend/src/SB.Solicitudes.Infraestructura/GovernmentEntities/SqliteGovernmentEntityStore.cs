using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;
using SB.Solicitudes.Aplicacion.Features.GovernmentEntities;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.GovernmentEntities;

namespace SB.Solicitudes.Infraestructura.GovernmentEntities;

public sealed class GovernmentEntitiesDbContext(DbContextOptions<GovernmentEntitiesDbContext> options) : DbContext(options)
{
    public DbSet<GovernmentEntity> GovernmentEntities => Set<GovernmentEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfiguration(new GovernmentEntityConfiguration());
}

internal sealed class GovernmentEntityConfiguration : IEntityTypeConfiguration<GovernmentEntity>
{
    public void Configure(EntityTypeBuilder<GovernmentEntity> builder)
    {
        builder.ToTable("EntidadesGubernamentales");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).HasColumnName("Id").ValueGeneratedOnAdd();
        builder.Property(entity => entity.Name).HasColumnName("Nombre").HasMaxLength(GovernmentEntity.NAME_MAXIMUM_LENGTH).IsRequired();
        builder.Property(entity => entity.Category).HasColumnName("Categoria").HasMaxLength(GovernmentEntity.CATEGORY_MAXIMUM_LENGTH);
        builder.Property(entity => entity.StateBranch).HasColumnName("PoderDelEstado").HasMaxLength(GovernmentEntity.STATE_BRANCH_MAXIMUM_LENGTH);
        builder.Property(entity => entity.Sector).HasColumnName("Sector").HasMaxLength(GovernmentEntity.SECTOR_MAXIMUM_LENGTH);
        builder.Ignore(nameof(IHasDomainEvents.DomainEvents));
        builder.HasIndex(entity => entity.Name).HasDatabaseName("IX_EntidadesGubernamentales_Nombre");
    }
}

internal sealed class SqliteGovernmentEntityStore(GovernmentEntitiesDbContext context) : IGovernmentEntityReader, IGovernmentEntityWriter
{
    public async Task<IReadOnlyList<GovernmentEntity>> GetAllAsync(CancellationToken cancellationToken)
        => await context.GovernmentEntities.AsNoTracking().OrderBy(entity => entity.Id).ToListAsync(cancellationToken);

    public async Task<GovernmentEntity> AddAsync(GovernmentEntity governmentEntity, CancellationToken cancellationToken)
    {
        context.GovernmentEntities.Add(governmentEntity);
        await context.SaveChangesAsync(cancellationToken);
        return governmentEntity;
    }

    public async Task<bool> UpdateAsync(GovernmentEntity governmentEntity, CancellationToken cancellationToken)
    {
        int affectedRows = await context.GovernmentEntities
            .Where(entity => entity.Id == governmentEntity.Id)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(entity => entity.Name, governmentEntity.Name)
                    .SetProperty(entity => entity.Category, governmentEntity.Category)
                    .SetProperty(entity => entity.StateBranch, governmentEntity.StateBranch)
                    .SetProperty(entity => entity.Sector, governmentEntity.Sector),
                cancellationToken);

        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
        => await context.GovernmentEntities.Where(entity => entity.Id == id).ExecuteDeleteAsync(cancellationToken) > 0;

    public async Task ReplaceAllAsync(IReadOnlyList<GovernmentEntity> governmentEntities, CancellationToken cancellationToken)
    {
        await using IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);

        await context.GovernmentEntities.ExecuteDeleteAsync(cancellationToken);
        context.GovernmentEntities.AddRange(governmentEntities.Select(entity =>
            GovernmentEntity.Restore(entity.Id, entity.Name, entity.Category, entity.StateBranch, entity.Sector)));
        await context.SaveChangesAsync(cancellationToken);
        context.ChangeTracker.Clear();

        await transaction.CommitAsync(cancellationToken);
    }
}
