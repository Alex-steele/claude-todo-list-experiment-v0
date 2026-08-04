using TodoApp.Features.Todos;
using TodoApp.Features.Todos.QuickAdd;
using Xunit;

namespace TodoApp.Tests.Features.Todos.QuickAdd;

public class NaturalLanguageParserTests
{
    private static readonly DateTime Today = new(2026, 4, 12);

    [Fact]
    public void Parse_PlainTitle_ReturnsUnchanged()
    {
        var result = NaturalLanguageParser.Parse("Buy groceries", Today);

        Assert.Equal("Buy groceries", result.CleanTitle);
        Assert.Null(result.Priority);
        Assert.Null(result.DueDate);
        Assert.Empty(result.Tags);
    }

    [Fact]
    public void Parse_Empty_ReturnsEmpty()
    {
        var result = NaturalLanguageParser.Parse("", Today);

        Assert.Equal("", result.CleanTitle);
        Assert.Null(result.Priority);
        Assert.Null(result.DueDate);
        Assert.Empty(result.Tags);
    }

    // --- Priority parsing ---

    [Theory]
    [InlineData("Buy milk !high", "Buy milk", TodoPriority.High)]
    [InlineData("Buy milk !h", "Buy milk", TodoPriority.High)]
    [InlineData("Buy milk !medium", "Buy milk", TodoPriority.Medium)]
    [InlineData("Buy milk !med", "Buy milk", TodoPriority.Medium)]
    [InlineData("Buy milk !m", "Buy milk", TodoPriority.Medium)]
    [InlineData("Buy milk !low", "Buy milk", TodoPriority.Low)]
    [InlineData("Buy milk !l", "Buy milk", TodoPriority.Low)]
    public void Parse_PriorityHint_ExtractsPriorityAndStripsToken(string input, string expectedTitle, TodoPriority expectedPriority)
    {
        var result = NaturalLanguageParser.Parse(input, Today);

        Assert.Equal(expectedTitle, result.CleanTitle);
        Assert.Equal(expectedPriority, result.Priority);
        Assert.Null(result.DueDate);
    }

    [Fact]
    public void Parse_PriorityHint_CaseInsensitive()
    {
        var result = NaturalLanguageParser.Parse("Task !HIGH", Today);

        Assert.Equal(TodoPriority.High, result.Priority);
        Assert.Equal("Task", result.CleanTitle);
    }

    // --- Date parsing ---

    [Fact]
    public void Parse_Today_ExtractsDueDateAsToday()
    {
        var result = NaturalLanguageParser.Parse("Stand-up today", Today);

        Assert.Equal("Stand-up", result.CleanTitle);
        Assert.Equal(Today, result.DueDate!.Value.Date);
    }

    [Fact]
    public void Parse_Tomorrow_ExtractsDueDateAsTomorrow()
    {
        var result = NaturalLanguageParser.Parse("Call dentist tomorrow", Today);

        Assert.Equal("Call dentist", result.CleanTitle);
        Assert.Equal(Today.AddDays(1), result.DueDate!.Value.Date);
    }

    [Fact]
    public void Parse_NextWeek_ExtractsDueDateAsSevenDaysOut()
    {
        var result = NaturalLanguageParser.Parse("Review budget next week", Today);

        Assert.Equal("Review budget", result.CleanTitle);
        Assert.Equal(Today.AddDays(7), result.DueDate!.Value.Date);
    }

    [Fact]
    public void Parse_InNDays_ExtractsDueDateCorrectly()
    {
        var result = NaturalLanguageParser.Parse("Pay bill in 3 days", Today);

        Assert.Equal("Pay bill", result.CleanTitle);
        Assert.Equal(Today.AddDays(3), result.DueDate!.Value.Date);
    }

    [Fact]
    public void Parse_In1Day_ExtractsDueDateCorrectly()
    {
        var result = NaturalLanguageParser.Parse("Submit form in 1 day", Today);

        Assert.Equal("Submit form", result.CleanTitle);
        Assert.Equal(Today.AddDays(1), result.DueDate!.Value.Date);
    }

    [Fact]
    public void Parse_DateKeyword_CaseInsensitive()
    {
        var result = NaturalLanguageParser.Parse("task TOMORROW", Today);

        Assert.Equal(Today.AddDays(1), result.DueDate!.Value.Date);
        Assert.Equal("task", result.CleanTitle);
    }

    // --- Combined hints ---

    [Fact]
    public void Parse_PriorityAndDate_ExtractsBoth()
    {
        var result = NaturalLanguageParser.Parse("Buy milk tomorrow !high", Today);

        Assert.Equal("Buy milk", result.CleanTitle);
        Assert.Equal(TodoPriority.High, result.Priority);
        Assert.Equal(Today.AddDays(1), result.DueDate!.Value.Date);
    }

    [Fact]
    public void Parse_DateBeforePriority_ExtractsBoth()
    {
        var result = NaturalLanguageParser.Parse("Finish report !medium next week", Today);

        Assert.Equal("Finish report", result.CleanTitle);
        Assert.Equal(TodoPriority.Medium, result.Priority);
        Assert.Equal(Today.AddDays(7), result.DueDate!.Value.Date);
    }

    // --- No match cases ---

    [Fact]
    public void Parse_NoHints_ReturnsOriginalTitle()
    {
        var result = NaturalLanguageParser.Parse("Write unit tests", Today);

        Assert.Equal("Write unit tests", result.CleanTitle);
        Assert.Null(result.Priority);
        Assert.Null(result.DueDate);
        Assert.Empty(result.Tags);
    }

    [Fact]
    public void Parse_OnlyHints_ReturnsEmptyCleanTitle()
    {
        var result = NaturalLanguageParser.Parse("!high tomorrow", Today);

        Assert.Equal("", result.CleanTitle);
        Assert.Equal(TodoPriority.High, result.Priority);
        Assert.Equal(Today.AddDays(1), result.DueDate!.Value.Date);
    }

    // --- Tag parsing ---

    [Fact]
    public void Parse_SingleTag_ExtractsTagAndStripsToken()
    {
        var result = NaturalLanguageParser.Parse("Buy milk #groceries", Today);

        Assert.Equal("Buy milk", result.CleanTitle);
        Assert.Equal(["groceries"], result.Tags);
    }

    [Fact]
    public void Parse_MultipleTags_ExtractsAllInOrder()
    {
        var result = NaturalLanguageParser.Parse("Plan trip #travel #urgent #home", Today);

        Assert.Equal("Plan trip", result.CleanTitle);
        Assert.Equal(["travel", "urgent", "home"], result.Tags);
    }

    [Fact]
    public void Parse_Tag_LowercasesTagName()
    {
        var result = NaturalLanguageParser.Parse("Ship release #Work", Today);

        Assert.Equal(["work"], result.Tags);
    }

    [Fact]
    public void Parse_DuplicateTags_Deduplicated()
    {
        var result = NaturalLanguageParser.Parse("Task #work #work", Today);

        Assert.Equal(["work"], result.Tags);
    }

    [Fact]
    public void Parse_TagWithHyphenAndUnderscore_ExtractsFullName()
    {
        var result = NaturalLanguageParser.Parse("Task #deep-work #side_project", Today);

        Assert.Equal(["deep-work", "side_project"], result.Tags);
    }

    [Fact]
    public void Parse_NoTag_ReturnsEmptyTagsList()
    {
        var result = NaturalLanguageParser.Parse("Just a title", Today);

        Assert.Empty(result.Tags);
    }

    [Fact]
    public void Parse_TagPriorityAndDate_ExtractsAllThree()
    {
        var result = NaturalLanguageParser.Parse("Buy milk #groceries tomorrow !high", Today);

        Assert.Equal("Buy milk", result.CleanTitle);
        Assert.Equal(TodoPriority.High, result.Priority);
        Assert.Equal(Today.AddDays(1), result.DueDate!.Value.Date);
        Assert.Equal(["groceries"], result.Tags);
    }

    [Fact]
    public void Parse_OnlyTag_ReturnsEmptyCleanTitle()
    {
        var result = NaturalLanguageParser.Parse("#urgent", Today);

        Assert.Equal("", result.CleanTitle);
        Assert.Equal(["urgent"], result.Tags);
    }
}
