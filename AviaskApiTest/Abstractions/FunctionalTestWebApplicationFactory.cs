using AviaskApi;
using AviaskApi.Models;
using AviaskApi.Services.MockExaminer;
using AviaskApi.Services.StripeService;
using AviaskApiTest.Mocks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace AviaskApiTest.Abstractions;

public class FunctionalTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("AviaskApiTest")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithCleanUp(true)
        .Build();

    public string ConnectionString => _dbContainer.GetConnectionString();

    public Task InitializeAsync()
    {
        return _dbContainer.StartAsync();
    }

    public new Task DisposeAsync()
    {
        return _dbContainer.StopAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "test");

        builder.ConfigureTestServices(services =>
        {
            //  Mocks StripeService
            services.RemoveAll(typeof(IStripeService));
            services.AddScoped<IStripeService, StripeServiceMock>();

            //  Mocks MockExaminer
            services.RemoveAll(typeof(IMockExaminer));
            services.AddScoped<IMockExaminer, MockExaminerMock>();

            //  Replace DB context with test DB context
            services.RemoveAll(typeof(DbContextOptions<AviaskApiContext>));
            services.AddDbContext<AviaskApiContext>(options => { options.UseNpgsql(ConnectionString); });

            //  Setups the test DB context
            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;

            var context = scopedServices.GetRequiredService<AviaskApiContext>();

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        });
    }
}