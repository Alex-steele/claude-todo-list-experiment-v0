# Daily Evolution Prompt (v2)

Use this prompt with Claude Code once per day. Copy everything below the line.

---

You are evolving a .NET Blazor todo list application. Your job is to make exactly ONE meaningful change to this codebase today. Read the entire codebase, understand its current state, then decide what to do next.

## Architecture

This application follows **vertical slice architecture**. Each feature should be organized as a self-contained vertical slice — its own folder containing the relevant components, services, models, and logic for that feature. Shared infrastructure (e.g., base classes, common UI components, persistence abstractions) can live in shared/common folders, but feature-specific logic should not leak across slice boundaries.

All new features you add must follow this architecture. When you encounter existing code that violates vertical slice principles (e.g., a monolithic service handling multiple features, cross-feature dependencies, logic for one feature living inside another feature's folder), you are encouraged to refactor it toward better slice isolation — as long as that refactor is paired with a feature in the same commit.

## Your constraints

### What you MUST do
1. **Add a feature.** Every commit must introduce at least one new user-facing feature or meaningful enhancement. You may refactor code as part of the same commit, but refactoring alone is not sufficient — there must be a feature.
2. **Keep changes incremental.** Build on what exists. You are NOT allowed to delete everything and start from scratch, rewrite the entire application, or replace the tech stack. Each change should be a natural next step from the current state of the app.
3. **Follow vertical slice architecture.** New features should be organized as vertical slices. Do not dump all logic into a single shared service or scatter a feature's concerns across unrelated folders.
4. **Write tests following the testing pyramid.** Prefer classical-school, low-mocking unit tests. Use real objects and state-based verification wherever possible. Only mock external dependencies (HTTP clients, databases, file systems). End-to-end tests should be rare and only for critical user paths.
5. **Write bUnit tests for all Blazor component behaviour.** Every piece of UI logic that you add or change must be covered by a bUnit component test. This is not optional. See the **bUnit testing rules** section below for exactly what this means.
6. **All tests MUST pass before you commit.** Run `dotnet test` and confirm a clean pass. If tests fail, fix them. Do not commit with failing tests under any circumstances.
7. **Update `change_log.md`** at the root of the repo with an entry for today's change (format specified below).
8. **Update `summary.md`** at the root of the repo with running totals (format specified below).
9. **Commit and push** all changes with a clear, conventional commit message.

### What you are allowed to do
- **Refactor code** as part of a feature commit. Refactoring alone does not count as the daily change — it must accompany a feature. Refactoring toward better vertical slice architecture is specifically encouraged — if you spot a feature's logic leaking across slices, tighten it up while delivering the day's feature.
- **Remove features**, but ONLY when it makes product sense. Valid reasons: a new feature supersedes and makes the old one redundant, or the old feature conflicts with a better UX direction you are taking. Invalid reasons: simplifying for the sake of it, removing something you just added, or reducing scope without clear justification. If you remove a feature, explain the product reasoning in the changelog.
- **Alter existing tests** if the behavior they test has changed due to your feature work.
- **Remove tests** only if the behavior they tested has been removed, or if another test already covers the same behavior. Never delete a test just because it is inconvenient.

### What you are NOT allowed to do
- Make a commit that is purely refactoring with no feature.
- Delete the application and start over.
- Replace the entire architecture in a single commit.
- Commit with failing tests.
- Add a feature and then remove it the next day without strong product justification.
- Skip updating `change_log.md` or `summary.md`.
- Add or change Blazor component behaviour without a corresponding bUnit test.

## bUnit testing rules

bUnit renders Blazor components in memory without a browser. Use it to test all UI logic in `Home.razor` (and any future page or component files). Tests live in `TodoApp.Tests` under `Components/Pages/` mirroring the production structure.

### Setup

`TodoApp.Tests` must reference the `bunit` NuGet package. If it is not already present, add it:

```
dotnet add TodoApp.Tests package bunit
```

Each bUnit test class inherits from `TestContext` (or uses one via `using var ctx = new TestContext()`). Register the same services the real app registers (handlers, `Database`) using a fresh `TestDatabase` so tests remain isolated and hit real SQLite — do not mock the handlers.

### What you MUST test with bUnit

For every piece of UI behaviour you add or change, write at least one bUnit test covering each of the following that applies:

1. **Rendering** — the new element/component is present in the rendered output with the correct initial state (e.g., a new button exists, a chip shows the right label, a field starts empty).
2. **User interactions** — simulate the user action (button click, input change, dropdown selection, checkbox toggle) and assert the DOM updates correctly afterward.
3. **State after async operations** — after clicking "Add Todo", assert the new item appears in the list. After clicking delete, assert the item is gone. Use `cut.WaitForState(...)` or `cut.WaitForAssertion(...)` for async operations.
4. **Validation/error paths** — if the feature has validation (e.g., empty title shows an error), test the failure case as well as the success case.

### What you do NOT need to test with bUnit

- Logic that is already covered by a handler unit test (e.g., "the DB correctly stores the priority" — that belongs in `AddTodoHandlerTests`, not a component test).
- Pure CSS/styling changes with no logic attached.

### Example structure

```csharp
// TodoApp.Tests/Components/Pages/HomeTests.cs
public class HomeTests : TestContext
{
    public HomeTests()
    {
        // Register real services backed by an in-memory test DB
        var db = TestDatabase.CreateAsync().GetAwaiter().GetResult();
        Services.AddScoped(_ => new AddTodoHandler(db));
        Services.AddScoped(_ => new GetTodosHandler(db));
        Services.AddScoped(_ => new CompleteTodoHandler(db));
        Services.AddScoped(_ => new DeleteTodoHandler(db));
        // Register MudBlazor services required by the component
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public async Task AddTodo_ValidTitle_AppearsInList()
    {
        var cut = RenderComponent<Home>();
        cut.Find("input").Change("Walk the dog");
        cut.Find("button.mud-button-root").Click();
        await cut.WaitForAssertionAsync(() =>
            cut.FindAll(".mud-list-item").Count.ShouldBe(1));
    }
}
```

Adjust selectors to match the actual rendered MudBlazor markup. Use `cut.Find` / `cut.FindAll` with CSS selectors, or `cut.FindComponent<MudButton>()` for typed lookups.

## Decision-making process

Before writing any code, do the following:

1. **Read the full codebase.** Understand every file, every component, every test.
2. **Read `change_log.md`** to understand the history of changes. Do not repeat a recently added feature. Do not undo recent work without justification.
3. **Read `summary.md`** to understand the running totals.
4. **Decide on today's feature.** Choose something that makes the app genuinely more useful as a todo application. Think about what a real user would want next. Consider this rough feature progression as inspiration (you do not have to follow it exactly, but it gives you a sense of natural growth):
   - Early stage: basic CRUD, marking complete, persistence, validation
   - Mid stage: categories/tags, due dates, priority levels, filtering, sorting, search
   - Later stage: drag-and-drop reordering, recurring tasks, subtasks, dark mode, keyboard shortcuts, bulk operations, undo, notifications, import/export
   - Advanced: multiple lists, collaboration hints, analytics/stats dashboard, natural language input, smart suggestions
5. **Plan your implementation.** Think about what components, services, and models you need to add or change. Think about what tests you will write.

## File formats

### change_log.md

If the file does not exist, create it with a header. Prepend each new entry at the top (newest first). Use this exact format:

```markdown
# Change Log

## Day <N> — [YYYY-MM-DD] — Feature: <short feature name>

**Description:** <1-3 sentences describing what was added or changed from a user's perspective>

**Reason for change:** <1-2 sentences explaining why this feature was chosen as the next step>

**Removals:** <If any features or behaviors were removed, describe what and why. Otherwise write "None">

**Stats:**
- Lines added: <number>
- Lines deleted: <number>
- Tests added: <number>
- Tests removed: <number>
- Test failures before green: <number — how many times `dotnet test` failed before all tests passed>
```

### summary.md

If the file does not exist, create it. Overwrite it entirely each time. Use this exact format:

```markdown
# Evolution Summary

- **Current day:** <number>
- **Total commits:** <number>
- **Total features added:** <number>
- **Total features removed:** <number>
- **Total refactors (as part of feature commits):** <number — count a commit as including a refactor if you restructured existing code beyond what was strictly necessary for the feature>
- **Total tests:** <number — current count of test methods in the solution>
- **Total test failures before green (all time):** <number — sum of all "test failures before green" from every changelog entry>
- **Lines of production code (approx):** <number — exclude test files>
- **Lines of test code (approx):** <number — test files only>
```

## Execution steps

Do these in order:

1. **Switch to `main`.** Run `git checkout main && git pull` to ensure you are on the latest main branch.
2. Read the codebase and the changelog/summary files.
3. **Determine the day number.** Count the existing entries in `change_log.md`. If there are none, today is Day 1. If there are N entries, today is Day N+1.
4. Decide on the feature. Do not ask me — just pick one.
5. **Create a feature branch.** Run `git checkout -b day-<N>/<short-kebab-description>` (e.g., `day-7/add-due-dates`).
6. Implement the feature (and any refactoring).
7. Write or update tests. This means **both**:
   - Handler unit tests (in `TodoApp.Tests/Features/...`) for any new or changed backend logic.
   - bUnit component tests (in `TodoApp.Tests/Components/...`) for any new or changed Blazor UI behaviour. Follow the **bUnit testing rules** above exactly. If `bunit` is not yet in `TodoApp.Tests.csproj`, add it now.
8. Run `dotnet test`. If it fails, fix and re-run. Count failures.
9. Calculate the line stats using `git diff --stat` (staged changes).
10. Update `change_log.md` (prepend new entry with the day number).
11. Update `summary.md` (overwrite with new totals).
12. Stage all changes: `git add -A`
13. Commit with message format: `feat(day-<N>): <short description>` (or `feat!(day-<N>): <description>` if it includes a removal).
14. **Merge to main and push.** Run:
    ```
    git checkout main
    git merge day-<N>/<short-kebab-description> --no-ff -m "Merge day-<N>: <short description>"
    git push origin main
    ```
15. **Clean up the feature branch.** Run `git branch -d day-<N>/<short-kebab-description>`

Begin now. Do not ask me any questions. Read the repo and start.
