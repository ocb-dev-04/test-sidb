using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SB.Solicitudes.Dominio.Notifications;
using SB.Solicitudes.Dominio.ServiceRequests;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Infraestructura.Persistence.Configurations;

internal sealed class ServiceRequestConfiguration : IEntityTypeConfiguration<ServiceRequest>
{
    public const string ROW_VERSION_PROPERTY = "RowVersion";

    public void Configure(EntityTypeBuilder<ServiceRequest> builder)
    {
        builder.ToTable("Solicitudes");
        builder.HasKey(serviceRequest => serviceRequest.Id);
        builder.Property(serviceRequest => serviceRequest.Id).HasColumnName("Id").ValueGeneratedNever();
        builder.Property(serviceRequest => serviceRequest.Code).HasColumnName("Codigo").HasMaxLength(ServiceRequestCode.MAXIMUM_LENGTH).IsRequired();
        builder.Property(serviceRequest => serviceRequest.Title).HasColumnName("Titulo").HasMaxLength(ServiceRequest.TITLE_MAXIMUM_LENGTH).IsRequired();
        builder.Property(serviceRequest => serviceRequest.Description).HasColumnName("Descripcion").HasMaxLength(ServiceRequest.DESCRIPTION_MAXIMUM_LENGTH).IsRequired();
        builder.Property(serviceRequest => serviceRequest.EvidenceReference).HasColumnName("ReferenciaEvidencia").HasMaxLength(ServiceRequest.EVIDENCE_REFERENCE_MAXIMUM_LENGTH);
        builder.Property(serviceRequest => serviceRequest.Priority).HasColumnName("Prioridad").HasConversion<string>().HasMaxLength(ModelBuilderExtensions.ENUM_MAXIMUM_LENGTH);
        builder.Property(serviceRequest => serviceRequest.Status).HasColumnName("Estado").HasConversion<string>().HasMaxLength(ModelBuilderExtensions.ENUM_MAXIMUM_LENGTH);
        builder.Property(serviceRequest => serviceRequest.RequestTypeId).HasColumnName("TipoSolicitudId");
        builder.Property(serviceRequest => serviceRequest.AreaId).HasColumnName("AreaId");
        builder.Property(serviceRequest => serviceRequest.RequesterId).HasColumnName("SolicitanteId");
        builder.Property(serviceRequest => serviceRequest.AssigneeId).HasColumnName("ResponsableId");
        builder.Property(serviceRequest => serviceRequest.CreatedOnUtc).HasColumnName("FechaCreacion");
        builder.Property(serviceRequest => serviceRequest.DueDateUtc).HasColumnName("FechaCompromiso");
        builder.Property(serviceRequest => serviceRequest.UpdatedOnUtc).HasColumnName("FechaActualizacion");
        builder.Property(serviceRequest => serviceRequest.ResolvedOnUtc).HasColumnName("FechaResolucion");
        builder.Property(serviceRequest => serviceRequest.ClosedOnUtc).HasColumnName("FechaCierre");
        builder.Property(serviceRequest => serviceRequest.ResolutionComment).HasColumnName("ComentarioResolucion").HasMaxLength(ServiceRequest.TRANSITION_COMMENT_MAXIMUM_LENGTH);
        builder.Property<byte[]>(ROW_VERSION_PROPERTY).HasColumnName("VersionFila").IsRowVersion().IsRequired();

        builder.HasOne(serviceRequest => serviceRequest.RequestType).WithMany().HasForeignKey(serviceRequest => serviceRequest.RequestTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(serviceRequest => serviceRequest.Area).WithMany().HasForeignKey(serviceRequest => serviceRequest.AreaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(serviceRequest => serviceRequest.Requester).WithMany().HasForeignKey(serviceRequest => serviceRequest.RequesterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(serviceRequest => serviceRequest.Assignee).WithMany().HasForeignKey(serviceRequest => serviceRequest.AssigneeId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(serviceRequest => serviceRequest.StatusHistory).WithOne().HasForeignKey(history => history.ServiceRequestId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(serviceRequest => serviceRequest.Comments).WithOne().HasForeignKey(comment => comment.ServiceRequestId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(serviceRequest => serviceRequest.StatusHistory).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(serviceRequest => serviceRequest.Comments).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(serviceRequest => serviceRequest.Code).IsUnique().HasDatabaseName("UX_Solicitudes_Codigo");
        builder.HasIndex(serviceRequest => new { serviceRequest.Status, serviceRequest.Priority }).HasDatabaseName("IX_Solicitudes_Estado_Prioridad");
        builder.HasIndex(serviceRequest => new { serviceRequest.RequesterId, serviceRequest.CreatedOnUtc }).HasDatabaseName("IX_Solicitudes_Solicitante_Fecha");
        builder.HasIndex(serviceRequest => new { serviceRequest.AssigneeId, serviceRequest.Status }).HasDatabaseName("IX_Solicitudes_Responsable_Estado");
        builder.HasIndex(serviceRequest => serviceRequest.CreatedOnUtc).HasDatabaseName("IX_Solicitudes_FechaCreacion");
        builder.HasIndex(serviceRequest => serviceRequest.DueDateUtc).HasDatabaseName("IX_Solicitudes_FechaCompromiso");
    }
}

internal sealed class StatusHistoryConfiguration : IEntityTypeConfiguration<StatusHistory>
{
    public void Configure(EntityTypeBuilder<StatusHistory> builder)
    {
        builder.ToTable("HistorialEstados");
        builder.HasKey(history => history.Id);
        builder.Property(history => history.Id).HasColumnName("Id").ValueGeneratedNever();
        builder.Property(history => history.ServiceRequestId).HasColumnName("SolicitudId");
        builder.Property(history => history.PreviousStatus).HasColumnName("EstadoAnterior").HasConversion<string>().HasMaxLength(ModelBuilderExtensions.ENUM_MAXIMUM_LENGTH);
        builder.Property(history => history.NewStatus).HasColumnName("EstadoNuevo").HasConversion<string>().HasMaxLength(ModelBuilderExtensions.ENUM_MAXIMUM_LENGTH);
        builder.Property(history => history.ChangedById).HasColumnName("UsuarioId");
        builder.Property(history => history.ChangedOnUtc).HasColumnName("Fecha");
        builder.Property(history => history.Comment).HasColumnName("Comentario").HasMaxLength(ServiceRequest.TRANSITION_COMMENT_MAXIMUM_LENGTH);

        builder.HasOne(history => history.ChangedBy).WithMany().HasForeignKey(history => history.ChangedById).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(history => new { history.ServiceRequestId, history.ChangedOnUtc }).HasDatabaseName("IX_HistorialEstados_Solicitud_Fecha");
    }
}

internal sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comentarios");
        builder.HasKey(comment => comment.Id);
        builder.Property(comment => comment.Id).HasColumnName("Id").ValueGeneratedNever();
        builder.Property(comment => comment.ServiceRequestId).HasColumnName("SolicitudId");
        builder.Property(comment => comment.AuthorId).HasColumnName("UsuarioId");
        builder.Property(comment => comment.Text).HasColumnName("Texto").HasMaxLength(Comment.TEXT_MAXIMUM_LENGTH).IsRequired();
        builder.Property(comment => comment.Visibility).HasColumnName("Visibilidad").HasConversion<string>().HasMaxLength(ModelBuilderExtensions.ENUM_MAXIMUM_LENGTH);
        builder.Property(comment => comment.CreatedOnUtc).HasColumnName("Fecha");

        builder.HasOne(comment => comment.Author).WithMany().HasForeignKey(comment => comment.AuthorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(comment => new { comment.ServiceRequestId, comment.CreatedOnUtc }).HasDatabaseName("IX_Comentarios_Solicitud_Fecha");
    }
}

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notificaciones");
        builder.HasKey(notification => notification.Id);
        builder.Property(notification => notification.Id).HasColumnName("Id").ValueGeneratedNever();
        builder.Property(notification => notification.RecipientUserId).HasColumnName("UsuarioDestinoId");
        builder.Property(notification => notification.ServiceRequestId).HasColumnName("SolicitudId");
        builder.Property(notification => notification.Channel).HasColumnName("Canal").HasConversion<string>().HasMaxLength(ModelBuilderExtensions.ENUM_MAXIMUM_LENGTH);
        builder.Property(notification => notification.Type).HasColumnName("Tipo").HasConversion<string>().HasMaxLength(ModelBuilderExtensions.ENUM_MAXIMUM_LENGTH);
        builder.Property(notification => notification.Subject).HasColumnName("Asunto").HasMaxLength(Notification.SUBJECT_MAXIMUM_LENGTH).IsRequired();
        builder.Property(notification => notification.Message).HasColumnName("Mensaje").HasMaxLength(Notification.MESSAGE_MAXIMUM_LENGTH).IsRequired();
        builder.Property(notification => notification.Status).HasColumnName("Estado").HasConversion<string>().HasMaxLength(ModelBuilderExtensions.ENUM_MAXIMUM_LENGTH);
        builder.Property(notification => notification.DeliveryAttempts).HasColumnName("Intentos");
        builder.Property(notification => notification.LastError).HasColumnName("UltimoError").HasMaxLength(Notification.LAST_ERROR_MAXIMUM_LENGTH);
        builder.Property(notification => notification.CreatedOnUtc).HasColumnName("FechaCreacion");
        builder.Property(notification => notification.SentOnUtc).HasColumnName("FechaEnvio");
        builder.Property(notification => notification.ReadOnUtc).HasColumnName("FechaLectura");

        builder.HasOne<User>().WithMany().HasForeignKey(notification => notification.RecipientUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ServiceRequest>().WithMany().HasForeignKey(notification => notification.ServiceRequestId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(notification => new { notification.Status, notification.CreatedOnUtc }).HasDatabaseName("IX_Notificaciones_Estado_Fecha");
        builder.HasIndex(notification => new { notification.RecipientUserId, notification.Channel, notification.ReadOnUtc }).HasDatabaseName("IX_Notificaciones_Destino_Canal_Lectura");
    }
}

internal sealed class ServiceRequestCounterConfiguration : IEntityTypeConfiguration<ServiceRequestCounter>
{
    public void Configure(EntityTypeBuilder<ServiceRequestCounter> builder)
    {
        builder.ToTable("ContadoresSolicitud");
        builder.HasKey(counter => counter.Year);
        builder.Property(counter => counter.Year).HasColumnName("Anio").ValueGeneratedNever();
        builder.Property(counter => counter.LastNumber).HasColumnName("UltimoNumero");
    }
}
