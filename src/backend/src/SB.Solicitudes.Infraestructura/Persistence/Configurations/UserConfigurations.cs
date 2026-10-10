using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.Solicitudes.Dominio.Catalogs;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Infraestructura.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).HasColumnName("Id").ValueGeneratedNever();
        builder.Property(user => user.FullName).HasColumnName("NombreCompleto").HasMaxLength(User.FULL_NAME_MAXIMUM_LENGTH).IsRequired();
        builder.Property(user => user.Email).HasColumnName("Correo").HasMaxLength(User.EMAIL_MAXIMUM_LENGTH).IsRequired();
        builder.Property(user => user.PasswordHash).HasColumnName("ClaveHash").HasMaxLength(User.PASSWORD_HASH_MAXIMUM_LENGTH).IsRequired();
        builder.Property(user => user.Role).HasColumnName("Rol").HasConversion<string>().HasMaxLength(ModelBuilderExtensions.ENUM_MAXIMUM_LENGTH);
        builder.Property(user => user.IsActive).HasColumnName("Activo");
        builder.Property(user => user.CreatedOnUtc).HasColumnName("FechaCreacion");
        builder.Ignore(user => user.CanManageRequests);

        builder.HasIndex(user => user.Email).IsUnique().HasDatabaseName("UX_Usuarios_Correo");
        builder.HasIndex(user => new { user.Role, user.IsActive }).HasDatabaseName("IX_Usuarios_Rol_Activo");
    }
}

internal sealed class AreaConfiguration : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> builder)
    {
        builder.ToTable("Areas");
        builder.HasKey(area => area.Id);
        builder.Property(area => area.Id).HasColumnName("Id").UseIdentityColumn();
        builder.Property(area => area.Name).HasColumnName("Nombre").HasMaxLength(Area.NAME_MAXIMUM_LENGTH).IsRequired();
        builder.Property(area => area.IsActive).HasColumnName("Activa");

        builder.HasIndex(area => area.Name).IsUnique().HasDatabaseName("UX_Areas_Nombre");
    }
}

internal sealed class RequestTypeConfiguration : IEntityTypeConfiguration<RequestType>
{
    public void Configure(EntityTypeBuilder<RequestType> builder)
    {
        builder.ToTable("TiposSolicitud");
        builder.HasKey(requestType => requestType.Id);
        builder.Property(requestType => requestType.Id).HasColumnName("Id").UseIdentityColumn();
        builder.Property(requestType => requestType.Name).HasColumnName("Nombre").HasMaxLength(RequestType.NAME_MAXIMUM_LENGTH).IsRequired();
        builder.Property(requestType => requestType.Description).HasColumnName("Descripcion").HasMaxLength(RequestType.DESCRIPTION_MAXIMUM_LENGTH).IsRequired();
        builder.Property(requestType => requestType.IsActive).HasColumnName("Activo");

        builder.HasIndex(requestType => requestType.Name).IsUnique().HasDatabaseName("UX_TiposSolicitud_Nombre");
    }
}
