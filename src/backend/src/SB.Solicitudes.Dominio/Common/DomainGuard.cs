using System.Runtime.CompilerServices;

namespace SB.Solicitudes.Dominio.Common;

// Invariantes de texto de las entidades. FluentValidation filtra la entrada de los comandos con mensajes para el
// usuario; esto protege los demás caminos (semillas, importaciones, archivo de texto) y los errores de programación.
public static class DomainGuard
{
    public static string RequiredText(
        string value,
        int maximumLength,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return BoundedText(value, maximumLength, parameterName);
    }

    public static string BoundedText(
        string value,
        int maximumLength,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        string trimmed = value.Trim();
        ThrowIfTooLong(trimmed, maximumLength, parameterName);
        return trimmed;
    }

    public static string? OptionalText(
        string? value,
        int maximumLength,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        ThrowIfTooLong(trimmed, maximumLength, parameterName);
        return trimmed;
    }

    private static void ThrowIfTooLong(string value, int maximumLength, string? parameterName)
    {
        if (value.Length > maximumLength)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value.Length,
                $"The value exceeds the maximum length of {maximumLength} characters.");
        }
    }
}
