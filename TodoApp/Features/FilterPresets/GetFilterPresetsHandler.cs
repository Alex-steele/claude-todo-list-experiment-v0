using System.Text.Json;
using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.FilterPresets;

public class GetFilterPresetsHandler(Database db)
{
    public async Task<IReadOnlyList<FilterPreset>> HandleAsync()
    {
        using var conn = db.CreateConnection();
        var rows = await conn.QueryAsync<(int Id, string Name, string FiltersJson)>(
            "SELECT Id, Name, FiltersJson FROM FilterPresets ORDER BY Id ASC");

        return rows
            .Select(r => new FilterPreset(r.Id, r.Name,
                JsonSerializer.Deserialize<FilterPresetOptions>(r.FiltersJson)!))
            .ToList();
    }
}
