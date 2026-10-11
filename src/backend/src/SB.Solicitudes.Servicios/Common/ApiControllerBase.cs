using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SB.Solicitudes.Dominio.Common;

namespace SB.Solicitudes.Servicios.Common;

[ApiController]
[Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public abstract class ApiControllerBase(ISender sender) : ControllerBase
{
    public const string ERROR_CODE_EXTENSION = "code";

    protected ISender Sender { get; } = sender;

    protected async Task<IActionResult> SendAsync<TResponse>(
        IRequest<Result<TResponse>> request,
        Func<TResponse, IActionResult> onSuccess,
        CancellationToken cancellationToken)
    {
        Result<TResponse> result = await Sender.Send(request, cancellationToken);
        return result.IsSuccess ? onSuccess(result.Value) : HandleFailure(result.Error);
    }

    protected Task<IActionResult> SendForNoContentAsync<TResponse>(IRequest<Result<TResponse>> request, CancellationToken cancellationToken)
        => SendAsync(request, _ => NoContent(), cancellationToken);

    protected IActionResult HandleFailure(Error error)
    {
        if (error is ValidationError validationError)
        {
            ValidationProblemDetails validationDetails = new(validationError.Errors.ToDictionary(pair => pair.Key, pair => pair.Value))
            {
                Title = validationError.Description,
                Status = StatusCodes.Status400BadRequest,
            };
            validationDetails.Extensions[ERROR_CODE_EXTENSION] = validationError.Code;
            return ValidationProblem(validationDetails);
        }

        ObjectResult problem = Problem(detail: error.Description, statusCode: StatusCodeFor(error.Type), title: TitleFor(error.Type));
        if (problem.Value is ProblemDetails problemDetails)
        {
            problemDetails.Extensions[ERROR_CODE_EXTENSION] = error.Code;
        }

        return problem;
    }

    private static int StatusCodeFor(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError,
    };

    private static string TitleFor(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => "Validation failed",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden => "Forbidden",
        ErrorType.NotFound => "Resource not found",
        ErrorType.Conflict => "Conflict",
        _ => "Unexpected error",
    };
}
