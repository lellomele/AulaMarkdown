using System.Windows;
namespace AulaMarkdown;
public partial class AboutWindow : Window
{
    public string RequestedDocument { get; private set; } = "";
    public AboutWindow(Window owner)
    {
        Resources.MergedDictionaries.Add(owner.Resources);InitializeComponent();Owner=owner;
        foreach(var key in new[]{"PanelBg","TextBrush","MutedBrush"})Resources[key]=owner.FindResource(key);
        VersionLabel.Text=UiText.T("Version")+" "+AppInfo.Version;
        CopyrightText.Text=AppInfo.Copyright;
    }
    private void Close_Click(object s,RoutedEventArgs e)=>Close();
    private void Guide_Click(object s,RoutedEventArgs e){RequestedDocument=UiText.GuideFile;Close();}
    private void Licenses_Click(object s,RoutedEventArgs e){RequestedDocument="THIRD-PARTY-NOTICES.md";Close();}
}
