using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SwimmingCoach.Api.Data;

namespace SwimmingCoach.Api.Profiles;

public static class ProfileEndpoints
{
    public static IEndpointRouteBuilder MapProfileEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var profiles = endpoints.MapGroup("/api/profile")
            .RequireAuthorization();

        profiles.MapGet("/", GetProfileAsync);
        profiles.MapPut("/", UpdateProfileAsync);

        return endpoints;
    }

    private static async Task<IResult> GetProfileAsync(
        ClaimsPrincipal principal,
        ApplicationDbContext dbContext)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var profile = await dbContext.AthleteProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(value => value.UserId == userId);

        return profile is null
            ? Results.NotFound()
            : Results.Ok(ToResponse(profile));
    }

    private static async Task<IResult> UpdateProfileAsync(
        UpdateAthleteProfileRequest request,
        HttpRequest httpRequest,
        ClaimsPrincipal principal,
        ApplicationDbContext dbContext)
    {
        if (httpRequest.Headers["X-Requested-With"] != "SwimmingCoach.Web")
        {
            return Results.BadRequest();
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
        {
            return Results.ValidationProblem(validationErrors);
        }

        var profile = await dbContext.AthleteProfiles
            .SingleOrDefaultAsync(value => value.UserId == userId);

        if (profile is null)
        {
            profile = new AthleteProfile
            {
                UserId = userId,
                TimeZoneId = request.TimeZoneId.Trim(),
            };
            dbContext.AthleteProfiles.Add(profile);
        }

        profile.TimeZoneId = request.TimeZoneId.Trim();
        profile.PreferredUnits = request.PreferredUnits;
        profile.PoolLength = request.PoolLength;
        profile.PoolLengthUnit = request.PoolLengthUnit;
        profile.SwimmingExperience = request.SwimmingExperience;
        profile.RunningExperience = request.RunningExperience;
        profile.TrainingDaysPerWeek = request.TrainingDaysPerWeek;
        profile.TypicalSessionMinutes = request.TypicalSessionMinutes;
        profile.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return Results.Ok(ToResponse(profile));
    }

    private static Dictionary<string, string[]> Validate(
        UpdateAthleteProfileRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (!Enum.IsDefined(request.PreferredUnits))
        {
            errors[nameof(request.PreferredUnits)] =
                ["Preferred units are not recognized."];
        }

        if (!Enum.IsDefined(request.PoolLengthUnit))
        {
            errors[nameof(request.PoolLengthUnit)] =
                ["Pool length unit is not recognized."];
        }

        if (!Enum.IsDefined(request.SwimmingExperience)
            || request.SwimmingExperience == ExperienceLevel.None)
        {
            errors[nameof(request.SwimmingExperience)] =
                ["Swimming experience is required."];
        }

        if (!Enum.IsDefined(request.RunningExperience))
        {
            errors[nameof(request.RunningExperience)] =
                ["Running experience is not recognized."];
        }

        if (string.IsNullOrWhiteSpace(request.TimeZoneId)
            || request.TimeZoneId.Length > 100)
        {
            errors[nameof(request.TimeZoneId)] =
                ["A time zone of at most 100 characters is required."];
        }
        else
        {
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(request.TimeZoneId.Trim());
            }
            catch (TimeZoneNotFoundException)
            {
                errors[nameof(request.TimeZoneId)] =
                    ["The time zone is not recognized by the server."];
            }
            catch (InvalidTimeZoneException)
            {
                errors[nameof(request.TimeZoneId)] =
                    ["The time zone is invalid."];
            }
        }

        if (request.PoolLength is < 10 or > 100)
        {
            errors[nameof(request.PoolLength)] =
                ["Pool length must be between 10 and 100."];
        }

        if (request.TrainingDaysPerWeek is < 1 or > 7)
        {
            errors[nameof(request.TrainingDaysPerWeek)] =
                ["Training days per week must be between 1 and 7."];
        }

        if (request.TypicalSessionMinutes is < 15 or > 300)
        {
            errors[nameof(request.TypicalSessionMinutes)] =
                ["Session duration must be between 15 and 300 minutes."];
        }

        return errors;
    }

    private static AthleteProfileResponse ToResponse(AthleteProfile profile) =>
        new(
            profile.TimeZoneId,
            profile.PreferredUnits,
            profile.PoolLength,
            profile.PoolLengthUnit,
            profile.SwimmingExperience,
            profile.RunningExperience,
            profile.TrainingDaysPerWeek,
            profile.TypicalSessionMinutes,
            OnboardingComplete: true,
            profile.UpdatedAtUtc);
}
