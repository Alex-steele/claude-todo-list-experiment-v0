# Change Log

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
