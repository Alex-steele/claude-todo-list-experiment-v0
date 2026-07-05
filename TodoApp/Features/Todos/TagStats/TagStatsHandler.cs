using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.Tags;

namespace TodoApp.Features.Todos.TagStats;

public record TagStat(string TagName, int Active, int Completed)
{
    public int Total => Active + Completed;
    public int CompletionPercent => Total == 0 ? 0 : Completed * 100 / Total;
}

public class TagStatsHandler
{
    public IReadOnlyList<TagStat> Handle(
        IReadOnlyList<TodoSummary> todos,
        Dictionary<int, List<Tag>> todoTags)
    {
        var todoById = todos.ToDictionary(t => t.Id);

        // Build per-tag counts by iterating all tag assignments
        var statsByTag = new Dictionary<string, (int Active, int Completed)>(StringComparer.OrdinalIgnoreCase);

        foreach (var (todoId, tags) in todoTags)
        {
            if (!todoById.TryGetValue(todoId, out var todo))
                continue;

            foreach (var tag in tags)
            {
                var name = tag.Name;
                statsByTag.TryGetValue(name, out var counts);
                if (todo.IsCompleted)
                    statsByTag[name] = (counts.Active, counts.Completed + 1);
                else
                    statsByTag[name] = (counts.Active + 1, counts.Completed);
            }
        }

        return statsByTag
            .Select(kv => new TagStat(kv.Key, kv.Value.Active, kv.Value.Completed))
            .Where(s => s.Total > 0)
            .OrderBy(s => s.CompletionPercent)
            .ThenBy(s => s.TagName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
