using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
namespace AulaMarkdown.Core;
public sealed record HeadingItem(string Text, string Id, int Level);
public sealed record RenderedDocument(string Html, IReadOnlyDictionary<int,string> Links, IReadOnlyDictionary<int,string> Images, IReadOnlyList<HeadingItem> Headings);
public static class DocumentRenderer
{
    public const string Origin = "https://aulamarkdown.invalid";
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UsePipeTables().UseAutoIdentifiers(AutoIdentifierOptions.GitHub).UseTaskLists().UseEmphasisExtras().UseAutoLinks().DisableHtml().Build();
    public static RenderedDocument Render(string source, string documentPath)
    {
        var document = Markdown.Parse(source, Pipeline);
        var links = new Dictionary<int,string>();
        var images = new Dictionary<int,string>();
        foreach (var link in document.Descendants<LinkInline>())
        {
            var original = link.Url ?? "";
            if (link.IsImage)
            {
                var id = images.Count;
                try
                {
                    var target = LinkResolver.Resolve(original, documentPath);
                    if (target.Kind == LinkKind.Local && Path.GetExtension(target.Value).ToLowerInvariant() is ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp" or ".bmp" or ".ico")
                        images.Add(id, target.Value);
                }
                catch (Exception ex) when (ex is ArgumentException or NotSupportedException or IOException) { }
                link.Url = Origin + "/image/" + (images.ContainsKey(id) ? id.ToString() : "blocked");
            }
            else
            {
                int id = links.Count; links.Add(id, original);
                link.Url = Origin + "/link/" + id;
                link.Title = original;
            }
        }
        var headings = document.Descendants<HeadingBlock>().Select(h => new HeadingItem(
            string.Concat(h.Inline?.Descendants<LiteralInline>().Select(i => i.Content.ToString()) ?? []),
            h.GetAttributes().Id ?? "", h.Level)).ToList();
        return new(Markdown.ToHtml(document, Pipeline), links, images, headings);
    }
}