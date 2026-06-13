using TodoApp.Features.Todos.Notes;
using Xunit;

namespace TodoApp.Tests.Features.Todos.Notes;

public class MarkdownRendererTests
{
    [Fact]
    public void Render_EmptyString_ReturnsEmpty()
    {
        var result = MarkdownRenderer.Render(string.Empty);
        Assert.Equal(string.Empty, result.Trim());
    }

    [Fact]
    public void Render_PlainText_WrapsInParagraph()
    {
        var result = MarkdownRenderer.Render("Hello world");
        Assert.Contains("Hello world", result);
        Assert.Contains("<p>", result);
    }

    [Fact]
    public void Render_BoldText_ProducesStrongTag()
    {
        var result = MarkdownRenderer.Render("**bold**");
        Assert.Contains("<strong>bold</strong>", result);
    }

    [Fact]
    public void Render_ItalicText_ProducesEmTag()
    {
        var result = MarkdownRenderer.Render("*italic*");
        Assert.Contains("<em>italic</em>", result);
    }

    [Fact]
    public void Render_UnorderedList_ProducesUlAndLi()
    {
        var result = MarkdownRenderer.Render("- item one\n- item two");
        Assert.Contains("<ul>", result);
        Assert.Contains("<li>item one</li>", result);
        Assert.Contains("<li>item two</li>", result);
    }

    [Fact]
    public void Render_Heading_ProducesH1Tag()
    {
        var result = MarkdownRenderer.Render("# My heading");
        Assert.Contains("<h1>My heading</h1>", result);
    }

    [Fact]
    public void Render_AutoLink_ProducesAnchorTag()
    {
        var result = MarkdownRenderer.Render("https://example.com");
        Assert.Contains("<a href=\"https://example.com\"", result);
    }

    [Fact]
    public void Render_RawHtml_IsStripped()
    {
        // DisableHtml() strips raw HTML to prevent XSS
        var result = MarkdownRenderer.Render("<script>alert('xss')</script>");
        Assert.DoesNotContain("<script>", result);
    }

    [Fact]
    public void Render_NullInput_ReturnsEmpty()
    {
        var result = MarkdownRenderer.Render(null!);
        Assert.Equal(string.Empty, result.Trim());
    }

    [Fact]
    public void Render_SoftLineBreak_BecomesHardBreak()
    {
        var result = MarkdownRenderer.Render("line one\nline two");
        Assert.Contains("<br />", result);
    }
}
