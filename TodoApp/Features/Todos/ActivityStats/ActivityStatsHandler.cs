using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.ActivityStats;

public class ActivityStatsHandler(Database db)
{
    public async Task<ActivityStats> HandleAsync()
    {
        using var conn = db.CreateConnection();

        // Fetch all distinct completion dates (UTC, normalized to date-only strings)
        var rawDates = await conn.QueryAsync<string>(
            "SELECT DISTINCT date(CompletedAt) FROM Todos WHERE CompletedAt IS NOT NULL ORDER BY 1 DESC");

        var completionDates = rawDates
            .Where(d => d is not null)
            .Select(d => DateTime.Parse(d).Date)
            .ToHashSet();

        var today = DateTime.UtcNow.Date;
        var weekStart = today.AddDays(-6);

        var completedToday = completionDates.Contains(today) ? await CountOnDate(conn, today) : 0;
        var completedThisWeek = await CountSinceDate(conn, weekStart);

        var (currentStreak, longestStreak) = ComputeStreaks(completionDates, today);

        return new ActivityStats(currentStreak, longestStreak, completedToday, completedThisWeek);
    }

    private static async Task<int> CountOnDate(Microsoft.Data.Sqlite.SqliteConnection conn, DateTime date)
    {
        var dateStr = date.ToString("yyyy-MM-dd");
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Todos WHERE date(CompletedAt) = @date",
            new { date = dateStr });
    }

    private static async Task<int> CountSinceDate(Microsoft.Data.Sqlite.SqliteConnection conn, DateTime from)
    {
        var fromStr = from.ToString("yyyy-MM-dd");
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Todos WHERE date(CompletedAt) >= @from",
            new { from = fromStr });
    }

    public static (int current, int longest) ComputeStreaks(HashSet<DateTime> completionDates, DateTime today)
    {
        if (completionDates.Count == 0)
            return (0, 0);

        // Current streak: count consecutive days ending today (or yesterday if nothing done today)
        var current = 0;
        var cursor = completionDates.Contains(today) ? today : today.AddDays(-1);
        while (completionDates.Contains(cursor))
        {
            current++;
            cursor = cursor.AddDays(-1);
        }

        // Longest streak: walk all dates in ascending order
        var sorted = completionDates.OrderBy(d => d).ToList();
        var longest = 0;
        var run = 1;
        for (var i = 1; i < sorted.Count; i++)
        {
            if ((sorted[i] - sorted[i - 1]).Days == 1)
            {
                run++;
                if (run > longest) longest = run;
            }
            else
            {
                if (run > longest) longest = run;
                run = 1;
            }
        }
        if (run > longest) longest = run;

        return (current, longest);
    }
}
