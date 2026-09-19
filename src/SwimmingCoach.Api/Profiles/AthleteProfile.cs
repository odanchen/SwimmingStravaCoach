namespace SwimmingCoach.Api.Profiles;

public sealed class AthleteProfile
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required string TimeZoneId { get; set; }
    public MeasurementSystem PreferredUnits { get; set; }
    public decimal PoolLength { get; set; }
    public PoolLengthUnit PoolLengthUnit { get; set; }
    public ExperienceLevel SwimmingExperience { get; set; }
    public ExperienceLevel RunningExperience { get; set; }
    public int TrainingDaysPerWeek { get; set; }
    public int TypicalSessionMinutes { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public enum MeasurementSystem
{
    Metric,
    Imperial,
}

public enum PoolLengthUnit
{
    Meters,
    Yards,
}

public enum ExperienceLevel
{
    None,
    Beginner,
    Intermediate,
    Advanced,
    Competitive,
}
