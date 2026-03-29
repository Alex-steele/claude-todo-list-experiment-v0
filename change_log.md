# Change Log

## Day 7 — [2026-03-29] — Feature: Bulk Operations

**Description:** Users can now select multiple todos and act on them all at once. A "Select" button in the filter toolbar enters selection mode, revealing a checkbox beside each todo. As soon as at least one todo is selected, a bulk action bar appears with "Mark Complete" and "Delete" buttons alongside the count of selected items. Clicking "Cancel" exits selection mode and clears the selection.

**Reason for change:** With filter, sort, search, and inline editing in place, the app is well-suited for managing growing lists. The next friction point is performing repetitive operations — a user who wants to clear all completed todos, or mark a whole set of items done, has to click one at a time. Bulk operations eliminate that friction and make the app meaningfully more useful for anyone with more than a handful of todos.

**Removals:** None

**Stats:**
- Lines added: 322
- Lines deleted: 13
- Tests added: 12
- Tests removed: 0
- Test failures before green: 0

## Day 6 — [2026-03-26] — Feature: Inline Todo Editing

**Description:** Users can now edit a todo's title directly in the list. Each todo shows a pencil icon button; clicking it switches the title to an editable text field with Save (checkmark) and Cancel (X) buttons. Pressing Enter saves the change, pressing Escape cancels. The updated title is persisted immediately.

**Reason for change:** The app has CRUD for create, complete, and delete but was missing the "update" half of the equation. Users inevitably make typos or change their minds — inline editing is a fundamental feature for any real-world todo list.

**Removals:** None

**Stats:**
- Lines added: 152
- Lines deleted: 12
- Tests added: 10
- Tests removed: 0
- Test failures before green: 2

## Day 5 — [2026-03-25] — Feature: Filter, Sort, and Search

**Description:** Users can now filter their todo list by status (All / Active / Completed), sort by four orderings (Newest first, Oldest first, Due date, Priority), and search todos by title. These controls appear in a card above the list whenever at least one todo exists. When a combination of filters produces no results, a friendly "No todos match your filters" message is shown instead of an empty screen.

**Reason for change:** With priorities and due dates in place the list can grow long and become hard to navigate. Filter, sort, and search make the list genuinely useful at scale — a user can instantly isolate what still needs doing, jump to overdue items, or find a specific task by name.

**Removals:** None

**Stats:**
- Lines added: 253
- Lines deleted: 59
- Tests added: 6
- Tests removed: 0
- Test failures before green: 2

## Day 4 — [2026-03-24] — Feature: Due Dates

**Description:** Users can now optionally set a due date when creating a todo. The due date is displayed on each todo item formatted as "MMM d, yyyy". Todos that are past their due date and not yet completed are highlighted in red with a warning icon. Todos due today (and not completed) are shown in orange. Future due dates appear in a muted secondary color. The add-todo form includes a clearable date picker labeled "Due date (optional)".

**Reason for change:** Priority levels tell you what matters most, but not when. Due dates add a time dimension so users can see at a glance which tasks are urgent because they are overdue or due soon. Overdue highlighting creates a natural visual pressure to act on delinquent tasks.

**Removals:** None

**Stats:**
- Lines added: 241
- Lines deleted: 11
- Tests added: 7
- Tests removed: 0
- Test failures before green: 2

## Day 3 — [2026-03-24] — Fix: MudPopoverProvider circuit isolation

**Description:** The priority dropdown (and the entire interactive page) was broken because `MudPopoverProvider` lives in `MainLayout.razor` which is rendered statically, while `Home.razor` ran as `@rendermode InteractiveServer`. They ended up on different DI scopes with different `PopoverService` instances, so when `MudSelect` tried to open its dropdown it threw `Missing <MudPopoverProvider />`, terminating the Blazor Server circuit and making all interactivity (including the Add button) stop responding. Fixed by moving `@rendermode InteractiveServer` to `Routes.razor` so the entire tree — layout and pages — shares one circuit and one `PopoverService`.

Also corrected `MudSelectItem Value` attributes to use `@` prefix so Blazor evaluates them as C# enum expressions rather than string literals.

**Reason for change:** Bug introduced in Day 3 feature; rendered the app completely non-functional.

**Removals:** None

**Stats:**
- Lines added: 3
- Lines deleted: 2
- Tests added: 0
- Tests removed: 0
- Test failures before green: 0

## Day 3 — [2026-03-23] — Feature: Priority Levels

**Description:** Users can now assign a priority level (None, Low, Medium, High) to each todo when creating it. Priorities are displayed as color-coded chips next to the todo title — blue for Low, orange for Medium, red for High — making it easy to see at a glance which tasks matter most.

**Reason for change:** After basic CRUD is in place, priority is the most natural next step. A flat unordered list treats all tasks as equal, but real users always have some tasks that are more urgent than others. Priority labels let users capture that urgency without needing categories or due dates yet.

**Removals:** None

**Stats:**
- Lines added: 106
- Lines deleted: 15
- Tests added: 5
- Tests removed: 0
- Test failures before green: 0

## Day 2 — [2026-03-22] — Feature: Delete Todo

**Description:** Users can now delete individual todos by clicking the trash icon button on any todo item. The todo is permanently removed and the list refreshes immediately.

**Reason for change:** Delete is fundamental CRUD — without it, the list can only grow. Users need the ability to remove tasks they no longer care about, not just mark them complete.

**Removals:** None

**Stats:**
- Lines added: 101
- Lines deleted: 2
- Tests added: 4
- Tests removed: 0
- Test failures before green: 0

## Day 1 — [2026-03-22] — Feature: Mark Todo as Complete

**Description:** Users can now check off todos by clicking a checkbox next to each item. Completed todos appear with a strikethrough and reduced opacity. Clicking the checkbox again toggles the todo back to incomplete.

**Reason for change:** Marking items as done is the most fundamental interaction in any todo app — without it, the list is purely additive and provides no sense of progress or completion. This is the obvious next step after basic add/list functionality.

**Removals:** None

**Stats:**
- Lines added: 284
- Lines deleted: 13
- Tests added: 12
- Tests removed: 0
- Test failures before green: 1
