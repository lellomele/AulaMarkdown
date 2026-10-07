using System.Text.RegularExpressions;
namespace AulaMarkdown.Core;
public enum LinkKind { Local, Web, Anchor, Unsupported }
public sealed record LinkTarget(LinkKind Kind, string Value, string Fragment = "");
public static class LinkResolver
{
    public static LinkTarget Resolve(string raw, string documentPath)
    {
        var value = raw.Trim();
        if (value.StartsWith('<') && value.EndsWith('>')) value = value[1..^1];
        if (string.IsNullOrWhiteSpace(value)) return new(LinkKind.Unsupported, value);
        if (value.StartsWith('#')) return new(LinkKind.Anchor, Decode(value[1..]));
        // Drive letters must be classified before URI schemes (G: is not a website).
        bool drive = Regex.IsMatch(value, @"^[A-Za-z]:[/\\]");
        bool unc = value.StartsWith(@"\\") || value.StartsWith("//");
        if (!drive && !unc && Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            if (uri.Scheme is "https" or "http") return new(LinkKind.Web, uri.AbsoluteUri);
            if (uri.IsFile) return new(LinkKind.Local, Path.GetFullPath(uri.LocalPath), Decode(uri.Fragment.TrimStart('#')));
            return new(LinkKind.Unsupported, value);
        }
        if (!drive && !unc && Regex.IsMatch(value, @"^[A-Za-z][A-Za-z0-9+.-]*:")) return new(LinkKind.Unsupported, value);
        string fragment = "";
        // A real local name may itself contain '#'; prefer an exact existing file.
        var decoded = Decode(value);
        var baseDir = Path.GetDirectoryName(Path.GetFullPath(documentPath))!;
        var exact = Path.GetFullPath(decoded.Replace('/', Path.DirectorySeparatorChar), baseDir);
        if (!File.Exists(exact) && !Directory.Exists(exact))
        {
            int hash = value.IndexOf('#');
            if (hash >= 0) { fragment = Decode(value[(hash + 1)..]); decoded = Decode(value[..hash]); }
        }
        if (decoded.IndexOfAny(['\0','\r','\n']) >= 0) return new(LinkKind.Unsupported, value);
        return new(LinkKind.Local, Path.GetFullPath(decoded.Replace('/', Path.DirectorySeparatorChar), baseDir), fragment);
    }
    private static string Decode(string value) => Uri.UnescapeDataString(value);
    public static bool IsMarkdown(string path) => Path.GetExtension(path).ToLowerInvariant() is ".md" or ".markdown" or ".mdown";
    public static bool RequiresConfirmation(string path) => Path.GetExtension(path).ToLowerInvariant() is
        ".exe" or ".com" or ".bat" or ".cmd" or ".ps1" or ".vbs" or ".js" or ".msi" or ".lnk" or ".url" or ".scr" or ".hta" or ".reg";
}