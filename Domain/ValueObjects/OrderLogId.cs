namespace Domain.ValueObjects;

/// <summary>
/// Strongly typed identifier for OrderLog entity
/// </summary>
public record OrderLogId(Guid Value)
{
    public static OrderLogId New() => new(Guid.NewGuid());
    public static OrderLogId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
