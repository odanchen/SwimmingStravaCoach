namespace SwimmingCoach.Api.Profiles;

public sealed record UpdateAthleteProfileRequest(
    string TimeZoneId,
    MeasurementSystem PreferredUnits,
    decimal PoolLength,
    PoolLengthUnit PoolLengthUnit,
    ExperienceLevel SwimmingExperience,
    ExperienceLevel RunningExperience,
    int TrainingDaysPerWeek,
    int TypicalSessionMinutes);

public sealed record AthleteProfileResponse(
    string TimeZoneId,
    MeasurementSystem PreferredUnits,
    decimal PoolLength,
    PoolLengthUnit PoolLengthUnit,
    ExperienceLevel SwimmingExperience,
    ExperienceLevel RunningExperience,
    int TrainingDaysPerWeek,
    int TypicalSessionMinutes,
    bool OnboardingComplete,
    DateTime UpdatedAtUtc);
