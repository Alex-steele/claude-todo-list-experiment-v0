namespace TodoApp.Features.Todos.Subtasks;

public record Subtask(int Id, int TodoId, string Title, bool IsCompleted, DateTime CreatedAt);
