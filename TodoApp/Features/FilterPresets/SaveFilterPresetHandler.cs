using System.Text.Json;
using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.FilterPresets;

public class SaveFilterPresetHandler(Database db)
{
    public async Task<int> HandleAsync(string name, FilterPresetOptions options)
    {
        name = name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Preset name cannot be empty.", nameof(name));

        var json = JsonSerializer.Serialize(options);
        using var conn = db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "INSERT INTO FilterPresets (Name, FiltersJson) VALUES (@Name, @Json); SELECT last_insert_rowid()",
            new { Name = name, Json = json });
    }
}
