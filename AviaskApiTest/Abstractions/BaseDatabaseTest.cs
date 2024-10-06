using AviaskApi.Models;
using Xunit.Abstractions;

namespace AviaskApiTest.Abstractions;

public class BaseDatabaseTest : IClassFixture<TestDatabaseFixture>, IDisposable
{
    protected readonly ITestOutputHelper Output;
    protected AviaskApiContext Context;

    public BaseDatabaseTest(TestDatabaseFixture fixture, ITestOutputHelper output)
    {
        Context = fixture.CreateContext();
        Output = output;

        Context.Database.BeginTransaction();
    }

    public void Dispose()
    {
        Context.Database.RollbackTransaction();
        Context.Dispose();
    }
}