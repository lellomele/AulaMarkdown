using System.Reflection;
namespace AulaMarkdown;
public static class AppInfo
{
    public const string Product = "AulaMarkdown";
    public const string Copyright = "(c) 2026 - Prof. Ing. Raffaele Mele";
    public static string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.1.0";
    public static string WindowTitle(string file) => Product + " — 🇮🇹 " + Copyright + (string.IsNullOrEmpty(file) ? "" : " — " + file);
}
