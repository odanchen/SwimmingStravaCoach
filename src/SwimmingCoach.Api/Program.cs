using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SwimmingCoach.Api.Data;
using SwimmingCoach.Api.Identity;
using SwimmingCoach.Api.Profiles;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "The DefaultConnection connection string is missing.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySQL(connectionString);
});

builder.Services.AddAuthorization();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter(allowIntegerValues: false));
});

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // External provider IDs, rather than email addresses, identify accounts.
        options.User.RequireUniqueEmail = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
var googleIsConfigured =
    !string.IsNullOrWhiteSpace(googleClientId)
    && !string.IsNullOrWhiteSpace(googleClientSecret);

if (googleIsConfigured)
{
    builder.Services
        .AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = googleClientId!;
            options.ClientSecret = googleClientSecret!;

            // Google proves identity; the app doesn't need Google API tokens.
            options.SaveTokens = false;
        });
}

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "SwimmingCoach.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;

    // API clients need status codes, not redirects to an HTML login page.
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok(new
{
    Status = "ok"
}));

var authentication = app.MapGroup("/api/auth");

authentication.MapGet(
    "/google",
    (SignInManager<ApplicationUser> signInManager) =>
    {
        if (!googleIsConfigured)
        {
            return Results.Problem(
                "Google authentication has not been configured.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        var properties = signInManager.ConfigureExternalAuthenticationProperties(
            GoogleDefaults.AuthenticationScheme,
            "/api/auth/google/complete");

        return Results.Challenge(
            properties,
            [GoogleDefaults.AuthenticationScheme]);
    });

authentication.MapGet(
    "/google/complete",
    async (SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager) =>
    {
        var frontendUrl =
            builder.Configuration["FrontendUrl"] ?? "http://localhost:5173";
        var failureUrl = $"{frontendUrl.TrimEnd('/')}/?login=failed";

        var loginInfo = await signInManager.GetExternalLoginInfoAsync();
        if (loginInfo is null)
        {
            return Results.Redirect(failureUrl);
        }

        var email = loginInfo.Principal.FindFirstValue(ClaimTypes.Email);
        var user = await userManager.FindByLoginAsync(
            loginInfo.LoginProvider,
            loginInfo.ProviderKey);

        if (user is null)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Results.Redirect(failureUrl);
            }

            // The random user name is an Identity implementation detail. Email is
            // mutable profile data and is never used as the account's durable key.
            user = new ApplicationUser
            {
                UserName = Guid.NewGuid().ToString("N"),
                Email = email,
                EmailConfirmed = true,
            };

            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return Results.Redirect(failureUrl);
            }

            var addLoginResult = await userManager.AddLoginAsync(user, loginInfo);
            if (!addLoginResult.Succeeded)
            {
                await userManager.DeleteAsync(user);
                return Results.Redirect(failureUrl);
            }

            await signInManager.SignInAsync(
                user,
                isPersistent: true,
                loginInfo.LoginProvider);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(email)
                && (!string.Equals(
                        user.Email,
                        email,
                        StringComparison.OrdinalIgnoreCase)
                    || !user.EmailConfirmed))
            {
                var setEmailResult = await userManager.SetEmailAsync(user, email);
                if (!setEmailResult.Succeeded)
                {
                    return Results.Redirect(failureUrl);
                }

                user.EmailConfirmed = true;
                var confirmEmailResult = await userManager.UpdateAsync(user);
                if (!confirmEmailResult.Succeeded)
                {
                    return Results.Redirect(failureUrl);
                }
            }

            var signInResult = await signInManager.ExternalLoginSignInAsync(
                loginInfo.LoginProvider,
                loginInfo.ProviderKey,
                isPersistent: true,
                bypassTwoFactor: true);

            if (!signInResult.Succeeded)
            {
                return Results.Redirect(failureUrl);
            }
        }

        return Results.Redirect($"{frontendUrl.TrimEnd('/')}/?login=success");
    });

authentication
    .MapGet("/me", async (ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager) =>
    {
        var user = await userManager.GetUserAsync(principal);

        return user is null
            ? Results.Unauthorized()
            : Results.Ok(new
            {
                UserId = user.Id,
                user.Email,
            });
    })
    .RequireAuthorization();

authentication
    .MapPost(
        "/logout",
        async (HttpRequest request,
            SignInManager<ApplicationUser> signInManager) =>
        {
            if (!IsFrontendRequest(request))
            {
                return Results.BadRequest();
            }

            await signInManager.SignOutAsync();
            return Results.NoContent();
        })
    .RequireAuthorization();

authentication
    .MapDelete(
        "/account",
        async (HttpRequest request,
            ClaimsPrincipal principal,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager) =>
        {
            if (!IsFrontendRequest(request))
            {
                return Results.BadRequest();
            }

            var user = await userManager.GetUserAsync(principal);
            if (user is null)
            {
                return Results.Unauthorized();
            }

            var deleteResult = await userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                return Results.Problem(
                    "The local account could not be deleted.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }

            await signInManager.SignOutAsync();
            return Results.NoContent();
        })
    .RequireAuthorization();

app.MapProfileEndpoints();

app.Run();

static bool IsFrontendRequest(HttpRequest request) =>
    request.Headers["X-Requested-With"] == "SwimmingCoach.Web";

public partial class Program;
