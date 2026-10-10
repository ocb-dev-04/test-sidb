using SB.Solicitudes.Dominio.ServiceRequests;

namespace SB.Solicitudes.Dominio.Tests.ServiceRequests;

public sealed class ServiceRequestCodeTests
{
    private const int YEAR = 2026;

    [Theory]
    [InlineData(1, "SOL-2026-0001")]
    [InlineData(42, "SOL-2026-0042")]
    [InlineData(12345, "SOL-2026-12345")]
    public void FormatShouldPadSequenceNumber(int sequenceNumber, string expectedCode)
        => ServiceRequestCode.Format(YEAR, sequenceNumber).ShouldBe(expectedCode);

    [Fact]
    public void FormatShouldRejectNonPositiveSequenceNumber()
        => Should.Throw<ArgumentOutOfRangeException>(() => ServiceRequestCode.Format(YEAR, 0));
}
