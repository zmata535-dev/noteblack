using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Noteblack {
    public partial class MainWindow : Window {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private string _currentFilePath = null;
        private bool _isModified = false;
        private string _savedText = string.Empty;
        private LineEndingType _savedLineEnding = LineEndingType.CRLF;
        private DocumentEncoding _savedEncoding = DocumentEncoding.Utf8NoBom;
        private LineEndingType _currentLineEnding = LineEndingType.CRLF;
        private DocumentEncoding _currentEncoding = DocumentEncoding.Utf8NoBom;
        private BlackThemeStyle _currentTheme = BlackThemeStyle.PureBlack;
        private double _baseFontSize = 14.0;
        private const string AppName = "Noteblack";

        public MainWindow() {
            InitializeComponent();
            ApplyTheme(_currentTheme);
            UpdateLineEndingUI();
            UpdateEncodingUI();
            UpdateTitle();
            UpdateStatus();

            SourceInitialized += MainWindow_SourceInitialized;
            StateChanged += MainWindow_StateChanged;
            Closing += MainWindow_Closing;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e) {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            int trueVal = 1;
            // DWMWA_USE_IMMERSIVE_DARK_MODE (20 on Win 10 20H1+ and Win 11, 19 on older Win 10)
            DwmSetWindowAttribute(hwnd, 20, ref trueVal, sizeof(int));
            DwmSetWindowAttribute(hwnd, 19, ref trueVal, sizeof(int));
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && File.Exists(args[1])) {
                OpenFile(args[1]);
            }
            Editor.Focus();
        }

        private void MainWindow_StateChanged(object sender, EventArgs e) {
            if (WindowState == WindowState.Maximized) {
                // Restore glyph
                MaximizeGlyph.Data = Geometry.Parse("M 2.5,0.5 L 9.5,0.5 9.5,7.5 2.5,7.5 Z M 0.5,2.5 L 0.5,9.5 7.5,9.5 7.5,2.5 Z");
                BtnMaximize.ToolTip = "Restore Down";
                RootGrid.Margin = new Thickness(7); // Prevent screen bleed when maximized
            } else {
                // Maximize glyph
                MaximizeGlyph.Data = Geometry.Parse("M 0.5,0.5 L 9.5,0.5 9.5,9.5 0.5,9.5 Z");
                BtnMaximize.ToolTip = "Maximize";
                RootGrid.Margin = new Thickness(0);
            }
        }

        #region Title Bar & Window Controls
        private void BtnMinimize_Click(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e) {
            WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void UpdateTitle() {
            string docName = string.IsNullOrEmpty(_currentFilePath) ? "Untitled" : Path.GetFileName(_currentFilePath);
            string dirty = _isModified ? "*" : "";
            string fullTitle = string.Format("{0}{1} - {2}", dirty, docName, AppName);
            Title = fullTitle;
            TitleText.Text = fullTitle;
            UpdateSaveState();
        }

        private void UpdateSaveState() {
            if (MenuTopSave != null) {
                MenuTopSave.IsEnabled = _isModified;
            }
        }

        private void CheckModified() {
            bool textMatches = string.Equals(Editor.Text ?? string.Empty, _savedText ?? string.Empty, StringComparison.Ordinal);
            bool endingMatches = (_currentLineEnding == _savedLineEnding);
            bool encodingMatches = (_currentEncoding == _savedEncoding);

            bool newModified = !(textMatches && endingMatches && encodingMatches);
            if (_isModified != newModified) {
                _isModified = newModified;
                UpdateTitle();
            }
        }
        #endregion

        #region Theme Support
        private void ApplyTheme(BlackThemeStyle theme) {
            _currentTheme = theme;
            ThemeColors colors = ThemeManager.GetColors(theme);

            SolidColorBrush bgBrush = new SolidColorBrush(colors.WindowBackground);
            SolidColorBrush titleBrush = new SolidColorBrush(colors.TitleBarBackground);
            SolidColorBrush editorBrush = new SolidColorBrush(colors.EditorBackground);
            SolidColorBrush menuBrush = new SolidColorBrush(colors.MenuBackground);
            SolidColorBrush textBrush = new SolidColorBrush(colors.TextForeground);
            SolidColorBrush borderBrush = new SolidColorBrush(colors.BorderColor);
            SolidColorBrush panelBrush = new SolidColorBrush(colors.PanelBackground);

            Background = bgBrush;
            RootGrid.Background = bgBrush;
            TitleBarGrid.Background = titleBrush;
            MainMenu.Background = menuBrush;
            Editor.Background = editorBrush;
            Editor.Foreground = textBrush;
            StatusBarBorder.Background = titleBrush;
            StatusBarBorder.BorderBrush = borderBrush;
            FindReplacePanel.Background = panelBrush;
            FindReplacePanel.BorderBrush = borderBrush;
            GoToLinePanel.Background = panelBrush;
            GoToLinePanel.BorderBrush = borderBrush;

            MenuThemePureBlack.IsChecked = (theme == BlackThemeStyle.PureBlack);
            MenuThemeDeepDark.IsChecked = (theme == BlackThemeStyle.DeepDark);
            MenuThemeCharcoal.IsChecked = (theme == BlackThemeStyle.Charcoal);
        }

        private void MenuThemePureBlack_Click(object sender, RoutedEventArgs e) {
            ApplyTheme(BlackThemeStyle.PureBlack);
        }

        private void MenuThemeDeepDark_Click(object sender, RoutedEventArgs e) {
            ApplyTheme(BlackThemeStyle.DeepDark);
        }

        private void MenuThemeCharcoal_Click(object sender, RoutedEventArgs e) {
            ApplyTheme(BlackThemeStyle.Charcoal);
        }
        #endregion

        #region Newline Handling (CRLF, LF, CR, Unicode)
        public LineEndingType CurrentLineEnding {
            get { return _currentLineEnding; }
            set {
                if (_currentLineEnding != value) {
                    _currentLineEnding = value;
                    UpdateLineEndingUI();
                }
            }
        }

        private void UpdateLineEndingUI() {
            string name = LineEndingHelper.GetDisplayName(_currentLineEnding);
            BtnStatusLineEnding.Content = name;
            MenuEndingCRLF.IsChecked = (_currentLineEnding == LineEndingType.CRLF);
            MenuEndingLF.IsChecked = (_currentLineEnding == LineEndingType.LF);
            MenuEndingCR.IsChecked = (_currentLineEnding == LineEndingType.CR);
        }

        private void SetLineEndingAndConvert(LineEndingType newType) {
            _currentLineEnding = newType;
            UpdateLineEndingUI();

            // Convert document text to the new line ending
            if (!string.IsNullOrEmpty(Editor.Text)) {
                int caret = Editor.SelectionStart;
                Editor.Text = LineEndingHelper.NormalizeLineEndings(Editor.Text, newType);
                Editor.SelectionStart = Math.Min(caret, Editor.Text.Length);
            }
            CheckModified();
        }

        private void MenuEndingCRLF_Click(object sender, RoutedEventArgs e) {
            SetLineEndingAndConvert(LineEndingType.CRLF);
        }

        private void MenuEndingLF_Click(object sender, RoutedEventArgs e) {
            SetLineEndingAndConvert(LineEndingType.LF);
        }

        private void MenuEndingCR_Click(object sender, RoutedEventArgs e) {
            SetLineEndingAndConvert(LineEndingType.CR);
        }

        private void BtnStatusLineEnding_Click(object sender, RoutedEventArgs e) {
            LineEndingContextMenu.PlacementTarget = BtnStatusLineEnding;
            LineEndingContextMenu.IsOpen = true;
        }

        // Intercept Enter key in editor to insert the document's native newline, and Ctrl+S to save!
        private void Editor_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
                e.Handled = true;
                SaveFile();
                return;
            }
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) {
                e.Handled = true;
                string nl = LineEndingHelper.GetString(_currentLineEnding);
                int selStart = Editor.SelectionStart;

                Editor.SelectedText = nl;
                Editor.SelectionStart = selStart + nl.Length;
                Editor.SelectionLength = 0;
            }
        }
        #endregion

        #region Encoding Handling
        public DocumentEncoding CurrentEncoding {
            get { return _currentEncoding; }
            set {
                _currentEncoding = value;
                UpdateEncodingUI();
            }
        }

        private void UpdateEncodingUI() {
            string name = EncodingHelper.GetDisplayName(_currentEncoding);
            BtnStatusEncoding.Content = name;
            MenuEncUtf8.IsChecked = (_currentEncoding == DocumentEncoding.Utf8NoBom);
            MenuEncUtf8Bom.IsChecked = (_currentEncoding == DocumentEncoding.Utf8Bom);
            MenuEncUtf16LE.IsChecked = (_currentEncoding == DocumentEncoding.Utf16LE);
            MenuEncUtf16BE.IsChecked = (_currentEncoding == DocumentEncoding.Utf16BE);
            MenuEncAnsi.IsChecked = (_currentEncoding == DocumentEncoding.ANSI);
        }

        private void SetEncoding(DocumentEncoding enc) {
            _currentEncoding = enc;
            UpdateEncodingUI();
            CheckModified();
        }

        private void MenuEncUtf8_Click(object sender, RoutedEventArgs e) { SetEncoding(DocumentEncoding.Utf8NoBom); }
        private void MenuEncUtf8Bom_Click(object sender, RoutedEventArgs e) { SetEncoding(DocumentEncoding.Utf8Bom); }
        private void MenuEncUtf16LE_Click(object sender, RoutedEventArgs e) { SetEncoding(DocumentEncoding.Utf16LE); }
        private void MenuEncUtf16BE_Click(object sender, RoutedEventArgs e) { SetEncoding(DocumentEncoding.Utf16BE); }
        private void MenuEncAnsi_Click(object sender, RoutedEventArgs e) { SetEncoding(DocumentEncoding.ANSI); }

        private void BtnStatusEncoding_Click(object sender, RoutedEventArgs e) {
            EncodingContextMenu.PlacementTarget = BtnStatusEncoding;
            EncodingContextMenu.IsOpen = true;
        }
        #endregion

        #region File Operations
        private bool PromptSaveIfModified() {
            if (!_isModified) return true;

            string docName = string.IsNullOrEmpty(_currentFilePath) ? "Untitled" : Path.GetFileName(_currentFilePath);
            var dlg = new PromptSaveDialog(docName) { Owner = this };
            dlg.ShowDialog();

            if (dlg.Result == SavePromptResult.Save) {
                return SaveFile();
            } else if (dlg.Result == SavePromptResult.DontSave) {
                return true;
            } else {
                return false; // Cancel
            }
        }

        private void MenuFileNew_Click(object sender, RoutedEventArgs e) {
            if (!PromptSaveIfModified()) return;

            Editor.Clear();
            _currentFilePath = null;
            _savedText = string.Empty;
            _savedLineEnding = LineEndingType.CRLF;
            _savedEncoding = DocumentEncoding.Utf8NoBom;
            _isModified = false;
            CurrentLineEnding = LineEndingType.CRLF;
            CurrentEncoding = DocumentEncoding.Utf8NoBom;
            UpdateTitle();
            UpdateStatus();
        }

        private void MenuFileNewWindow_Click(object sender, RoutedEventArgs e) {
            try {
                string exePath = Process.GetCurrentProcess().MainModule.FileName;
                Process.Start(exePath);
            } catch (Exception ex) {
                MessageBox.Show("Could not launch new window: " + ex.Message, "Noteblack", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuFileOpen_Click(object sender, RoutedEventArgs e) {
            if (!PromptSaveIfModified()) return;

            var dlg = new Microsoft.Win32.OpenFileDialog {
                Filter = "Text Documents (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Open"
            };

            if (dlg.ShowDialog() == true) {
                OpenFile(dlg.FileName);
            }
        }

        private void OpenFile(string path) {
            try {
                string content;
                DocumentEncoding enc;
                LineEndingType lineEnding;

                if (EncodingHelper.DetectFile(path, out content, out enc, out lineEnding)) {
                    _currentFilePath = path;
                    _currentEncoding = enc;
                    _currentLineEnding = lineEnding;
                    _savedText = content;
                    _savedLineEnding = lineEnding;
                    _savedEncoding = enc;

                    Editor.Text = content;
                    _isModified = false;

                    UpdateEncodingUI();
                    UpdateLineEndingUI();
                    UpdateTitle();
                    UpdateStatus();
                }
            } catch (Exception ex) {
                MessageBox.Show("Error opening file: " + ex.Message, "Noteblack", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool SaveFile() {
            if (string.IsNullOrEmpty(_currentFilePath)) {
                return SaveFileAs();
            } else {
                try {
                    EncodingHelper.SaveFile(_currentFilePath, Editor.Text, _currentEncoding, _currentLineEnding);
                    _savedText = Editor.Text;
                    _savedLineEnding = _currentLineEnding;
                    _savedEncoding = _currentEncoding;
                    _isModified = false;
                    UpdateTitle();
                    return true;
                } catch (Exception ex) {
                    MessageBox.Show("Error saving file: " + ex.Message, "Noteblack", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
        }

        private bool SaveFileAs() {
            var dlg = new Microsoft.Win32.SaveFileDialog {
                Filter = "Text Documents (*.txt)|*.txt|All Files (*.*)|*.*",
                DefaultExt = ".txt",
                FileName = string.IsNullOrEmpty(_currentFilePath) ? "Untitled.txt" : Path.GetFileName(_currentFilePath),
                Title = "Save As"
            };

            if (dlg.ShowDialog() == true) {
                _currentFilePath = dlg.FileName;
                return SaveFile();
            }
            return false;
        }

        private void MenuFileSave_Click(object sender, RoutedEventArgs e) {
            SaveFile();
        }

        private void MenuFileSaveAs_Click(object sender, RoutedEventArgs e) {
            SaveFileAs();
        }

        private void MenuFileExit_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (!PromptSaveIfModified()) {
                e.Cancel = true;
            }
        }

        private void Window_DragOver(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                e.Effects = DragDropEffects.Copy;
            } else {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void Window_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0 && File.Exists(files[0])) {
                    if (PromptSaveIfModified()) {
                        OpenFile(files[0]);
                    }
                }
            }
        }
        #endregion

        #region Editor Events & Status Bar
        private void Editor_TextChanged(object sender, TextChangedEventArgs e) {
            CheckModified();
            UpdateStatus();
        }

        private void Editor_SelectionChanged(object sender, RoutedEventArgs e) {
            UpdateStatus();
        }

        private void UpdateStatus() {
            int caretIndex = Editor.SelectionStart;
            int line = Editor.GetLineIndexFromCharacterIndex(caretIndex);
            int lineStart = Editor.GetCharacterIndexFromLineIndex(line);
            int col = caretIndex - lineStart;

            int selLen = Editor.SelectionLength;
            if (selLen > 0) {
                StatusCaretPos.Text = string.Format("Ln {0}, Col {1}  ({2} selected)", line + 1, col + 1, selLen);
            } else {
                StatusCaretPos.Text = string.Format("Ln {0}, Col {1}", line + 1, col + 1);
            }

            int lineCount = Editor.LineCount;
            int charCount = Editor.Text.Length;
            StatusDocStats.Text = string.Format("{0} {1}, {2} {3}",
                lineCount, lineCount == 1 ? "line" : "lines",
                charCount, charCount == 1 ? "character" : "characters");
        }
        #endregion

        #region Zoom Support
        private void Editor_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) {
                e.Handled = true;
                if (e.Delta > 0) {
                    ZoomIn();
                } else if (e.Delta < 0) {
                    ZoomOut();
                }
            }
        }

        private void ZoomIn() {
            if (Editor.FontSize < 72) {
                Editor.FontSize += 1.5;
                UpdateZoomUI();
            }
        }

        private void ZoomOut() {
            if (Editor.FontSize > 8) {
                Editor.FontSize -= 1.5;
                UpdateZoomUI();
            }
        }

        private void ResetZoom() {
            Editor.FontSize = _baseFontSize;
            UpdateZoomUI();
        }

        private void UpdateZoomUI() {
            int pct = (int)Math.Round((Editor.FontSize / _baseFontSize) * 100);
            BtnStatusZoom.Content = string.Format("{0}%", pct);
        }

        private void MenuZoomIn_Click(object sender, RoutedEventArgs e) { ZoomIn(); }
        private void MenuZoomOut_Click(object sender, RoutedEventArgs e) { ZoomOut(); }
        private void MenuZoomReset_Click(object sender, RoutedEventArgs e) { ResetZoom(); }
        private void BtnStatusZoom_Click(object sender, RoutedEventArgs e) { ResetZoom(); }
        #endregion

        #region Format & View Menu
        private void MenuViewWordWrap_Click(object sender, RoutedEventArgs e) {
            if (MenuViewWordWrap.IsChecked) {
                Editor.TextWrapping = TextWrapping.Wrap;
                Editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            } else {
                Editor.TextWrapping = TextWrapping.NoWrap;
                Editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
        }

        private void MenuViewFont_Click(object sender, RoutedEventArgs e) {
            var dlg = new FontDialogWindow(Editor.FontFamily, Editor.FontSize) { Owner = this };
            if (dlg.ShowDialog() == true) {
                Editor.FontFamily = dlg.SelectedFontFamily;
                _baseFontSize = dlg.SelectedFontSize;
                Editor.FontSize = dlg.SelectedFontSize;
                UpdateZoomUI();
            }
        }

        private void MenuViewStatusBar_Click(object sender, RoutedEventArgs e) {
            StatusBarBorder.Visibility = MenuViewStatusBar.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        }
        #endregion

        #region Edit Actions
        private void MenuEditUndo_Click(object sender, RoutedEventArgs e) {
            if (Editor.CanUndo) Editor.Undo();
        }

        private void MenuEditRedo_Click(object sender, RoutedEventArgs e) {
            if (Editor.CanRedo) Editor.Redo();
        }

        private void MenuEditDelete_Click(object sender, RoutedEventArgs e) {
            if (Editor.SelectionLength > 0) {
                Editor.SelectedText = string.Empty;
            } else if (Editor.SelectionStart < Editor.Text.Length) {
                int start = Editor.SelectionStart;
                Editor.Text = Editor.Text.Remove(start, 1);
                Editor.SelectionStart = start;
            }
        }

        private void MenuEditTimeDate_Click(object sender, RoutedEventArgs e) {
            string timestamp = DateTime.Now.ToString("h:mm tt M/d/yyyy");
            int selStart = Editor.SelectionStart;
            Editor.SelectedText = timestamp;
            Editor.SelectionStart = selStart + timestamp.Length;
            Editor.SelectionLength = 0;
        }
        #endregion

        #region Find & Replace
        private void MenuEditFind_Click(object sender, RoutedEventArgs e) {
            ShowFindReplace(showReplace: false);
        }

        private void MenuEditReplace_Click(object sender, RoutedEventArgs e) {
            ShowFindReplace(showReplace: true);
        }

        private void ShowFindReplace(bool showReplace) {
            FindReplacePanel.Visibility = Visibility.Visible;
            ReplaceRow.Visibility = showReplace ? Visibility.Visible : Visibility.Collapsed;

            if (Editor.SelectionLength > 0 && !Editor.SelectedText.Contains("\n") && !Editor.SelectedText.Contains("\r")) {
                TxtFind.Text = Editor.SelectedText;
            }

            TxtFind.Focus();
            TxtFind.SelectAll();
        }

        private void BtnCloseFind_Click(object sender, RoutedEventArgs e) {
            FindReplacePanel.Visibility = Visibility.Collapsed;
            Editor.Focus();
        }

        private void TxtFind_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) {
                    FindPrevious();
                } else {
                    FindNext();
                }
                e.Handled = true;
            } else if (e.Key == Key.Escape) {
                BtnCloseFind_Click(sender, e);
                e.Handled = true;
            }
        }

        private void TxtReplace_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                ReplaceNext();
                e.Handled = true;
            } else if (e.Key == Key.Escape) {
                BtnCloseFind_Click(sender, e);
                e.Handled = true;
            }
        }

        private void BtnFindNext_Click(object sender, RoutedEventArgs e) {
            FindNext();
        }

        private void BtnFindPrev_Click(object sender, RoutedEventArgs e) {
            FindPrevious();
        }

        private void MenuEditFindNext_Click(object sender, RoutedEventArgs e) {
            if (FindReplacePanel.Visibility == Visibility.Visible) {
                FindNext();
            } else {
                ShowFindReplace(showReplace: false);
            }
        }

        private void MenuEditFindPrevious_Click(object sender, RoutedEventArgs e) {
            if (FindReplacePanel.Visibility == Visibility.Visible) {
                FindPrevious();
            } else {
                ShowFindReplace(showReplace: false);
            }
        }

        private bool FindNext() {
            string query = TxtFind.Text;
            if (string.IsNullOrEmpty(query)) return false;

            string content = Editor.Text;
            StringComparison comp = (ChkMatchCase.IsChecked == true) ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;

            int startIndex = Editor.SelectionStart + Editor.SelectionLength;
            int foundIndex = content.IndexOf(query, startIndex, comp);

            if (foundIndex < 0 && ChkWrapAround.IsChecked == true) {
                foundIndex = content.IndexOf(query, 0, comp);
            }

            if (foundIndex >= 0) {
                Editor.Focus();
                Editor.Select(foundIndex, query.Length);
                TxtFindStatus.Text = string.Empty;
                return true;
            } else {
                TxtFindStatus.Text = string.Format("Cannot find \"{0}\"", query);
                return false;
            }
        }

        private bool FindPrevious() {
            string query = TxtFind.Text;
            if (string.IsNullOrEmpty(query)) return false;

            string content = Editor.Text;
            StringComparison comp = (ChkMatchCase.IsChecked == true) ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;

            int startIndex = Editor.SelectionStart;
            int foundIndex = -1;

            if (startIndex > 0) {
                foundIndex = content.LastIndexOf(query, startIndex - 1, comp);
            }

            if (foundIndex < 0 && ChkWrapAround.IsChecked == true) {
                foundIndex = content.LastIndexOf(query, content.Length - 1, comp);
            }

            if (foundIndex >= 0) {
                Editor.Focus();
                Editor.Select(foundIndex, query.Length);
                TxtFindStatus.Text = string.Empty;
                return true;
            } else {
                TxtFindStatus.Text = string.Format("Cannot find \"{0}\"", query);
                return false;
            }
        }

        private void BtnReplace_Click(object sender, RoutedEventArgs e) {
            ReplaceNext();
        }

        private void ReplaceNext() {
            string query = TxtFind.Text;
            if (string.IsNullOrEmpty(query)) return;

            StringComparison comp = (ChkMatchCase.IsChecked == true) ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;

            if (Editor.SelectionLength > 0 && string.Equals(Editor.SelectedText, query, comp)) {
                Editor.SelectedText = TxtReplace.Text ?? string.Empty;
            }
            FindNext();
        }

        private void BtnReplaceAll_Click(object sender, RoutedEventArgs e) {
            string query = TxtFind.Text;
            if (string.IsNullOrEmpty(query)) return;

            string replacement = TxtReplace.Text ?? string.Empty;
            string content = Editor.Text;
            StringComparison comp = (ChkMatchCase.IsChecked == true) ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;

            int count = 0;
            int idx = 0;
            var sb = new System.Text.StringBuilder();

            while (idx < content.Length) {
                int match = content.IndexOf(query, idx, comp);
                if (match >= 0) {
                    sb.Append(content.Substring(idx, match - idx));
                    sb.Append(replacement);
                    idx = match + query.Length;
                    count++;
                } else {
                    sb.Append(content.Substring(idx));
                    break;
                }
            }

            if (count > 0) {
                Editor.Text = sb.ToString();
                TxtFindStatus.Text = string.Format("Replaced {0} {1}", count, count == 1 ? "occurrence" : "occurrences");
            } else {
                TxtFindStatus.Text = string.Format("Cannot find \"{0}\"", query);
            }
        }
        #endregion

        #region Go To Line
        private void MenuEditGoTo_Click(object sender, RoutedEventArgs e) {
            GoToLinePanel.Visibility = Visibility.Visible;
            TxtGoToLine.Text = (Editor.GetLineIndexFromCharacterIndex(Editor.SelectionStart) + 1).ToString();
            TxtGoToLine.Focus();
            TxtGoToLine.SelectAll();
            TxtGoToLineStatus.Text = string.Empty;
        }

        private void BtnGoToLineConfirm_Click(object sender, RoutedEventArgs e) {
            ExecuteGoToLine();
        }

        private void BtnGoToLineCancel_Click(object sender, RoutedEventArgs e) {
            GoToLinePanel.Visibility = Visibility.Collapsed;
            Editor.Focus();
        }

        private void TxtGoToLine_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                ExecuteGoToLine();
                e.Handled = true;
            } else if (e.Key == Key.Escape) {
                BtnGoToLineCancel_Click(sender, e);
                e.Handled = true;
            }
        }

        private void ExecuteGoToLine() {
            int lineNum;
            if (int.TryParse(TxtGoToLine.Text.Trim(), out lineNum)) {
                int totalLines = Editor.LineCount;
                if (lineNum >= 1 && lineNum <= totalLines) {
                    int charIndex = Editor.GetCharacterIndexFromLineIndex(lineNum - 1);
                    if (charIndex >= 0) {
                        Editor.Focus();
                        Editor.SelectionStart = charIndex;
                        Editor.SelectionLength = 0;
                        GoToLinePanel.Visibility = Visibility.Collapsed;
                        return;
                    }
                }
                TxtGoToLineStatus.Text = string.Format("The line number is beyond the total number of lines ({0}).", totalLines);
            } else {
                TxtGoToLineStatus.Text = "Please enter a valid line number.";
            }
        }
        #endregion

        #region Help Menu
        private void MenuTimeInsert_Click(object sender, RoutedEventArgs e) {
            string timestamp = DateTime.Now.ToString("[yyyy MM dd HH mm ss]");
            int selStart = Editor.SelectionStart;
            Editor.SelectedText = timestamp;
            Editor.SelectionStart = selStart + timestamp.Length;
            Editor.SelectionLength = 0;
        }

        private void MenuHelpLineEndings_Click(object sender, RoutedEventArgs e) {
            var dlg = new LineEndingsGuideDialog { Owner = this };
            dlg.ShowDialog();
        }

        private void MenuHelpAbout_Click(object sender, RoutedEventArgs e) {
            var dlg = new AboutWindow { Owner = this };
            dlg.ShowDialog();
        }
        #endregion
    }
}
