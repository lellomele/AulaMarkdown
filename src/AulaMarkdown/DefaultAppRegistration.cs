using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace AulaMarkdown;

public static class DefaultAppRegistration
{
    private const string ProgId = "AulaMarkdown.Markdown";
    private const string Capabilities = @"Software\AulaMarkdown\Capabilities";
    private static readonly string[] Extensions = [".md", ".markdown", ".mdown"];

    public static void Register()
    {
        string exe = Path.Combine(AppContext.BaseDirectory, "AulaMarkdown.exe");
        if (!File.Exists(exe)) throw new FileNotFoundException("AulaMarkdown.exe", exe);
        WriteRegistration(Registry.CurrentUser, exe);
        SHChangeNotify(0x08000000, 0, IntPtr.Zero, IntPtr.Zero);
    }

    // Separate registry root allows isolated verification without changing associations.
    public static void WriteRegistration(RegistryKey root, string exe)
    {
        if (!Path.IsPathFullyQualified(exe) || exe.Contains('"'))
            throw new ArgumentException("An absolute executable path is required.", nameof(exe));
        void Set(string path, string name, string value)
        {
            using var key = root.CreateSubKey(path);
            key.SetValue(name, value, RegistryValueKind.String);
        }
        string command = $"\"{exe}\" \"%1\"";
        string icon = $"\"{exe}\",0";
        string prog = @"Software\Classes\" + ProgId;
        Set(prog, "", "Markdown document");
        Set(prog + @"\DefaultIcon", "", icon);
        Set(prog + @"\shell\open\command", "", command);
        Set(Capabilities, "ApplicationName", "AulaMarkdown");
        Set(Capabilities, "ApplicationDescription", "Markdown document viewer / Visualizzatore Markdown");
        Set(Capabilities, "ApplicationIcon", icon);
        string app = @"Software\Classes\Applications\AulaMarkdown.exe";
        Set(app, "FriendlyAppName", "AulaMarkdown");
        Set(app + @"\shell\open\command", "", command);
        foreach (string extension in Extensions)
        {
            Set(Capabilities + @"\FileAssociations", extension, ProgId);
            Set(app + @"\SupportedTypes", extension, "");
            using var openWith = root.CreateSubKey(@"Software\Classes\" + extension + @"\OpenWithProgids");
            openWith.SetValue(ProgId, Array.Empty<byte>(), RegistryValueKind.None);
        }
        Set(@"Software\RegisteredApplications", "AulaMarkdown", Capabilities);
        // UserChoice is managed by Windows; registration does not force a default.
    }

    public static void OpenSettings()
    {
        string uri = OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000)
            ? "ms-settings:defaultapps?registeredAppUser=AulaMarkdown"
            : "ms-settings:defaultapps";
        Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
    }

    [DllImport("shell32.dll")]
    private static extern void SHChangeNotify(uint eventId, uint flags, IntPtr item1, IntPtr item2);
}
