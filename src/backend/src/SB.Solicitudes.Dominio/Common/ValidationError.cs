namespace SB.Solicitudes.Dominio.Common;

public sealed record ValidationError(IReadOnlyDictionary<string, string[]> Errors)
    : Error(VALIDATION_CODE, VALIDATION_DESCRIPTION, ErrorType.Validation)
{
    public const string VALIDATION_CODE = "validation.failed";
    public const string VALIDATION_DESCRIPTION = "One or more validation errors occurred.";
}
