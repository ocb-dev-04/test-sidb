using SB.Solicitudes.Dominio.Common;

namespace SB.Solicitudes.Dominio.Users;

public sealed class User : Entity<Guid>
{
    public const int FULL_NAME_MAXIMUM_LENGTH = 150;
    public const int EMAIL_MAXIMUM_LENGTH = 256;
    public const int PASSWORD_HASH_MAXIMUM_LENGTH = 256;

    private User()
    {
    }

    private User(Guid id)
        : base(id)
    {
    }

    public string FullName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    public bool CanManageRequests => Role is UserRole.Administrator or UserRole.Analyst;

    public static User Create(string fullName, string email, string passwordHash, UserRole role, DateTime utcNow)
    {
        return new User(Guid.NewGuid())
        {
            FullName = DomainGuard.RequiredText(fullName, FULL_NAME_MAXIMUM_LENGTH),
            Email = NormalizeEmail(DomainGuard.RequiredText(email, EMAIL_MAXIMUM_LENGTH)),
            PasswordHash = DomainGuard.RequiredText(passwordHash, PASSWORD_HASH_MAXIMUM_LENGTH),
            Role = role,
            IsActive = true,
            CreatedOnUtc = utcNow,
        };
    }

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public void Update(string fullName, UserRole role, bool isActive)
    {
        FullName = DomainGuard.RequiredText(fullName, FULL_NAME_MAXIMUM_LENGTH);
        Role = role;
        IsActive = isActive;
    }

    public void ChangePasswordHash(string passwordHash)
    {
        PasswordHash = DomainGuard.RequiredText(passwordHash, PASSWORD_HASH_MAXIMUM_LENGTH);
    }
}
