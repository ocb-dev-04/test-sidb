using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SB.Solicitudes.Aplicacion;
using SB.Solicitudes.Aplicacion.Abstractions.Authentication;
using SB.Solicitudes.Dominio.Users;
using SB.Solicitudes.Infraestructura.Authentication;
using SB.Solicitudes.Servicios.Common;

namespace SB.Solicitudes.Servicios;

internal static class PresentationServiceCollectionExtensions
{
    private const string CORS_POLICY = "Frontend";
    private const string CORS_SECTION = "Cors:AllowedOrigins";
    private const string TRACE_ID_EXTENSION = "traceId";
    private const string API_TITLE = "SB.Solicitudes API";
    private const string API_VERSION = "v1";
    private const string BEARER_SCHEME = "Bearer";
    private const int CLOCK_SKEW_SECONDS = 30;
    private const string HEALTH_ROUTE = "/health";

    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
            context.ProblemDetails.Extensions[TRACE_ID_EXTENSION] = Activity.Current?.TraceId.ToString() ?? context.HttpContext.TraceIdentifier);
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();

        services.AddJwtAuthentication();
        services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicies.ADMINISTRATOR, policy => policy.RequireRole(RoleNames.ADMINISTRATOR))
            .AddPolicy(AuthorizationPolicies.MANAGEMENT, policy => policy.RequireRole(RoleNames.ADMINISTRATOR, RoleNames.ANALYST))
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        services.AddOptions<RateLimitingOptions>().Bind(configuration.GetSection(RateLimitingOptions.SECTION_NAME));
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(RateLimitPolicies.LOGIN, httpContext =>
            {
                RateLimitingOptions rateLimiting = httpContext.RequestServices.GetRequiredService<IOptions<RateLimitingOptions>>().Value;
                return RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimiting.LoginPermitsPerWindow,
                        Window = TimeSpan.FromSeconds(rateLimiting.LoginWindowSeconds),
                    });
            });
        });

        services.AddCors();
        services.AddOptions<CorsOptions>().Configure<IConfiguration>((cors, appConfiguration) =>
        {
            string[] allowedOrigins = appConfiguration.GetSection(CORS_SECTION).Get<string[]>() ?? [];
            cors.AddPolicy(CORS_POLICY, policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod());
        });

        services.AddSwagger();
        return services;
    }

    public static WebApplication UsePresentation(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();

        if (!app.Environment.IsProduction())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options => options.DocumentTitle = API_TITLE);
        }

        app.UseCors(CORS_POLICY);
        app.UseAuthentication();
        app.UseMiddleware<LogContextEnrichmentMiddleware>();
        app.UseRateLimiter();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks(HEALTH_ROUTE).AllowAnonymous();
        return app;
    }

    private static void AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
            {
                bearer.MapInboundClaims = false;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtOptions.Value.Issuer,
                    ValidAudience = jwtOptions.Value.Audience,
                    IssuerSigningKey = JwtSigningKey.Create(jwtOptions.Value.SigningKey),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromSeconds(CLOCK_SKEW_SECONDS),
                    NameClaimType = ApplicationClaimTypes.NAME,
                    RoleClaimType = ApplicationClaimTypes.ROLE,
                };
            });
    }

    private static void AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(API_VERSION, new OpenApiInfo
            {
                Title = API_TITLE,
                Version = API_VERSION,
                Description = "Gestión de solicitudes internas y mantenimiento de entidades gubernamentales.",
            });

            OpenApiSecurityScheme bearerScheme = new()
            {
                Name = "Authorization",
                Description = "Token JWT obtenido en POST /api/auth/login.",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = BEARER_SCHEME },
            };
            options.AddSecurityDefinition(BEARER_SCHEME, bearerScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement { [bearerScheme] = [] });

            Assembly[] documentedAssemblies = [typeof(Program).Assembly, typeof(DependencyInjection).Assembly];
            foreach (Assembly assembly in documentedAssemblies)
            {
                string xmlPath = Path.Combine(AppContext.BaseDirectory, $"{assembly.GetName().Name}.xml");
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            }

            options.SupportNonNullableReferenceTypes();
            options.CustomSchemaIds(type => type.FullName?.Replace("+", ".", StringComparison.Ordinal));
        });
    }
}
