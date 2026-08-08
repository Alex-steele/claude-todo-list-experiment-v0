using Dapper;
using TodoApp.Infrastructure;

namespace TodoApp.Features.Todos.Dependencies;

public class SetDependencyHandler(Database db)
{
    public async Task HandleAsync(int todoId, int? dependsOnTodoId)
    {
        if (dependsOnTodoId is null)
        {
            using var clearConn = db.CreateConnection();
            var cleared = await clearConn.ExecuteAsync(
                "UPDATE Todos SET DependsOnTodoId = NULL WHERE Id = @Id",
                new { Id = todoId });

            if (cleared == 0)
                throw new ArgumentException($"Todo with id {todoId} not found.");

            return;
        }

        if (dependsOnTodoId == todoId)
            throw new ArgumentException("A todo cannot depend on itself.", nameof(dependsOnTodoId));

        using var conn = db.CreateConnection();

        var exists = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Todos WHERE Id = @Id",
            new { Id = todoId });
        if (exists == 0)
            throw new ArgumentException($"Todo with id {todoId} not found.");

        var targetExists = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Todos WHERE Id = @Id",
            new { Id = dependsOnTodoId });
        if (targetExists == 0)
            throw new ArgumentException($"Todo with id {dependsOnTodoId} not found.", nameof(dependsOnTodoId));

        // Walk the chain starting from the proposed dependency; if it ever
        // reaches back to todoId, setting this link would create a cycle.
        var currentId = dependsOnTodoId;
        var visited = new HashSet<int>();
        while (currentId is not null)
        {
            if (currentId == todoId)
                throw new ArgumentException("Setting this dependency would create a circular chain.", nameof(dependsOnTodoId));

            if (!visited.Add(currentId.Value))
                break; // pre-existing cycle unrelated to this change; stop walking

            currentId = await conn.ExecuteScalarAsync<int?>(
                "SELECT DependsOnTodoId FROM Todos WHERE Id = @Id",
                new { Id = currentId });
        }

        await conn.ExecuteAsync(
            "UPDATE Todos SET DependsOnTodoId = @DependsOnTodoId WHERE Id = @Id",
            new { Id = todoId, DependsOnTodoId = dependsOnTodoId });
    }
}
