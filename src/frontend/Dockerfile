# Etapa 1: compilación de la SPA con Vite
FROM node:22-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
# Vite incrusta la URL de la API en tiempo de compilación.
ARG VITE_API_BASE_URL=http://localhost:5080
ENV VITE_API_BASE_URL=${VITE_API_BASE_URL}
RUN npm run build

# Etapa 2: servidor estático
FROM nginx:1.27-alpine AS runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
