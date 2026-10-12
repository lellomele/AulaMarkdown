using Microsoft.Win32;
using AulaMarkdown;
string testKey = @"Software\AulaMarkdown.RegistrationTest." + Guid.NewGuid().ToString("N");
int checks = 0;
void Check(bool value) { if (!value) throw new Exception("Registration check failed: " + checks); checks++; }
try
{
    using var root = Registry.CurrentUser.CreateSubKey(testKey);
    using(var choice = root.CreateSubKey(@"Software\Classes\.md")) choice.SetValue("", "Existing.Viewer");
    foreach(string exe in new[]{ @"D:\Programmi con spazi\città\AulaMarkdown.exe", @"D:\Nuova posizione\AulaMarkdown.exe" })
    {
        DefaultAppRegistration.WriteRegistration(root, exe);
        using var command = root.OpenSubKey(@"Software\Classes\AulaMarkdown.Markdown\shell\open\command");
        Check((string?)command?.GetValue("") == $"\"{exe}\" \"%1\"");
        using var registered = root.OpenSubKey(@"Software\RegisteredApplications");
        Check((string?)registered?.GetValue("AulaMarkdown") == @"Software\AulaMarkdown\Capabilities");
        foreach(string ext in new[]{".md", ".markdown", ".mdown"})
        {
            using var associations = root.OpenSubKey(@"Software\AulaMarkdown\Capabilities\FileAssociations");
            Check((string?)associations?.GetValue(ext) == "AulaMarkdown.Markdown");
            using var openWith = root.OpenSubKey(@"Software\Classes\" + ext + @"\OpenWithProgids");
            Check(openWith?.GetValueKind("AulaMarkdown.Markdown") == RegistryValueKind.None);
        }
        using var original = root.OpenSubKey(@"Software\Classes\.md");
        Check((string?)original?.GetValue("") == "Existing.Viewer");
        Check(root.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts") == null);
    }
    Console.WriteLine($"Registration: {checks} checks passed; real associations unchanged.");
}
finally { Registry.CurrentUser.DeleteSubKeyTree(testKey, throwOnMissingSubKey:false); }
