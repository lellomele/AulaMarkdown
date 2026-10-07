# AulaMarkdown

Visualizzatore Markdown per Windows, pensato per dispense e tabelle didattiche. Include cinque temi, quattro caratteri leggibili, ricerca, indice, regolazione del testo e apertura esplicita dei file locali.

## Avvio

Scaricare il pacchetto Windows dalla [release più recente](https://github.com/lellomele/AulaMarkdown/releases/latest).

Estrarre tutto il contenuto di `AulaMarkdown-1.1.0-win-x64.zip` in una cartella e avviare `AulaMarkdown.exe`. Non spostare il solo eseguibile. Aprire un `.md` con **Apri…**, trascinandolo nella finestra o passando il percorso come argomento all'eseguibile.

## Requisiti

- Windows 10/11 a 64 bit.
- [Microsoft Edge WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/): già incluso in molti PC; se assente, usare l'installer Microsoft, disponibile anche offline.
- Flowgorithm per aprire i `.fprg`, o il programma associato in Windows per gli altri file.

La distribuzione autonoma include .NET. Documenti, stili e preferenze vengono elaborati localmente; non serve una connessione per leggere file locali.

## Uso

**Tema** cambia i colori; **Font** seleziona Segoe UI, Arial, Verdana o Georgia. **A− / A+** cambia la dimensione fra 12 e 36 px; il pulsante centrale ripristina 18 px. **Lettura** limita la larghezza del testo, **Tabelle** usa tutta la pagina. Il menu `⋯` contiene documenti recenti, ricarica, guida e **Informazioni / About**. L’apertura dei `.fprg` resta automatica.

I collegamenti relativi vengono risolti dalla cartella del documento, quelli assoluti sul disco indicato. Sono gestiti anche URI `file:///`, percorsi UNC e ancore. Altri Markdown si aprono nel visualizzatore; `.fprg` in Flowgorithm; gli altri file con l'app associata. Se manca l'associazione si può scegliere un programma. Solo HTTP/HTTPS apre il browser.

Le preferenze sono conservate in `%LOCALAPPDATA%\AulaMarkdown\settings.json`. La modifica dello stile non modifica i documenti. Per condividere una cartella di dispense fra PC, usare preferibilmente collegamenti relativi.

**Italiano / English** cambia l’intera interfaccia e la guida; il contenuto dei documenti resta nella lingua originale. La scelta viene ricordata. Le finestre di sistema di Windows possono seguire la lingua del sistema operativo.

**Indice**, **‹ / ›** e **Ctrl+I** riducono o espandono il pannello delle sezioni. **Solo lettura / F11** nasconde barre e indice e dedica tutto lo schermo al documento; **Esc**, **F11** o **Esci · Esc** ripristinano la disposizione precedente. La barra del titolo conserva i normali comandi di riduzione a icona, ingrandimento e chiusura.

## Limiti operativi

- Formato Markdown con tabelle, liste, codice, immagini locali e link; HTML incorporato, script, formule LaTeX e diagrammi Mermaid non vengono eseguiti o interpretati.
- Immagini remote non scaricate; immagini locali PNG/JPEG/GIF/WebP/BMP/ICO fino a 20 MB. SVG non visualizzati.
- Documenti fino a 16 MB, letti come UTF-8 (o UTF-16 se identificato dal BOM).
- I file sul Drive devono essere disponibili localmente; per risorse di rete servono connessione e autorizzazioni Windows già valide.
- Le tabelle molto larghe hanno scorrimento orizzontale. La ricerca opera sul testo visualizzato.
- Le associazioni di Windows non vengono modificate. Prima di aprire programmi o script collegati viene richiesta conferma.

## Compilazione

Su Windows, con PowerShell 7, .NET SDK 10 e accesso a NuGet, dalla cartella del progetto eseguire `./tools/Build.ps1`. Lo script compila in una cartella temporanea locale (utile se il progetto è su Google Drive), verifica e prepara cartella e ZIP in `dist`.

In alternativa, per una compilazione diretta su disco locale:

```powershell
dotnet run --project tests/AulaMarkdown.Tests
dotnet publish src/AulaMarkdown/AulaMarkdown.csproj -c Release -r win-x64 --self-contained true -o dist/AulaMarkdown-win-x64
```

L'icona è inclusa nell'eseguibile e nella finestra. Il sorgente vettoriale è in `assets/AulaMarkdown.svg`; lo script `tools/Build-Icon.ps1` rigenera PNG e ICO. Le librerie di terze parti e le rispettive licenze sono elencate in `THIRD-PARTY-NOTICES.md`.

## Copyright

(c) 2026 - Prof. Ing. Raffaele Mele
