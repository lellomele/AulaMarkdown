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
    public string FlowgorithmPath { get; set; } = "";
    public List<string> Recent { get; set; } = [];
    public static string DataDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AulaMarkdown");
    public static ReaderSettings Load()
    {
        try {
            var s = JsonSerializer.Deserialize<ReaderSettings>(File.ReadAllText(Path.Combine(DataDirectory,"settings.json"))) ?? new();
            s.FontSize = Math.Clamp(s.FontSize, 12, 36); s.Recent ??= []; return s;
        } catch { return new(); }
    }
    public void Save()
    {
        Directory.CreateDirectory(DataDirectory);
        string path=Path.Combine(DataDirectory,"settings.json");
        string temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
        File.WriteAllText(temp,JsonSerializer.Serialize(this,new JsonSerializerOptions{WriteIndented=true}));
        File.Move(temp,path,true);
    }
}