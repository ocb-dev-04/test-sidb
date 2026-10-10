using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Dominio.ServiceRequests;

public sealed class Comment : Entity<Guid>
{
    public const int TEXT_MAXIMUM_LENGTH = 2000;

    private Comment()
    {
    }

    private Comment(Guid id)
        : base(id)
    {
    }

    public Guid ServiceRequestId { get; private set; }

    public Guid AuthorId { get; private set; }

    public User Author { get; private set; } = null!;

    public string Text { get; private set; } = string.Empty;

    public CommentVisibility Visibility { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    internal static Comment Create(Guid serviceRequestId, Guid authorId, string text, CommentVisibility visibility, DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        return new Comment(Guid.NewGuid())
        {
            ServiceRequestId = serviceRequestId,
            AuthorId = authorId,
            Text = text.Trim(),
            Visibility = visibility,
            CreatedOnUtc = utcNow,
        };
    }
}
