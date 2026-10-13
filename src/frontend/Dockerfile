# Etapa 1: compilación de la SPA con Vite
FROM node:22-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

# Etapa 2: servidor estático. La variante unprivileged corre como el usuario nginx (no root) en el puerto 8080.
FROM nginxinc/nginx-unprivileged:1.27-alpine AS runtime
# nginx reenvía /api a esta dirección; se puede cambiar al arrancar el contenedor sin recompilar.
ENV API_UPSTREAM=http://api:8080
COPY nginx.conf.template /etc/nginx/templates/default.conf.template
COPY security-headers.conf /etc/nginx/snippets/security-headers.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 8080
