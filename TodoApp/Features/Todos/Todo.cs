namespace TodoApp.Features.Todos;

public record Todo(int Id, string Title, bool IsCompleted, DateTime CreatedAt);
