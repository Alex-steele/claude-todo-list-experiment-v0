using System.Net;
using System.Text;
using Microsoft.AspNetCore.Components;

namespace TodoApp.Features.Todos.Search;

public static class SearchHighlighter
{
    public static MarkupString Highlight(string text, string query)
    {
        if (string.IsNullOrEmpty(text))
            return (MarkupString)string.Empty;

        if (string.IsNullOrWhiteSpace(query))
            return (MarkupString)WebUtility.HtmlEncode(text);

        var sb = new StringBuilder();
        var pos = 0;
        while (pos < text.Length)
        {
            var idx = text.IndexOf(query, pos, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
            {
                sb.Append(WebUtility.HtmlEncode(text[pos..]));
                break;
            }
            sb.Append(WebUtility.HtmlEncode(text[pos..idx]));
            sb.Append("<mark class=\"search-highlight\">");
            sb.Append(WebUtility.HtmlEncode(text.Substring(idx, query.Length)));
            sb.Append("</mark>");
            pos = idx + query.Length;
        }
        return (MarkupString)sb.ToString();
    }
}
