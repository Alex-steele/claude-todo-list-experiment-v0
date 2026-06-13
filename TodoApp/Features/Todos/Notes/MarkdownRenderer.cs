using Markdig;

namespace TodoApp.Features.Todos.Notes;

public static class MarkdownRenderer
{
    private static readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UseAutoLinks()
        .UseSoftlineBreakAsHardlineBreak()
        .DisableHtml()
        .Build();

    public static string Render(string markdown) =>
        Markdown.ToHtml(markdown ?? string.Empty, _pipeline);
}
