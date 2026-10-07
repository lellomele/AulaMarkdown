using System.IO;
using System.Text.Json;
using AulaMarkdown.Core;
using Microsoft.Web.WebView2.Core;
namespace AulaMarkdown;
public partial class MainWindow
{
    private bool diagnosticsStarted;
    private async Task RunDiagnosticsAsync()
    {
        if(diagnosticsStarted)return;diagnosticsStarted=true;
        Directory.CreateDirectory(diagnosticDirectory);
        try
        {
            var checks=new List<object>();
            async Task Inspect(string label,string expression)
            {
                string json=await Browser.ExecuteScriptAsync(expression);
                using var doc=JsonDocument.Parse(json);
                if(doc.RootElement.ValueKind==JsonValueKind.False||doc.RootElement.ValueKind==JsonValueKind.Null)throw new Exception("Controllo fallito: "+label+" / "+json);
                checks.Add(new{label,result=doc.RootElement.Clone()});
            }
            await Inspect("documento renderizzato","document.querySelectorAll('main h1').length > 0");
            await Inspect("tabelle renderizzate","document.querySelectorAll('.table-scroll table').length > 0");
            await Inspect("link intercettati","Array.from(document.querySelectorAll('main a')).every(a=>a.getAttribute('href').startsWith('https://aulamarkdown.invalid/link/'))");
            foreach(var t in ReaderTheme.All)
            {
                settings.Theme=t.Name;await ApplyPreferencesAsync();
                await Inspect("tema "+t.Name,"getComputedStyle(document.documentElement).getPropertyValue('--paper').trim() === "+JsonSerializer.Serialize(t.Paper));
            }
            foreach(string font in new[]{"Segoe UI","Arial","Verdana","Georgia"})
            {
                settings.Font=font;await ApplyPreferencesAsync();
                await Inspect("font "+font,"getComputedStyle(document.documentElement).getPropertyValue('--font').trim() === "+JsonSerializer.Serialize(font));
            }
            settings.FontSize=26;settings.Font="Segoe UI";settings.Theme="Chiaro";await ApplyPreferencesAsync();
            await Inspect("dimensione 26 px","getComputedStyle(document.body).fontSize === '26px'");
            settings.Wide=false;await ApplyPreferencesAsync();await Inspect("larghezza lettura","!document.body.classList.contains('wide')");
            settings.Wide=true;settings.FontSize=18;await ApplyPreferencesAsync();
            await Inspect("ricerca nel testo","findText('Flowgorithm').count > 0");
            await Browser.ExecuteScriptAsync("findText('');window.scrollTo(0,0)");
            await Task.Delay(250);
            using(var image=File.Create(Path.Combine(diagnosticDirectory,"lettura-chiara.png")))await Browser.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png,image);
            settings.Theme="Blu notte";await ApplyPreferencesAsync();await Task.Delay(200);
            using(var image=File.Create(Path.Combine(diagnosticDirectory,"lettura-blu-notte.png")))await Browser.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png,image);
            settings.Theme="Chiaro";await ApplyPreferencesAsync();
            void Check(string label,bool result){if(!result)throw new Exception("Controllo fallito: "+label);checks.Add(new{label,result});}
            Check("copyright nella barra del titolo",CopyrightLabel.Text=="(c) 2026 - Prof. Ing. Raffaele Mele"&&Title.Contains(AppInfo.Copyright));
            Check("barra compatta",TitleBar.ActualHeight<=32&&ToolbarPanel.ActualHeight<=40);
            Check("voce Flowgorithm rimossa",!CreateMenu().Items.OfType<System.Windows.Controls.MenuItem>().Any(m=>(m.Header?.ToString()??"").Contains("Scegli Flowgorithm")));
            string documentBefore=await Browser.ExecuteScriptAsync("document.querySelector('main').innerText");
            LanguageBox.SelectedValue="en";await Task.Delay(150);
            Check("interfaccia inglese",UiText.Language=="en"&&FindResource("Ui.Open").ToString()=="Open…"&&((UiChoice)ThemeBox.SelectedItem).Label=="Light");
            ThemeBox.UpdateLayout();
            Check("etichetta visibile nel selettore tema",VisualText(ThemeBox).Contains("Light")&&!VisualText(ThemeBox).Contains("UiChoice"));
            await Inspect("etichette tabella inglesi","Array.from(document.querySelectorAll('.table-scroll')).every(t=>t.getAttribute('aria-label')==='Table: scroll to read all columns')");
            Check("documento invariato al cambio lingua",documentBefore==await Browser.ExecuteScriptAsync("document.querySelector('main').innerText"));
            await CaptureWindowAsync(Path.Combine(diagnosticDirectory,"english.png"));
            var about=new AboutWindow(this){ShowActivated=false};about.Show();await Task.Delay(100);
            Check("About inglese con copyright",about.Title=="About — AulaMarkdown"&&about.CopyrightText.Text==AppInfo.Copyright);
            CaptureElement((System.Windows.FrameworkElement)about.Content,Path.Combine(diagnosticDirectory,"about-english.png"));about.Close();
            LanguageBox.SelectedValue="it";await Task.Delay(150);
            Check("ritorno all'italiano",UiText.Language=="it"&&FindResource("Ui.Open").ToString()=="Apri…");
            await ToggleIndexAsync();UpdateLayout();
            Check("indice ridotto a linguetta",!settings.Sidebar&&IndexPanel.Visibility==System.Windows.Visibility.Collapsed&&IndexRail.Visibility==System.Windows.Visibility.Visible&&IndexColumn.ActualWidth==26);
            await CaptureWindowAsync(Path.Combine(diagnosticDirectory,"indice-ridotto.png"));
            var beforeState=WindowState;
            await ToggleFullscreenAsync();await Task.Delay(200);UpdateLayout();
            Check("solo documento senza pannelli",fullscreen&&TitleBar.Visibility==System.Windows.Visibility.Collapsed&&ToolbarPanel.Visibility==System.Windows.Visibility.Collapsed&&StatusPanel.Visibility==System.Windows.Visibility.Collapsed&&IndexColumn.ActualWidth==0&&Math.Abs(Browser.ActualHeight-RootSurface.ActualHeight)<2);
            await Inspect("uscita lettura disponibile","document.body.classList.contains('reading-mode') && !document.getElementById('exit-reading').hidden");
            await CaptureWindowAsync(Path.Combine(diagnosticDirectory,"solo-lettura.png"));
            await ShortcutAsync("escape");await Task.Delay(100);UpdateLayout();
            Check("Esc ripristina finestra e indice ridotto",!fullscreen&&WindowState==beforeState&&!settings.Sidebar&&IndexRail.Visibility==System.Windows.Visibility.Visible);
            await ToggleIndexAsync();UpdateLayout();
            Check("indice riespanso",settings.Sidebar&&IndexPanel.Visibility==System.Windows.Visibility.Visible&&IndexColumn.ActualWidth==212);
            await CaptureWindowAsync(Path.Combine(diagnosticDirectory,"finestra.png"));
            await Browser.ExecuteScriptAsync("goHeading('percorso-consigliato')");await Task.Delay(200);
            using(var tableImage=File.Create(Path.Combine(diagnosticDirectory,"tabella.png")))await Browser.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png,tableImage);
            int local=0,existing=0;
            foreach(var link in rendered!.Links.Values){var t=LinkResolver.Resolve(link,currentPath);if(t.Kind==LinkKind.Local){local++;if(File.Exists(t.Value)||Directory.Exists(t.Value))existing++;}}
            checks.Add(new{label="file locali nel documento",local,existing});
            if(local!=existing)throw new Exception("Collegamenti locali non risolti");
            if(args.Contains("--open-flowgorithm"))
            {
                var link=rendered.Links.First(x=>x.Value.EndsWith(".fprg",StringComparison.OrdinalIgnoreCase));
                await Browser.ExecuteScriptAsync("document.querySelector('a[href=\"https://aulamarkdown.invalid/link/"+link.Key+"\"]').click()");
                await Task.Delay(1200);
                checks.Add(new{label="clic reale inviato al gestore Flowgorithm",file=link.Value});
            }
            await File.WriteAllTextAsync(Path.Combine(diagnosticDirectory,"integration.json"),JsonSerializer.Serialize(new{success=true,checks},new JsonSerializerOptions{WriteIndented=true}));
            StatusText.Text="Collaudo completato";
        }
        catch(Exception ex){await File.WriteAllTextAsync(Path.Combine(diagnosticDirectory,"integration.json"),JsonSerializer.Serialize(new{success=false,error=ex.ToString()}));}
        if(args.Contains("--exit-after-diagnostics"))Close();
    }
    private async Task CaptureWindowAsync(string output)
    {
        // Capture WPF chrome and composite the native WebView surface at its actual bounds.
        using var stream=new MemoryStream();
        await Browser.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png,stream);stream.Position=0;
        var webImage=new System.Windows.Media.Imaging.BitmapImage();webImage.BeginInit();webImage.CacheOption=System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;webImage.StreamSource=stream;webImage.EndInit();
        var surface=(System.Windows.FrameworkElement)Content;
        var dpi=System.Windows.Media.VisualTreeHelper.GetDpi(surface);
        int width=(int)(surface.ActualWidth*dpi.DpiScaleX),height=(int)(surface.ActualHeight*dpi.DpiScaleY);
        var frame=new System.Windows.Media.Imaging.RenderTargetBitmap(width,height,dpi.PixelsPerInchX,dpi.PixelsPerInchY,System.Windows.Media.PixelFormats.Pbgra32);frame.Render(surface);
        var drawing=new System.Windows.Media.DrawingVisual();
        using(var dc=drawing.RenderOpen()){
            dc.DrawImage(frame,new System.Windows.Rect(0,0,surface.ActualWidth,surface.ActualHeight));
            var point=Browser.TransformToAncestor(surface).Transform(new System.Windows.Point());
            dc.DrawImage(webImage,new System.Windows.Rect(point.X,point.Y,Browser.ActualWidth,Browser.ActualHeight));
        }
        var result=new System.Windows.Media.Imaging.RenderTargetBitmap(width,height,dpi.PixelsPerInchX,dpi.PixelsPerInchY,System.Windows.Media.PixelFormats.Pbgra32);result.Render(drawing);
        var encoder=new System.Windows.Media.Imaging.PngBitmapEncoder();encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(result));using var file=File.Create(output);encoder.Save(file);
    }

    private static string VisualText(System.Windows.DependencyObject root)
    {
        string result=root is System.Windows.Controls.TextBlock text?text.Text:"";
        for(int i=0;i<System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);i++) result+=" "+VisualText(System.Windows.Media.VisualTreeHelper.GetChild(root,i));
        return result;
    }
    private static void CaptureElement(System.Windows.FrameworkElement element,string output)
    {
        element.UpdateLayout();var dpi=System.Windows.Media.VisualTreeHelper.GetDpi(element);
        var bitmap=new System.Windows.Media.Imaging.RenderTargetBitmap((int)(element.ActualWidth*dpi.DpiScaleX),(int)(element.ActualHeight*dpi.DpiScaleY),dpi.PixelsPerInchX,dpi.PixelsPerInchY,System.Windows.Media.PixelFormats.Pbgra32);bitmap.Render(element);
        var encoder=new System.Windows.Media.Imaging.PngBitmapEncoder();encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));using var file=File.Create(output);encoder.Save(file);
    }

}