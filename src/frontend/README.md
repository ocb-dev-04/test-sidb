# Frontend — Gestión de Solicitudes (Superintendencia de Bancos)

SPA en **React 18 + TypeScript (strict) + Vite** que consume la API REST `SB.Solicitudes.Servicios`.

## Requisitos

- Node.js 20 o superior (probado con Node 22) y npm.
- La API en ejecución (por defecto en `http://localhost:5080`, con CORS habilitado para `http://localhost:5173`).

## Configuración

| Variable            | Descripción                         | Valor por defecto       |
| ------------------- | ----------------------------------- | ----------------------- |
| `VITE_API_BASE_URL` | URL base de la API (sin `/` final). | `http://localhost:5080` |

Copie `.env.example` a `.env` si necesita otro valor. Vite la incrusta al compilar.

## Scripts

| Comando             | Acción                                                  |
| ------------------- | ------------------------------------------------------- |
| `npm install`       | Instala las dependencias.                               |
| `npm run dev`       | Servidor de desarrollo en `http://localhost:5173`.      |
| `npm run build`     | Verificación de tipos (`tsc`) y compilación a `dist/`.  |
| `npm run preview`   | Sirve la compilación de `dist/`.                        |
| `npm test`          | Pruebas unitarias y de componentes (Vitest + RTL).      |
| `npm run lint`      | ESLint (typescript-eslint, reglas de hooks).            |
| `npm run typecheck` | Solo verificación de tipos.                             |
| `npm run format`    | Formatea el código con Prettier.                        |

## Usuarios de demostración

Contraseña común: `Demo2026*` — `admin@solicitudes.local` (Administrador), `analista1@solicitudes.local`,
`analista2@solicitudes.local` (Analista), `solicitante1@solicitudes.local`, `solicitante2@solicitudes.local` (Solicitante).

## Estructura

```
src/
  assets/      Logo de la SB e ícono de inicio (recursos oficiales de la prueba)
  auth/        Proveedor de sesión, rutas protegidas y reglas de permisos por rol
  components/  Componentes reutilizables (tarjetas, insignias, tablas, formularios, diálogos, notificaciones)
  context/     Contextos de React (sesión y avisos)
  hooks/       Hooks de datos y utilidades (catálogos, notificaciones, fuente de entidades, debounce)
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
- **Sesión**: el token JWT se guarda en `sessionStorage` y se descarta ante cualquier respuesta 401.
- **Permisos**: la interfaz oculta las acciones que el rol no puede ejecutar; el backend sigue siendo la autoridad.
- **Diseño**: CSS propio con variables (azul `rgba(13, 48, 72, .9)`, gris `rgb(237, 240, 247)`, acento `#d9480f`),
  tipografía Saira empaquetada con `@fontsource` (sin CDN). La barra lateral se colapsa en pantallas angostas.

## Docker

```bash
docker build --build-arg VITE_API_BASE_URL=http://localhost:5080 -t sb-solicitudes-web .
docker run -p 8080:80 sb-solicitudes-web
```

La imagen sirve la SPA con nginx (`nginx.conf`), con fallback a `index.html` para las rutas del cliente.
