# Contexto de construcción: raíz del repositorio (necesita el .editorconfig con las reglas de nombres).
# Compilación con el SDK de .NET 8 y ejecución sobre la imagen mínima de ASP.NET Core.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY .editorconfig ./
COPY src/backend/global.json src/backend/nuget.config src/backend/Directory.Build.props src/backend/Directory.Packages.props src/backend/
COPY src/backend/src/SB.Solicitudes.Dominio/SB.Solicitudes.Dominio.csproj src/backend/src/SB.Solicitudes.Dominio/
COPY src/backend/src/SB.Solicitudes.Aplicacion/SB.Solicitudes.Aplicacion.csproj src/backend/src/SB.Solicitudes.Aplicacion/
COPY src/backend/src/SB.Solicitudes.Infraestructura/SB.Solicitudes.Infraestructura.csproj src/backend/src/SB.Solicitudes.Infraestructura/
COPY src/backend/src/SB.Solicitudes.Servicios/SB.Solicitudes.Servicios.csproj src/backend/src/SB.Solicitudes.Servicios/
RUN dotnet restore src/backend/src/SB.Solicitudes.Servicios/SB.Solicitudes.Servicios.csproj

COPY src/backend/src/ src/backend/src/
RUN dotnet publish src/backend/src/SB.Solicitudes.Servicios/SB.Solicitudes.Servicios.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
# Las carpetas que se montan como volumen deben existir con dueño app; si no, Docker las crea como root.
RUN mkdir -p /app/Data /app/logs /app/simulated-mail && chown -R $APP_UID:$APP_UID /app
# UID numérico (no el nombre) para que orquestadores con runAsNonRoot puedan verificarlo.
USER $APP_UID

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "SB.Solicitudes.Servicios.dll"]
