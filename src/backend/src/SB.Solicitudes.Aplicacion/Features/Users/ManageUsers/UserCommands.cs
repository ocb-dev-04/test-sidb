using FluentValidation;
using SB.Solicitudes.Aplicacion.Abstractions.Authentication;
using SB.Solicitudes.Aplicacion.Abstractions.Messaging;
using SB.Solicitudes.Aplicacion.Abstractions.Persistence;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Aplicacion.Features.Users.ManageUsers;

public sealed record CreateUserCommand(string FullName, string Email, string Password, UserRole Role) : ICommand<UserResponse>;

public sealed record UpdateUserCommand(Guid UserId, string FullName, UserRole Role, bool IsActive) : ICommand<Unit>;

internal sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.FullName).NotEmpty().MaximumLength(User.FULL_NAME_MAXIMUM_LENGTH);
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(User.EMAIL_MAXIMUM_LENGTH);
        RuleFor(command => command.Role).IsInEnum();
        RuleFor(command => command.Password)
            .NotEmpty()
            .MinimumLength(PasswordPolicy.MINIMUM_LENGTH)
            .MaximumLength(PasswordPolicy.MAXIMUM_LENGTH)
            .Must(password => password.Any(char.IsUpper)).WithMessage("Password must contain an uppercase letter.")
            .Must(password => password.Any(char.IsLower)).WithMessage("Password must contain a lowercase letter.")
            .Must(password => password.Any(char.IsDigit)).WithMessage("Password must contain a digit.");
    }
}

internal sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(command => command.FullName).NotEmpty().MaximumLength(User.FULL_NAME_MAXIMUM_LENGTH);
        RuleFor(command => command.Role).IsInEnum();
    }
}

internal sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<CreateUserCommand, UserResponse>
{
    public async Task<Result<UserResponse>> HandleAsync(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (await userRepository.ExistsByEmailAsync(User.NormalizeEmail(request.Email), cancellationToken))
        {
            return UserErrors.EMAIL_ALREADY_IN_USE;
        }

        User user = User.Create(
            request.FullName,
            request.Email,
            passwordHasher.Hash(request.Password),
            request.Role,
            timeProvider.GetUtcNow().UtcDateTime);

        userRepository.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UserResponse.From(user);
    }
}

internal sealed class UpdateUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : ICommandHandler<UpdateUserCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return UserErrors.NOT_FOUND;
        }

        // Evita que el sistema quede sin administrador por una auto-degradación accidental.
        bool isSelfLockout = user.Id == currentUser.UserId
            && (!request.IsActive || request.Role != UserRole.Administrator);
        if (isSelfLockout)
        {
            return UserErrors.CANNOT_DEACTIVATE_SELF;
        }

        user.Update(request.FullName, request.Role, request.IsActive);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.VALUE;
    }
}
