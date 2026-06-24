# Change Log

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
