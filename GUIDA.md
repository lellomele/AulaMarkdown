# Leggi con i tuoi colori

Benvenuto in **AulaMarkdown**, il visualizzatore per documenti e materiali didattici sul tuo PC.

Usa **Apri…** o trascina qui un file `.md`. Puoi regolare colori e caratteri in qualunque momento: il documento originale resta invariato.

## Una lettura su misura

| Controllo | Cosa fa |
|---|---|
| **Tema** | Scegli fra Chiaro, Carta, Scuro, Blu notte e Alto contrasto. |
| **Font** | Segoe UI, Arial, Verdana o Georgia. |
| **A− / A+** | Riduci o aumenta il testo. Premi la dimensione per tornare a 18 px. |
| **Lettura / Tabelle** | Passa da una pagina raccolta a una vista ampia. |
| **Indice** | Riduce l’elenco delle sezioni a una linguetta; premi › per riaprirlo. |
| **Italiano / English** | Cambia la lingua dell’interfaccia; il documento conserva la lingua originale. |
| **Cerca** | Cerca parole nel documento e passa al risultato successivo. |

> Premi **Solo lettura / F11** per visualizzare soltanto il documento: barra del titolo, comandi, stato e indice scompaiono. **Esc**, **F11** o il piccolo pulsante **Esci · Esc** ripristinano la finestra e la disposizione precedente.

## Collegamenti ai file locali

I link locali vengono aperti sul PC. I documenti `.fprg` si aprono in **Flowgorithm**; altri file Markdown restano in questa finestra; PDF, immagini e documenti usano l'applicazione associata in Windows.

L’apertura dei `.fprg` è automatica. Il programma cerca Flowgorithm e usa le associazioni dei file di Windows. Se non trova un’applicazione associata, consente di indicarne una soltanto al momento dell’apertura.

Questi sono esempi di sintassi da scrivere nel Markdown:

```markdown
[Esempio relativo](esempi_integrativi/01_input_output_scambio.fprg)
[File con spazi](<G:/Il mio Drive/Lezioni/esempio.fprg>)
[URI locale](file:///G:/Lezioni/esempio.fprg)
[Cartella di rete](<\\server\materiali\esempio.fprg>)
[Altra dispensa](<dispense/lezione 2.md>)
[Sezione interna](#collegamenti-ai-file-locali)
```

Il percorso relativo parte dalla cartella del file Markdown, anche quando avvii il programma da un'altra cartella. Se un file manca, il messaggio mostra esattamente dove è stato cercato.

## Scorciatoie

| Tasti | Azione |
|---|---|
| Ctrl+O | Apri documento |
| Ctrl+F | Cerca nel testo |
| Ctrl+rotellina | Cambia dimensione del carattere |
| Ctrl++ / Ctrl+− | Ingrandisci / riduci |
| Ctrl+0 | Dimensione iniziale |
| Invio / Maiusc+Invio nella ricerca | Risultato successivo / precedente |
| F11 / Esc | Entra / esci dalla lettura a tutto schermo |
| Ctrl+I | Riduci / espandi l’indice |
| F1 | Informazioni sul programma |
| F5 | Ricarica il documento |
| Alt+Sinistra | Torna al documento precedente |

## Per continuare

I documenti recenti sono nel menu **⋯**. Puoi aprire la cartella del documento dallo stesso menu. Tema, dimensione, font, lingua e stato dell’indice vengono ricordati alla prossima apertura.

[Leggi i requisiti e le istruzioni complete](README.md)

## Informazioni

La voce **⋯ → Informazioni / About…**, il pulsante **ⓘ** nella barra del titolo e **F1** mostrano versione, copyright, guida e licenze.

© 2026 - Prof. Ing. Raffaele Mele

## App predefinita

Nel menu ⋯ scegli **Imposta come app predefinita…**, quindi conferma AulaMarkdown nelle impostazioni di Windows per `.md`, `.markdown` e `.mdown`. Non occorrono privilegi di amministratore. Se sposti la cartella del programma, ripeti questa operazione dalla nuova posizione.

Con il **tasto destro su un link locale → Apri cartella del file** apri Esplora file con il file selezionato. Se il file non esiste più, viene aperta la cartella quando disponibile. I link a cartelle aprono direttamente la cartella.


Nel menu **⋯**, evidenziato come **Apri**, gli ultimi 10 documenti sono elencati direttamente, senza sottomenu. La cronologia resta salvata dopo la chiusura; il percorso completo appare passando il mouse sul nome. I temi sono **Chiaro, Carta, Scuro, Blu notte, Alto contrasto e Foresta**.

**Modifica** apre il documento in Blocco note. Per usare un altro programma, scegli **⋯ → Scegli editor esterno…** e seleziona il suo eseguibile; la scelta viene ricordata. **Usa Blocco note per modificare** ripristina l’editor iniziale. Dopo aver salvato nell’editor, torna al visualizzatore e premi **F5** per aggiornare il documento.
