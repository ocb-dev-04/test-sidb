namespace SB.Solicitudes.Dominio.Common;

public enum ErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Forbidden = 4,
    Unauthorized = 5,
    Failure = 6,
}
