using ChMS.Domain.Finance;

namespace ChMS.Domain.Tests;

public sealed class MoneyTests
{
    [Fact]
    public void Sums_exactly_without_floating_point_drift()
    {
        var tenCents = new Money(0.10m);

        var total = Money.Sum(Enumerable.Repeat(tenCents, 10));

        Assert.Equal(new Money(1.00m), total);
    }

    [Theory]
    [InlineData(0.001)]
    [InlineData(12.345)]
    public void Rejects_fractions_of_a_cent(double amount)
    {
        Assert.Throws<ArgumentException>(() => new Money((decimal)amount));
    }

    [Fact]
    public void Converts_to_and_from_cents()
    {
        var money = Money.FromCents(123_456);

        Assert.Equal(1234.56m, money.Amount);
        Assert.Equal(123_456, money.ToCents());
    }

    [Fact]
    public void Formats_as_us_dollars()
    {
        Assert.Equal("$1,234.50", new Money(1234.50m).ToString());
        Assert.True(new Money(-5m).IsNegative);
    }
}
