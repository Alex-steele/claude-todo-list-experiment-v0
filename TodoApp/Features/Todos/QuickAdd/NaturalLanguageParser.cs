using System.Text.RegularExpressions;
using TodoApp.Features.Todos;

namespace TodoApp.Features.Todos.QuickAdd;

public record ParseResult(string CleanTitle, TodoPriority? Priority, DateTime? DueDate);

public static class NaturalLanguageParser
{
    // Matches: !high, !h, !medium, !m, !low, !l (case-insensitive)
    private static readonly Regex PriorityPattern =
        new(@"\s*!(high|h|medium|med|m|low|l)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Matches: "in N days" (e.g. "in 3 days")
    private static readonly Regex InNDaysPattern =
        new(@"\s*\bin\s+(\d+)\s+days?\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Matches fixed keywords: today, tomorrow, next week
    private static readonly Regex DateKeywordPattern =
        new(@"\s*\b(today|tomorrow|next\s+week)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static ParseResult Parse(string input, DateTime? today = null)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new ParseResult(input, null, null);

        var baseDate = today?.Date ?? DateTime.Today;
        var working = input;

        // Extract priority
        TodoPriority? priority = null;
        var priorityMatch = PriorityPattern.Match(working);
        if (priorityMatch.Success)
        {
            priority = ParsePriority(priorityMatch.Groups[1].Value);
            working = PriorityPattern.Replace(working, string.Empty);
        }

        // Extract date — "in N days" first, then keywords
        DateTime? dueDate = null;
        var inNDaysMatch = InNDaysPattern.Match(working);
        if (inNDaysMatch.Success)
        {
            var days = int.Parse(inNDaysMatch.Groups[1].Value);
            dueDate = baseDate.AddDays(days);
            working = InNDaysPattern.Replace(working, string.Empty);
        }
        else
        {
            var keywordMatch = DateKeywordPattern.Match(working);
            if (keywordMatch.Success)
            {
                dueDate = ParseDateKeyword(keywordMatch.Groups[1].Value, baseDate);
                working = DateKeywordPattern.Replace(working, string.Empty);
            }
        }

        var cleanTitle = working.Trim();
        return new ParseResult(cleanTitle, priority, dueDate);
    }

    private static TodoPriority ParsePriority(string token) => token.ToLowerInvariant() switch
    {
        "high" or "h"           => TodoPriority.High,
        "medium" or "med" or "m" => TodoPriority.Medium,
        "low"  or "l"           => TodoPriority.Low,
        _                       => TodoPriority.None
    };

    private static DateTime ParseDateKeyword(string keyword, DateTime baseDate) =>
        keyword.ToLowerInvariant() switch
        {
            "today"     => baseDate,
            "tomorrow"  => baseDate.AddDays(1),
            _           => baseDate.AddDays(7)  // next week
        };
}
