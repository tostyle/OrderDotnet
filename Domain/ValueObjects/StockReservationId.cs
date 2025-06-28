namespace Domain.ValueObjects;

/// <summary>
/// Value object representing a stock reservation identifier
/// </summary>
public record StockReservationId(Guid Value)
{
    public static StockReservationId New() => new(Guid.NewGuid());
    public static StockReservationId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
