namespace TodoApp.Features.Todos.Trash;

public record TrashedTodo(
    int TrashId,
    string Title,
    bool IsCompleted,
    TodoPriority Priority,
    DateTime? DueDate,
    int ListId,
    DateTime DeletedAt);
