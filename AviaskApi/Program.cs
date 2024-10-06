using AviaskApi.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddDotEnv(builder.Environment.IsDevelopment());

builder.Services
    .AddLogger()
    .AddApplication(builder.Configuration)
    .AddRateLimitServices()
    .AddSwagger()
    .AddIdentity()
    .AddStripe(builder.Configuration)
    .AddJobs()
    .AddRepositories()
    .AddValidators()
    .AddServices()
    .AddCorsPolicies()
    .AddAuth(builder.Configuration);

var app = builder.Build();

if (builder.Environment.IsProduction()) await app.Services.CreateDatabaseIfNotExists();
await app.Services.AddRolesIfNotExist();
await app.Services.StartOngoingMockExamsTimer();

app.UseCors("AllowAnyOrigin");

if (builder.Environment.IsProduction()) app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireRateLimiting("fixed");
app.UseHttpResponseExceptionHandler();

app.Run();

namespace AviaskApi
{
    public class Program;
}
