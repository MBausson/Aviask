using System.Diagnostics.CodeAnalysis;
using AviaskApi.Entities;
using AviaskApi.Models;
using AviaskApi.Services.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AviaskApiTest.Abstractions;

public class FunctionalTest : IClassFixture<FunctionalTestWebApplicationFactory>, IAsyncLifetime
{
    public FunctionalTest(FunctionalTestWebApplicationFactory factory, ITestOutputHelper output)
    {
        Output = output;
        Factory = factory;

        HttpClient = Factory.CreateClient();
        DbContext = new AviaskApiContext(new DbContextOptionsBuilder().UseNpgsql(Factory.ConnectionString).Options);
    }

    protected HttpClient HttpClient { get; init; }
    protected AviaskApiContext DbContext { get; init; }
    protected ITestOutputHelper Output { get; init; }
    protected IServiceScope ServiceScope => Factory.Services.CreateScope();

    private FunctionalTestWebApplicationFactory Factory { get; }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    [SuppressMessage("Security", "EF1002:Risk of vulnerability to SQL injection.")]
    public async Task DisposeAsync()
    {
        //  Remove data from the database after each test
        //  Cleaner alternatives didn't work out (Respawn, Transactions)

        //  /!\ Order of types matters

        Type[] entities =
        [
            typeof(AnswerRecord), typeof(MockExam), typeof(Question),
            typeof(QuestionAnswers), typeof(QuestionReport), typeof(Attachment),
            typeof(AviaskUser)
        ];

        foreach (var entity in entities)
        {
            var tableName = GetTableName(entity);

            await DbContext.Database.ExecuteSqlRawAsync($"DELETE FROM \"{tableName}\"");
        }

        await DbContext.SaveChangesAsync();
    }

    protected async Task CreateUser(AviaskUser user)
    {
        if (await DbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id) is not null) return;

        var role = DbContext.Roles.First(r => r.Name!.ToLower() == (user.Role ?? "member"));

        await DbContext.Users.AddAsync(user);
        await DbContext.UserRoles.AddAsync(new IdentityUserRole<Guid>
            { UserId = user.Id, RoleId = role.Id });

        await DbContext.SaveChangesAsync();
    }

    protected async Task<Question[]> CreateQuestions(params Question[] questions)
    {
        foreach (var question in questions) question.PublishedAt = DateTime.Now.ToUniversalTime();

        await DbContext.Questions.AddRangeAsync(questions);
        await DbContext.SaveChangesAsync();

        return questions;
    }

    protected async Task<Question> CreateQuestion(Question question)
    {
        question.PublishedAt = DateTime.Now.ToUniversalTime();

        await DbContext.Questions.AddAsync(question);
        await DbContext.SaveChangesAsync();

        return question;
    }

    protected async Task<MockExam> CreateMockExam(MockExam mockExam)
    {
        await DbContext.MockExams.AddAsync(mockExam);
        await DbContext.SaveChangesAsync();

        return mockExam;
    }

    /// <summary>
    ///     Creates a user with a given role, and adds the Authorization header related to that user for every request sent
    ///     from HttpClient
    /// </summary>
    /// <param name="role">User's role, default being admin</param>
    protected async Task<AviaskUser> Authenticate(string role = "admin", bool premium = false)
    {
        //  Creates user
        var user = new UserFactory().GetOne();
        user.Role = role;
        user.IsPremium = premium;
        user.PasswordHash = "testing";

        await CreateUser(user);

        //  Get JWT service
        using var scope = Factory.Services.CreateScope();

        var jwtService = new JwtService(scope.ServiceProvider.GetRequiredService<UserManager<AviaskUser>>(),
            scope.ServiceProvider.GetRequiredService<IAviaskRepository<AviaskUser, Guid>>());

        var jwt = await jwtService.CreateTokenAsync(user);

        //  Adds the authorization header
        HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");

        return user;
    }

    private string GetTableName(Type tEntity)
    {
        var entityType = DbContext.Model.FindEntityType(tEntity);

        if (entityType is null)
            throw new InvalidOperationException($"Could not retrieve table name for entity {tEntity.Name}");

        return entityType.GetTableName() ??
               throw new InvalidOperationException($"Table name for {tEntity.Name} is empty or null");
    }
}