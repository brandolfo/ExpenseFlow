var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new HealthResponse("Healthy")));

app.Run();

internal sealed record HealthResponse(string Status);

public partial class Program;
