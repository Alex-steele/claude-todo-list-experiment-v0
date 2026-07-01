using TodoApp.Features.Todos.GetTodos;
using TodoApp.Features.Todos.Tags;

namespace TodoApp.Features.Todos.FilterCounts;

public record FilterCountsResult(
    IReadOnlyDictionary<TodoPriority, int> ByPriority,
    IReadOnlyDictionary<string, int> ByTag);

public class FilterCountsHandler
{
    public FilterCountsResult Handle(
        IReadOnlyList<TodoSummary> todos,
        IReadOnlyDictionary<int, List<Tag>> todoTags)
    {
        var active = todos.Where(t => !t.IsCompleted).ToList();

        var byPriority = active
            .GroupBy(t => t.Priority)
            .ToDictionary(g => g.Key, g => g.Count());

        var byTag = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var todo in active)
        {
            if (!todoTags.TryGetValue(todo.Id, out var tags)) continue;
            foreach (var tag in tags)
            {
                byTag.TryGetValue(tag.Name, out var existing);
                byTag[tag.Name] = existing + 1;
            }
        }

        return new FilterCountsResult(byPriority, byTag);
    }
}
