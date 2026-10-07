# AulaMarkdown user guide

Open a Markdown document with **Open…**, or drag a `.md` file into the window. Appearance settings never change the original document.

## Compact controls

| Control | Purpose |
|---|---|
| **Theme** | Light, Paper, Dark, Midnight blue or High contrast. |
| **Font** | Segoe UI, Arial, Verdana or Georgia. |
| **A− / A+** | Decrease or increase text size. Click the size to reset it to 18 px. |
| **Reading / Tables** | Choose a narrower reading page or use the available width. |
| **Italiano / English** | Switch the interface language. Document text stays in its original language. |
| **Outline** | Collapse the section list to a narrow rail. Click › to expand it. |
| **Find** | Search the document and move between matches. |
| **Reading only** | Show only the Markdown document, without toolbars or the outline. |

In full-screen reading, press **Esc** or **F11**, or use the small **Exit · Esc** button at the top right, to restore the previous window and outline state.

## Local file links

Local links open files on your PC. `.fprg` documents open in **Flowgorithm**. Other Markdown documents open in this viewer; PDFs, images and other documents use the associated Windows application.

Flowgorithm is detected automatically. If no suitable file association is available, you can select an application when opening the file. There is no Flowgorithm configuration item in the menu.

Supported Markdown examples:

```markdown
[Relative file](examples/example.fprg)
[Path with spaces](<G:/My Drive/Lessons/example.fprg>)
[Local URI](file:///G:/Lessons/example.fprg)
[Network share](<\\server\materials\example.fprg>)
[Another document](<lessons/lesson 2.md>)
[Document section](#local-file-links)
```

Relative paths start from the Markdown document’s folder, not the application’s folder. If a file is missing, the message shows the path that was checked. Only HTTP/HTTPS links open the web browser.

## Keyboard shortcuts

| Keys | Action |
|---|---|
| Ctrl+O | Open document |
| Ctrl+F | Find in document |
| Ctrl+mouse wheel | Change text size |
| Ctrl++ / Ctrl+− | Larger / smaller text |
| Ctrl+0 | Reset text size |
| Enter / Shift+Enter in the search field | Next / previous match |
| F11 | Toggle full-screen reading |
| Esc | Leave full-screen reading |
| Ctrl+I | Collapse / expand the outline |
| F1 | About AulaMarkdown |
| F5 | Reload document |
| Alt+Left | Previous document |

## About and preferences

Use **⋯ → About AulaMarkdown…**, the **ⓘ** title-bar button or **F1** for version, copyright, help and licences. The menu also provides recent documents and the document folder.

Theme, font, text size, language and outline state are remembered between sessions. Windows system dialogs may use the language configured in Windows.

(c) 2026 - Prof. Ing. Raffaele Mele
