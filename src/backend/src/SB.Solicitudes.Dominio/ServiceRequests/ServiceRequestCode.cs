using System.Globalization;

namespace SB.Solicitudes.Dominio.ServiceRequests;

public static class ServiceRequestCode
{
    public const string PREFIX = "SOL";
    public const int MAXIMUM_LENGTH = 20;
    private const string NUMBER_FORMAT = "D4";

    public static string Format(int year, int sequenceNumber)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sequenceNumber);

        string number = sequenceNumber.ToString(NUMBER_FORMAT, CultureInfo.InvariantCulture);
        return string.Create(CultureInfo.InvariantCulture, $"{PREFIX}-{year}-{number}");
    }
}
