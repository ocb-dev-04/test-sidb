// Por defecto la API está en el mismo origen (/api lo reenvían nginx o el servidor de Vite).
export const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL || window.location.origin).replace(/\/+$/, '');
