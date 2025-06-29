namespace Domain.ValueObjects;

/// <summary>
/// Value object representing an OrderItem identifier
/// </summary>
public record OrderItemId
{
    public Guid Value { get; }

    private OrderItemId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("OrderItemId cannot be empty", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Creates a new OrderItemId with a new GUID
    /// </summary>
    public static OrderItemId New() => new(Guid.NewGuid());

    /// <summary>
    /// Creates an OrderItemId from an existing GUID
    /// </summary>
    public static OrderItemId From(Guid value) => new(value);

    /// <summary>
    /// Creates an OrderItemId from a string representation
    /// </summary>
    public static OrderItemId From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("OrderItemId string cannot be null or empty", nameof(value));

        if (!Guid.TryParse(value, out var guid))
            throw new ArgumentException("Invalid OrderItemId format", nameof(value));

        return new OrderItemId(guid);
    }

    /// <summary>
    /// Implicit conversion from OrderItemId to Guid
    /// </summary>
    public static implicit operator Guid(OrderItemId id) => id.Value;

    /// <summary>
    /// Implicit conversion from Guid to OrderItemId
    /// </summary>
    public static implicit operator OrderItemId(Guid value) => From(value);

    public override string ToString() => Value.ToString();
}
