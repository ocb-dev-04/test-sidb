# Frontend — Gestión de Solicitudes (Superintendencia de Bancos)

SPA en **React 18 + TypeScript (strict) + Vite** que consume la API REST `SB.Solicitudes.Servicios`.

## Requisitos

- Node.js 20 o superior (probado con Node 22) y npm.
- La API en ejecución (por defecto en `http://localhost:5080`). El servidor de Vite reenvía `/api` a esa dirección,
  así que el navegador habla con un solo origen.

## Configuración

| Variable            | Descripción                                                                  | Valor por defecto        |
| ------------------- | ---------------------------------------------------------------------------- | ------------------------ |
| `API_PROXY_TARGET`  | Destino del proxy `/api` en `npm run dev` y `npm run preview`.               | `http://localhost:5080`  |
| `VITE_API_BASE_URL` | Opcional: URL absoluta de la API (salta el proxy; se incrusta al compilar). | mismo origen que la SPA  |

Copie `.env.example` a `.env` si necesita otro valor.

## Scripts

| Comando             | Acción                                                  |
| ------------------- | ------------------------------------------------------- |
| `npm install`       | Instala las dependencias.                               |
| `npm run dev`       | Servidor de desarrollo en `http://localhost:5173`.      |
| `npm run build`     | Verificación de tipos (`tsc`) y compilación a `dist/`.  |
| `npm run preview`   | Sirve la compilación de `dist/`.                        |
| `npm test`          | Pruebas de componentes y flujos (Vitest + RTL + MSW).   |
| `npm run lint`      | ESLint (typescript-eslint, reglas de hooks).            |
| `npm run typecheck` | Solo verificación de tipos.                             |
| `npm run format`    | Formatea el código con Prettier.                        |
| `npm run e2e`       | Pruebas e2e (Playwright) contra el stack de Docker.     |
| `npm run e2e:evidence` | Copia los vídeos e2e a `docs/evidencias/e2e`.        |

## Usuarios de demostración

Contraseña común: `Demo2026*` — `admin@solicitudes.local` (Administrador), `analista1@solicitudes.local`,
`analista2@solicitudes.local` (Analista), `solicitante1@solicitudes.local`, `solicitante2@solicitudes.local` (Solicitante).

## Estructura

```
src/
  assets/      Logo de la SB e ícono de inicio (recursos oficiales de la prueba)
  auth/        Proveedor de sesión, rutas protegidas y reglas de permisos por rol
  components/  Componentes reutilizables (tarjetas, insignias, tablas, formularios, diálogos, notificaciones)
  context/     Contextos de React (sesión, avisos y diálogos)
  hooks/       Hooks de datos y utilidades (catálogos, notificaciones, filtros en la URL, cambios sin guardar)
  layout/      Estructura visual (barra lateral, encabezado), rutas y menú
  pages/       Pantallas: inicio, solicitudes, entidades gubernamentales, administración, login
  schemas/     Esquemas zod que replican las reglas de validación del backend
  services/    Cliente HTTP (Bearer, ProblemDetails, 401) y servicios por recurso de la API
  styles/      Hoja de estilos global con tokens de diseño (variables CSS)
  types/       Tipos del contrato OpenAPI
  utils/       Etiquetas en español, traducción de códigos de error, formato y paginación
```

## Decisiones

- **Estado del servidor** con TanStack Query (caché, reintentos solo para errores 5xx/red, invalidación tras mutaciones).
- **Formularios** con react-hook-form + zod; los límites (`validationLimits.ts`) replican los del backend y los errores
  de validación que devuelve la API se muestran junto a cada campo.
- **Errores**: los `code` de ProblemDetails se traducen en `utils/errorMessages.ts`; si el código no es conocido se
  muestra el `detail` del servidor.
- **Sesión**: el token JWT se guarda en `sessionStorage`. Se avisa 5 minutos antes de que expire y, al vencer o ante
  un 401, se cierra la sesión y el login explica que expiró; al volver se recupera la ruta con sus filtros.
- **Listados**: filtros, orden y página viven en la URL (`useListSearchParameters`), así que se conservan al volver de
  un detalle o formulario y se pueden compartir.
- **Cambios sin guardar**: los diálogos y las páginas de alta piden confirmación antes de descartar lo escrito.
- **Concurrencia**: los cambios de una solicitud envían la `version` leída; si otro usuario guardó antes (409), se
  avisa y se recargan los datos sin perder lo que el usuario estaba escribiendo.
- **Errores de pantalla**: una página de error en español (`RouteErrorPage`) mantiene el menú cuando falla una vista.
- **Carga**: las pantallas, salvo login e inicio, se descargan bajo demanda (`React.lazy`).
- **Permisos**: la interfaz oculta las acciones que el rol no puede ejecutar; el backend sigue siendo la autoridad.
- **Diseño**: CSS propio con variables (azul `rgba(13, 48, 72, .9)`, gris `rgb(237, 240, 247)`, acento `#d9480f`),
  tipografía Saira empaquetada con `@fontsource` (sin CDN). La barra lateral se colapsa en pantallas angostas.

## Docker

```bash
docker build -t sb-solicitudes-web .
docker run -p 8080:8080 -e API_UPSTREAM=http://host.docker.internal:5080 sb-solicitudes-web
```

La imagen sirve la SPA con nginx (`nginx.conf.template`), con fallback a `index.html` para las rutas del cliente, y
reenvía `/api` a `API_UPSTREAM` (por defecto `http://api:8080`, el servicio de `docker-compose.yml`). La misma imagen
sirve para cualquier entorno: la dirección de la API se fija al arrancar el contenedor, no al compilar.
nginx agrega además cabeceras de seguridad (`security-headers.conf`): CSP que solo permite el propio origen,
`X-Frame-Options`, `X-Content-Type-Options`, `Referrer-Policy`, `Permissions-Policy` y `Cross-Origin-Opener-Policy`.
