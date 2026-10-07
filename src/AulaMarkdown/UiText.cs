using System.Windows;
namespace AulaMarkdown;
public sealed record UiChoice(string Value,string Label)
{
    public override string ToString() => Label;
}
public static class UiText
{
    public static string Language { get; private set; } = "it";
    public static readonly Dictionary<string,(string It,string En)> Strings = new()
    {
        ["Open"]=("Apri…","Open…"),
        ["OpenTip"]=("Apri Markdown (Ctrl+O)","Open Markdown (Ctrl+O)"),
        ["BackTip"]=("Documento precedente (Alt+Sinistra)","Previous document (Alt+Left)"),
        ["Theme"]=("Tema colori","Colour theme"),
        ["Font"]=("Carattere","Font"),
        ["Smaller"]=("Riduci carattere (Ctrl+-)","Smaller text (Ctrl+-)"),
        ["Reset"]=("Ripristina 18 px (Ctrl+0)","Reset to 18 px (Ctrl+0)"),
        ["Larger"]=("Ingrandisci carattere (Ctrl++)","Larger text (Ctrl++)"),
        ["Width"]=("Larghezza del testo","Text width"),
        ["Index"]=("Indice","Outline"),
        ["IndexUpper"]=("INDICE","OUTLINE"),
        ["IndexTip"]=("Riduci / espandi indice (Ctrl+I)","Collapse / expand outline (Ctrl+I)"),
        ["CollapseIndex"]=("Riduci il pannello delle sezioni","Collapse the outline panel"),
        ["ExpandIndex"]=("Espandi l’indice","Expand outline"),
        ["Find"]=("Cerca","Find"),
        ["FindTip"]=("Cerca nel testo (Ctrl+F)","Find in document (Ctrl+F)"),
        ["Fullscreen"]=("Solo lettura ⛶","Reading only ⛶"),
        ["FullscreenTip"]=("Documento a tutto schermo, senza barre e indice (F11). Esc per uscire.","Full-screen document, without toolbars or outline (F11). Esc to exit."),
        ["MenuTip"]=("Documenti recenti, guida e informazioni","Recent documents, help and about"),
        ["Close"]=("Chiudi","Close"),
        ["SearchText"]=("Testo da cercare","Find text"),
        ["Starting"]=("Avvio…","Starting…"),
        ["Welcome"]=("Benvenuto","Welcome"),
        ["AboutTip"]=("Informazioni su AulaMarkdown (F1)","About AulaMarkdown (F1)"),
        ["About"]=("Informazioni","About"),
        ["Minimize"]=("Riduci a icona","Minimize"),
        ["Maximize"]=("Massimizza / ripristina finestra","Maximize / restore window"),
        ["Italy"]=("Italia","Italy"),
        ["TableScroll"]=("Tabella: scorrere per leggere tutte le colonne","Table: scroll to read all columns"),
        ["Language"]=("Lingua dell’interfaccia","Interface language"),
        ["Sections"]=("sezioni","sections"),
        ["Links"]=("collegamenti","links"),
        ["Light"]=("Chiaro","Light"),
        ["Paper"]=("Carta","Paper"),
        ["Dark"]=("Scuro","Dark"),
        ["Night"]=("Blu notte","Midnight blue"),
        ["Contrast"]=("Alto contrasto","High contrast"),
        ["Reading"]=("Lettura","Reading"),
        ["Tables"]=("Tabelle","Tables"),
        ["Reload"]=("Ricarica documento (F5)","Reload document (F5)"),
        ["Folder"]=("Apri cartella del documento","Open document folder"),
        ["Guide"]=("Guida","Help"),
        ["AboutMenu"]=("Informazioni / About…","About AulaMarkdown…"),
        ["Version"]=("Versione","Version"),
        ["AboutTitle"]=("Informazioni — AulaMarkdown","About — AulaMarkdown"),
        ["AboutDescription"]=("Visualizzatore di documenti Markdown per Windows.","Markdown document viewer for Windows."),
        ["AboutFeatures"]=("Temi e caratteri personalizzabili, indice delle sezioni e modalità di lettura a tutto schermo. I collegamenti ai file locali si aprono con le applicazioni del PC.","Customisable themes and fonts, document outline and full-screen reading. Local file links open in your PC’s applications."),
        ["AboutShortcuts"]=("F11: solo documento • Esc: torna alla finestra\nCtrl+I: riduci / espandi l’indice","F11: document only • Esc: return to window\nCtrl+I: collapse / expand outline"),
        ["Licenses"]=("Licenze","Licences"),
        ["ExitReading"]=("Esci · Esc","Exit · Esc"),
        ["RenderError"]=("Errore visualizzazione: ","Display error: "),
        ["RuntimeMissing"]=("Serve Microsoft Edge WebView2 Runtime. Consultare README.md per il collegamento e l’installazione offline.","Microsoft Edge WebView2 Runtime is required. See README.md for the download link and offline installation."),
        ["StartError"]=("Impossibile avviare il visualizzatore: ","Unable to start the viewer: "),
        ["FileMissing"]=("File non trovato:\n","File not found:\n"),
        ["SizeLimit"]=("Il documento supera il limite di 16 MB.","The document exceeds the 16 MB limit."),
        ["UnsupportedLink"]=("Tipo di collegamento non supportato:\n","Unsupported link type:\n"),
        ["LocalMissing"]=("File locale non trovato:\n","Local file not found:\n"),
        ["OpenProgramQuestion"]=("Aprire questo programma o collegamento?\n\n","Open this program or shortcut?\n\n"),
        ["OpenFileTitle"]=("Apertura file","Open file"),
        ["OpenedFlowgorithm"]=("Aperto in Flowgorithm: ","Opened in Flowgorithm: "),
        ["OpenedLocal"]=("Aperto file locale: ","Opened local file: "),
        ["OpenError"]=("Impossibile aprire il collegamento:\n","Unable to open link:\n"),
        ["ChooseProgram"]=("Scegli il programma per aprire ","Choose an application to open "),
        ["ProgramFilter"]=("Programmi Windows (*.exe)|*.exe","Windows applications (*.exe)|*.exe"),
        ["SettingsError"]=("Preferenze non salvate: ","Settings could not be saved: "),
        ["OpenDocument"]=("Apri documento Markdown","Open Markdown document"),
        ["MarkdownFilter"]=("Markdown (*.md;*.markdown;*.mdown)|*.md;*.markdown;*.mdown|Tutti i file (*.*)|*.*","Markdown (*.md;*.markdown;*.mdown)|*.md;*.markdown;*.mdown|All files (*.*)|*.*"),
    };
    public static string T(string key) => Language=="en" ? Strings[key].En : Strings[key].It;
    public static void Apply(ResourceDictionary resources,string language)
    {
        Language=language=="en"?"en":"it";
        foreach(var key in Strings.Keys)resources["Ui."+key]=T(key);
    }
    public static string ThemeLabel(string name)=>T(name switch{"Carta"=>"Paper","Scuro"=>"Dark","Blu notte"=>"Night","Alto contrasto"=>"Contrast",_=>"Light"});
    public static string GuideFile=>Language=="en"?"GUIDA.en.md":"GUIDA.md";
}
