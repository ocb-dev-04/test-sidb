using FluentValidation;
using Microsoft.Extensions.Logging;
using SB.Solicitudes.Aplicacion.Abstractions;
using SB.Solicitudes.Aplicacion.Abstractions.Authentication;
using SB.Solicitudes.Aplicacion.Abstractions.Messaging;
using SB.Solicitudes.Aplicacion.Abstractions.Persistence;
using SB.Solicitudes.Aplicacion.Features.Users;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Aplicacion.Features.Authentication;

/// <summary>Credenciales para obtener un token JWT.</summary>
public sealed record LoginCommand(string Email, string Password) : ICommand<LoginResponse>;

public sealed record LoginResponse(string AccessToken, DateTime ExpiresOnUtc, UserResponse User);

internal sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(User.EMAIL_MAXIMUM_LENGTH);
        RuleFor(command => command.Password).NotEmpty().MaximumLength(PasswordPolicy.MAXIMUM_LENGTH);
    }
}

internal sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenProvider jwtTokenProvider,
    ILogger<LoginCommandHandler> logger)
    : ICommandHandler<LoginCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> HandleAsync(LoginCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByEmailAsync(User.NormalizeEmail(request.Email), cancellationToken);

        // Mismo error para usuario inexistente o clave incorrecta: evita enumerar correos registrados.
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            ApplicationLogMessages.LoginFailed(logger);
            return UserErrors.INVALID_CREDENTIALS;
        }

        if (!user.IsActive)
        {
            return UserErrors.INACTIVE;
        }

        AccessToken accessToken = jwtTokenProvider.Create(user);
        ApplicationLogMessages.LoginSucceeded(logger, user.Id);

        return new LoginResponse(accessToken.Value, accessToken.ExpiresOnUtc, UserResponse.From(user));
    }
}
