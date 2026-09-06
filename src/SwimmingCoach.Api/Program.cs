var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/api/health", () => Results.Ok(new
{
    Status = "ok"
}));

app.Run();

public partial class Program;