using System.Globalization;
using SB.Solicitudes.Dominio.Common;

namespace SB.Solicitudes.Dominio.Tests.Common;

public sealed class ResultTests
{
    private const int VALUE = 7;
    private static readonly Error SAMPLE_ERROR = Error.Validation("sample.code", "Sample description.");

    [Fact]
    public void SuccessShouldExposeValue()
    {
        Result<int> result = VALUE;

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(VALUE);
        result.Error.ShouldBe(Error.NONE);
    }

    [Fact]
    public void FailureShouldThrowWhenValueIsAccessed()
    {
        Result<int> result = SAMPLE_ERROR;

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SAMPLE_ERROR);
        Should.Throw<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void FailureShouldRequireAnError()
        => Should.Throw<ArgumentException>(() => Result.Failure(Error.NONE));

    [Fact]
    public void MapShouldPropagateFailure()
    {
        Result<int> result = SAMPLE_ERROR;

        Result<string> mapped = result.Map(value => value.ToString(CultureInfo.InvariantCulture));

        mapped.Error.ShouldBe(SAMPLE_ERROR);
    }
}
