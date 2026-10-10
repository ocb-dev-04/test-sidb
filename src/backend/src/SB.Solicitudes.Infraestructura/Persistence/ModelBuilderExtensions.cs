using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SB.Solicitudes.Dominio.Common;

namespace SB.Solicitudes.Infraestructura.Persistence;

internal static class ModelBuilderExtensions
{
    public const int ENUM_MAXIMUM_LENGTH = 30;

    public static void IgnoreDomainEvents(this ModelBuilder modelBuilder)
    {
        IEnumerable<Type> entityTypes = modelBuilder.Model
            .GetEntityTypes()
            .Select(entityType => entityType.ClrType)
            .Where(clrType => typeof(IHasDomainEvents).IsAssignableFrom(clrType))
            .ToArray();

        foreach (Type entityType in entityTypes)
        {
            modelBuilder.Entity(entityType).Ignore(nameof(IHasDomainEvents.DomainEvents));
        }
    }

    // SQL Server no guarda DateTimeKind: sin esto las fechas llegarían al cliente sin la marca UTC.
    public static void UseUtcDateTimes(this ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableUtcDateTimeConverter>();
    }

    private sealed class UtcDateTimeConverter()
        : ValueConverter<DateTime, DateTime>(
            value => value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime(),
            value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private sealed class NullableUtcDateTimeConverter()
        : ValueConverter<DateTime?, DateTime?>(
            value => value.HasValue ? (value.Value.Kind == DateTimeKind.Utc ? value : value.Value.ToUniversalTime()) : value,
            value => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value);
}
