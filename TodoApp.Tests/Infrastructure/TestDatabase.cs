using TodoApp.Infrastructure;

namespace TodoApp.Tests.Infrastructure;

public static class TestDatabase
{
    public static async Task<Database> CreateAsync()
    {
        var path = Path.Combine(Path.GetTempPath(), $"TodoApp_Test_{Guid.NewGuid()}.db");
        var db = new Database($"Data Source={path}");
        await db.InitializeAsync();
        return db;
    }
}
