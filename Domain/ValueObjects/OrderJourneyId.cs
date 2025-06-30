namespace Domain.ValueObjects;

/// <summary>
/// Strongly typed identifier for OrderJourney entity
/// </summary>
public record OrderJourneyId(Guid Value)
{
    public static OrderJourneyId New() => new(Guid.NewGuid());
    public static OrderJourneyId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
