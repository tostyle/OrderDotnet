namespace Domain.ValueObjects;

/// <summary>
/// Value object representing a product identifier
/// </summary>
public record ProductId(Guid Value)
{
    public static ProductId New() => new(Guid.NewGuid());
    public static ProductId From(Guid value) => new(value);
    public static ProductId From(string value) => new(Guid.Parse(value));

    public override string ToString() => Value.ToString();
}
