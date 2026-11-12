# Noteblack

A Windows Notepad clone crafted in pure black with universal newline character support and a sleek dark title bar.

---

## Highlights

- **Pure Black UI & Title Bar**:
  - Deep black custom title bar with native Windows DWM dark mode integration (`DwmSetWindowAttribute`).
  - Window caption controls (minimize, maximize/restore, close) with smooth dark hover states and red close button.
  - Dark menus, dark dialogs, dark status bar, and custom sleek dark scrollbars (no bright white Windows scrollbars).
  - 3 customizable dark themes: **Pure Black (OLED)**, **Deep Dark**, and **Charcoal**.

- **Universal Newline Support**:
  - **Windows (CRLF - `\r\n`)**: Standard DOS/Windows line endings.
  - **Unix / Linux / macOS (LF - `\n`)**: Standard POSIX / modern line endings.
  - **Macintosh (CR - `\r`)**: Classic Mac OS line endings.
  - **Unicode Line Breaks**: Full recognition of Next Line (NEL `\u0085`), Line Separator (LS `\u2028`), Paragraph Separator (PS `\u2029`).
  - **Automatic Detection**: Detects file newline type automatically upon opening.
  - **Live Typing Conformity**: When typing `Enter`, inserts the active newline character (`\r\n`, `\n`, or `\r`).
  - **One-Click Conversion**: Click the status bar line ending badge or use `Edit` -> `Line Endings` to instantly normalize the entire document.

- **Encoding Support**:
  - UTF-8 (without BOM)
  - UTF-8 with BOM
  - UTF-16 LE (Unicode)
  - UTF-16 BE (Big Endian Unicode)
  - ANSI / Windows Default Code Page
  - Clickable status bar encoding switcher.

- **Complete Notepad Feature Set**:
  - **File**: New (`Ctrl+N`), New Window (`Ctrl+Shift+N`), Open (`Ctrl+O`), Save (`Ctrl+S`), Save As (`Ctrl+Shift+S`), Exit (`Alt+F4`), Drag & Drop file opening, CLI argument opening (`Noteblack.exe file.txt`).
  - **Edit**: Undo (`Ctrl+Z`), Redo (`Ctrl+Y`), Cut, Copy, Paste, Delete, Select All (`Ctrl+A`), Time/Date stamp (`F5`).
  - **Find & Replace**: Modern collapsible dark search bar with Find Next (`F3`), Find Previous (`Shift+F3`), Replace, Replace All, Match Case, Wrap Around (`Ctrl+F`, `Ctrl+H`).
  - **Go To Line**: Jump to any line number (`Ctrl+G`).
  - **View / Format**: Word Wrap toggle, Font picker dialog (family and size), Zoom In (`Ctrl++`), Zoom Out (`Ctrl+-`), Reset Zoom (`Ctrl+0` / `Ctrl+MouseWheel`), Status Bar toggle.
  - **Help**: Built-in Line Endings Guide & About Noteblack dialog.

---

## Executable

The compiled executable is ready to run:
```powershell
.\Noteblack.exe
```

Or open a file directly:
```powershell
.\Noteblack.exe "C:\path\to\document.txt"
```
