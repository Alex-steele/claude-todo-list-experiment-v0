# Change Log

## Day 94 — [2026-07-22] — Feature: Analytics Dashboard (Completions by Day of Week)

**Description:** A new "Analytics" page (📊 icon in the toolbar, next to the Calendar icon) shows a bar chart of how many todos in the active list have been completed on each day of the week (Sunday–Saturday). The day with the most completions is highlighted in green and called out below the chart ("Your most productive day is Wednesday"), so users can see which day they're most productive on. A list switcher lets users view the breakdown for any list, and an empty state explains the feature when no todos have been completed yet.

**Reason for change:** The app already had rich but scattered productivity signals — streaks, a 14-day heatmap, completion-time averages, priority and tag completion rates — all living as chips inside the Home page's stats panel. None of them answered a simple, recurring question: "which day of the week am I actually productive on?" That's a genuinely new insight (not derivable from any existing chip) and a natural fit for its own dedicated page, following the same pattern established by the Calendar view (Day 89) — a focused route with its own vertical slice (`DayOfWeekStatsHandler`) rather than more chips bolted onto the already 4,000+ line `Home.razor`.

**Removals:** None

**Stats:**
- Lines added: 510
- Lines deleted: 0
- Tests added: 14
- Tests removed: 0
- Test failures before green: 1

## Day 93 — [2026-07-21] — Feature: Subtask Drag-to-Reorder

**Description:** Subtasks within a todo can now be reordered by dragging. Each subtask row shows a drag-handle icon whenever a todo has 2 or more subtasks; dragging one subtask onto another moves it to that position, persists immediately, and survives a page reload. Todos with a single subtask show no handle since there is nothing to reorder. The checkbox, edit, and delete controls on each subtask row are unaffected.

**Reason for change:** Subtasks already supported add (Day 19), complete (Day 19), delete (Day 19), and rename (Day 42), but not reordering — the one CRUD-adjacent operation still missing. Todos and lists already support drag-to-reorder (Days 27 and 77), so subtasks were the last place in the app where a user could not put items in the order they actually intend to tackle them (e.g., moving a blocking step to the top of a checklist). The feature reuses the exact `SortOrder` + `ReorderXHandler` pattern already established by `ReorderTodosHandler` and `ReorderListsHandler`, so it required no new architectural concepts — just a `SortOrder` column on `Subtasks`, a new `ReorderSubtasksHandler` in the existing `Subtasks` slice, and the same drag/drop markup already used for todos.

**Removals:** None

**Stats:**
- Lines added: 307
- Lines deleted: 5
- Tests added: 9
- Tests removed: 0
- Test failures before green: 1

## Day 92 — [2026-07-21] — Feature: Calendar Day-Detail Panel

**Description:** Day cells on the calendar only ever showed up to three todos, with a "+N more" label for anything beyond that and no way to see the rest. Clicking a day's date number (or its "+N more" label) now opens a day-detail panel below the grid listing every todo due that day, each with a checkbox to toggle completion and a delete button. Completing or deleting a todo from the panel updates both the panel and the month grid immediately. Clicking the same day number again, or the panel's close button, collapses it. Selecting a different day, navigating months, jumping to Today, or switching lists all close the panel since it no longer applies.

**Reason for change:** Day 89 introduced the calendar as a read-only preview (top 3 todos per day) and Days 90–91 made it a place to create and reschedule todos, but there was still no way to actually see or act on everything due on a busy day short of leaving the calendar for the list view. A day-detail panel closes that gap and reuses the existing `CompleteTodoHandler` and `DeleteTodoHandler` as-is, so the change stays entirely inside the `CalendarView` page and slice — no new handler was needed since `CalendarViewHandler` already returns the full (untruncated) per-day todo list that the grid was only rendering three of.

**Removals:** None

**Stats:**
- Lines added: 251
- Lines deleted: 3
- Tests added: 7
- Tests removed: 0
- Test failures before green: 0

## Day 91 — [2026-07-21] — Feature: Drag-to-Reschedule on Calendar

**Description:** Todos shown on the calendar (Day 89) can now be rescheduled by dragging them from one day cell to another. Each todo chip in a day cell is draggable; dropping it on a different day updates its due date to that day via the existing `SetDueDateHandler`, the cell refreshes immediately, and a snackbar confirms ("Rescheduled to Jul 22"). The target cell highlights while a todo is dragged over it. Dropping back onto the same day is a no-op — no update, no snackbar.

**Reason for change:** Day 90 made the calendar a place to create todos, but rescheduling an existing one still meant opening the todo in the list view and editing its due date by hand — a detour from a view whose entire point is showing due dates spatially. Drag-and-drop is the obvious interaction once todos are laid out on a grid (this mirrors the drag-to-reorder pattern already used for lists and todos), and it reuses `SetDueDateHandler` as-is, so the change stays entirely inside the `CalendarView` page and slice.

**Removals:** None

**Bug found during live verification:** Manually exercising the Day 90 quick-add feature in a real browser (not just bUnit) surfaced a live bug: the quick-add `MudTextField` bound its value without `Immediate="true"`, so pressing Enter without first blurring the field never committed the typed title — the todo silently failed to save. bUnit's tests passed because `.Change()` fires a synthetic "change" DOM event directly, masking the real-browser behavior where a bare Enter keypress does not. Fixed by adding `Immediate="true"` and switching the affected bUnit tests to bUnit's `.Input()` (which fires "input", matching `Immediate` binding) instead of `.Change()`.

**Stats:**
- Lines added: 119
- Lines deleted: 5
- Tests added: 3
- Tests removed: 0
- Test failures before green: 1

## Day 90 — [2026-07-20] — Feature: Quick-Add Todo from Calendar

**Description:** The calendar view (added Day 89) was read-only — it could show what was due on a day but not create new work. Each day cell now shows a "+" button on hover; clicking it opens an inline text field right in that cell. Typing a title and pressing Enter creates a new todo in the active list with its due date set to that cell's date, and the cell refreshes to show it immediately. Pressing Escape or submitting an empty title cancels without creating anything.

**Reason for change:** Once a user is looking at a month laid out spatially, the natural next action is "I need to do something on the 15th" — but the only way to add it was to leave the calendar, go back to the list view, and set a due date by hand. Letting the calendar accept input as well as display it closes that loop and makes the view a genuine planning surface rather than a read-only summary. It also keeps the new logic entirely inside the existing `CalendarView` slice and page (reusing the existing `AddTodoHandler`) rather than adding cross-slice coupling.

**Removals:** None

**Stats:**
- Lines added: 151
- Lines deleted: 2
- Tests added: 4
- Tests removed: 0
- Test failures before green: 2

## Day 89 — [2026-07-19] — Feature: Calendar View

**Description:** Adds a new "Calendar view" (📅 icon, in the toolbar next to the export buttons) that opens a dedicated `/calendar` page showing a full month grid for the active list. Each day cell shows up to three todos due that day (with a priority-colored dot and strikethrough for completed items, "+N more" when a day is packed), today's cell is outlined, and days with incomplete overdue todos get a red left border. Prev/Next month arrows, a "Today" shortcut, a list switcher, and a "Back to list" link round out the page. An empty state reads "No todos due this month" when the grid has nothing to show.

**Reason for change:** With 88 days of due-date-aware features (overdue banners, "No date" filter, bulk reschedule, today view) the app had no way to see due dates laid out spatially across a month — the only view was a flat, single-list-at-a-time feed. A calendar is the natural next step once due dates carry real weight in the app, and it's also a good forcing function for the encouraged vertical-slice refactor: rather than bolting more UI onto the already 4,000+ line `Home.razor`, this feature lives entirely in its own route, its own component, and a new pure `CalendarViewHandler` slice (modeled on the existing `TodayViewHandler`/`DueSummaryHandler` pattern of operating on already-fetched `TodoSummary` lists rather than hitting the database directly).

**Removals:** None

**Stats:**
- Lines added: 550
- Lines deleted: 0
- Tests added: 14
- Tests removed: 0
- Test failures before green: 4

## Day 88 — [2026-07-12] — Feature: Auto-Expiring Trash (30-Day Retention)

**Description:** Trash is no longer unbounded — each trashed todo now shows a "Purges in N days" countdown alongside its "Deleted X ago" timestamp, turning red once 3 days or fewer remain. Items older than 30 days are automatically and permanently purged the moment the app loads (checked once at startup), and if any were purged a snackbar reports how many ("N trash items older than 30 days were automatically purged"). Restoring or manually deleting an item before its countdown expires works exactly as before.

**Reason for change:** Day 87 added a Trash view but gave it no retention policy — deleted todos, and everything snapshotted with them (notes, tags, subtasks), would sit in the `DeletedTodos` table forever unless a user remembered to manually empty it. Every real-world trash bin (Gmail, Finder, Notion) auto-expires after a fixed window; adding that here closes an obvious gap and keeps the trash table from growing without bound, while the countdown gives users fair warning before anything is gone for good.

**Removals:** None

**Stats:**
- Lines added: 269
- Lines deleted: 0
- Tests added: 10
- Tests removed: 0
- Test failures before green: 0

## Day 87 — [2026-07-10] — Feature: Trash / Recently Deleted

**Description:** Deleting a todo (individually, in bulk, or via "Clear completed") no longer discards it permanently right away — it now lands in a new persistent "Trash" view, accessible via a 🗑 chip in the list selector row that shows a live count of trashed items. Opening Trash lists every deleted todo with its title, source list, priority, and a relative "Deleted X ago" timestamp, alongside per-item "Restore" and "Delete forever" buttons and an "Empty trash" action to clear everything at once. Restoring a todo re-inserts it into its original list as a fresh active item. This complements (and outlives) the existing ephemeral "Undo" snackbar — if a user closes the snackbar or comes back later, the deleted item is still recoverable from Trash. The two mechanisms stay in sync: using the snackbar's "Undo" also removes the matching Trash entry so nothing is left duplicated.

**Reason for change:** The only safety net for deletion was a 5-ish-second "Undo" snackbar — miss it, refresh the page, or delete something and get distracted, and the todo (and any notes, tags, or subtasks tied to it) was gone forever with no recovery path. A real trash bin is one of the most expected safety features in any file or task manager (Finder, Gmail, Notion, Todoist all have one), and it was a conspicuous gap given how many destructive bulk actions the app already has (bulk delete, clear completed). Implementing it as a snapshot-on-delete into a new `DeletedTodos` table — rather than a soft-delete flag on `Todos` — meant zero changes were needed to any of the app's ~40 other read-side handlers and filters, keeping the change isolated to a new vertical slice plus the three call sites that perform deletion.

**Removals:** None

**Stats:**
- Lines added: 839
- Lines deleted: 6
- Tests added: 28
- Tests removed: 0
- Test failures before green: 0

## Day 86 — [2026-07-09] — Feature: Search Result Highlighting

**Description:** When a search query is active, the matching portion of each todo title is now highlighted with a yellow tint using an inline `<mark class="search-highlight">` element. All occurrences within a title are highlighted (not just the first), and matching is case-insensitive while preserving the original casing in the rendered output. HTML special characters in titles are safely encoded before injection. When the search is cleared, titles render as plain text with no mark tags. The `SearchHighlighter` static class is a focused utility with no dependencies on the database or DI container.

**Files changed:**
- `TodoApp/Features/Todos/Search/SearchHighlighter.cs` — new static utility, wraps matched substrings in `<mark>` tags with HTML encoding
- `TodoApp/Components/Pages/Home.razor` — added `@using` for Search namespace; title now uses `SearchHighlighter.Highlight(todo.Title, _searchQuery.Trim())`
- `TodoApp/wwwroot/app.css` — `.search-highlight` style: `background-color: rgba(255,213,0,0.35)`, rounded corners, inherits color

**Tests added:** 12 unit tests (`SearchHighlighterTests`), 5 bUnit tests (`HomeTests`); 4 existing search tests updated to account for split title markup

---

## Day 85 — [2026-07-09] — Feature: Quick Priority Toggle

**Description:** Priority is now editable in a single click directly from the todo list, without opening the full edit form. Todos with a set priority (Low, Medium, or High) show a clickable colored chip beside the title; each click cycles the priority upward (Low → Medium → High → None). Todos with no priority show a faint flag icon that sets the priority to Low on click, completing the cycle from the other end (None → Low). The `SetPriorityHandler` is a focused new handler that updates only the `Priority` column; no database migration is needed.

**Files changed:**
- `TodoApp/Features/Todos/SetPriority/SetPriorityHandler.cs` — new handler, updates Priority only
- `TodoApp/Program.cs` — registered `SetPriorityHandler`
- `TodoApp/Components/Pages/Home.razor` — clickable priority chip (`todo-priority-chip`), faint flag button (`todo-set-priority-btn`) for None-priority active todos, `NextPriority` helper, `HandleCyclePriority` method

**Tests added:** 7 unit tests (`SetPriorityHandlerTests`), 6 bUnit tests (`HomeTests`)

---

## Day 84 — [2026-07-09] — Feature: No Due Date Filter

**Description:** Adds a "No date" chip to the date filter row, letting users instantly see all active todos that have no due date assigned. The chip shows a count badge (e.g. "No date (5)") when any such todos exist. Selecting the chip filters the list to only undated, non-completed todos; clicking the "Any" chip resets back to the full list. The new `NoDueDate = 4` value extends `TodoDateFilter`, the filter case is handled in `FilterSortTodosHandler`, and `FilterCountsHandler` computes the count for the badge. No database changes are required.

**Files changed:**
- `TodoApp/Features/Todos/FilterSortTodos/FilterSortOptions.cs` — added `NoDueDate = 4` to `TodoDateFilter`
- `TodoApp/Features/Todos/FilterSortTodos/FilterSortTodosHandler.cs` — new filter case for `NoDueDate`
- `TodoApp/Features/Todos/FilterCounts/FilterCountsHandler.cs` — `NoDueDate` count added to `FilterCountsResult`
- `TodoApp/Components/Pages/Home.razor` — "No date" chip in the date filter row

**Tests added:** 4 unit tests (`FilterSortTodosHandlerTests`), 4 unit tests (`FilterCountsHandlerTests`), 4 bUnit tests (`HomeTests`)

---

## Day 83 — [2026-07-09] — Feature: Overdue Bulk-Reschedule

**Description:** The urgency banner (which appears when the current list has overdue or due-today todos) now includes two quick-action buttons when overdue todos are present: "→ Today" and "→ Tomorrow". Clicking either button bulk-updates all overdue, non-completed todos in the current list to the chosen date, dismisses the urgency banner, reloads the list, and shows a snackbar confirming how many todos were rescheduled (e.g. "Rescheduled 4 overdue todos to today"). Completed todos, future todos, and todos with no due date are never touched.

**Reason for change:** A common real-world scenario is opening the app after a few days away and finding a pile of overdue tasks that all need new dates. Previously the only option was to edit each todo individually or filter to overdue and manually reschedule one by one. A single-click bulk reschedule turns a tedious multi-step chore into a two-second cleanup that lets the user get back to actual work. The feature fits naturally inside the urgency banner — the place users already look when they see overdue items.

**Removals:** None

**Stats:**
- Lines added: 341
- Lines deleted: 0
- Tests added: 15
- Tests removed: 0
- Test failures before green: 0

## Day 82 — [2026-07-09] — Feature: URL Link Attachment

**Description:** Every todo can now have an optional URL link attached to it. A link icon button (🔗) appears in each todo's action bar; clicking it opens an inline URL editor with a text field for the URL, a Save button, a "Remove link" button (shown only when a URL is already set), and a Cancel button. Once saved, the URL is displayed as a compact clickable link in the todo body showing the hostname and path (e.g. `github.com/org/repo/pull/1`) — clicking it opens the URL in a new tab. The link icon button turns blue when a URL is attached, giving a clear visual indicator. URLs are persisted to SQLite via a new `Url` column on the `Todos` table (migration with ignore-if-exists guard).

**Reason for change:** Task management tools in real workflows are rarely self-contained — a todo often corresponds to a Jira ticket, a GitHub PR, a Confluence doc, a Figma design, or a reference URL. Without a link field, users must either embed long URLs in the todo title (ugly and hard to click) or keep the reference in a separate note (fragmented). A first-class URL field makes todos actionable: one click goes directly to the source of truth. It completes a natural gap in the todo model alongside notes, tags, and color labels.

**Removals:** None

**Stats:**
- Lines added: 441
- Lines deleted: 4
- Tests added: 18
- Tests removed: 0
- Test failures before green: 0

## Day 81 — [2026-07-08] — Feature: JSON Import

**Description:** A new "Import from JSON" button (the `[]` data-array icon, after the Markdown import) lets users load a `.json` file and import todos from it. The parser reads the envelope format produced by the JSON Export handler (Day 79): it deserialises the `Todos` array and inserts each item into the active list, preserving `Title`, `IsCompleted`, `Priority` (high/medium/low/none), `DueDate`, `CompletedAt`, `IsPinned`, `IsBlocked`, `TimeEstimate`, `ColorLabel`, `Tags`, and `Notes`. Invalid JSON throws a `JsonException` which the UI catches and surfaces as an error snackbar. An empty or missing `Todos` array returns zero and shows a warning. The feature completes a full JSON round-trip alongside the existing Markdown export→import round-trip.

**Reason for change:** Day 79 added JSON export but left no way to re-import the file. A developer who exports a list, processes it with a script, and wants to re-import the result had no path back. JSON import closes that loop: the schema is already well-defined (the export envelope), the parsing is straightforward (`System.Text.Json`), and the UI pattern mirrors the Markdown import added in Day 78. Together, export and import make JSON a fully usable interchange format for the app's data.

**Removals:** None

**Stats:**
- Lines added: 165
- Lines deleted: 0
- Tests added: 17
- Tests removed: 0
- Test failures before green: 2

## Day 80 — [2026-07-07] — Feature: List Archiving

**Description:** Lists can now be archived instead of deleted. Each non-default list chip in the list selector shows a small archive icon (opacity 0.5) on hover; clicking it archives the list and removes it from the active selector immediately. Archived lists are surfaced via a collapsible "Archived (N)" toggle below the chip row. Expanding it shows each archived list with two action buttons: Unarchive (restores the list to the active selector with all its todos intact) and Delete (permanently removes it via the existing delete flow). The default "Personal" list cannot be archived. The `TodoLists` table gains an `IsArchived` column (migration via `ALTER TABLE … ADD COLUMN`, ignored if already present) and `GetListsHandler` now filters to `WHERE IsArchived = 0`.

**Reason for change:** The app previously only offered permanent deletion for lists. Users who want to put seasonal or completed projects aside — without losing todos or breaking their workflow — had no lighter-weight option. Archiving fills that gap: it's a reversible soft-delete that keeps the list and all its todos available for later retrieval.

**Removals:** None

**Stats:**
- Lines added: 210
- Lines deleted: 3
- Tests added: 14
- Tests removed: 0
- Test failures before green: 1

## Day 79 — [2026-07-07] — Feature: JSON Export

**Description:** A new "Export to JSON" button (the `{}` data-object icon, between the Markdown export and the CSV import buttons) downloads the current list as a structured `.json` file. The file contains a top-level envelope with the list name, export date, and a count summary (total / active / completed), plus a `Todos` array where each item carries every field: `Id`, `Title`, `IsCompleted`, `Priority` (as a lowercase string), `DueDate`, `CompletedAt`, `CreatedAt`, `IsPinned`, `IsBlocked`, `TimeEstimate`, `ColorLabel`, and a `Tags` array of tag name strings. Null and default-value fields (`DueDate`, `Notes`, `TimeEstimate`, `ColorLabel`, `CompletedAt`) are omitted from the output to keep the JSON clean.

**Reason for change:** The app already offers CSV (for spreadsheets) and Markdown (for docs/notes tools) exports. JSON completes the export story for developers and power users: it provides structured, machine-readable data that can be consumed by scripts, piped into `jq`, imported into a database, or processed by any language without a CSV parser. The schema is self-describing — field names carry meaning — making it useful for long-term archival where the format needs to be interpretable without context.

**Removals:** None

**Stats:**
- Lines added: 355
- Lines deleted: 0
- Tests added: 13
- Tests removed: 0
- Test failures before green: 0

## Day 78 — [2026-07-07] — Feature: Import from Markdown

**Description:** A new "Import from Markdown" button (the upload-file icon, next to the existing CSV import) lets users load a `.md` file and import todos from it. The parser recognises `- [ ] Title` lines as active todos and `- [x] ~~Title~~` lines as completed todos, both in the exact format produced by the Markdown export (Day 76). Metadata in the trailing `_(...)_` block is parsed for priority (high/medium/low), due date (`due YYYY-MM-DD`, `overdue YYYY-MM-DD`, `due today`), and tags (`#tagname`). Non-todo lines (headings, blank lines, the export date line) are silently ignored. Imported todos land in the currently active list. A success snackbar confirms how many todos were imported, and a warning appears if no valid todo lines were found.

**Reason for change:** The Markdown export (Day 76) introduced a human-readable export format but there was no way to re-import it. Closing this round-trip means users can export a list, edit it in any text editor or markdown tool (Obsidian, Notion, a Git repo), and import the result back without data loss. Priority, due date, and tag metadata survive the round-trip because the import parser understands the same `_(...)_` convention the exporter writes.

**Removals:** None

**Stats:**
- Lines added: 490
- Lines deleted: 0
- Tests added: 19
- Tests removed: 0
- Test failures before green: 0

## Day 77 — [2026-07-05] — Feature: List Drag-to-Reorder

**Description:** Lists in the sidebar can now be reordered by dragging. Each list chip is wrapped in a draggable element; dragging one chip over another and releasing drops it into that position. The new order is persisted immediately to SQLite via a `SortOrder` column on `TodoLists`. Lists are returned from `GetListsHandler` in `SortOrder ASC, Id ASC` order, so the arrangement survives page refreshes and server restarts. The "Today" chip is fixed at the top and cannot be dragged. Newly created lists are appended at the end (`MAX(SortOrder) + 1`), so create-then-reorder works without breaking the initial ordering invariant.

**Reason for change:** Users who maintain multiple lists naturally develop a preferred order — inbox at the top, reference lists at the bottom. Without reordering, every new list lands in creation order and cannot be moved without deleting and recreating. Drag-to-reorder is the standard affordance for this (iOS Reminders, Things, Todoist all use it), and the HTML5 drag API makes it achievable without a JS library. Persisting `SortOrder` also gives a stable ordering foundation for future list features (e.g. pinning a list, hiding a list).

**Removals:** None

**Stats:**
- Lines added: 211
- Lines deleted: 2
- Tests added: 9
- Tests removed: 0
- Test failures before green: 0

## Day 76 — [2026-07-05] — Feature: Markdown Export

**Description:** A new "Export to Markdown" button (the document icon, next to the existing CSV export) downloads the current list as a `.md` file. Active todos are formatted as `- [ ] Title _(priority · due date · #tags)_` and completed todos as `- [x] ~~Title~~`. The file has an `## Active` section followed by a `## Completed` section; active todos are sorted by priority (High first) then due date; completed todos are sorted newest-completion-first. Pinned todos show a 📌 marker. Overdue dates are flagged as `overdue YYYY-MM-DD`; today's date shows as `due today`.

**Reason for change:** CSV export is useful for data analysis, but many users work in markdown-native tools — Notion, Obsidian, GitHub issues, Bear, Logseq, plain text editors. The markdown export produces a human-readable, pasteable task list that renders correctly in any of those tools without further formatting. A user finishing a sprint can copy the markdown into a GitHub issue or meeting note without any manual reformatting.

**Removals:** None

**Stats:**
- Lines added: 363
- Lines deleted: 0
- Tests added: 19
- Tests removed: 0
- Test failures before green: 0

## Day 75 — [2026-07-05] — Feature: Tag Completion Stats

**Description:** The stats panel now shows a "By tag:" breakdown row with a chip for each tag used in the current list. Each chip displays completed/total count and completion percentage — e.g. "#work 3/5 (60%)" — and a tooltip with the exact active/completed split. Tags are ordered by completion rate ascending, so the most incomplete tags appear first and demand attention. The row is hidden when no todos carry a tag, keeping the stats panel clean for simple lists. Multi-tagged todos count toward each of their tags.

**Reason for change:** The filter tag chips (Day 68) already show how many active todos exist per tag, and the priority breakdown (Day 72) shows completion rates per priority tier. But for users who use tags as project labels, there was no completion view per project. A "#work 1/8 (12%)" chip alongside "#personal 6/7 (86%)" is immediately actionable — it reveals that one area of the list is barely being processed while another is nearly done. Tag stats give a lightweight project-level health view without requiring a separate project management system.

**Removals:** None

**Stats:**
- Lines added: 335
- Lines deleted: 0
- Tests added: 16
- Tests removed: 0
- Test failures before green: 0

## Day 74 — [2026-07-05] — Feature: Today Cross-List View

**Description:** A new "Today" tab in the list selector shows all overdue and due-today todos across every list in one aggregated view. The chip displays a live count of urgent todos (e.g. "Today (3)") and turns red when items are waiting. Clicking it opens the Today panel, which groups urgent todos by their source list with the list name as a header and a "Go to list" shortcut. Each todo shows its title, an overdue or due-today chip, and a priority chip; users can complete or delete todos directly from the Today view. The Add a New Todo form is hidden while Today view is active, replaced by a clean read-only panel. A "Back to list" button or clicking any list chip exits the Today view.

**Reason for change:** Users who maintain multiple lists (e.g., Work and Personal) have no single place to see what demands their attention right now. They must switch between lists one by one to find overdue or today's items — a friction point that grows with the number of lists. The Today view solves the morning planning problem: open the app, glance at Today, see exactly what is urgent and which list it belongs to, take action.

**Removals:** None

**Stats:**
- Lines added: 547
- Lines deleted: 1
- Tests added: 21
- Tests removed: 0
- Test failures before green: 0

## Day 73 — [2026-07-04] — Feature: Blocked / Waiting For Task Flag

**Description:** Every todo now has a "blocked / waiting for" flag. A ⏸ toggle button appears in each todo row; clicking it marks the task as blocked and shows a "⏸ Waiting for" badge inline on the todo. A "Blocked:" filter chip row (below the staleness filters) lets users filter to show only blocked/waiting-for tasks — the chip label includes a live count of blocked active todos in the current list. Focus mode automatically excludes blocked todos so the focus queue only surfaces actionable items. The flag persists to the database via a new `IsBlocked` column with toggle-on-update semantics (no separate set/clear calls needed).

**Reason for change:** Task management tools commonly distinguish between tasks you can act on now and tasks stalled on an external dependency (waiting for a reply, a review, a decision, a handoff). Without this distinction every todo in the list looks equally actionable, making it impossible to know at a glance how much of the backlog is actually blocked. The blocked flag lets users park waiting-for tasks without deleting or completing them, and the focused filter view gives a quick audit of what's in limbo — useful for weekly review or when chasing up dependencies.

**Removals:** None

**Stats:**
- Lines added: 248
- Lines deleted: 7
- Tests added: 10
- Tests removed: 0
- Test failures before green: 0

## Day 72 — [2026-07-04] — Feature: Priority Completion Rates

**Description:** The stats panel now shows a "By priority:" breakdown row with a chip for each priority level (High / Medium / Low) that has at least one todo. Each chip displays completed/total count and completion percentage — e.g. "High 4/6 (67%)". The chip is coloured by priority (red/amber/blue) and includes a tooltip with the exact active/completed split. The row is hidden when no todos carry an explicit priority, keeping the stats panel uncluttered for simple lists.

**Reason for change:** The filter chip counts (Day 68) already show how many active todos exist per priority, but give no sense of completion progress within each priority tier. The new breakdown tells users whether their high-priority work is being tackled proportionally — a High completion rate of 20% while Low sits at 80% is a clear signal that priorities are inverted and worth addressing. This turns the stats panel from a simple count display into a lightweight productivity health check.

**Removals:** None

**Stats:**
- Lines added: 302
- Lines deleted: 0
- Tests added: 13
- Tests removed: 0
- Test failures before green: 0

## Day 71 — [2026-07-04] — Feature: Completion Time Analytics

**Description:** The stats panel now shows an "Avg: X days to complete" chip whenever the current list has completed todos with completion timestamps. Hovering over the chip reveals a tooltip with a priority-level breakdown — e.g., "High: 1 days · Medium: 3.5 days · Low: 9 days" — alongside the sample count used for the average. Todos completed within the same day round to 0 days; the handler clamps negative values (clock skew) to zero to keep the display clean.

**Reason for change:** The app already tracked streaks, today's completions, weekly completions, and time estimates, but had no insight into turnaround speed — how quickly todos move from creation to completion. The average completion time tells users whether their backlog is being processed briskly or accumulating lag, and the priority breakdown surfaces a concrete signal: high-priority items completed in days while low-priority ones take weeks indicates healthy prioritisation; the reverse pattern flags a problem worth addressing.

**Removals:** None

**Stats:**
- Lines added: 345
- Lines deleted: 0
- Tests added: 15
- Tests removed: 0
- Test failures before green: 0

## Day 70 — [2026-07-01] — Feature: Streak Protection Nudge Banner

**Description:** A warning banner now appears automatically when the user has an active completion streak (≥1 consecutive day) but has not yet completed any todo today. The banner shows "Your N-day streak is at risk! Complete at least 1 todo today to keep it going." with the live streak count, and includes a dismiss (×) button to hide it for the session. The banner disappears automatically the moment a todo is completed (since the activity stats are refreshed after every completion), and resets when the user switches lists so it can surface again on a fresh session. It is never shown when there is no streak or when at least one todo has already been completed today.

**Reason for change:** The app already tracked streaks and showed the current count in the stats panel, but there was no proactive signal when a streak was in danger. A user who opens the app, scans their list, and closes it without completing anything would silently lose their streak — an outcome that is easy to prevent with a one-time nudge. The banner follows the same proactive pattern as the overdue urgency banner (Day 67), adding just enough friction to prompt action without being intrusive.

**Removals:** None

**Stats:**
- Lines added: 55
- Lines deleted: 0
- Tests added: 9
- Tests removed: 0

## Day 69 — [2026-07-01] — Feature: Recommended Smart Sort Order

**Description:** Added a new "Recommended" sort option that ranks todos by composite urgency — overdue todos appear first (most overdue at the top), followed by todos due today, then due this week, then everything else. Within each urgency bucket, todos are ranked by priority (High → Medium → Low → None). Pinned todos always stay first regardless of urgency. Section headers ("Overdue", "Due today", "Due this week") are shown for items that have a due date in those categories, giving at-a-glance context about why items appear at the top. The option is listed first in the sort dropdown as the most actionable default.

**Reason for change:** The existing sort options require the user to know what they're looking for — "Due date" shows earliest due first but doesn't account for priority, and "Priority" ignores when things are due. Neither answers the real question: "what should I work on right now?" Recommended solves this with a composite urgency score that combines deadline imminence and priority, surfacing the most pressing work without requiring the user to apply multiple filters manually.

**Removals:** None

**Stats:**
- Lines added: 85
- Lines deleted: 1
- Tests added: 9
- Tests removed: 0

## Day 68 — [2026-07-01] — Feature: Filter Chip Active Counts

**Description:** Priority filter chips and tag filter chips now show the count of active (non-completed) todos for that value alongside the label — e.g. `High (3)`, `Medium (1)`, `work (5)`. Counts only appear when non-zero, keeping the UI uncluttered when a priority or tag has no active items. Completed todos are excluded from all counts so the numbers reflect actionable work. The counts update immediately when todos are added, completed, or deleted, and reset correctly when switching lists.

**Reason for change:** Filter chips were passive selectors with no hint of what applying each filter would show. A user looking at a list with many todos had to click each priority or tag chip to discover how many items fell into it. Adding counts turns the filter row into a lightweight dashboard — users can see at a glance that "High (4)" demands attention, or that "shopping (0)" can be skipped, without clicking anything.

**Removals:** None

**Stats:**
- Lines added: 165
- Lines deleted: 2
- Tests added: 13
- Tests removed: 0
- Test failures before green: 0

## Day 67 — [2026-07-01] — Feature: Overdue & Due-Today Urgency Banner

**Description:** A red alert banner now appears automatically below the filter panel whenever the current list has overdue or due-today active tasks and the date filter is not already set. The banner shows distinct counts — "2 overdue" and/or "3 due today" — each paired with a one-click "Show overdue" or "Show today's" button that applies the corresponding date filter and dismisses the banner. A dismiss (×) button hides the banner for the current session without changing the filter. The banner is invisible once the date filter is active (no duplicate signal) and resets when the user switches lists or clears all filters, so it resurfaces if needed.

**Reason for change:** Overdue tasks were surfaced only passively — a small "Overdue: N" chip in the stats panel and filter options the user had to actively seek out. Users who are not looking at the stats panel can easily miss urgent tasks. The banner is proactive: it appears at the top of the list and demands a decision (show them, or explicitly dismiss), making it much harder to unknowingly skip overdue and same-day work.

**Removals:** None

**Stats:**
- Lines added: 240
- Lines deleted: 0
- Tests added: 18
- Tests removed: 0
- Test failures before green: 2

## Day 66 — [2026-07-01] — Feature: Pick for Me (Random Task Picker)

**Description:** A "Pick for me" button appears in the filter bar whenever there is at least one active (non-completed) todo in the current view. Clicking it randomly selects one active todo and shows a "You picked: [title]" banner above the list, while also applying a coloured outline highlight to the selected item in the list so it is easy to spot. The banner includes a "Pick another" button that picks a different task (excluding the current pick to avoid repeating the same item when two or more are available), and a dismiss button to clear the selection. The picked item resets when the user switches lists.

**Reason for change:** With 65+ features in the app, users who have a long active todo list can suffer from decision paralysis — many items to choose from, no clear "next" signal. The random picker removes the need to decide: one click and the app nominates a task. It complements Focus mode (which is rules-based) with a purely random alternative, useful for clearing backlogs when there is no urgent item.

**Removals:** None

**Stats:**
- Lines added: 230
- Lines deleted: 2
- Tests added: 15
- Tests removed: 0
- Test failures before green: 1

## Day 65 — [2026-06-30] — Feature: Task Age Indicator & Stale Task Filter

**Description:** Active todos that have been sitting uncompleted for 7 or more days now show a subtle age badge — "1 wk old" (amber) for tasks 7–13 days old, "2+ wks old" (amber) for 14–29 days, and "1+ mo old" (red) for tasks 30+ days old. The badge includes a tooltip showing the exact number of days and a nudge to tackle the item. A new "Age:" filter row in the filter panel lets users narrow the list to tasks that are 1+ week, 2+ weeks, or 1+ month old, making it easy to surface and review neglected work. Completed todos are never badged or matched by the staleness filter, and the filter resets when switching lists or clearing all filters.

**Reason for change:** The app tracks due dates but had no awareness of how long an active task had been sitting in the list without progress. A task with no due date can lurk indefinitely — the age indicator makes procrastinated tasks visible at a glance, and the staleness filter lets users run a deliberate "stale task review" session to clear out or reprioritize long-ignored items.

**Removals:** None

**Stats:**
- Lines added: 308
- Lines deleted: 4
- Tests added: 14
- Tests removed: 0
- Test failures before green: 0

## Day 64 — [2026-06-29] — Feature: Sort by Time Estimate

**Description:** Two new sort options — "Shortest first" and "Longest first" — appear in the Sort dropdown alongside the existing seven sort orders. "Shortest first" surfaces quick-win tasks by ordering todos from least to most estimated time, with unestimated todos sorted to the end. "Longest first" reverses this, putting the most time-intensive tasks at the top. Both options still respect pinned items (pinned todos always sort first) and combine correctly with all existing filters.

**Reason for change:** The app already lets users filter by time estimate (≤15 min, ≤30 min, etc.) and shows total estimated time in the stats panel, but sorting by estimate wasn't possible. The two most common estimate-driven workflows — "what can I knock out quickly?" (shortest first) and "what big task should I tackle now?" (longest first) — were left to manual scanning. These sort orders complete the time-estimate story.

**Removals:** None

**Stats:**
- Lines added: 174
- Lines deleted: 4
- Tests added: 9
- Tests removed: 0
- Test failures before green: 2

## Day 63 — [2026-06-29] — Feature: Weekly Summary Copy

**Description:** A "Copy weekly summary" button appears in the stats panel whenever at least one todo was completed in the past 7 days. Clicking it copies a plain-text bullet list to the clipboard — "Completed this week (N):" followed by one "• Title" line per item, ordered most-recent first — and shows a snackbar confirming how many items were copied. The button is hidden when no completions exist in the window, keeping the UI clean on inactive weeks.

**Reason for change:** Users already had the activity stats panel showing how many items they'd completed this week, but no way to share or save that list. The weekly summary makes it trivial to paste a done-list into a standup message, a personal log, or a weekly review doc. The feature reuses the existing `_todos` list already in memory and requires no database round-trip, so it adds capability with minimal overhead.

**Removals:** None

**Stats:**
- Lines added: 250
- Lines deleted: 0
- Tests added: 13
- Tests removed: 0
- Test failures before green: 0

## Day 62 — [2026-06-29] — Feature: Daily Completion Goal

**Description:** Users can now set a daily completion target. A "Set daily goal" button appears in the stats panel; clicking it opens an inline numeric input where the user enters their target count. Once set, a goal chip shows real-time progress — "🎯 3 / 5 today" — and turns green with a checkmark when the goal is reached. Clicking the chip re-opens the editor to change the target, with a "Remove" option to clear it. The goal persists across sessions.

**Reason for change:** The stats panel already tracks completions-today and streak data, but there was no way for users to declare what "enough" looks like on a given day. A daily goal turns the passive counter into an active motivator — the kind of lightweight gamification that makes a simple todo list feel like a productivity system without adding complexity.

**Removals:** None

**Stats:**
- Lines added: 365
- Lines deleted: 0
- Tests added: 12
- Tests removed: 0
- Test failures before green: 0

## Day 61 — [2026-06-29] — Feature: Search Within Notes

**Description:** The todo search now searches notes content in addition to titles. When a search query matches a todo's notes but not its title, the todo appears in results with a "Matched in notes" chip indicator, making it clear why the item was included. The search field label was updated to "Search todos and notes..." to communicate the expanded scope. This makes notes genuinely discoverable — users can now find a todo by remembering any detail they wrote in its notes, not just its title.

**Reason for change:** The search has always searched only titles, but many todos have rich notes with details that are useful for retrieval. A user who writes "Include Q3 revenue figures" in a todo's notes couldn't search for "revenue" to find it. Extending search to notes turns the notes field into a proper memory tool, not just decoration.

**Removals:** None

**Stats:**
- Lines added: 194
- Lines deleted: 4
- Tests added: 11
- Tests removed: 0
- Test failures before green: 0

## Day 60 — [2026-06-25] — Feature: Todo Templates

**Description:** Users can now save a named template from the current add-form configuration (priority, time estimate, and recurrence) and reuse it with one click. When any of those three fields is set to a non-default value, a "Save as template" button appears; clicking it opens an inline name input and pressing Enter (or clicking ✓) saves the template. Saved templates appear as teal chips above the "Add Todo" button. Clicking a template chip pre-fills the priority, time estimate, and recurrence fields instantly, reducing repetitive setup for recurring task types. Each chip has an × button to delete it. Templates persist across sessions.

**Reason for change:** Recurring todos auto-create copies on a fixed schedule, but many workflows involve creating the same type of task on demand rather than automatically — e.g., a "Bug fix" task always with High priority and a 1-hour estimate, or a "Weekly review" with Medium priority and a 2-hour block. Previously, users had to re-select these values from scratch every time. Templates make any repeating form configuration one-click to apply.

**Removals:** None

**Stats:**
- Lines added: 531
- Lines deleted: 0
- Tests added: 19
- Tests removed: 0
- Test failures before green: 1

## Day 59 — [2026-06-24] — Feature: Multi-line Quick Add

**Description:** A "Paste multiple" toggle button in the "Add a New Todo" card switches the input to multi-add mode. In this mode a textarea appears where users type or paste one todo per line. A live preview shows how many todos will be created ("3 todos to add"), and clicking "Add 3 Todos" creates them all at once in the active list. Toggling back exits multi-add mode and returns to the normal single-add form.

**Reason for change:** Users often have a burst of tasks to capture — a project kick-off list, a grocery run, notes from a meeting. The single-add form requires a separate submit for each one. Multi-line quick-add lets users dump an entire list at once, making the capture experience dramatically faster for batch creation.

**Removals:** None

**Stats:**
- Lines added: 366
- Lines deleted: 17
- Tests added: 14
- Tests removed: 0
- Test failures before green: 2

## Day 58 — [2026-06-24] — Feature: Saved Filter Presets

**Description:** Users can now save the current filter state as a named preset and restore it with one click. When any filter is active, a bookmark icon appears in the filter panel — clicking it opens an inline name input; pressing Enter or clicking the check saves the preset. Saved presets appear as chips below the filter rows and persist across sessions. Clicking a preset chip applies all its saved filters (status, priority, due date, tag, color, time estimate, and sort order) at once. Each preset chip has an × button to delete it permanently.

**Reason for change:** As the filter panel grew to six independent filter rows plus search, returning to a frequently-used filter combination (e.g., "Active high-priority work items due this week") required resetting and reapplying filters by hand every time. Saved presets give that combination a name and make it one-click to restore, completing the filtering story.

**Removals:** None

**Stats:**
- Lines added: 491
- Lines deleted: 0
- Tests added: 15
- Tests removed: 0
- Test failures before green: 2

## Day 57 — [2026-06-23] — Feature: Bulk Color Label Assignment

**Description:** The bulk action bar in select mode now includes a color dot palette and "Set Color" button. After selecting any number of todos, users click a color swatch (or the grey "none" dot to clear) and then "Set Color" to apply that color label to all selected todos at once. A snackbar confirms how many todos were updated. The bulk color controls sit alongside the existing complete, move, tag, priority, due date, time estimate, and delete bulk actions.

**Reason for change:** Every other todo field — priority, due date, time estimate, tags — already had a bulk counterpart in the action bar. Color labels were the last remaining field with no bulk assignment. When color-coding a batch of related todos (e.g., marking a sprint's tasks in blue, or clearing a set of ad-hoc red flags), having to open each item's color picker individually was tedious. Bulk color assignment closes this gap and makes the bulk editing story complete.

**Removals:** None

**Stats:**
- Lines added: 202
- Lines deleted: 0
- Tests added: 9
- Tests removed: 0
- Test failures before green: 0

## Day 56 — [2026-06-22] — Feature: Clear All Filters Button

**Description:** When any filter is active — status (Active/Completed), priority, due date, tag, color label, time estimate, or search text — a "Clear all filters" button now appears at the bottom of the filter panel. Clicking it resets all filters to their defaults in one action, returning the full unfiltered todo list. The button disappears automatically once all filters are cleared.

**Reason for change:** As the filter panel grew (it now has 6 independent filter rows plus search), resetting the view required clicking each filter individually. With many filters potentially active at once, getting back to the full list was tedious. A single "clear all" action completes the filtering story by making it just as easy to exit a filtered view as it is to enter one.

**Removals:** None

**Stats:**
- Lines added: 122
- Lines deleted: 0
- Tests added: 5
- Tests removed: 0
- Test failures before green: 1

## Day 55 — [2026-06-22] — Feature: Bulk Time Estimate Assignment

**Description:** The bulk action bar in select mode now includes a time estimate dropdown and "Set Time" button. After selecting any number of todos, users pick a time estimate (No estimate, 15 min, 30 min, 1 hour, 2 hours, 4 hours, or 1 day) and click "Set Time" to apply it to all selected todos at once. A snackbar confirms how many todos were updated. The bulk time estimate controls sit alongside the existing complete, move, tag, priority, due date, and delete bulk actions.

**Reason for change:** Bulk priority, tag, move, and due date assignment were all available in the bulk action bar. Time estimate was the one remaining field a user could set on a todo that had no bulk counterpart. When planning a sprint or estimating a batch of work items, being able to stamp a time estimate on a whole selection is far faster than editing each todo individually.

**Removals:** None

**Stats:**
- Lines added: 188
- Lines deleted: 0
- Tests added: 8
- Tests removed: 0
- Test failures before green: 0

## Day 54 — [2026-06-21] — Feature: Bulk Due Date Assignment

**Description:** The bulk action bar in select mode now includes a date picker and "Set Date" button. After selecting any number of todos, users pick a date and click "Set Date" to apply it to all selected todos at once. A "Clear date" button also appears once a date is chosen in the picker, allowing users to bulk-remove due dates from the selected set. A snackbar confirms how many todos were updated. The bulk due date controls sit alongside the existing tag, priority, move, complete, and delete bulk actions.

**Reason for change:** Bulk priority, tag, and move assignment were already in the bulk action bar, but there was no way to set or clear due dates in bulk. A common scenario is planning a sprint or work session: you select a set of todos and want to stamp them all with a due date — previously that required editing each todo individually.

**Removals:** None

**Stats:**
- Lines added: 211
- Lines deleted: 0
- Tests added: 7
- Tests removed: 0
- Test failures before green: 2

## Day 53 — [2026-06-21] — Feature: Clickable Tag Chips on Todos Filter by Tag

**Description:** Clicking the body of any tag chip on a todo item now activates the tag filter for that tag — instantly narrowing the visible list to todos sharing that tag. Clicking the same tag again clears the filter (toggle behavior). While a tag filter is active, that chip renders in its filled/primary style so users can see which tag is active. The close × button still removes the tag as before; only clicking the chip body triggers the filter.

**Reason for change:** The tag filter panel at the top requires the user to scroll up and find the right tag chip. When you spot a useful tag on a todo mid-list — e.g., "work" or "urgent" — clicking it directly to narrow the view is the most natural interaction. This closes a discoverability gap where the data (tag chips on todos) and the action (tag filter) were visually disconnected.

**Removals:** None

**Stats:**
- Lines added: 114
- Lines deleted: 3
- Tests added: 4
- Tests removed: 0
- Test failures before green: 0

## Day 52 — [2026-06-21] — Feature: Active Todo Count Badges on List Tabs

**Description:** Each list tab chip now shows a small pill badge with the count of active (incomplete) todos in that list. For example, a "Work" list with 5 incomplete todos displays "Work 5". The badge only appears when the count is greater than zero — a list with all todos completed, or an empty list, shows just the name. Also fixed a long-standing bug where `app.css` was never linked in `App.razor`, meaning all accumulated custom CSS (drag-over highlights, heatmap day styles, shortcut-key chips, etc.) was dead — that stylesheet is now correctly included.

**Reason for change:** With multiple lists, users had no way to see at a glance which lists had outstanding work — they had to switch to each list to check. The count badge makes list state visible without clicking, so users can prioritise which list to focus on first.

**Removals:** None

**Stats:**
- Lines added: 113
- Lines deleted: 0
- Tests added: 4
- Tests removed: 0
- Test failures before green: 1

## Day 51 — [2026-06-21] — Feature: Alphabetical Sort (A→Z and Z→A)

**Description:** The sort dropdown now offers two new options: "Title A→Z" and "Title Z→A". Selecting either sorts all displayed todos alphabetically by title in a case-insensitive manner. Pinned todos still float to the top of the list regardless of the chosen alpha sort order, and the alphabetical sort composes correctly with all existing filters (status, priority, date, tag, color, time estimate).

**Reason for change:** As the todo list grows, users often want to scan for a specific item by name without using search — for example, to quickly jump to all "Buy ..." tasks or find a project by its first word. Alphabetical sort makes the list scannable in a predictable way that search and the other sorts (newest, priority, due date) do not provide.

**Removals:** None

**Stats:**
- Lines added: 170
- Lines deleted: 1
- Tests added: 9
- Tests removed: 0
- Test failures before green: 1

## Day 50 — [2026-06-20] — Feature: Select All / Deselect All in Select Mode

**Description:** When select mode is active, a "Select all" button now appears next to the Cancel button. Clicking it instantly selects every todo currently visible in the list (respecting all active filters). Once all todos are selected the button becomes "Deselect all", which clears the entire selection in one click. The bulk action bar still shows the live count of selected items and all existing bulk operations (complete, delete, move, tag, set priority) work seamlessly with the mass-selected set.

**Reason for change:** Bulk operations existed for all the major actions, but selecting todos one-by-one was tedious when the goal was to act on the whole visible set — for example, marking every item in a filtered view complete, or deleting everything in a list. Select All / Deselect All makes those flows instant.

**Removals:** None

**Stats:**
- Lines added: 151
- Lines deleted: 0
- Tests added: 5
- Tests removed: 0
- Test failures before green: 1

## Day 49 — [2026-06-20] — Feature: Bulk Priority Assignment

**Description:** In select mode, the bulk action bar now includes a priority dropdown and a "Set Priority" button. After selecting any number of todos, users choose a priority level (High, Medium, Low, or None) and click "Set Priority" to apply it to all selected todos at once. The selector resets after each successful operation and a snackbar confirms how many todos were updated. The bulk action bar now displays the priority controls alongside the existing tag, move, complete, and delete actions.

**Reason for change:** Bulk complete, delete, move, and tag assignment were already available in select mode, but there was no way to reprioritize multiple todos at once. After a review session — identifying which tasks are really urgent — being able to stamp a priority on a whole selection in one click is far faster than editing each todo individually.

**Removals:** None

**Stats:**
- Lines added: 220
- Lines deleted: 0
- Tests added: 8
- Tests removed: 0
- Test failures before green: 1

## Day 48 — [2026-06-19] — Feature: Time Estimate Filter

**Description:** The filter panel now includes a "Time:" filter row with six chips: Any, No estimate, ≤15 min, ≤30 min, ≤1 hour, and ≤2 hours. Selecting a chip narrows the visible todo list to only those whose time estimate falls within the chosen budget. "No estimate" shows todos without any estimate set. The filter composes with all other filters (status, priority, date, tag, color) and resets when switching lists.

**Reason for change:** Todos already carry a time estimate badge, but there was no way to filter by estimate. A common real-world need is "what can I finish in the next 30 minutes?" — this filter answers that instantly, turning the estimate data into a usable task-selection tool.

**Removals:** None

**Stats:**
- Lines added: 299
- Lines deleted: 6
- Tests added: 13
- Tests removed: 0
- Test failures before green: 0

## Day 47 — [2026-06-18] — Feature: Grouped List View with Section Headers

**Description:** When sorting by Priority or Due Date, the todo list now shows subtle section headers that divide todos into named groups. Priority sort shows "High priority", "Medium priority", "Low priority", and "No priority" groups. Due date sort shows "Overdue", "Today", "Tomorrow", "This week", "Later", and "No date" groups. Pinned todos always appear in a "Pinned" group at the top when grouping is active. Headers use an overline typographic style with a thin divider line above each group. All other sort orders (Newest, Oldest, Custom) show no headers — the list renders as a plain undivided sequence as before.

**Reason for change:** With dozens of todos sorted by priority or due date, it's hard to see at a glance where one tier ends and the next begins. Section headers give the sorted list visual structure, letting users scan to the right group instantly rather than reading individual items to find the boundary.

**Removals:** None

**Stats:**
- Lines added: 162
- Lines deleted: 0
- Tests added: 7
- Tests removed: 0
- Test failures before green: 0

## Day 46 — [2026-06-18] — Feature: Bulk Tag Assignment

**Description:** In select mode, the bulk action bar now includes a tag name input and "Tag" button. After selecting any number of todos, users can type a tag name and click "Tag" to apply that tag to all selected todos at once. The tag is normalized to lowercase (matching the per-todo tagging behavior), duplicates are silently skipped, and the input clears after each successful operation. A snackbar confirms how many todos were tagged and with which tag. The tag filter panel and per-todo tag chips both update immediately.

**Reason for change:** Bulk complete, delete, and move-to-list were already available in select mode, but there was no way to tag multiple todos at once. Tagging is a common organizational operation — after importing a batch of todos or reviewing a large list, assigning a shared tag ("work", "urgent", "q3") to a group is far faster in bulk than one-by-one.

**Removals:** None

**Stats:**
- Lines added: 232
- Lines deleted: 0
- Tests added: 9
- Tests removed: 0
- Test failures before green: 1

## Day 45 — [2026-06-17] — Feature: Quick Due Date Editing

**Description:** Todos can now have their due date changed directly from the list view without opening the full edit form. Todos without a due date show a "Set date" button inline; clicking it reveals a compact date picker with Save and Cancel buttons. Todos that already have a due date show a clickable due date chip — clicking it opens the same editor with an extra "Remove date" button to clear the date entirely. The editor closes automatically after saving or clearing, and the list updates immediately to reflect the new due date display.

**Reason for change:** Changing a due date previously required opening the full inline edit form (pencil button), which also exposes priority and time estimate controls. For a quick reschedule — bumping a task from today to tomorrow — that's unnecessary friction. A dedicated quick-edit path on the due date itself makes the most common date adjustment a single click.

**Removals:** None

**Stats:**
- Lines added: 279
- Lines deleted: 2
- Tests added: 12
- Tests removed: 0
- Test failures before green: 189

## Day 44 — [2026-06-16] — Feature: Rename Tag

**Description:** Users can now rename a tag across all todos at once by double-clicking any tag chip in the filter panel. Double-clicking a tag chip replaces it with a small text input pre-filled with the current tag name. Pressing Enter saves the rename — every todo that carried the old name is updated to the new name in one operation. Pressing Escape cancels without changes. If a todo already has the new name, the old-name tag is removed rather than duplicated. The active tag filter updates automatically if it pointed to the renamed tag.

**Reason for change:** Tags could be added and filtered but never renamed. A typo in a frequently-used tag (e.g., "wrok" instead of "work") required manually removing it from every todo and re-adding the correct version. Rename-in-place fixes the whole set at once, which is the natural complement to the autocomplete added on Day 43.

**Removals:** None

**Stats:**
- Lines added: 330
- Lines deleted: 6
- Tests added: 11
- Tests removed: 0
- Test failures before green: 0

## Day 43 — [2026-06-15] — Feature: Tag Autocomplete

**Description:** When a user starts typing in the tag input field on a todo, a dropdown appears beneath the field showing existing tag names that start with the typed prefix. Suggestions exclude tags already on that todo and exact matches (since pressing Enter saves the tag as-is). Clicking any suggestion instantly applies that tag and closes the input. Suggestions are sourced from all tags in the database — not just visible todos — so tags from other lists or completed todos are always available for reuse. The dropdown shows up to 5 matches and disappears when the input is empty.

**Reason for change:** Without autocomplete, users had to remember exact tag names to maintain consistency. Minor variations ("Work", "work", "work-related") would create fragmented tags that couldn't be filtered together. Autocomplete makes reuse the path of least resistance, which is essential for a tagging system to stay useful over time.

**Removals:** None

**Stats:**
- Lines added: 256
- Lines deleted: 7
- Tests added: 8
- Tests removed: 0
- Test failures before green: 4

## Day 42 — [2026-06-15] — Feature: Subtask Editing

**Description:** Subtask titles can now be edited inline. Each subtask row gains a pencil (edit) icon button; clicking it replaces the title with a text field pre-filled with the current title. The user can type a new name, then press Enter or click the checkmark to save, or press Escape or click × to cancel. Double-clicking the subtask title also activates edit mode. Empty titles are rejected silently (the edit is cancelled). This closes the last gap in the subtask feature — previously the only way to fix a typo was to delete and re-add the subtask.

**Reason for change:** Subtasks already support add, complete, and delete, but not rename. Fixing a misspelled subtask title required deleting it and creating a new one, losing its completion state. Inline editing is the natural complement to the existing subtask CRUD, and it follows the same inline-edit pattern already used for todo titles.

**Removals:** None

**Stats:**
- Lines added: 320
- Lines deleted: 10
- Tests added: 10
- Tests removed: 0
- Test failures before green: 0

## Day 41 — [2026-06-14] — Feature: 14-Day Activity Heatmap

**Description:** The stats panel now shows a 14-day activity heatmap — a row of 14 small squares, one per day, spanning the past two weeks. Each square's color indicates how many todos were completed on that day: gray = 0, light green = 1–2, medium green = 3–4, dark green = 5+. Today's square gets a colored border to make it easy to locate. Hovering any square shows a tooltip with the exact date and count (e.g., "Jun 12: 3 completed"). The heatmap sits below the streak/count chips in the stats panel and gives users an instant visual sense of their recent productivity rhythm.

**Reason for change:** The app already tracked completion timestamps and computed streaks and weekly counts, but only surfaced those as text chips. The heatmap makes the same data visual and spatial — you can see at a glance which days were productive and whether there are gaps in your habit. It's the lightest possible analytics enhancement without requiring a separate dashboard page.

**Removals:** None

**Stats:**
- Lines added: 215
- Lines deleted: 2
- Tests added: 10
- Tests removed: 0
- Test failures before green: 0

## Day 40 — [2026-06-14] — Feature: Bulk Move to List

**Description:** In select mode, a "Move to list" dropdown and "Move" button now appear in the bulk action bar alongside "Mark Complete" and "Delete". When multiple lists exist, users can select any number of todos and move them all to a different list in one action. The dropdown lists every list except the current active one. After moving, select mode exits automatically and a snackbar confirms how many todos were moved and to which list. The button is hidden when only a single list exists.

**Reason for change:** Bulk complete and bulk delete were already available in select mode, but bulk move was missing — yet moving a group of tasks between lists (e.g., from Personal to Work after a planning session) is a common multi-item operation. This completes the bulk operations feature set for multi-list users.

**Removals:** None

**Stats:**
- Lines added: 215
- Lines deleted: 1
- Tests added: 7
- Tests removed: 0
- Test failures before green: 2

## Day 39 — [2026-06-13] — Feature: Color Labels

**Description:** Todos can now be assigned a color label — Red, Orange, Yellow, Green, Blue, or Purple. A 🎨 palette button in each todo's action row opens an inline color picker; selecting a color immediately applies a colored left-border stripe to the todo row. A color filter row in the filter panel lets users narrow the list to todos of a specific color (or clear it to show all). The duplicate action copies the color label. Colors default to None (no border) for all existing and new todos.

**Reason for change:** Tags and priority give textual/ordinal organization, but many users think visually. Color labels provide a fast, at-a-glance organizational layer — you can color-code by project, urgency, context, or any personal scheme without naming it.

**Removals:** None

**Stats:**
- Lines added: 301
- Lines deleted: 11
- Tests added: 16
- Tests removed: 0
- Test failures before green: 1

## Day 38 — [2026-06-13] — Feature: Markdown Notes

**Description:** Todo notes now render as Markdown. When not editing, notes are displayed as formatted HTML — **bold**, *italic*, bullet lists, headings, and auto-linked URLs all work. A small "Markdown supported" hint appears in the notes editor so users know the syntax is available. Raw HTML tags in notes are stripped to prevent injection. The editing experience is unchanged — users write plain text and see the formatted result when they save.

**Reason for change:** Notes are currently displayed verbatim as a single text blob, which means any formatting is invisible. Markdown is a natural fit: it's plain text when editing and rich text when reading, which matches exactly how the notes editor already works (type to edit, click away to view).

**Removals:** None

**Stats:**
- Lines added: 196
- Lines deleted: 3
- Tests added: 15
- Tests removed: 0
- Test failures before green: 0

## Day 37 — [2026-06-13] — Feature: Completion Streaks

**Description:** The stats panel now shows productivity streak information. Completing any todo starts (or extends) a streak — a count of consecutive calendar days on which at least one todo was completed. A "🔥 N day streak" chip appears in the stats panel when a streak is active; a "Best: N days" chip shows the all-time longest streak when it exceeds the current one. Two additional chips show how many todos were completed today and this week. All stats update live whenever a todo is completed or uncompleted.

**Reason for change:** The app already tracked exact completion timestamps (added Day 32) but never surfaced any productivity patterns derived from them. Streaks are a natural and motivating way to make that data visible — they reward consistency and give users a quick sense of their recent productivity without requiring a full analytics dashboard.

**Removals:** None

**Stats:**
- Lines added: 387
- Lines deleted: 0
- Tests added: 17
- Tests removed: 0
- Test failures before green: 1

## Day 36 — [2026-06-13] — Feature: Duplicate Todo

**Description:** Every todo now has a duplicate button (⧉ copy icon) in its action row. Clicking it instantly creates a copy of the todo with all user-authored attributes preserved — title (suffixed with " (copy)"), priority, due date, time estimate, notes, recurrence rule, and list. The copy starts with a clean slate: not completed, not pinned, no completion timestamp. A snackbar appears with an Undo button that deletes the copy if clicked. The duplicate button appears alongside the existing pin, notes, move, and delete buttons.

**Reason for change:** Users frequently want to create a new task that is similar to an existing one — the same recurring task with a different due date, or the same task for a different project. Manually re-entering all fields is tedious. Duplicate-and-edit is the fastest path for this common pattern.

**Removals:** None

**Stats:**
- Lines added: 320
- Lines deleted: 0
- Tests added: 14
- Tests removed: 0
- Test failures before green: 2

## Day 35 — [2026-06-11] — Feature: Focus Mode

**Description:** A "Focus" toggle button now appears in the filter toolbar. Clicking it activates Focus Mode: the list instantly narrows to only the todos that need attention right now — pinned items, high- and medium-priority tasks, overdue todos, and todos due today. A green banner appears above the filtered list showing how many todos need attention and their combined time estimate (e.g., "3 todos need attention · ~2 h 30 min"). Clicking "Exit focus" or clicking Focus again returns to the normal view. Focus Mode is automatically deactivated when switching lists.

**Reason for change:** With time estimates (Day 34), due dates, priorities, and snooze all in place, the app has everything needed to answer "what should I work on right now?" — but users still had to manually apply multiple filters to find out. Focus Mode surfaces that curated view in a single click, making the answer instant.

**Removals:** None

**Stats:**
- Lines added: 408
- Lines deleted: 0
- Tests added: 22
- Tests removed: 0
- Test failures before green: 1

## Day 34 — [2026-06-10] — Feature: Time Estimates

**Description:** Todos now support a time estimate field (No estimate, 15 min, 30 min, 1 hour, 2 hours, 4 hours, 1 day). A dropdown appears in both the "Add Todo" form and the inline edit form. Todos with an estimate display a small badge (e.g., "~1 h") next to the title. The stats panel gains a new "~X h Y min remaining" chip that sums the estimates of all active (incomplete) todos, giving users a quick sense of their total workload at a glance.

**Reason for change:** Knowing what's on your plate is core to any useful todo system, but the app had no way to express how long things take. Time estimates are the simplest bridge between a task list and a capacity plan — they let users quickly answer "how long will all of this take me?"

**Removals:** None

**Stats:**
- Lines added: 409
- Lines deleted: 24
- Tests added: 19
- Tests removed: 0
- Test failures before green: 2

## Day 33 — [2026-06-09] — Feature: Snooze / Defer Todo

**Description:** Each todo now has a snooze button (clock icon) in its action row. Clicking it replaces the button with three one-click options — **Tomorrow**, **+1 week**, and **+2 weeks** — plus a cancel (✕) button. Selecting an option sets (or updates) the todo's due date to the chosen offset from today, closes the options panel, and shows a confirmation snackbar ("Snoozed to Jun 10"). The feature works on todos that have no existing due date as well as on those that do.

**Reason for change:** Users frequently need to defer a task without going through the full edit form — snoozing is the fastest way to say "I'll deal with this later." The three preset options cover the most common deferral intervals, and the feature composes naturally with the due-date quick filters and relative-date labels already in place.

**Removals:** None

**Stats:**
- Lines added: 250
- Lines deleted: 0
- Tests added: 11
- Tests removed: 0
- Test failures before green: 0

## Day 32 — [2026-04-20] — Feature: Completion Date Tracking

**Description:** The app now records when each todo was completed. Completing a todo stamps it with a `CompletedAt` timestamp; un-completing it clears the stamp. Completed todos show a green "✓ Completed today at 3:45 PM" (or "yesterday", "N days ago", or a full date for older completions) below the creation date. Incomplete todos show nothing extra. Restored todos (via Undo) preserve their original completion timestamp.

**Reason for change:** "Completed" was a boolean with no history — you had no way to know whether you finished something an hour ago or a week ago. The completion timestamp makes the completed state meaningful and complements the due-date tracking already in place.

**Removals:** None

**Stats:**
- Lines added: 157
- Lines deleted: 9
- Tests added: 7
- Tests removed: 0
- Test failures before green: 1

## Day 31 — [2026-04-20] — Feature: Edit Priority and Due Date

**Description:** The inline edit form now lets you change a todo's priority and due date, not just its title. Clicking the edit (✏️) button expands a two-row form: the top row has the title field and save/cancel buttons (unchanged); the bottom row shows a Priority dropdown pre-filled with the current priority and a Due date picker pre-filled with the current due date (or blank if none). Saving persists all three fields atomically. The due date can be cleared by clearing the picker. Priority defaults to None if unset.

**Reason for change:** Previously editing a todo only let you fix a typo in the title — there was no way to reprioritise it or push its due date without deleting and recreating it. This was a significant usability gap, especially given the due-date quick filters added in Day 30.

**Removals:** None

**Stats:**
- Lines added: 210
- Lines deleted: 25
- Tests added: 11
- Tests removed: 0
- Test failures before green: 0

## Day 30 — [2026-04-20] — Feature: Due-Date Quick Filters

**Description:** Three new filter chips — **Overdue**, **Today**, and **This week** — appear in the filter bar below the priority filters. Clicking one instantly narrows the list to incomplete todos matching that date range: Overdue shows anything past due, Today shows items due on the current date, and This week shows items due within the next 7 days. When multiple lists exist, date filters automatically search across all lists (the same "Showing results across all lists" banner appears). Clicking **Any** resets to unfiltered. The date filter is also cleared when switching lists.

**Reason for change:** The existing filters (status, priority, tags, search) are all attribute-based but there was no way to quickly answer "what's overdue right now?" or "what do I need to do this week?" — the two most common time-sensitive questions a todo app needs to answer. Date quick-filters complete the filter bar.

**Removals:** None

**Stats:**
- Lines added: 274
- Lines deleted: 9
- Tests added: 12
- Tests removed: 0
- Test failures before green: 2

## Day 29 — [2026-04-20] — Feature: Global Search Across Lists

**Description:** The search box now automatically searches across all lists when multiple lists exist. Typing a query while on the "Personal" list will surface matching todos from "Work", "Shopping", or any other list — no need to switch lists manually. A subtle "Searching all lists" banner (with a layers icon) appears below the search field to signal the expanded scope. Todos from other lists display a small info-coloured chip showing which list they belong to, so results remain easy to contextualise. With a single list the behaviour is unchanged.

**Reason for change:** Once you have more than one list, per-list search is a real friction point — you have to remember which list a task is in before you can find it. Global search with clear provenance labels is the natural complement to the multiple-lists feature and makes the whole list system feel cohesive rather than siloed.

**Removals:** None

**Stats:**
- Lines added: 129
- Lines deleted: 3
- Tests added: 6
- Tests removed: 0
- Test failures before green: 0

## Day 28 — [2026-04-17] — Feature: Move Todo Between Lists

**Description:** Todos can now be moved from one list to another after they've been created. When multiple lists exist, a move icon (→ folder) appears on each todo row. Clicking it reveals a compact dropdown listing all other lists; selecting one immediately moves the todo to that list and reloads the view. A cancel button (✕) dismisses the picker without making any change. The move button is hidden when only a single list exists, keeping the UI uncluttered for single-list users.

**Reason for change:** Day 25 introduced multiple lists and Day 26 added renaming, but there was no way to move a todo between lists after creation. This is an obvious and frequently-needed action — adding a task to the wrong list, or reorganising work vs personal items — and without it the list feature feels incomplete.

**Removals:** None

**Stats:**
- Lines added: 269
- Lines deleted: 0
- Tests added: 10
- Tests removed: 0
- Test failures before green: 0

## Day 27 — [2026-04-16] — Feature: Drag-to-Reorder

**Description:** Todos can now be manually reordered by drag and drop. Selecting "Custom order" from the Sort dropdown switches the list to manual mode: a drag-handle icon (⠿) appears on each item, and items can be dragged up or down into any position. The order is immediately persisted to the database. Dragging over an item highlights it; releasing the mouse drops the dragged item into that slot. All other sort modes (Newest, Oldest, Due date, Priority) remain unaffected — they simply re-sort the items using those criteria. A new `SortOrder` column on the `Todos` table stores the manual position, seeded from the existing row IDs on first migration. New todos are appended at the bottom of the manual order.

**Reason for change:** Filtering and sorting by attributes (priority, date) is useful but sometimes you want an arbitrary personal order — "I'll do these three things first, in this sequence." Drag-to-reorder is the standard solution and the last major missing interaction pattern.

**Removals:** None

**Stats:**
- Lines added: 123
- Lines deleted: 7
- Tests added: 7
- Tests removed: 0
- Test failures before green: 3

## Day 26 — [2026-04-15] — Feature: Rename List

**Description:** List names are now editable. Double-clicking any list chip puts it into an inline edit mode — the chip is replaced with a compact text field pre-filled with the current name. Pressing Enter saves the new name; pressing Escape cancels without changes. All lists can be renamed, including the default "Personal" list. An empty name is rejected and the rename is silently cancelled. The feature is surfaced through a new `RenameListHandler` vertical slice.

**Reason for change:** Day 25 let users create and delete lists but not rename them — an obvious gap. Users who create a list with a typo or want to repurpose it had no recourse. Inline rename is the lowest-friction UX pattern for this.

**Removals:** None

**Stats:**
- Lines added: 194
- Lines deleted: 8
- Tests added: 8
- Tests removed: 0
- Test failures before green: 3

## Day 25 — [2026-04-15] — Feature: Multiple Lists

**Description:** Users can now organise their todos into multiple named lists. A list selector row appears at the top of the page showing all lists as chips — clicking a chip switches to that list, showing only its todos. A "+" button lets you create a new list by typing a name and pressing Enter (Escape cancels). Non-default lists show a close button on their chip to delete them; deleting a list moves its todos to the default "Personal" list rather than discarding them. New todos are always added to the currently active list. The default "Personal" list is seeded on first run and cannot be deleted.

**Reason for change:** Multiple lists are one of the most-requested features in any todo application — "Work", "Shopping", "Personal" are the classic split. Without them the list grows unbounded with no separation of concerns. This is the natural next step after subtasks, tags, and priority filtering.

**Removals:** None

**Stats:**
- Lines added: 255
- Lines deleted: 13
- Tests added: 13
- Tests removed: 0
- Test failures before green: 2

## Day 24 — [2026-04-12] — Feature: Dark Mode Persistence

**Description:** The dark mode preference now persists across page reloads. On first render, the app reads from `localStorage` via JS interop (`todoApp.getDarkModePreference`). If no stored preference exists, it falls back to the system's `prefers-color-scheme` media query — so users on dark OS themes automatically get dark mode on first visit. Toggling dark mode saves the preference to `localStorage` via `todoApp.saveDarkModePreference`. Previously the toggle reset to light mode on every page reload.

**Reason for change:** Dark mode was already toggleable but ephemeral — it reset on every reload. Persisting the preference is the minimum needed for the feature to be genuinely useful. Respecting the system color scheme on first visit is a low-cost, high-quality UX improvement.

**Removals:** None

**Stats:**
- Lines added: 59
- Lines deleted: 3
- Tests added: 2
- Tests removed: 0
- Test failures before green: 2

## Day 23 — [2026-04-12] — Feature: Natural Language Quick-Add

**Description:** The title input now understands natural language date and priority hints. Type "call dentist tomorrow !high" and the app detects the hints in real time: a small preview strip appears below the input showing "Quick-add: call dentist · Due tomorrow · High priority". When the todo is added, the hints are stripped from the title and the parsed due date and priority are applied automatically — no need to touch the dropdowns. Date keywords supported: `today`, `tomorrow`, `next week`, `in N days`. Priority shortcuts: `!high` / `!h`, `!medium` / `!med` / `!m`, `!low` / `!l`. Manual selections in the Priority dropdown or date picker always take precedence over hints.

**Reason for change:** The form already has separate fields for title, priority, and due date, but filling all three is slow for power users. Natural language parsing makes the common case (add a task with a time and urgency in one typing motion) much faster — it's inspired by tools like Things and Todoist's quick-entry.

**Removals:** None

**Stats:**
- Lines added: 152
- Lines deleted: 2
- Tests added: 24
- Tests removed: 0
- Test failures before green: 0

## Day 22 — [2026-04-12] — Feature: Recurring Todos

**Description:** Todos can now repeat on a schedule. When adding a todo, a "Repeats" dropdown lets you choose Daily, Weekly, Monthly, or "Does not repeat". Recurring todos display a badge (Daily/Weekly/Monthly) next to the title. When a recurring todo is completed, a new active instance is automatically created with the due date advanced by the interval (overdue todos advance from today). A snackbar notification confirms the next instance was scheduled.

**Reason for change:** Recurring tasks are a core productivity feature — things like daily standups, weekly reviews, or monthly bills naturally repeat, and having to manually re-create them breaks the flow. This brings the app closer to real-world todo app utility.

**Removals:** None

**Stats:**
- Lines added: 147
- Lines deleted: 12
- Tests added: 15
- Tests removed: 0
- Test failures before green: 2

## Day 21 — [2026-04-12] — Feature: Priority Filter

**Description:** A "Priority:" chip row now appears in the filter toolbar between the sort controls and the tag filter. Clicking High, Medium, Low, or None restricts the list to todos of that priority; clicking All resets it. The priority filter compounds with the existing status filter, search, and tag filter. Internally, `FilterSortTodosHandler.Handle()` gained an optional `TodoPriority? priorityFilter = null` parameter — no callers needed updating since it defaults to null. Color coding matches the priority chips on todo items (red for High, orange for Medium, blue for Low, default for None).

**Reason for change:** The app already supports sorting by priority and filtering by status/tag. Priority filtering was the obvious missing piece — a user managing a large list needs to be able to focus just on High priority items without scrolling past everything else.

**Removals:** None

**Stats:**
- Lines added: 130
- Lines deleted: 2
- Tests added: 10
- Tests removed: 0
- Test failures before green: 1 (missing `using TodoApp.Features.Todos` in HomeTests.cs)

## Day 20 — [2026-04-12] — Feature: Import from CSV

**Description:** A file-upload icon button now sits next to the existing export button in the filter toolbar. Clicking it opens a native file picker filtered to `.csv`; selecting a file reads it in the browser, calls `ImportTodosHandler`, and shows a success snackbar ("Imported N todos") or a warning if no valid rows were found. The handler parses the same format the app exports (columns: `Id, Title, Priority, DueDate, IsCompleted, CreatedAt, Tags, Notes`): it skips the `Id` column and assigns new IDs, handles RFC 4180 quoted fields (titles with commas), parses pipe-separated tags and creates them in `TodoTags`, sets notes, and silently skips any malformed rows. A round-trip test exports from one database and re-imports into a clean one, verifying title, priority, and count are preserved.

**Reason for change:** Day 14 added export; import completes the data portability story. Users can now back up their todos to a CSV, open it in Excel, and re-import — or migrate from another system by editing the CSV into the right format.

**Removals:** None

**Stats:**
- Lines added: 235
- Lines deleted: 0
- Tests added: 13
- Tests removed: 0
- Test failures before green: 1 (missing `using TodoApp.Features.Todos` in test file)

## Day 19 — [2026-04-12] — Feature: Subtasks

**Description:** Each todo now has a collapsible subtask list. A "Subtasks" toggle button at the bottom of every todo item expands an inline list of subtasks; the button also shows a `done/total` progress count when subtasks exist (e.g. "1/2 subtasks"). Expanded subtasks can be individually checked off (toggle), deleted with the × button, or added via an inline "Add subtask" text field (Enter to save, Escape to cancel). A new `Subtasks` table stores subtask data; four new handler slices cover add, complete, delete, and get operations.

**Reason for change:** Complex tasks rarely fit in a single title. Subtasks are the most impactful remaining feature for making the app genuinely useful for real work — they let users break a todo into smaller, trackable steps without needing a separate todo per step. The collapsible design keeps the list clean when subtasks aren't needed.

**Removals:** None

**Stats:**
- Lines added: 370
- Lines deleted: 8
- Tests added: 21
- Tests removed: 0
- Test failures before green: 0

## Day 18 — [2026-04-11] — Feature: Due Date UX Improvements

**Description:** Two complementary improvements to how due dates work. First, three shortcut buttons ("Today", "Tomorrow", "Next week") appear below the date picker in the Add Todo form; clicking a button sets the due date instantly without opening the calendar — clicking it again toggles the date back off. Second, the due date label on list items now uses relative language instead of raw dates for nearby/overdue items: "Due today", "Due tomorrow", "Due in 5 days", "Overdue by 1 day", "Overdue by 3 days" etc. Dates more than 7 days away still display in absolute `MMM d, yyyy` format. Two existing tests that asserted on the old absolute-date text were updated to match the new relative phrasing.

**Reason for change:** The date picker requires multiple clicks just to set "today" or "tomorrow" — the two most common due dates. Shortcuts make the common case a single click. Relative labels make the urgency immediately readable: "Overdue by 2 days" communicates more at a glance than "Apr 9, 2026" would.

**Removals:** None

**Stats:**
- Lines added: 134
- Lines deleted: 12
- Tests added: 5 (+ 2 existing tests updated)
- Tests removed: 0
- Test failures before green: 2 (pre-existing tests checking old absolute-date format)

## Day 17 — [2026-04-11] — Feature: Notes / Description Field

**Description:** Each todo now has a sticky-note icon button in its action row. When no notes exist the icon is outlined (grey); when notes are present it fills in blue. Clicking the icon opens an inline multiline editor with a Save and Cancel button directly on the todo item. Existing notes are shown as a clickable caption below the tags row — clicking anywhere on the note text opens the editor. Notes are trimmed on save; saving whitespace clears the note back to null. Notes survive undo/restore and are included as an additional column in CSV exports.

**Reason for change:** Tags provide short categorical labels; notes provide free-form context. At this stage of a real todo list, users frequently need to capture "how" alongside "what" — e.g. a phone number, a URL, a reminder of why something matters. The inline editor mirrors the existing title-edit pattern, keeping the UI consistent without opening a dialog.

**Removals:** None

**Stats:**
- Lines added: 487
- Lines deleted: 12
- Tests added: 12
- Tests removed: 0
- Test failures before green: 0

## Day 16 — [2026-04-11] — Feature: Clear Completed

**Description:** A "Clear completed" button now appears at the bottom-right of the stats panel whenever at least one todo is marked done. Clicking it removes every completed todo in a single action and shows an undo snackbar (e.g. "3 completed todos cleared") with a full undo that restores them all — leveraging the existing `RestoreTodosHandler`. The button disappears as soon as no completed todos remain. Active and pinned todos are never touched.

**Reason for change:** The stats panel already tells users how many completed todos they have; "Clear completed" lets them act on that information without selecting items one-by-one or using bulk select. It's the classic TodoMVC sweep, and it fits naturally at this stage: the list is now long enough that cleaning it up periodically is genuinely useful.

**Removals:** None

**Stats:**
- Lines added: 348
- Lines deleted: 0
- Tests added: 10
- Tests removed: 0
- Test failures before green: 0

## Day 15 — [2026-04-11] — Feature: Pin/Star Todos

**Description:** Every todo now has a star (☆) icon button on its right edge. Clicking it pins the todo — the star fills in amber and the tooltip changes to "Unpin". Clicking again unpins. Pinned todos are always sorted to the top of the list, regardless of the active sort order (Newest, Oldest, Due Date, or Priority). The pin state persists in the database and survives undo/restore. Pinning is additive with all existing filters — e.g. pinned todos appear first within an "Active" filter or a tag filter.

**Reason for change:** Priority levels let users rank todos by importance, but they require editing the todo. Pinning is a faster, one-click way to say "this is what I'm working on right now". It's a natural later-stage feature — users with a long list benefit most from being able to keep a few critical items anchored at the top without disrupting the rest of their sort order.

**Removals:** None

**Stats:**
- Lines added: 383
- Lines deleted: 11
- Tests added: 10
- Tests removed: 0
- Test failures before green: 0

## Day 14 — [2026-04-11] — Feature: Export to CSV

**Description:** A download icon button (⬇) now appears next to the sort dropdown whenever todos exist. Clicking it generates a `todos-YYYY-MM-DD.csv` file and triggers an immediate browser download. The CSV includes seven columns: Id, Title, Priority, DueDate, IsCompleted, CreatedAt, Tags (tags are pipe-separated within the cell). Titles containing commas, double-quotes, or newlines are automatically quoted and escaped per RFC 4180.

**Reason for change:** Export is a practical "later stage" feature — once a user has accumulated a meaningful list of todos, they want a way to get the data out for archiving, sharing, or importing into another tool. The implementation is clean: `CsvExportHandler` is a pure, stateless class (no DB access) that takes an in-memory list and returns a string, making it easy to unit-test exhaustively. The JS file-download helper is a small, reusable addition to the existing `todoApp` namespace.

**Removals:** None

**Stats:**
- Lines added: 305
- Lines deleted: 12
- Tests added: 13
- Tests removed: 0
- Test failures before green: 0

## Day 13 — [2026-04-11] — Feature: Filter by Tag

**Description:** The filter bar now shows a row of tag chips whenever any todos have tags. An "All" chip is always present (selected by default). Clicking any tag chip narrows the list to todos that carry that tag. The tag filter composes cleanly with the existing status filter (All / Active / Completed) and the search field — for example, selecting "health" and then "Active" shows only active todos tagged "health". Clicking "All" resets the tag filter. The chip row is hidden entirely when no tags exist.

**Reason for change:** Tags were added on Day 12 but had no way to browse by label — the natural follow-up is making them actionable. Filtering by tag is the minimum useful behaviour: users can now say "show me everything tagged 'work'" with a single click. The implementation required no new handlers; the logic sits entirely in the `DisplayedTodos` computed property inside `Home.razor`, keeping the vertical-slice boundaries intact.

**Removals:** None

**Stats:**
- Lines added: 157
- Lines deleted: 2
- Tests added: 5
- Tests removed: 0
- Test failures before green: 0

## Day 12 — [2026-04-08] — Feature: Tags/Labels

**Description:** Users can now attach tags (labels) to any todo. Each todo displays a row of small tag chips below its title and due date. Clicking the label icon on a todo opens an inline text input — press Enter to save the tag, Escape to cancel. Tags are normalized to lowercase for consistency. Clicking the × on a tag chip removes it immediately. Tags persist in the database and reload correctly between sessions.

**Reason for change:** Tags provide lightweight categorization without the overhead of a full list/project system. They complement the existing search and filter features by letting users organize todos semantically (e.g., "work", "personal", "urgent"). This is a natural mid-to-later stage feature that makes the app meaningfully more useful as a real task manager.

**Removals:** None

**Stats:**
- Lines added: 392
- Lines deleted: 0
- Tests added: 16
- Tests removed: 0
- Test failures before green: 1

## Day 11 — [2026-04-08] — Feature: Keyboard Shortcuts

**Description:** Users can now navigate the app without reaching for the mouse. Pressing `N` anywhere on the page (while not typing in an input) jumps focus directly to the new-todo input. Pressing `/` jumps to the search field. Pressing `?` toggles a shortcuts reference panel that lists all available shortcuts. The reference panel is also accessible via a keyboard icon button in the "Add a New Todo" card header.

**Reason for change:** The app now has enough features that power users benefit from faster navigation. Keyboard shortcuts are a natural "later stage" enhancement — they don't change what the app can do, but they make it significantly faster to use for anyone who prefers the keyboard over the mouse. The `?` help panel ensures discoverability: users can always see what shortcuts are available without needing external documentation.

**Removals:** None

**Stats:**
- Lines added: 209
- Lines deleted: 2
- Tests added: 8
- Tests removed: 0
- Test failures before green: 1

## Day 10 — [2026-04-08] — Feature: Stats/Progress Summary Panel

**Description:** A summary panel now appears below the "Add Todo" card whenever at least one todo exists. It shows four chips — Total, Active, Completed, and Overdue (only shown when there are overdue items) — along with a progress bar and percentage reflecting how many todos have been completed. The panel updates instantly as todos are added, completed, or deleted.

**Reason for change:** The app has rich data — priorities, due dates, completion states — but no way to see your overall progress at a glance. A stats panel is the natural complement to filtering and sorting: instead of navigating through todos to count them mentally, users can see their productivity at a single glance. It's a meaningful quality-of-life addition before moving to more complex features like categories or recurring tasks.

**Removals:** None

**Stats:**
- Lines added: 127
- Lines deleted: 0
- Tests added: 12
- Tests removed: 0
- Test failures before green: 0

## Day 9 — [2026-03-29] — Feature: Dark Mode Toggle

**Description:** A sun/moon icon button now appears in the top-right corner of the app bar. Clicking it switches the application between light and dark themes. The button's icon and tooltip update to reflect the current mode — a moon icon when in light mode, a sun icon when in dark mode.

**Reason for change:** After eight days of feature work the app is fully functional but visually one-dimensional. Dark mode is one of the most universally requested UI preferences: it reduces eye strain in low-light environments and gives the app a polished, modern feel. MudBlazor's built-in theme provider makes it a natural next step before diving into more complex features.

**Removals:** None

**Stats:**
- Lines added: 86
- Lines deleted: 2
- Tests added: 4
- Tests removed: 0
- Test failures before green: 2

## Day 8 — [2026-03-29] — Feature: Undo Delete and Bulk Complete

**Description:** After deleting a todo (single or bulk) or bulk-completing todos, a snackbar notification appears at the bottom of the screen with an "Undo" button. Clicking Undo reverses the action — deleted todos are fully restored with their original title, priority, due date, and completion state; bulk-completed todos are toggled back to active.

**Reason for change:** With bulk operations in place, a single misclick can now wipe out multiple todos at once. Undo is the natural safety net: it converts destructive actions into reversible ones and gives users confidence to act quickly without fear of accidents. It is one of the most universally expected features in any task management tool.

**Removals:** None

**Stats:**
- Lines added: 322
- Lines deleted: 1
- Tests added: 9
- Tests removed: 0
- Test failures before green: 0

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
