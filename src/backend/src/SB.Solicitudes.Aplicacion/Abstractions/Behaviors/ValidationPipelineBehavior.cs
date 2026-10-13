using FluentValidation;
using FluentValidation.Results;
using MediatR;
using SB.Solicitudes.Dominio.Common;

namespace SB.Solicitudes.Aplicacion.Abstractions.Behaviors;

internal sealed class ValidationPipelineBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result, IFailureFactory<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        IValidator<TRequest>[] requestValidators = validators.ToArray();
        if (!requestValidators.Any())
        {
            return await next(cancellationToken);
        }

        ValidationContext<TRequest> context = new(request);
        List<ValidationFailure> failures = [];
        foreach (IValidator<TRequest> validator in requestValidators)
        {
            ValidationResult validationResult = await validator.ValidateAsync(context, cancellationToken);
            failures.AddRange(validationResult.Errors);
        }

        if (!failures.Any())
        {
            return await next(cancellationToken);
        }

        Dictionary<string, string[]> errors = failures
            .GroupBy(failure => ToCamelCase(failure.PropertyName), StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).Distinct(StringComparer.Ordinal).ToArray(),
                StringComparer.Ordinal);

        return TResponse.CreateFailure(new ValidationError(errors));
    }

    private static string ToCamelCase(string propertyName)
        => string.IsNullOrEmpty(propertyName) || char.IsLower(propertyName[0])
            ? propertyName
            : string.Concat(char.ToLowerInvariant(propertyName[0]).ToString(), propertyName[1..]);
}
