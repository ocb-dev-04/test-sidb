IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE TABLE [Areas] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Activa] bit NOT NULL,
        CONSTRAINT [PK_Areas] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE TABLE [ContadoresSolicitud] (
        [Anio] int NOT NULL,
        [UltimoNumero] int NOT NULL,
        CONSTRAINT [PK_ContadoresSolicitud] PRIMARY KEY ([Anio])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE TABLE [TiposSolicitud] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Descripcion] nvarchar(500) NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_TiposSolicitud] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE TABLE [Usuarios] (
        [Id] uniqueidentifier NOT NULL,
        [NombreCompleto] nvarchar(150) NOT NULL,
        [Correo] nvarchar(256) NOT NULL,
        [ClaveHash] nvarchar(256) NOT NULL,
        [Rol] varchar(50) NOT NULL,
        [Activo] bit NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE TABLE [Solicitudes] (
        [Id] uniqueidentifier NOT NULL,
        [Codigo] nvarchar(20) NOT NULL,
        [Titulo] nvarchar(150) NOT NULL,
        [Descripcion] nvarchar(4000) NOT NULL,
        [ReferenciaEvidencia] nvarchar(500) NULL,
        [Prioridad] varchar(50) NOT NULL,
        [Estado] varchar(50) NOT NULL,
        [TipoSolicitudId] int NOT NULL,
        [AreaId] int NOT NULL,
        [SolicitanteId] uniqueidentifier NOT NULL,
        [ResponsableId] uniqueidentifier NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaCompromiso] datetime2 NOT NULL,
        [FechaActualizacion] datetime2 NULL,
        [FechaResolucion] datetime2 NULL,
        [FechaCierre] datetime2 NULL,
        [ComentarioResolucion] nvarchar(1000) NULL,
        [VersionFila] rowversion NOT NULL,
        CONSTRAINT [PK_Solicitudes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Solicitudes_Areas_AreaId] FOREIGN KEY ([AreaId]) REFERENCES [Areas] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Solicitudes_TiposSolicitud_TipoSolicitudId] FOREIGN KEY ([TipoSolicitudId]) REFERENCES [TiposSolicitud] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Solicitudes_Usuarios_ResponsableId] FOREIGN KEY ([ResponsableId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Solicitudes_Usuarios_SolicitanteId] FOREIGN KEY ([SolicitanteId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE TABLE [Comentarios] (
        [Id] uniqueidentifier NOT NULL,
        [SolicitudId] uniqueidentifier NOT NULL,
        [UsuarioId] uniqueidentifier NOT NULL,
        [Texto] nvarchar(2000) NOT NULL,
        [Visibilidad] varchar(50) NOT NULL,
        [Fecha] datetime2 NOT NULL,
        CONSTRAINT [PK_Comentarios] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Comentarios_Solicitudes_SolicitudId] FOREIGN KEY ([SolicitudId]) REFERENCES [Solicitudes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Comentarios_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE TABLE [HistorialEstados] (
        [Id] uniqueidentifier NOT NULL,
        [SolicitudId] uniqueidentifier NOT NULL,
        [EstadoAnterior] varchar(50) NULL,
        [EstadoNuevo] varchar(50) NOT NULL,
        [UsuarioId] uniqueidentifier NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [Comentario] nvarchar(1000) NULL,
        CONSTRAINT [PK_HistorialEstados] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HistorialEstados_Solicitudes_SolicitudId] FOREIGN KEY ([SolicitudId]) REFERENCES [Solicitudes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_HistorialEstados_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE TABLE [Notificaciones] (
        [Id] uniqueidentifier NOT NULL,
        [UsuarioDestinoId] uniqueidentifier NOT NULL,
        [SolicitudId] uniqueidentifier NULL,
        [Canal] varchar(50) NOT NULL,
        [Tipo] varchar(50) NOT NULL,
        [Asunto] nvarchar(200) NOT NULL,
        [Mensaje] nvarchar(2000) NOT NULL,
        [Estado] varchar(50) NOT NULL,
        [Intentos] int NOT NULL,
        [UltimoError] nvarchar(1000) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaEnvio] datetime2 NULL,
        [FechaLectura] datetime2 NULL,
        CONSTRAINT [PK_Notificaciones] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notificaciones_Solicitudes_SolicitudId] FOREIGN KEY ([SolicitudId]) REFERENCES [Solicitudes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Notificaciones_Usuarios_UsuarioDestinoId] FOREIGN KEY ([UsuarioDestinoId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE UNIQUE INDEX [UX_Areas_Nombre] ON [Areas] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_Comentarios_Solicitud_Fecha] ON [Comentarios] ([SolicitudId], [Fecha]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_Comentarios_UsuarioId] ON [Comentarios] ([UsuarioId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_HistorialEstados_Solicitud_Fecha] ON [HistorialEstados] ([SolicitudId], [Fecha]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_HistorialEstados_UsuarioId] ON [HistorialEstados] ([UsuarioId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_Notificaciones_Destino_Canal_Lectura] ON [Notificaciones] ([UsuarioDestinoId], [Canal], [FechaLectura]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_Notificaciones_Estado_Fecha] ON [Notificaciones] ([Estado], [FechaCreacion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_Notificaciones_SolicitudId] ON [Notificaciones] ([SolicitudId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_Solicitudes_AreaId] ON [Solicitudes] ([AreaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_Solicitudes_Estado_Prioridad] ON [Solicitudes] ([Estado], [Prioridad]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_Solicitudes_FechaCompromiso] ON [Solicitudes] ([FechaCompromiso]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_Solicitudes_FechaCreacion] ON [Solicitudes] ([FechaCreacion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_Solicitudes_Responsable_Estado] ON [Solicitudes] ([ResponsableId], [Estado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_Solicitudes_Solicitante_Fecha] ON [Solicitudes] ([SolicitanteId], [FechaCreacion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_Solicitudes_TipoSolicitudId] ON [Solicitudes] ([TipoSolicitudId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE UNIQUE INDEX [UX_Solicitudes_Codigo] ON [Solicitudes] ([Codigo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE UNIQUE INDEX [UX_TiposSolicitud_Nombre] ON [TiposSolicitud] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE INDEX [IX_Usuarios_Rol_Activo] ON [Usuarios] ([Rol], [Activo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    CREATE UNIQUE INDEX [UX_Usuarios_Correo] ON [Usuarios] ([Correo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261013025511_InitialSchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261013025511_InitialSchema', N'8.0.31');
END;
GO

COMMIT;
GO

