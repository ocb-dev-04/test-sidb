# Evidencias y guía del flujo principal

Capturas tomadas sobre la aplicación en ejecución (Chrome, 1440 px) con los datos de ejemplo que la API carga en el primer arranque.

## 1. Inicio de sesión

![Login](evidencias/01-login.png)

Cualquier usuario de la tabla del README con la clave `Demo2026*`. Credenciales inválidas muestran un mensaje
genérico (no se revela si el correo existe) y el login tiene límite de intentos por IP.

## 2. Tablero (Inicio)

![Tablero](evidencias/02-inicio-tablero.png)

Totales, abiertas, cerradas y vencidas; distribución por estado y por prioridad; últimas solicitudes con estado y
responsable. Las cifras respetan la visibilidad del rol.

## 3. Solicitudes · Consulta

![Consulta de solicitudes](evidencias/03-solicitudes-consulta.png)

Paginación y filtros resueltos en el servidor (estado, prioridad, área, tipo, fechas, texto, solo vencidas) y orden
configurable. Los filtros se conservan en la URL.

## 4. Solicitudes · Detalle

![Detalle de solicitud](evidencias/04-solicitud-detalle.png)

Historial de estados con usuario, fecha y comentario; comentarios públicos e internos; cambio de estado limitado a
las transiciones permitidas (con comentario obligatorio para resolver, cerrar o reabrir) y asignación de
responsable.

## 5. Solicitudes · Crear registro

![Crear solicitud](evidencias/05-solicitud-crear.png)

Validaciones visibles que replican las del backend (obligatorios y longitudes máximas).

## 6. Entidades gubernamentales · Consulta (archivo de texto)

![Entidades - archivo de texto](evidencias/06-entidades-consulta-texto.png)

Fuente predeterminada: `Data/entidades-gubernamentales.txt`. Acciones de editar y eliminar para administradores.

## 7. Entidades gubernamentales · Consulta (Excel, solo lectura)

![Entidades - Excel](evidencias/07-entidades-consulta-excel.png)

La misma vista sobre el Excel original: las acciones de escritura quedan deshabilitadas y la API las rechaza con
`governmentEntity.sourceReadOnly`.

## 8. Entidades gubernamentales · Crear registro

![Crear entidad](evidencias/08-entidades-crear.png)

## 9. Administración · Usuarios

![Usuarios](evidencias/09-administracion-usuarios.png)

## 10. Vista del Solicitante

![Detalle visto por el solicitante](evidencias/10-solicitante-detalle-sin-internos.png)

La misma solicitud del punto 4 vista por su solicitante: sin comentarios internos y sin acciones de gestión.

## Recorrido sugerido para la revisión

1. Entrar como `solicitante1@solicitudes.local` y crear una solicitud → aparece con código `SOL-AAAA-NNNN` y
   la campana muestra la notificación de registro.
2. Entrar como `analista1@solicitudes.local`, abrir la solicitud, asignarla a sí misma, pasarla a *En análisis*
   y luego intentar *Cerrada* sin comentario (se rechaza) y con comentario (se acepta).
3. Volver como solicitante: la campana muestra los cambios y el detalle conserva el historial completo.
4. Entrar como `admin@solicitudes.local` y en *Entidades gubernamentales* alternar entre las fuentes, crear,
   editar y eliminar una entidad en el archivo de texto, y comprobar que en Excel no se permite.
5. Revisar `src/backend/src/SB.Solicitudes.Servicios/simulated-mail/` (o el contenedor de la API) para ver los
   correos simulados generados.
