using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Goals;

public class GetDailyGoalHandler(Database db)
{
    public async Task<int?> HandleAsync()
    {
        using var conn = db.CreateConnection();
        var value = await conn.ExecuteScalarAsync<int?>(
            "SELECT Value FROM Goals WHERE Key = 'DailyGoal'");
        return value is > 0 ? value : null;
    }
}
