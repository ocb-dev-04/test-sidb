using SB.Solicitudes.Dominio.Catalogs;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.GovernmentEntities;

namespace SB.Solicitudes.Dominio.Tests.Common;

public sealed class DomainGuardTests
{
    private const int MAXIMUM_LENGTH = 5;
    private const string PADDED_VALUE = "  abc  ";
    private const string TRIMMED_VALUE = "abc";
    private const string EXACT_LENGTH_VALUE = "abcde";
    private const string TOO_LONG_VALUE = "abcdef";

    [Fact]
    public void RequiredTextShouldTrimValidValues()
    {
        string result = DomainGuard.RequiredText(PADDED_VALUE, MAXIMUM_LENGTH);

        result.ShouldBe(TRIMMED_VALUE);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RequiredTextShouldRejectBlankValues(string value)
        => Should.Throw<ArgumentException>(() => DomainGuard.RequiredText(value, MAXIMUM_LENGTH));

    [Fact]
    public void RequiredTextShouldAcceptExactMaximumLength()
        => DomainGuard.RequiredText(EXACT_LENGTH_VALUE, MAXIMUM_LENGTH).ShouldBe(EXACT_LENGTH_VALUE);

    [Fact]
    public void RequiredTextShouldRejectValuesLongerThanMaximum()
    {
        ArgumentOutOfRangeException exception
            = Should.Throw<ArgumentOutOfRangeException>(() => DomainGuard.RequiredText(TOO_LONG_VALUE, MAXIMUM_LENGTH));

        exception.ParamName.ShouldBe("TOO_LONG_VALUE");
    }

    [Fact]
    public void BoundedTextShouldAllowEmptyButNotTooLong()
    {
        DomainGuard.BoundedText(string.Empty, MAXIMUM_LENGTH).ShouldBe(string.Empty);
        Should.Throw<ArgumentOutOfRangeException>(() => DomainGuard.BoundedText(TOO_LONG_VALUE, MAXIMUM_LENGTH));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void OptionalTextShouldNormalizeBlankToNull(string? value)
        => DomainGuard.OptionalText(value, MAXIMUM_LENGTH).ShouldBeNull();

    [Fact]
    public void OptionalTextShouldRejectValuesLongerThanMaximum()
        => Should.Throw<ArgumentOutOfRangeException>(() => DomainGuard.OptionalText(TOO_LONG_VALUE, MAXIMUM_LENGTH));

    [Fact]
    public void EntitiesShouldEnforceColumnLengthsOutsideValidators()
    {
        string tooLongAreaName = new('a', Area.NAME_MAXIMUM_LENGTH + 1);
        string tooLongCategory = new('c', GovernmentEntity.CATEGORY_MAXIMUM_LENGTH + 1);

        Should.Throw<ArgumentOutOfRangeException>(() => Area.Create(tooLongAreaName));
        Should.Throw<ArgumentOutOfRangeException>(
            () => GovernmentEntity.Create(TRIMMED_VALUE, tooLongCategory, TRIMMED_VALUE, TRIMMED_VALUE));
    }
}
