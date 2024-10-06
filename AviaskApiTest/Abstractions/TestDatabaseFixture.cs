using AviaskApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AviaskApiTest.Abstractions;

public class TestDatabaseFixture
{
    private const string ConnectionString =
        @"Host=localhost;Port=5432;Database=AviaskApiTest;Username=postgres;Password=aviaskapi;Include Error Detail=true";

    private static readonly object Lock = new();
    private static bool _databaseInitialized;

    public TestDatabaseFixture()
    {
        lock (Lock)
        {
            if (_databaseInitialized) return;

            using var context = CreateContext();

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            _databaseInitialized = true;
        }
    }

    public AviaskApiContext CreateContext()
    {
        return new AviaskApiContext(new DbContextOptionsBuilder<AviaskApiContext>().UseNpgsql(ConnectionString)
            .Options);
    }
}