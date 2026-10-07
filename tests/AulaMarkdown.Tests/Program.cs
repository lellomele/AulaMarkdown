using AulaMarkdown.Core;
using System.Text.RegularExpressions;
int checks=0;
void Equal<T>(T a,T b,string label){if(!EqualityComparer<T>.Default.Equals(a,b))throw new Exception($"{label}: {a} != {b}");checks++;}
string doc=@"G:\Materiali di prova\lezione.md";
void Local(string raw,string expected,string fragment="") {var t=LinkResolver.Resolve(raw,doc);Equal(t.Kind,LinkKind.Local,raw);Equal(t.Value,expected,raw);Equal(t.Fragment,fragment,"fragment");}
Local("esempi/prova.fprg",@"G:\Materiali di prova\esempi\prova.fprg");
Local("../altro/prova.fprg",@"G:\altro\prova.fprg");
Local("<G:/Il mio Drive/prova con spazi.fprg>",@"G:\Il mio Drive\prova con spazi.fprg");
Local(@"G:\Il mio Drive\città.fprg",@"G:\Il mio Drive\città.fprg");
Local("file:///G:/Il%20mio%20Drive/citt%C3%A0.fprg",@"G:\Il mio Drive\città.fprg");
Local(@"\\server\materiali\lezione.fprg",@"\\server\materiali\lezione.fprg");
Local("file://server/materiali/prova%20uno.fprg",@"\\server\materiali\prova uno.fprg");
Local("//server/materiali/prova.fprg",@"\\server\materiali\prova.fprg");
Local("lezione%202.md#titolo",@"G:\Materiali di prova\lezione 2.md","titolo");
Local("file:///G:/Lezioni/test.md#titolo",@"G:\Lezioni\test.md","titolo");
Local("nome%23parte.fprg",@"G:\Materiali di prova\nome#parte.fprg");
Local("file:///G:/Lezioni/100%25.fprg",@"G:\Lezioni\100%.fprg");
Local("100%25.fprg",@"G:\Materiali di prova\100%.fprg");
Equal(LinkResolver.Resolve("#sezione",doc),new LinkTarget(LinkKind.Anchor,"sezione"),"ancora");
Equal(LinkResolver.Resolve("https://example.org/a?q=1#x",doc).Kind,LinkKind.Web,"web");
foreach(string x in new[]{"javascript:alert(1)","data:text/html,ciao","mailto:a@b.it","powershell:foo","G:relative"})Equal(LinkResolver.Resolve(x,doc).Kind,LinkKind.Unsupported,x);
Equal(LinkResolver.IsMarkdown("TEST.MD"),true,"md maiuscolo");
Equal(LinkResolver.RequiresConfirmation("test.CMD"),true,"script");
Equal(LinkResolver.RequiresConfirmation("test.fprg"),false,"sorgente");
string source="""
# Titolo

| Nome | File |
|---|---|
| Prova | [apri](<G:/Il mio Drive/test.fprg>) |

[Relativo](sub/file.fprg)

<script>alert('x')</script>

[esterno](https://example.org)

[pericoloso](javascript:alert)

![immagine](figura.png)

![remota](https://example.org/a.png)
""";
var r=DocumentRenderer.Render(source,doc);
Equal(r.Html.Contains("<table>"),true,"tabella");
Equal(r.Html.Contains("<script>"),false,"HTML disabilitato");
Equal(r.Links.Count,4,"numero link");
Equal(r.Links[0],"G:/Il mio Drive/test.fprg","percorso preservato");
Equal(r.Html.Contains("https://aulamarkdown.invalid/link/0"),true,"intercettazione");
Equal(r.Images.Count,1,"solo immagine locale");
Equal(r.Headings[0].Id,"titolo","ID titolo");
if(args.Length>0)
{
 string path=Path.GetFullPath(args[0]);var rendered=DocumentRenderer.Render(File.ReadAllText(path),path);
 int local=0;
 foreach(var raw in rendered.Links.Values){var t=LinkResolver.Resolve(raw,path);if(t.Kind==LinkKind.Local){Equal(File.Exists(t.Value),true,t.Value);local++;}}
 Equal(local,121,"link del documento di riferimento");
 Console.WriteLine($"Documento di riferimento: {local} link locali validi.");
}
Console.WriteLine($"Superati {checks} controlli.");
