using System.Text.Json;
namespace AulaMarkdown.Core;
public sealed class ReaderSettings
{
    public string Language { get; set; } = "it";
    public string Theme { get; set; } = "Chiaro";
    public string Font { get; set; } = "Segoe UI";
    public int FontSize { get; set; } = 18;
    public bool Wide { get; set; } = true;
    public bool Sidebar { get; set; } = true;
    public string EditorPath { get; set; } = "";
    public string FlowgorithmPath { get; set; } = "";
    public List<string> Recent { get; set; } = [];
    public const int RecentLimit = 10;
    public void RememberFile(string path)
    {
        path = Path.GetFullPath(path);
        Recent ??= [];
        Recent.RemoveAll(x => string.Equals(x, path, StringComparison.OrdinalIgnoreCase));
        Recent.Insert(0, path);
        NormalizeRecent();
    }
    private void NormalizeRecent()
    {
        Recent = (Recent ?? []).Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase).Take(RecentLimit).ToList();
    }
    public static string DataDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AulaMarkdown");
    public static ReaderSettings Load(string? directory = null)
    {
        try {
            var s = JsonSerializer.Deserialize<ReaderSettings>(File.ReadAllText(Path.Combine(directory ?? DataDirectory,"settings.json"))) ?? new();
            s.FontSize = Math.Clamp(s.FontSize, 12, 36); s.NormalizeRecent(); return s;
        } catch { return new(); }
    }
    public void Save(string? directory = null)
    {
        NormalizeRecent();
        Directory.CreateDirectory(directory ?? DataDirectory);
        string path=Path.Combine(directory ?? DataDirectory,"settings.json");
        string temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
        File.WriteAllText(temp,JsonSerializer.Serialize(this,new JsonSerializerOptions{WriteIndented=true}));
        File.Move(temp,path,true);
    }
}