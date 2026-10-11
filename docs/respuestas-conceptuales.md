# Respuestas a las preguntas de conceptualización

Cada respuesta indica cómo quedó aplicada en la solución entregada, para que la teoría y la implementación se
puedan contrastar directamente.

---

### 1. ¿Cómo organizarías una solución .NET usando Clean Architecture o arquitectura por capas?

Por anillos concéntricos donde las dependencias apuntan siempre hacia el centro:

- **Dominio**: entidades con comportamiento e invariantes, objetos de valor, eventos de dominio y errores de
  negocio. No conoce bases de datos, HTTP ni frameworks.
- **Aplicación**: casos de uso (un comando o consulta por operación), validaciones de entrada, autorización por
  datos y los *puertos* (interfaces) que necesita: repositorios, reloj, envío de notificaciones, etc.
- **Infraestructura**: implementaciones de esos puertos (EF Core, archivos, correo, colas, JWT).
- **Presentación / Servicios**: API REST, autenticación, serialización y composición de dependencias.

En la entrega, la especificación pidió Onion con cuatro capas, así que quedaron
`SB.Solicitudes.Dominio`, `.Aplicacion`, `.Infraestructura` y `.Servicios` (host de la API). La regla de dependencias
no se deja a la disciplina: un proyecto de pruebas de arquitectura (NetArchTest) falla si Dominio o Aplicación
referencian EF Core, ASP.NET Core o una capa externa. Dentro de Aplicación organizo por *feature*
(`Features/ServiceRequests/ChangeServiceRequestStatus/...`) y no por tipo técnico, para que un cambio de negocio
toque una sola carpeta.

### 2. ¿Qué validaciones aplicarías en backend aun cuando el frontend ya valide formularios?

Todas las que protegen la integridad y la seguridad, porque el frontend se puede saltar con cualquier cliente HTTP:

- **Formato y límites**: obligatorios, longitudes máximas, rangos, enums válidos, tamaño de página máximo
  (FluentValidation en un *behavior* del pipeline, antes de llegar al caso de uso).
- **Existencia y estado de referencias**: que el área y el tipo de solicitud existan y estén activos, que el
  responsable sea un analista o administrador activo.
- **Reglas de negocio** en el dominio: transiciones de estado permitidas, comentario obligatorio para resolver,
  cerrar o reabrir, solo Administrador o Analista reabren, no modificar una solicitud cerrada.
- **Autorización por acción y por dato**: *policies* por rol y, además, verificar que el usuario pueda ver o
  modificar *esa* solicitud.
- **Unicidad y concurrencia**: correo y códigos únicos (validación más índice único) y `rowversion` para no pisar
  cambios simultáneos.

El frontend valida para dar una buena experiencia; el backend valida para tener la última palabra. Las respuestas
de validación salen como `ValidationProblemDetails` con los errores por campo, que el frontend muestra junto a
cada control.

### 3. ¿Cómo diseñarías el mecanismo de notificaciones para cambiar de correo a cola sin reescribir la lógica de negocio?

Separando tres preguntas que suelen mezclarse: *qué pasó*, *a quién avisar* y *por dónde entregar*.

1. **Qué pasó**: la entidad emite eventos de dominio (`ServiceRequestStatusChangedDomainEvent`, etc.). El caso de
   uso no sabe que existen notificaciones.
2. **A quién avisar**: un suscriptor de esos eventos decide destinatarios y contenido y guarda registros
   `Notificacion` pendientes **en la misma transacción** que el cambio de negocio (patrón *outbox* simplificado:
   no se pierde una notificación si falla el envío, ni se envía una si falla el guardado).
3. **Por dónde entregar**: un *worker* toma las pendientes y las entrega por la interfaz
   `INotificationDeliveryChannel`. Hoy hay dos canales: en la aplicación y correo simulado (archivos `.eml`).

Pasar a una cola es crear `QueueNotificationChannel : INotificationDeliveryChannel` y registrarlo; agregar un canal
nuevo es configurar `Notifications:Channels`. Ni el dominio ni los casos de uso cambian (principio abierto/cerrado).
El *worker* reintenta y marca `Failed` después de un máximo de intentos, lo que deja trazabilidad del envío.

### 4. ¿Qué estrategia usarías para evitar exponer datos de solicitudes a usuarios sin permisos?

Defensa en capas:

- **Autenticación obligatoria por defecto** (política de respaldo): solo `login` y `health` son anónimos.
- **Autorización por acción** con *policies* (cambiar estado o asignar exige Administrador o Analista).
- **Autorización por datos** con una especificación única, `ServiceRequestVisibility`: se traduce a SQL en los
  listados (el filtro se aplica en la base, no después de traer los datos) y se evalúa en memoria en los comandos.
  Un test garantiza que ambas formas coinciden.
- **404 en vez de 403** cuando la solicitud no es visible, para no confirmar que existe.
- **Proyecciones mínimas**: los DTO solo exponen lo necesario; los comentarios internos se excluyen en la consulta
  para el Solicitante.
- **Identificadores no secuenciales** (GUID) para las solicitudes y el código legible solo como referencia.

### 5. ¿Cómo implementarías paginación, filtros y ordenamiento sin afectar el rendimiento?

- Filtros y paginación **en la base de datos** (`IQueryable` que EF traduce a `WHERE ... OFFSET/FETCH`), nunca
  trayendo todo a memoria.
- `AsNoTracking` y **proyección directa al DTO**: se leen solo las columnas que viajan al cliente.
- **Índices compuestos** alineados con los filtros reales: (Estado, Prioridad), (SolicitanteId, FechaCreacion),
  (ResponsableId, Estado), FechaCompromiso, y único sobre Codigo.
- **Ordenamiento por lista blanca** (enum `ServiceRequestSortField`), con desempate estable por Id para que la
  paginación no repita ni salte filas. Prioridad y estado se ordenan por significado de negocio con `CASE`.
- **Tamaño de página máximo** validado (100) y metadatos de paginación (total, páginas) en la respuesta.
- Rangos de fecha convertidos a intervalos semiabiertos (`>= desde` y `< hasta + 1 día`) para que el índice sirva.
- Para volúmenes muy grandes, paginación por cursor (*keyset*) en lugar de `OFFSET`; aquí `OFFSET` es suficiente y
  más simple para la navegación por número de página que pide la interfaz.
- Un interceptor de EF registra las consultas lentas para detectar regresiones.

### 6. Diferencia entre pruebas unitarias, de integración y end-to-end. ¿Dónde aplicarías cada una?

- **Unitarias**: una unidad de lógica aislada, sin E/S, en milisegundos. En la solución: la máquina de estados
  (todas las transiciones como `[Theory]`), reglas de cierre y reapertura, generación del código, `Result`, la
  especificación de visibilidad, los handlers con dobles (NSubstitute) y los suscriptores de notificaciones.
- **Integración**: varias piezas reales juntas. Aquí la API completa con `WebApplicationFactory` contra un SQL Server
  real en Testcontainers: autenticación, *policies*, filtros SQL, migraciones, la carga inicial de datos, el código consecutivo y el
  CRUD de entidades en las tres fuentes. También el almacén de texto con archivos reales y el lector del Excel.
- **End-to-end**: el sistema desde la interfaz como lo usa una persona (navegador → frontend → API → base). Las
  reservaría para los flujos críticos (iniciar sesión, crear solicitud, cerrarla) con Playwright, porque son las
  más lentas y frágiles.

Además hay **pruebas de arquitectura** que fallan si se rompe la regla de dependencias o las convenciones de
nombres de la especificación.

### 7. ¿Cuándo elegirías Entity Framework Core y cuándo Dapper?

**EF Core** por defecto en aplicaciones de negocio: modelo rico con seguimiento de cambios, migraciones
versionadas, concurrencia optimista, LINQ tipado y proyecciones eficientes. Es lo usado aquí.

**Dapper** cuando el control del SQL importa más que la productividad: reportes complejos o con *hints*
específicos, lecturas masivas de solo lectura donde cada milisegundo cuenta, procedimientos almacenados existentes
o esquemas heredados que no encajan en un modelo de entidades. No son excluyentes: un mismo sistema puede escribir
con EF y tener consultas de reporte con Dapper sobre la misma conexión.

### 8. ¿Cómo manejarías errores de forma consistente en una API REST?

- **Errores esperados como valores**: el dominio y los casos de uso devuelven `Result` con un `Error` tipado
  (validación, no encontrado, conflicto, prohibido) y un **código estable** (`serviceRequest.commentRequired`).
- **Un solo traductor a HTTP** en el controller base: tipo de error → código HTTP, y siempre **ProblemDetails
  (RFC 7807)** con `code` y `traceId`. Las validaciones devuelven `ValidationProblemDetails` con errores por campo.
- **Excepciones solo para lo inesperado**, atendidas por un `IExceptionHandler` global: se registran completas en el
  log, pero al cliente solo llega un mensaje genérico con el `traceId` para correlacionar (concurrencia → 409).
- El frontend traduce los códigos a mensajes en español; así el texto mostrado no depende del mensaje técnico.

### 9. ¿Qué información incluirías en logs sin comprometer datos sensibles?

**Sí**: fecha, nivel, `traceId`, identificador del usuario (no su nombre ni correo), operación ejecutada, duración,
resultado y código de error, ids de las entidades afectadas, eventos de negocio (solicitud creada, estado cambiado,
asignación) y excepciones con su traza.

**No**: contraseñas, tokens, cadenas de conexión, cuerpos completos de las peticiones, valores de parámetros SQL ni
datos personales en claro. En la solución: Serilog estructurado (consola y archivo diario), un *middleware* agrega
`UserId` y `TraceId` a cada log, los mensajes se definen con `[LoggerMessage]` (plantillas fijas), el interceptor de
consultas lentas registra el SQL sin valores, y el correo del destinatario se enmascara (`a***@dominio`).

### 10. ¿Cómo documentarías decisiones técnicas relevantes?

Con **ADR** (*Architecture Decision Records*) versionados junto al código: contexto, decisión, alternativas y
consecuencias, en archivos Markdown que se revisan en el mismo *pull request* que el cambio. En la entrega están en
`docs/arquitectura.md` (onion y capas, mediator propio, `Result`, visibilidad por rol, notificaciones, código
consecutivo, almacenes de entidades, seguridad). Lo complemento con:

- README con cómo ejecutar, probar y los usuarios de prueba.
- Contrato OpenAPI generado (Swagger y `docs/api/openapi.json`).
- Diagramas como código (Mermaid), que se mantienen con el repositorio.
- Mensajes de commit descriptivos (Conventional Commits) que cuentan la evolución.
- Pruebas legibles que documentan las reglas de negocio (por ejemplo, la tabla de transiciones).
