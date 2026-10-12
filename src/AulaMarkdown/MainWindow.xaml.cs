using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AulaMarkdown.Core;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using System.Windows.Shell;
namespace AulaMarkdown;
public partial class MainWindow : Window
{
    private readonly ReaderSettings settings;
    private readonly string[] args;
    private readonly Stack<string> history = new();
    private RenderedDocument? rendered;
    private string currentPath = "";
    private string pendingFragment = "";
    private string documentUrl = "";
    private byte[] documentBytes = [];
    private string diagnosticDirectory = "";
    private bool initialized, ready, fullscreen;
    private long loadId;
    private WindowState previousState;
    private WindowChrome? normalChrome;
    private bool changingLanguage;
    private ReaderTheme Theme => ReaderTheme.All.FirstOrDefault(x=>x.Name==settings.Theme) ?? ReaderTheme.All[0];
    public MainWindow(string[] arguments)
    {
        args=arguments;
        int diag=Array.IndexOf(args,"--diagnostics");
        if(diag>=0 && diag+1<args.Length) diagnosticDirectory=Path.GetFullPath(args[diag+1]);
        settings=diagnosticDirectory.Length>0 ? new ReaderSettings() : ReaderSettings.Load();
        if(!ReaderTheme.All.Any(t=>t.Name==settings.Theme))settings.Theme="Chiaro";
        InitializeComponent();
        CopyrightLabel.Text=AppInfo.Copyright;Title=AppInfo.WindowTitle("");
        LanguageBox.ItemsSource=new[]{new UiChoice("it","Italiano"),new UiChoice("en","English")};
        FontBox.ItemsSource=new[]{"Segoe UI","Arial","Verdana","Georgia"};FontBox.SelectedItem=settings.Font;
        if(FontBox.SelectedIndex<0)FontBox.SelectedIndex=0;
        RefreshLanguage();
        StateChanged+=(_,_)=>MaximizeButton.Content=WindowState==WindowState.Maximized?"\uE923":"\uE922";
        Loaded += async (_,_)=> await InitializeAsync();
        Closing += (_,_)=>SaveSettings();
        Closed += (_,_)=>Browser.Dispose();
    }
    private async Task InitializeAsync()
    {
        try
        {
            string profile=Path.Combine(ReaderSettings.DataDirectory,"WebView2");
            if(diagnosticDirectory.Length>0) profile=Path.Combine(diagnosticDirectory,"WebView2");
            var environment=await CoreWebView2Environment.CreateAsync(null,profile);
            await Browser.EnsureCoreWebView2Async(environment);
            var web=Browser.CoreWebView2;
            web.Settings.AreDefaultContextMenusEnabled=false;
            web.Settings.AreDevToolsEnabled=false;
            web.Settings.AreHostObjectsAllowed=false;
            web.Settings.AreDefaultScriptDialogsEnabled=false;
            web.Settings.IsStatusBarEnabled=false;
            web.Settings.IsZoomControlEnabled=false;
            web.NavigationStarting += async (_,e)=> { if(e.Uri!=documentUrl) { e.Cancel=true; await HandleUrlAsync(e.Uri); } };
            web.NavigationCompleted += async (_,e)=> {
                if(!e.IsSuccess){StatusText.Text=UiText.T("RenderError")+e.WebErrorStatus;return;}
                ready=true;await ApplyPreferencesAsync();if(pendingFragment.Length>0)await JumpAsync(pendingFragment);await SearchAsync();
                if(diagnosticDirectory.Length>0)await RunDiagnosticsAsync();
            };
            web.NewWindowRequested += async (_,e)=> { e.Handled=true; await HandleUrlAsync(e.Uri); };
            web.DownloadStarting += (_,e)=> e.Cancel=true;
            web.PermissionRequested += (_,e)=> e.State=CoreWebView2PermissionState.Deny;
            web.AddWebResourceRequestedFilter("*",CoreWebView2WebResourceContext.All);
            web.WebResourceRequested += (_,e)=>ServeImage(environment,e);
            web.WebMessageReceived += async (_,e)=>await MessageAsync(e);
            initialized=true; await ApplyPreferencesAsync();
            string? file=args.FirstOrDefault(a=>File.Exists(a)&&LinkResolver.IsMarkdown(a));
            if(file!=null)await LoadFileAsync(file);
            else if(settings.Recent.FirstOrDefault() is string recent && File.Exists(recent)) await LoadFileAsync(recent);
            else await LoadFileAsync(Path.Combine(AppContext.BaseDirectory,UiText.GuideFile));
        }
        catch(WebView2RuntimeNotFoundException) { ShowError(UiText.T("RuntimeMissing")); }
        catch(Exception ex) { ShowError(UiText.T("StartError")+ex.Message); }
    }
    private void ServeImage(CoreWebView2Environment env, CoreWebView2WebResourceRequestedEventArgs e)
    {
        if(e.Request.Uri==documentUrl) { e.Response=env.CreateWebResourceResponse(new MemoryStream(documentBytes),200,"OK","Content-Type: text/html; charset=utf-8\r\nCache-Control: no-store");return; }
        if(rendered!=null && Uri.TryCreate(e.Request.Uri,UriKind.Absolute,out var uri) && uri.GetLeftPart(UriPartial.Authority)==DocumentRenderer.Origin && uri.AbsolutePath.StartsWith("/image/") && int.TryParse(uri.AbsolutePath[7..],out int id) && rendered.Images.TryGetValue(id,out string? image))
        {
            try { if(new FileInfo(image).Length<=20*1024*1024) {
                string mime=Path.GetExtension(image).ToLowerInvariant() switch { ".jpg" or ".jpeg"=>"image/jpeg", ".gif"=>"image/gif", ".webp"=>"image/webp", ".bmp"=>"image/bmp", ".ico"=>"image/x-icon", _=>"image/png" };
                e.Response=env.CreateWebResourceResponse(new MemoryStream(File.ReadAllBytes(image)),200,"OK","Content-Type: "+mime+"\r\nCache-Control: no-store"); return;
            }}catch(IOException){}catch(UnauthorizedAccessException){}
        }
        e.Response=env.CreateWebResourceResponse(new MemoryStream(),403,"Blocked","");
    }
    private static string Resource(string name)
    {
        using var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("AulaMarkdown.Web."+name)!;
        using var reader=new StreamReader(stream);return reader.ReadToEnd();
    }
    internal async Task LoadFileAsync(string path,string fragment="",bool remember=true)
    {
        try
        {
            path=Path.GetFullPath(path);
            if(!File.Exists(path))throw new FileNotFoundException(UiText.T("FileMissing")+path);
            if(new FileInfo(path).Length>16*1024*1024)throw new IOException(UiText.T("SizeLimit"));
            long request=++loadId;
            string text=await File.ReadAllTextAsync(path);
            var result=await Task.Run(()=>DocumentRenderer.Render(text,path));
            if(request!=loadId)return;
            if(remember && currentPath.Length>0 && currentPath!=path)history.Push(currentPath);
            EditButton.IsEnabled=true;
            currentPath=path; rendered=result; pendingFragment=fragment; ready=false;
            FileLabel.Text=Path.GetFileName(path);FileLabel.ToolTip=path;Title=AppInfo.WindowTitle(Path.GetFileName(path));
            Outline.ItemsSource=result.Headings;HeadingCount.Text=result.Headings.Count+" "+UiText.T("Sections");
            LinkCount.Text=result.Links.Count+" "+UiText.T("Links");StatusText.Text=path;
            BackButton.IsEnabled=history.Count>0;
            settings.RememberFile(path);SaveSettings();
            string nonce=Guid.NewGuid().ToString("N");
            string html="<!doctype html><html lang='it'><head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'><meta http-equiv='Content-Security-Policy' content=\"default-src 'none'; img-src "+DocumentRenderer.Origin+"; style-src 'unsafe-inline'; script-src 'nonce-"+nonce+"';\"><style>"+Resource("reader.css")+"</style></head><body><main>"+result.Html+"</main><script nonce='"+nonce+"'>"+Resource("reader.js")+"</script></body></html>";
            documentBytes=System.Text.Encoding.UTF8.GetBytes(html);documentUrl=DocumentRenderer.Origin+"/document/"+nonce;Browser.CoreWebView2.Navigate(documentUrl);
        }
        catch(Exception ex){ShowError(ex.Message);}
    }
    private async Task MessageAsync(CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            if(e.Source!=documentUrl)return;
            using var data=JsonDocument.Parse(e.WebMessageAsJson);var root=data.RootElement;
            string type=root.GetProperty("type").GetString()??"";
            if(type=="ready") { return; }
            else if(type=="link")await HandleUrlAsync(root.GetProperty("value").GetString()??"");
            else if(type=="linkContext")ShowLinkContextMenu(root.GetProperty("value").GetString()??"");
            else if(type=="drop") { var file=e.AdditionalObjects.OfType<CoreWebView2File>().FirstOrDefault(f=>LinkResolver.IsMarkdown(f.Path));if(file!=null)await LoadFileAsync(file.Path); }
            else if(type=="zoom")await ChangeSizeAsync(root.GetProperty("value").GetInt32());
            else if(type=="fullscreen")await ToggleFullscreenAsync();
            else if(type=="key")await ShortcutAsync(root.GetProperty("value").GetString()??"");
        }
        catch(Exception ex){ShowError(ex.Message);}
    }
    private async Task HandleUrlAsync(string url)
    {
        if(rendered==null)return;
        if(Uri.TryCreate(url,UriKind.Absolute,out var uri) && uri.GetLeftPart(UriPartial.Authority)==DocumentRenderer.Origin && uri.AbsolutePath.StartsWith("/link/") && int.TryParse(uri.AbsolutePath[6..],out var id) && rendered.Links.TryGetValue(id,out string? raw)) await OpenLinkAsync(raw);
    }
    private ContextMenu? localLinkMenu;
    private ContextMenu? CreateLinkContextMenu(string url)
    {
        if(rendered==null || !Uri.TryCreate(url,UriKind.Absolute,out var uri)
            || uri.GetLeftPart(UriPartial.Authority)!=DocumentRenderer.Origin
            || !uri.AbsolutePath.StartsWith("/link/")
            || !int.TryParse(uri.AbsolutePath[6..],out int id)
            || !rendered.Links.TryGetValue(id,out string? raw))return null;
        var target=LinkResolver.Resolve(raw,currentPath);
        if(target.Kind!=LinkKind.Local)return null;
        var menu=new ContextMenu();
        var item=new MenuItem{Header=UiText.T("LinkFolder")};
        item.Click+=(_,_)=>RevealLocalFile(target.Value);
        menu.Items.Add(item);
        return menu;
    }
    private void ShowLinkContextMenu(string url)
    {
        if(localLinkMenu!=null)localLinkMenu.IsOpen=false;
        localLinkMenu=CreateLinkContextMenu(url);
        if(localLinkMenu==null)return;
        localLinkMenu.PlacementTarget=Browser;
        localLinkMenu.Placement=System.Windows.Controls.Primitives.PlacementMode.MousePoint;
        localLinkMenu.IsOpen=true;
    }
    private void RevealLocalFile(string path)
    {
        try
        {
            string? folder=Directory.Exists(path)?path:Path.GetDirectoryName(path);
            if(folder==null || !Directory.Exists(folder)){ShowError(UiText.T("FolderMissing")+folder);return;}
            // Launch Explorer explicitly: never execute the linked file.
            var info=new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),"explorer.exe"))
            {
                UseShellExecute=true,
                Arguments=File.Exists(path)?"/select,\""+path+"\"":"\""+folder+"\""
            };
            Process.Start(info);
        }
        catch(Exception ex){ShowError(UiText.T("OpenError")+ex.Message);}
    }
    internal async Task OpenLinkAsync(string raw)
    {
        try
        {
            var target=LinkResolver.Resolve(raw,currentPath);
            if(target.Kind==LinkKind.Anchor){await JumpAsync(target.Value);return;}
            if(target.Kind==LinkKind.Web){Process.Start(new ProcessStartInfo(target.Value){UseShellExecute=true});return;}
            if(target.Kind!=LinkKind.Local){ShowError(UiText.T("UnsupportedLink")+raw);return;}
            if(!File.Exists(target.Value)&&!Directory.Exists(target.Value)){ShowError(UiText.T("LocalMissing")+target.Value);return;}
            if(LinkResolver.IsMarkdown(target.Value)){await LoadFileAsync(target.Value,target.Fragment);return;}
            if(LinkResolver.RequiresConfirmation(target.Value) && MessageBox.Show(this,UiText.T("OpenProgramQuestion")+target.Value,UiText.T("OpenFileTitle"),MessageBoxButton.YesNo,MessageBoxImage.Question)!=MessageBoxResult.Yes)return;
            if(Path.GetExtension(target.Value).Equals(".fprg",StringComparison.OrdinalIgnoreCase))
            {
                string exe=settings.FlowgorithmPath;
                if(!File.Exists(exe)) exe=new[]{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}.Select(p=>Path.Combine(p,"Flowgorithm","Flowgorithm.exe")).FirstOrDefault(File.Exists)??"";
                if(File.Exists(exe)){StartWith(exe,target.Value);StatusText.Text=UiText.T("OpenedFlowgorithm")+Path.GetFileName(target.Value);return;}
            }
            try { Process.Start(new ProcessStartInfo(target.Value){UseShellExecute=true}); }
            catch(System.ComponentModel.Win32Exception ex) when(ex.NativeErrorCode is 1155 or 31) { SelectProgramAndOpen(target.Value); }
            StatusText.Text=UiText.T("OpenedLocal")+target.Value;
        }
        catch(Exception ex){ShowError(UiText.T("OpenError")+ex.Message);}
    }
    private static void StartWith(string program,string path)
    { var start=new ProcessStartInfo(program){UseShellExecute=false};start.ArgumentList.Add(path);Process.Start(start); }
    private void SelectProgramAndOpen(string path)
    {
        var dialog=new OpenFileDialog{Title=UiText.T("ChooseProgram")+Path.GetFileName(path),Filter=UiText.T("ProgramFilter")};
        if(dialog.ShowDialog(this)!=true)return;
        if(Path.GetExtension(path).Equals(".fprg",StringComparison.OrdinalIgnoreCase)){settings.FlowgorithmPath=dialog.FileName;SaveSettings();}
        StartWith(dialog.FileName,path);
    }
    private async Task JumpAsync(string id){if(ready)await Browser.ExecuteScriptAsync("goHeading("+JsonSerializer.Serialize(id)+")");}
    private async Task ApplyPreferencesAsync()
    {
        var t=Theme; foreach(var pair in new Dictionary<string,string>{["WindowBg"]=t.Window,["PanelBg"]=t.Panel,["TextBrush"]=t.Text,["MutedBrush"]=t.Muted,["LineBrush"]=t.Line,["AccentBrush"]=t.Accent,["AccentTextBrush"]=t.Scheme=="dark"?"#071426":"#FFFFFF"})Resources[pair.Key]=new SolidColorBrush((Color)ColorConverter.ConvertFromString(pair.Value));
        UpdateChrome();SizeButton.Content=settings.FontSize+" px";
        if(ready){await Browser.ExecuteScriptAsync("applyStyle("+JsonSerializer.Serialize(new{colors=t.Colors,font=settings.Font,size=settings.FontSize,wide=settings.Wide})+")");await Browser.ExecuteScriptAsync("setUiLanguage("+JsonSerializer.Serialize(UiText.T("TableScroll"))+")");await Browser.ExecuteScriptAsync("setReadingMode("+JsonSerializer.Serialize(fullscreen)+","+JsonSerializer.Serialize(UiText.T("ExitReading"))+")");}
    }
    private void SaveSettings(){if(diagnosticDirectory.Length>0)return;try{settings.Save();}catch(Exception ex){StatusText.Text=UiText.T("SettingsError")+ex.Message;}}
    private void ShowError(string text){if(diagnosticDirectory.Length>0){Directory.CreateDirectory(diagnosticDirectory);File.WriteAllText(Path.Combine(diagnosticDirectory,"error.txt"),text);Close();return;}StatusText.Text=text.Replace('\n',' ');MessageBox.Show(this,text,"AulaMarkdown",MessageBoxButton.OK,MessageBoxImage.Information);}
    private async Task ChangeSizeAsync(int change){settings.FontSize=Math.Clamp(settings.FontSize+change,12,36);await ApplyPreferencesAsync();SaveSettings();}
    private async void Preferences_Changed(object sender,SelectionChangedEventArgs e){if(!initialized||changingLanguage)return;settings.Theme=ThemeBox.SelectedValue as string??"Chiaro";settings.Font=FontBox.SelectedItem as string??"Segoe UI";settings.Wide=LayoutBox.SelectedIndex==1;await ApplyPreferencesAsync();SaveSettings();}
    private async void Smaller_Click(object s,RoutedEventArgs e)=>await ChangeSizeAsync(-1);
    private async void Larger_Click(object s,RoutedEventArgs e)=>await ChangeSizeAsync(1);
    private async void ResetSize_Click(object s,RoutedEventArgs e){settings.FontSize=18;await ApplyPreferencesAsync();SaveSettings();}
    private void Open_Click(object s,RoutedEventArgs e)=>ChooseFile();
    private async void ChooseFile(){if(!initialized)return;var d=new OpenFileDialog{Title=UiText.T("OpenDocument"),Filter=UiText.T("MarkdownFilter")};if(d.ShowDialog(this)==true)await LoadFileAsync(d.FileName);}
    private async void Back_Click(object s,RoutedEventArgs e){if(history.TryPop(out var p))await LoadFileAsync(p,remember:false);}
    private async void Index_Click(object s,RoutedEventArgs e)=>await ToggleIndexAsync();
    private async Task ToggleIndexAsync(){if(fullscreen)await ToggleFullscreenAsync();settings.Sidebar=!settings.Sidebar;await ApplyPreferencesAsync();SaveSettings();}
    private async void Outline_Changed(object s,SelectionChangedEventArgs e){if(Outline.SelectedItem is HeadingItem h)await JumpAsync(h.Id);}
    private void Find_Click(object s,RoutedEventArgs e)=>ShowSearch();
    private void ShowSearch(){SearchPanel.Visibility=Visibility.Visible;SearchBox.Focus();SearchBox.SelectAll();}
    private async void CloseFind_Click(object s,RoutedEventArgs e){SearchBox.Text="";SearchPanel.Visibility=Visibility.Collapsed;await SearchAsync();Browser.Focus();}
    private async void Search_Changed(object s,TextChangedEventArgs e)=>await SearchAsync();
    private async void NextFind_Click(object s,RoutedEventArgs e)=>await SearchAsync(1);
    private async void PreviousFind_Click(object s,RoutedEventArgs e)=>await SearchAsync(-1);
    private async void Search_KeyDown(object s,KeyEventArgs e){if(e.Key==Key.Enter){await SearchAsync(Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)?-1:1);e.Handled=true;}if(e.Key==Key.Escape)CloseFind_Click(s,e);}
    private async Task SearchAsync(int direction=0)
    {
        if(!ready)return;
        string result=await Browser.ExecuteScriptAsync("findText("+JsonSerializer.Serialize(SearchBox.Text)+","+direction+")");
        using var doc=JsonDocument.Parse(result);if(doc.RootElement.ValueKind!=JsonValueKind.Object)return;
        SearchCount.Text=doc.RootElement.GetProperty("index")+" / "+doc.RootElement.GetProperty("count");
    }
    private async void Full_Click(object s,RoutedEventArgs e)=>await ToggleFullscreenAsync();
    private async Task ToggleFullscreenAsync()
    {
        if(fullscreen){
            fullscreen=false;WindowState=WindowState.Normal;ResizeMode=ResizeMode.CanResize;
            WindowChrome.SetWindowChrome(this,normalChrome);WindowState=previousState;
        } else {
            previousState=WindowState;normalChrome=WindowChrome.GetWindowChrome(this);
            fullscreen=true;SearchPanel.Visibility=Visibility.Collapsed;
            WindowState=WindowState.Normal;WindowChrome.SetWindowChrome(this,null);
            ResizeMode=ResizeMode.NoResize;WindowState=WindowState.Maximized;
        }
        UpdateChrome();UpdateLayout();await ApplyPreferencesAsync();Browser.Focus();
    }
    private void UpdateChrome()
    {
        TitleBar.Visibility=ToolbarPanel.Visibility=StatusPanel.Visibility=fullscreen?Visibility.Collapsed:Visibility.Visible;
        IndexPanel.Visibility=!fullscreen&&settings.Sidebar?Visibility.Visible:Visibility.Collapsed;
        IndexRail.Visibility=!fullscreen&&!settings.Sidebar?Visibility.Visible:Visibility.Collapsed;
        IndexColumn.Width=new GridLength(fullscreen?0:settings.Sidebar?212:26);
        IndexButton.ToolTip=UiText.T(settings.Sidebar?"CollapseIndex":"ExpandIndex")+" (Ctrl+I)";
    }
    private void RefreshLanguage()
    {
        changingLanguage=true;
        settings.Language=settings.Language=="en"?"en":"it";
        UiText.Apply(Resources,settings.Language);LanguageBox.SelectedValue=settings.Language;
        ThemeBox.ItemsSource=ReaderTheme.All.Select(t=>new UiChoice(t.Name,UiText.ThemeLabel(t.Name))).ToList();ThemeBox.SelectedValue=settings.Theme;
        if(ThemeBox.SelectedIndex<0)ThemeBox.SelectedIndex=0;
        LayoutBox.ItemsSource=new[]{new UiChoice("reading",UiText.T("Reading")),new UiChoice("tables",UiText.T("Tables"))};LayoutBox.SelectedIndex=settings.Wide?1:0;
        if(rendered!=null){HeadingCount.Text=rendered.Headings.Count+" "+UiText.T("Sections");LinkCount.Text=rendered.Links.Count+" "+UiText.T("Links");}
        changingLanguage=false;
    }
    private async void Language_Changed(object s,SelectionChangedEventArgs e)
    {
        if(!initialized||changingLanguage)return;
        settings.Language=LanguageBox.SelectedValue as string??"it";RefreshLanguage();await ApplyPreferencesAsync();SaveSettings();
    }
    private void Minimize_Click(object s,RoutedEventArgs e)=>WindowState=WindowState.Minimized;
    private void Maximize_Click(object s,RoutedEventArgs e)=>WindowState=WindowState==WindowState.Maximized?WindowState.Normal:WindowState.Maximized;
    private void Close_Click(object s,RoutedEventArgs e)=>Close();
    private async void About_Click(object s,RoutedEventArgs e)=>await ShowAboutAsync();
    private async Task ShowAboutAsync()
    {
        var about=new AboutWindow(this);about.ShowDialog();
        if(about.RequestedDocument.Length>0)await LoadFileAsync(Path.Combine(AppContext.BaseDirectory,about.RequestedDocument));
    }
    private async Task ShortcutAsync(string key){switch(key){case "f5":if(currentPath.Length>0)await LoadFileAsync(currentPath,remember:false);break;case "back":Back_Click(this,new RoutedEventArgs());break;case "escape":if(fullscreen)await ToggleFullscreenAsync();else if(SearchPanel.Visibility==Visibility.Visible)CloseFind_Click(this,new RoutedEventArgs());break;case "i":await ToggleIndexAsync();break;case "f1":await ShowAboutAsync();break;case "+":case "=":await ChangeSizeAsync(1);break;case "-":await ChangeSizeAsync(-1);break;case "0":settings.FontSize=18;await ApplyPreferencesAsync();SaveSettings();break;case "f":ShowSearch();break;case "o":ChooseFile();break;}}
    private async void Window_KeyDown(object s,KeyEventArgs e)
    {
        if(e.Key==Key.F5){if(currentPath.Length>0)await LoadFileAsync(currentPath,remember:false);e.Handled=true;}
        else if(e.Key==Key.F1){await ShowAboutAsync();e.Handled=true;}
        else if(e.Key==Key.F11){await ToggleFullscreenAsync();e.Handled=true;}
        else if(e.Key==Key.Escape&&fullscreen){await ToggleFullscreenAsync();e.Handled=true;}
        else if(e.SystemKey==Key.Left&&Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)){Back_Click(s,e);e.Handled=true;}
        else if(Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) { string? k=e.Key switch{Key.O=>"o",Key.F=>"f",Key.I=>"i",Key.Add or Key.OemPlus=>"+",Key.Subtract or Key.OemMinus=>"-",Key.D0 or Key.NumPad0=>"0",_=>null};if(k!=null){await ShortcutAsync(k);e.Handled=true;} }
    }
    private async void Window_Drop(object s,DragEventArgs e){if(initialized&&e.Data.GetData(DataFormats.FileDrop) is string[] paths&&paths.FirstOrDefault(LinkResolver.IsMarkdown) is string p){e.Handled=true;await LoadFileAsync(p);}}
    private ContextMenu CreateMenu()
    {
        var menu=new ContextMenu();
        void Add(string label,Action action){var item=new MenuItem{Header=label};item.Click+=(_,_)=>action();menu.Items.Add(item);}
        Add(UiText.T("Reload"),async()=>{if(currentPath.Length>0)await LoadFileAsync(currentPath,remember:false);});
        Add(UiText.T("Folder"),()=>{if(currentPath.Length>0)Process.Start(new ProcessStartInfo(Path.GetDirectoryName(currentPath)!){UseShellExecute=true});});
        menu.Items.Add(new Separator());
        foreach(string path in settings.Recent.Take(ReaderSettings.RecentLimit))
        {
            string p=path;
            var item=new MenuItem{Header=new TextBlock{Text=Path.GetFileName(p)},ToolTip=p};
            item.Click+=async(_,_)=>await LoadFileAsync(p);
            menu.Items.Add(item);
        }
        if(settings.Recent.Count==0)menu.Items.Add(new MenuItem{Header=UiText.T("NoRecentFiles"),IsEnabled=false});
        menu.Items.Add(new Separator());
        Add(UiText.T("ChooseEditor"),ChooseEditor);
        Add(UiText.T("ResetEditor"),()=>{settings.EditorPath="";SaveSettings();StatusText.Text=UiText.T("ResetEditor");});
        Add(UiText.T("Guide"),async()=>await LoadFileAsync(Path.Combine(AppContext.BaseDirectory,UiText.GuideFile)));
        Add(UiText.T("DefaultApp"),SetDefaultApp);
        Add(UiText.T("AboutMenu"),async()=>await ShowAboutAsync());
        return menu;
    }
    private void ChooseEditor()
    {
        var dialog=new OpenFileDialog{Title=UiText.T("ChooseEditor"),Filter=UiText.T("ProgramFilter"),CheckFileExists=true};
        if(dialog.ShowDialog(this)!=true)return;
        if(Path.GetFileName(dialog.FileName).Equals("AulaMarkdown.exe",StringComparison.OrdinalIgnoreCase))
        { ShowError(UiText.T("ChooseEditor"));return; }
        settings.EditorPath=dialog.FileName;SaveSettings();
        StatusText.Text=UiText.T("EditorSaved")+dialog.FileName;
    }
    internal ProcessStartInfo EditorStartInfo(string document)
    {
        string editor=string.IsNullOrEmpty(settings.EditorPath)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),"notepad.exe")
            : settings.EditorPath;
        if(!File.Exists(editor))throw new FileNotFoundException(UiText.T("EditorMissing"));
        var start=new ProcessStartInfo(editor){UseShellExecute=false};
        start.ArgumentList.Add(document);
        return start;
    }
    private void Edit_Click(object sender,RoutedEventArgs e)
    {
        try
        {
            if(currentPath.Length==0)return;
            if(!File.Exists(currentPath)){ShowError(UiText.T("FileMissing")+currentPath);return;}
            Process.Start(EditorStartInfo(currentPath));
            StatusText.Text=UiText.T("EditorOpened");
        }
        catch(Exception ex){ShowError(UiText.T("OpenError")+ex.Message);}
    }
    private void SetDefaultApp()
    {
        try
        {
            DefaultAppRegistration.Register();
            MessageBox.Show(this,UiText.T("DefaultAppHelp"),AppInfo.Product,MessageBoxButton.OK,MessageBoxImage.Information);
            DefaultAppRegistration.OpenSettings();
        }
        catch(Exception ex)
        {
            MessageBox.Show(this,UiText.T("DefaultAppError")+ex.Message,AppInfo.Product,MessageBoxButton.OK,MessageBoxImage.Error);
        }
    }
    private void Menu_Click(object sender,RoutedEventArgs e)
    {
        var menu=CreateMenu();menu.PlacementTarget=(Button)sender;menu.IsOpen=true;
    }
}