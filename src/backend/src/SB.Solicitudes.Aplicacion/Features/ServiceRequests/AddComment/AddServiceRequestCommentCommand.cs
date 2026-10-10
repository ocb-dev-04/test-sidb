using FluentValidation;
using SB.Solicitudes.Aplicacion.Abstractions.Authentication;
using SB.Solicitudes.Aplicacion.Abstractions.Messaging;
using SB.Solicitudes.Aplicacion.Abstractions.Persistence;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.ServiceRequests;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Aplicacion.Features.ServiceRequests.AddComment;

public sealed record AddServiceRequestCommentCommand(
    Guid ServiceRequestId,
    string Text,
    CommentVisibility Visibility) : ICommand<CommentResponse>;

internal sealed class AddServiceRequestCommentCommandValidator : AbstractValidator<AddServiceRequestCommentCommand>
{
    public AddServiceRequestCommentCommandValidator()
    {
        RuleFor(command => command.Text).NotEmpty().MaximumLength(Comment.TEXT_MAXIMUM_LENGTH);
        RuleFor(command => command.Visibility).IsInEnum();
    }
}

internal sealed class AddServiceRequestCommentCommandHandler(
    IServiceRequestRepository serviceRequestRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
    : ICommandHandler<AddServiceRequestCommentCommand, CommentResponse>
{
    public async Task<Result<CommentResponse>> HandleAsync(AddServiceRequestCommentCommand request, CancellationToken cancellationToken)
    {
        ServiceRequest? serviceRequest = await serviceRequestRepository.GetByIdAsync(request.ServiceRequestId, cancellationToken);
        ServiceRequestVisibility visibility = ServiceRequestVisibility.For(currentUser);
        if (serviceRequest is null || !visibility.CanView(serviceRequest))
        {
            return ServiceRequestErrors.NOT_FOUND;
        }

        if (request.Visibility == CommentVisibility.Internal && !visibility.CanSeeInternalComments)
        {
            return ServiceRequestErrors.INTERNAL_COMMENT_NOT_ALLOWED;
        }

        User? author = await userRepository.GetByIdAsync(currentUser.UserId, cancellationToken);
        if (author is null)
        {
            return UserErrors.NOT_FOUND;
        }

        Comment comment = serviceRequest.AddComment(author.Id, request.Text, request.Visibility, timeProvider.GetUtcNow().UtcDateTime);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CommentResponse(comment.Id, comment.Text, comment.Visibility, author.Id, author.FullName, comment.CreatedOnUtc);
    }
}
