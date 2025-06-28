namespace Domain.ValueObjects;

/// <summary>
/// Value object representing a payment identifier
/// </summary>
public record PaymentId(Guid Value)
{
    public static PaymentId New() => new(Guid.NewGuid());
    public static PaymentId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
