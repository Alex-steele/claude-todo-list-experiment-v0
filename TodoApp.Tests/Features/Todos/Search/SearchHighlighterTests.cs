using TodoApp.Features.Todos.Search;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Search;

public class SearchHighlighterTests
{
    [Fact]
    public void Highlight_EmptyQuery_ReturnsHtmlEncodedText()
    {
        var result = SearchHighlighter.Highlight("Buy milk", "");
        Assert.Equal("Buy milk", result.Value);
    }

    [Fact]
    public void Highlight_WhitespaceQuery_ReturnsHtmlEncodedText()
    {
        var result = SearchHighlighter.Highlight("Buy milk", "   ");
        Assert.Equal("Buy milk", result.Value);
    }

    [Fact]
    public void Highlight_EmptyText_ReturnsEmpty()
    {
        var result = SearchHighlighter.Highlight("", "milk");
        Assert.Equal(string.Empty, result.Value);
    }

    [Fact]
    public void Highlight_NoMatch_ReturnsHtmlEncodedText()
    {
        var result = SearchHighlighter.Highlight("Buy milk", "coffee");
        Assert.Equal("Buy milk", result.Value);
        Assert.DoesNotContain("mark", result.Value);
    }

    [Fact]
    public void Highlight_SingleMatch_WrapsInMarkTag()
    {
        var result = SearchHighlighter.Highlight("Buy milk today", "milk");
        Assert.Equal("Buy <mark class=\"search-highlight\">milk</mark> today", result.Value);
    }

    [Fact]
    public void Highlight_CaseInsensitive_MatchesIgnoringCase()
    {
        var result = SearchHighlighter.Highlight("Buy Milk today", "milk");
        Assert.Contains("<mark", result.Value);
        Assert.Contains("Milk", result.Value);
    }

    [Fact]
    public void Highlight_MultipleMatches_WrapsAll()
    {
        var result = SearchHighlighter.Highlight("milk and more milk", "milk");
        Assert.Equal(2, CountOccurrences(result.Value, "<mark"));
    }

    [Fact]
    public void Highlight_MatchAtStart_WrapsCorrectly()
    {
        var result = SearchHighlighter.Highlight("milk please", "milk");
        Assert.StartsWith("<mark", result.Value);
    }

    [Fact]
    public void Highlight_MatchAtEnd_WrapsCorrectly()
    {
        var result = SearchHighlighter.Highlight("buy milk", "milk");
        Assert.EndsWith("</mark>", result.Value);
    }

    [Fact]
    public void Highlight_HtmlSpecialChars_AreEncoded()
    {
        var result = SearchHighlighter.Highlight("Fix <bug> & test", "bug");
        Assert.Contains("&lt;", result.Value);
        Assert.Contains("&gt;", result.Value);
        Assert.Contains("&amp;", result.Value);
    }

    [Fact]
    public void Highlight_QueryIsEntireText_WrapsAll()
    {
        var result = SearchHighlighter.Highlight("milk", "milk");
        Assert.Equal("<mark class=\"search-highlight\">milk</mark>", result.Value);
    }

    [Fact]
    public void Highlight_PreservesOriginalCasingInMatch()
    {
        var result = SearchHighlighter.Highlight("Call MOM tonight", "mom");
        Assert.Contains(">MOM<", result.Value);
    }

    private static int CountOccurrences(string text, string pattern)
    {
        var count = 0;
        var pos = 0;
        while ((pos = text.IndexOf(pattern, pos, StringComparison.Ordinal)) >= 0)
        {
            count++;
            pos += pattern.Length;
        }
        return count;
    }
}
