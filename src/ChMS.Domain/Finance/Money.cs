using System.Globalization;

namespace ChMS.Domain.Finance;

/// <summary>
/// An exact US dollar amount. Always backed by <see cref="decimal"/>; never use float or double for money.
/// Amounts are limited to whole cents so that totals reconcile exactly with bank statements.
/// </summary>
public readonly record struct Money : IComparable<Money>
{
    public static readonly Money Zero = new(0m);

    public decimal Amount { get; }

    public Money(decimal amount)
    {
        if (decimal.Round(amount, 2) != amount)
        {
            throw new ArgumentException($"Money must be whole cents; got {amount}.", nameof(amount));
        }

        Amount = amount;
    }

    public static Money FromCents(long cents) => new(cents / 100m);

    public long ToCents() => decimal.ToInt64(Amount * 100m);

    public bool IsNegative => Amount < 0m;

    public static Money operator +(Money left, Money right) => new(left.Amount + right.Amount);

    public static Money operator -(Money left, Money right) => new(left.Amount - right.Amount);

    public static Money operator -(Money value) => new(-value.Amount);

    public static bool operator <(Money left, Money right) => left.Amount < right.Amount;

    public static bool operator >(Money left, Money right) => left.Amount > right.Amount;

    public static bool operator <=(Money left, Money right) => left.Amount <= right.Amount;

    public static bool operator >=(Money left, Money right) => left.Amount >= right.Amount;

    public static Money Sum(IEnumerable<Money> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        return values.Aggregate(Zero, (total, next) => total + next);
    }

    public int CompareTo(Money other) => Amount.CompareTo(other.Amount);

    public override string ToString() => Amount.ToString("C", CultureInfo.GetCultureInfo("en-US"));
}
