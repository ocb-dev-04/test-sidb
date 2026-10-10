BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010060959_SeedData'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activa', N'Nombre') AND [object_id] = OBJECT_ID(N'[Areas]'))
        SET IDENTITY_INSERT [Areas] ON;
    EXEC(N'INSERT INTO [Areas] ([Id], [Activa], [Nombre])
    VALUES (1, CAST(1 AS bit), N''Tecnología de la Información''),
    (2, CAST(1 AS bit), N''Recursos Humanos''),
    (3, CAST(1 AS bit), N''Finanzas''),
    (4, CAST(1 AS bit), N''Supervisión Bancaria''),
    (5, CAST(1 AS bit), N''Consultoría Jurídica''),
    (6, CAST(1 AS bit), N''Servicios Generales'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activa', N'Nombre') AND [object_id] = OBJECT_ID(N'[Areas]'))
        SET IDENTITY_INSERT [Areas] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010060959_SeedData'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Anio', N'UltimoNumero') AND [object_id] = OBJECT_ID(N'[ContadoresSolicitud]'))
        SET IDENTITY_INSERT [ContadoresSolicitud] ON;
    EXEC(N'INSERT INTO [ContadoresSolicitud] ([Anio], [UltimoNumero])
    VALUES (2026, 8)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Anio', N'UltimoNumero') AND [object_id] = OBJECT_ID(N'[ContadoresSolicitud]'))
        SET IDENTITY_INSERT [ContadoresSolicitud] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010060959_SeedData'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Descripcion', N'Activo', N'Nombre') AND [object_id] = OBJECT_ID(N'[TiposSolicitud]'))
        SET IDENTITY_INSERT [TiposSolicitud] ON;
    EXEC(N'INSERT INTO [TiposSolicitud] ([Id], [Descripcion], [Activo], [Nombre])
    VALUES (1, N''Fallas de software, correo o herramientas de oficina.'', CAST(1 AS bit), N''Soporte técnico''),
    (2, N''Altas, bajas y cambios de permisos en sistemas institucionales.'', CAST(1 AS bit), N''Acceso a sistemas''),
    (3, N''Nuevas funcionalidades, reportes o ajustes a aplicaciones.'', CAST(1 AS bit), N''Requerimiento de desarrollo''),
    (4, N''Correos sospechosos, malware o accesos no autorizados.'', CAST(1 AS bit), N''Incidente de seguridad''),
    (5, N''Computadoras, impresoras y otros dispositivos.'', CAST(1 AS bit), N''Equipos y periféricos'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Descripcion', N'Activo', N'Nombre') AND [object_id] = OBJECT_ID(N'[TiposSolicitud]'))
        SET IDENTITY_INSERT [TiposSolicitud] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010060959_SeedData'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'FechaCreacion', N'Correo', N'NombreCompleto', N'Activo', N'ClaveHash', N'Rol') AND [object_id] = OBJECT_ID(N'[Usuarios]'))
        SET IDENTITY_INSERT [Usuarios] ON;
    EXEC(N'INSERT INTO [Usuarios] ([Id], [FechaCreacion], [Correo], [NombreCompleto], [Activo], [ClaveHash], [Rol])
    VALUES (''00000001-0000-0000-0000-000000000001'', ''2026-09-01T12:00:00.0000000Z'', N''admin@solicitudes.local'', N''Administrador del Sistema'', CAST(1 AS bit), N''PBKDF2-SHA256.100000.+9YkaJuGlrd4t4HrEysALQ==.I5f5qXFwU9IHlmzqYETatOxi5vYzLgqQ+UJ3xrVoaK4='', N''Administrator''),
    (''00000002-0000-0000-0000-000000000001'', ''2026-09-01T12:00:00.0000000Z'', N''analista1@solicitudes.local'', N''María Fernández'', CAST(1 AS bit), N''PBKDF2-SHA256.100000.deqNkOcer45y2xzt1cDhGg==.eqP1WaWO78h70d3eK2c/M7ym2ZGAWWmlTEPEYiLoVZc='', N''Analyst''),
    (''00000003-0000-0000-0000-000000000001'', ''2026-09-01T12:00:00.0000000Z'', N''analista2@solicitudes.local'', N''José Martínez'', CAST(1 AS bit), N''PBKDF2-SHA256.100000.u8erlCmOzpORwWZi/JDJxw==.ojn6Mwnmak6NQcuF6eWd57RdvsZQ+o4WqTLqfXSyeUQ='', N''Analyst''),
    (''00000004-0000-0000-0000-000000000001'', ''2026-09-01T12:00:00.0000000Z'', N''solicitante1@solicitudes.local'', N''Ana Rodríguez'', CAST(1 AS bit), N''PBKDF2-SHA256.100000.RyQJhmuqJ62Hs7/iul9gdA==.+Hft5Dwo9lFoA4NbEnNoiRtukL6oxwL2t+w/t+XaGHE='', N''Requester''),
    (''00000005-0000-0000-0000-000000000001'', ''2026-09-01T12:00:00.0000000Z'', N''solicitante2@solicitudes.local'', N''Carlos Pérez'', CAST(1 AS bit), N''PBKDF2-SHA256.100000.SlmZNl8iBtMxVVnRXZy1+A==.6JGruT5IMLeerH2H+VdBjvUG1FTnpJqPcm15eAwc6h8='', N''Requester'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'FechaCreacion', N'Correo', N'NombreCompleto', N'Activo', N'ClaveHash', N'Rol') AND [object_id] = OBJECT_ID(N'[Usuarios]'))
        SET IDENTITY_INSERT [Usuarios] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010060959_SeedData'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AreaId', N'ResponsableId', N'FechaCierre', N'Codigo', N'FechaCreacion', N'Descripcion', N'FechaCompromiso', N'ReferenciaEvidencia', N'Prioridad', N'TipoSolicitudId', N'SolicitanteId', N'ComentarioResolucion', N'FechaResolucion', N'Estado', N'Titulo', N'FechaActualizacion') AND [object_id] = OBJECT_ID(N'[Solicitudes]'))
        SET IDENTITY_INSERT [Solicitudes] ON;
    EXEC(N'INSERT INTO [Solicitudes] ([Id], [AreaId], [ResponsableId], [FechaCierre], [Codigo], [FechaCreacion], [Descripcion], [FechaCompromiso], [ReferenciaEvidencia], [Prioridad], [TipoSolicitudId], [SolicitanteId], [ComentarioResolucion], [FechaResolucion], [Estado], [Titulo], [FechaActualizacion])
    VALUES (''00000001-0000-0000-0000-000000000002'', 3, ''00000002-0000-0000-0000-000000000001'', ''2026-09-16T09:00:00.0000000Z'', N''SOL-2026-0001'', ''2026-09-15T13:00:00.0000000Z'', N''Desde esta mañana Outlook indica que la cuenta está bloqueada.'', ''2026-09-16T13:00:00.0000000Z'', N''https://intranet.local/capturas/correo-bloqueado.png'', N''High'', 1, ''00000004-0000-0000-0000-000000000001'', N''La solicitante confirmó el acceso al correo.'', ''2026-09-15T18:00:00.0000000Z'', N''Closed'', N''No puedo acceder al correo institucional'', ''2026-09-16T09:00:00.0000000Z''),
    (''00000002-0000-0000-0000-000000000002'', 4, ''00000003-0000-0000-0000-000000000001'', NULL, N''SOL-2026-0002'', ''2026-09-25T14:30:00.0000000Z'', N''Requiero perfil de consulta en el sistema de supervisión para el nuevo analista del área.'', ''2026-09-28T14:30:00.0000000Z'', NULL, N''Medium'', 2, ''00000005-0000-0000-0000-000000000001'', NULL, NULL, N''InProgress'', N''Acceso al sistema de supervisión de entidades'', ''2026-09-26T14:30:00.0000000Z''),
    (''00000003-0000-0000-0000-000000000002'', 3, ''00000002-0000-0000-0000-000000000001'', NULL, N''SOL-2026-0003'', ''2026-10-01T15:00:00.0000000Z'', N''Automatizar el reporte mensual de cartera que hoy se arma manualmente.'', ''2026-10-06T15:00:00.0000000Z'', NULL, N''Low'', 3, ''00000004-0000-0000-0000-000000000001'', NULL, NULL, N''InAnalysis'', N''Reporte mensual de cartera en Excel'', ''2026-10-01T18:00:00.0000000Z''),
    (''00000004-0000-0000-0000-000000000002'', 4, ''00000003-0000-0000-0000-000000000001'', NULL, N''SOL-2026-0004'', ''2026-10-03T12:15:00.0000000Z'', N''Recibí un correo que simula ser de un banco con un adjunto .zip.'', ''2026-10-03T20:15:00.0000000Z'', N''Asunto: Estado de cuenta pendiente'', N''Critical'', 4, ''00000005-0000-0000-0000-000000000001'', N''Correo bloqueado en la pasarela y remitente agregado a la lista negra.'', ''2026-10-03T16:15:00.0000000Z'', N''Resolved'', N''Correo sospechoso con archivo adjunto'', ''2026-10-03T16:15:00.0000000Z''),
    (''00000005-0000-0000-0000-000000000002'', 3, ''00000003-0000-0000-0000-000000000001'', NULL, N''SOL-2026-0005'', ''2026-10-05T13:45:00.0000000Z'', N''La laptop asignada no enciende aun conectada al cargador.'', ''2026-10-06T13:45:00.0000000Z'', NULL, N''High'', 5, ''00000004-0000-0000-0000-000000000001'', NULL, NULL, N''WaitingForRequester'', N''Laptop no enciende'', ''2026-10-05T19:45:00.0000000Z''),
    (''00000006-0000-0000-0000-000000000002'', 4, NULL, NULL, N''SOL-2026-0006'', ''2026-10-08T16:00:00.0000000Z'', N''Solicito instalar R y RStudio para análisis de riesgo.'', ''2026-10-11T16:00:00.0000000Z'', NULL, N''Medium'', 1, ''00000005-0000-0000-0000-000000000001'', NULL, NULL, N''Registered'', N''Instalación de software estadístico'', NULL),
    (''00000007-0000-0000-0000-000000000002'', 3, NULL, NULL, N''SOL-2026-0007'', ''2026-10-09T13:20:00.0000000Z'', N''Necesito permisos de escritura en la carpeta compartida de presupuesto.'', ''2026-10-12T13:20:00.0000000Z'', NULL, N''Medium'', 2, ''00000004-0000-0000-0000-000000000001'', NULL, NULL, N''Registered'', N''Permisos de carpeta compartida'', NULL),
    (''00000008-0000-0000-0000-000000000002'', 2, ''00000002-0000-0000-0000-000000000001'', NULL, N''SOL-2026-0008'', ''2026-10-09T18:10:00.0000000Z'', N''El formulario de viáticos muestra un error 500 al guardar.'', ''2026-10-10T18:10:00.0000000Z'', N''https://intranet.local/viaticos'', N''High'', 3, ''00000005-0000-0000-0000-000000000001'', NULL, NULL, N''InAnalysis'', N''Error al guardar el formulario de viáticos'', ''2026-10-09T20:10:00.0000000Z'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AreaId', N'ResponsableId', N'FechaCierre', N'Codigo', N'FechaCreacion', N'Descripcion', N'FechaCompromiso', N'ReferenciaEvidencia', N'Prioridad', N'TipoSolicitudId', N'SolicitanteId', N'ComentarioResolucion', N'FechaResolucion', N'Estado', N'Titulo', N'FechaActualizacion') AND [object_id] = OBJECT_ID(N'[Solicitudes]'))
        SET IDENTITY_INSERT [Solicitudes] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010060959_SeedData'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'UsuarioId', N'Fecha', N'SolicitudId', N'Texto', N'Visibilidad') AND [object_id] = OBJECT_ID(N'[Comentarios]'))
        SET IDENTITY_INSERT [Comentarios] ON;
    EXEC(N'INSERT INTO [Comentarios] ([Id], [UsuarioId], [Fecha], [SolicitudId], [Texto], [Visibilidad])
    VALUES (''00000001-0000-0000-0000-000000000004'', ''00000002-0000-0000-0000-000000000001'', ''2026-09-15T13:30:00.0000000Z'', ''00000001-0000-0000-0000-000000000002'', N''Recibimos su solicitud y ya la estamos revisando.'', N''Public''),
    (''00000002-0000-0000-0000-000000000004'', ''00000002-0000-0000-0000-000000000001'', ''2026-09-15T13:45:00.0000000Z'', ''00000001-0000-0000-0000-000000000002'', N''Nota interna: validar con el supervisor del área antes de cerrar.'', N''Internal''),
    (''00000003-0000-0000-0000-000000000004'', ''00000003-0000-0000-0000-000000000001'', ''2026-09-25T15:00:00.0000000Z'', ''00000002-0000-0000-0000-000000000002'', N''Recibimos su solicitud y ya la estamos revisando.'', N''Public''),
    (''00000004-0000-0000-0000-000000000004'', ''00000003-0000-0000-0000-000000000001'', ''2026-09-25T15:15:00.0000000Z'', ''00000002-0000-0000-0000-000000000002'', N''Nota interna: validar con el supervisor del área antes de cerrar.'', N''Internal''),
    (''00000005-0000-0000-0000-000000000004'', ''00000002-0000-0000-0000-000000000001'', ''2026-10-01T15:30:00.0000000Z'', ''00000003-0000-0000-0000-000000000002'', N''Recibimos su solicitud y ya la estamos revisando.'', N''Public''),
    (''00000006-0000-0000-0000-000000000004'', ''00000002-0000-0000-0000-000000000001'', ''2026-10-01T15:45:00.0000000Z'', ''00000003-0000-0000-0000-000000000002'', N''Nota interna: validar con el supervisor del área antes de cerrar.'', N''Internal''),
    (''00000007-0000-0000-0000-000000000004'', ''00000003-0000-0000-0000-000000000001'', ''2026-10-03T12:45:00.0000000Z'', ''00000004-0000-0000-0000-000000000002'', N''Recibimos su solicitud y ya la estamos revisando.'', N''Public''),
    (''00000008-0000-0000-0000-000000000004'', ''00000003-0000-0000-0000-000000000001'', ''2026-10-03T13:00:00.0000000Z'', ''00000004-0000-0000-0000-000000000002'', N''Nota interna: validar con el supervisor del área antes de cerrar.'', N''Internal''),
    (''00000009-0000-0000-0000-000000000004'', ''00000003-0000-0000-0000-000000000001'', ''2026-10-05T14:15:00.0000000Z'', ''00000005-0000-0000-0000-000000000002'', N''Recibimos su solicitud y ya la estamos revisando.'', N''Public''),
    (''0000000a-0000-0000-0000-000000000004'', ''00000003-0000-0000-0000-000000000001'', ''2026-10-05T14:30:00.0000000Z'', ''00000005-0000-0000-0000-000000000002'', N''Nota interna: validar con el supervisor del área antes de cerrar.'', N''Internal''),
    (''0000000b-0000-0000-0000-000000000004'', ''00000002-0000-0000-0000-000000000001'', ''2026-10-09T18:40:00.0000000Z'', ''00000008-0000-0000-0000-000000000002'', N''Recibimos su solicitud y ya la estamos revisando.'', N''Public''),
    (''0000000c-0000-0000-0000-000000000004'', ''00000002-0000-0000-0000-000000000001'', ''2026-10-09T18:55:00.0000000Z'', ''00000008-0000-0000-0000-000000000002'', N''Nota interna: validar con el supervisor del área antes de cerrar.'', N''Internal'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'UsuarioId', N'Fecha', N'SolicitudId', N'Texto', N'Visibilidad') AND [object_id] = OBJECT_ID(N'[Comentarios]'))
        SET IDENTITY_INSERT [Comentarios] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010060959_SeedData'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'UsuarioId', N'Fecha', N'Comentario', N'EstadoNuevo', N'EstadoAnterior', N'SolicitudId') AND [object_id] = OBJECT_ID(N'[HistorialEstados]'))
        SET IDENTITY_INSERT [HistorialEstados] ON;
    EXEC(N'INSERT INTO [HistorialEstados] ([Id], [UsuarioId], [Fecha], [Comentario], [EstadoNuevo], [EstadoAnterior], [SolicitudId])
    VALUES (''00000001-0000-0000-0000-000000000003'', ''00000004-0000-0000-0000-000000000001'', ''2026-09-15T13:00:00.0000000Z'', NULL, N''Registered'', NULL, ''00000001-0000-0000-0000-000000000002''),
    (''00000002-0000-0000-0000-000000000003'', ''00000002-0000-0000-0000-000000000001'', ''2026-09-15T14:00:00.0000000Z'', NULL, N''InAnalysis'', N''Registered'', ''00000001-0000-0000-0000-000000000002''),
    (''00000003-0000-0000-0000-000000000003'', ''00000002-0000-0000-0000-000000000001'', ''2026-09-15T15:00:00.0000000Z'', NULL, N''InProgress'', N''InAnalysis'', ''00000001-0000-0000-0000-000000000002''),
    (''00000004-0000-0000-0000-000000000003'', ''00000002-0000-0000-0000-000000000001'', ''2026-09-15T18:00:00.0000000Z'', N''Se desbloqueó la cuenta y se restableció la contraseña.'', N''Resolved'', N''InProgress'', ''00000001-0000-0000-0000-000000000002''),
    (''00000005-0000-0000-0000-000000000003'', ''00000002-0000-0000-0000-000000000001'', ''2026-09-16T09:00:00.0000000Z'', N''La solicitante confirmó el acceso al correo.'', N''Closed'', N''Resolved'', ''00000001-0000-0000-0000-000000000002''),
    (''00000006-0000-0000-0000-000000000003'', ''00000005-0000-0000-0000-000000000001'', ''2026-09-25T14:30:00.0000000Z'', NULL, N''Registered'', NULL, ''00000002-0000-0000-0000-000000000002''),
    (''00000007-0000-0000-0000-000000000003'', ''00000003-0000-0000-0000-000000000001'', ''2026-09-25T16:30:00.0000000Z'', NULL, N''InAnalysis'', N''Registered'', ''00000002-0000-0000-0000-000000000002''),
    (''00000008-0000-0000-0000-000000000003'', ''00000003-0000-0000-0000-000000000001'', ''2026-09-26T14:30:00.0000000Z'', N''Pendiente aprobación del supervisor directo.'', N''InProgress'', N''InAnalysis'', ''00000002-0000-0000-0000-000000000002''),
    (''00000009-0000-0000-0000-000000000003'', ''00000004-0000-0000-0000-000000000001'', ''2026-10-01T15:00:00.0000000Z'', NULL, N''Registered'', NULL, ''00000003-0000-0000-0000-000000000002''),
    (''0000000a-0000-0000-0000-000000000003'', ''00000002-0000-0000-0000-000000000001'', ''2026-10-01T18:00:00.0000000Z'', NULL, N''InAnalysis'', N''Registered'', ''00000003-0000-0000-0000-000000000002''),
    (''0000000b-0000-0000-0000-000000000003'', ''00000005-0000-0000-0000-000000000001'', ''2026-10-03T12:15:00.0000000Z'', NULL, N''Registered'', NULL, ''00000004-0000-0000-0000-000000000002''),
    (''0000000c-0000-0000-0000-000000000003'', ''00000003-0000-0000-0000-000000000001'', ''2026-10-03T13:15:00.0000000Z'', NULL, N''InAnalysis'', N''Registered'', ''00000004-0000-0000-0000-000000000002''),
    (''0000000d-0000-0000-0000-000000000003'', ''00000003-0000-0000-0000-000000000001'', ''2026-10-03T16:15:00.0000000Z'', N''Correo bloqueado en la pasarela y remitente agregado a la lista negra.'', N''Resolved'', N''InAnalysis'', ''00000004-0000-0000-0000-000000000002''),
    (''0000000e-0000-0000-0000-000000000003'', ''00000004-0000-0000-0000-000000000001'', ''2026-10-05T13:45:00.0000000Z'', NULL, N''Registered'', NULL, ''00000005-0000-0000-0000-000000000002''),
    (''0000000f-0000-0000-0000-000000000003'', ''00000003-0000-0000-0000-000000000001'', ''2026-10-05T14:45:00.0000000Z'', NULL, N''InAnalysis'', N''Registered'', ''00000005-0000-0000-0000-000000000002''),
    (''00000010-0000-0000-0000-000000000003'', ''00000003-0000-0000-0000-000000000001'', ''2026-10-05T19:45:00.0000000Z'', N''Necesitamos el número de inventario del equipo.'', N''WaitingForRequester'', N''InAnalysis'', ''00000005-0000-0000-0000-000000000002''),
    (''00000011-0000-0000-0000-000000000003'', ''00000005-0000-0000-0000-000000000001'', ''2026-10-08T16:00:00.0000000Z'', NULL, N''Registered'', NULL, ''00000006-0000-0000-0000-000000000002''),
    (''00000012-0000-0000-0000-000000000003'', ''00000004-0000-0000-0000-000000000001'', ''2026-10-09T13:20:00.0000000Z'', NULL, N''Registered'', NULL, ''00000007-0000-0000-0000-000000000002''),
    (''00000013-0000-0000-0000-000000000003'', ''00000005-0000-0000-0000-000000000001'', ''2026-10-09T18:10:00.0000000Z'', NULL, N''Registered'', NULL, ''00000008-0000-0000-0000-000000000002''),
    (''00000014-0000-0000-0000-000000000003'', ''00000002-0000-0000-0000-000000000001'', ''2026-10-09T20:10:00.0000000Z'', NULL, N''InAnalysis'', N''Registered'', ''00000008-0000-0000-0000-000000000002'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'UsuarioId', N'Fecha', N'Comentario', N'EstadoNuevo', N'EstadoAnterior', N'SolicitudId') AND [object_id] = OBJECT_ID(N'[HistorialEstados]'))
        SET IDENTITY_INSERT [HistorialEstados] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010060959_SeedData'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261010060959_SeedData', N'8.0.31');
END;
GO

COMMIT;
GO

