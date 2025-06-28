namespace Domain.ValueObjects;

/// <summary>
/// Strongly typed identifier for Order entity
/// </summary>
public record OrderId(Guid Value)
{
    public static OrderId New() => new(Guid.NewGuid());
    public static OrderId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
