namespace Domain.ValueObjects;

/// <summary>
/// Value object representing a loyalty transaction identifier
/// </summary>
public record LoyaltyTransactionId(Guid Value)
{
    public static LoyaltyTransactionId New() => new(Guid.NewGuid());
    public static LoyaltyTransactionId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
