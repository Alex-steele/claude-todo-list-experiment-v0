using TodoApp.Features.Todos.GetTodos;

namespace TodoApp.Features.Todos.RandomPicker;

public class PickRandomTodoHandler
{
    private static readonly Random _rng = Random.Shared;

    public TodoSummary? Handle(IReadOnlyList<TodoSummary> candidates, int? excludeId = null)
    {
        var pool = candidates.Where(t => !t.IsCompleted && t.Id != excludeId).ToList();
        if (pool.Count == 0) return null;
        return pool[_rng.Next(pool.Count)];
    }
}
