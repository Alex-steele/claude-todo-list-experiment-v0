using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Goals;

public class SetDailyGoalHandler(Database db)
{
    public async Task HandleAsync(int target)
    {
        if (target < 0) throw new ArgumentException("Goal must be zero or positive.", nameof(target));

        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            """
            INSERT INTO Goals (Key, Value) VALUES ('DailyGoal', @target)
            ON CONFLICT(Key) DO UPDATE SET Value = @target
            """,
            new { target });
    }
}
