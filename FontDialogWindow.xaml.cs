using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Noteblack {
    public partial class FontDialogWindow : Window {
        public FontFamily SelectedFontFamily { get; private set; }
        public double SelectedFontSize { get; private set; }

        public FontDialogWindow(FontFamily currentFamily, double currentSize) {
            InitializeComponent();
            SelectedFontFamily = currentFamily;
            SelectedFontSize = currentSize;

            var popularFonts = new List<string> {
                "Consolas",
                "Cascadia Mono",
                "Cascadia Code",
                "Courier New",
                "Lucida Console",
                "Segoe UI",
                "Calibri",
                "Arial",
                "MS Gothic"
            };

            foreach (var font in popularFonts) {
                ListFontFamily.Items.Add(font);
            }

            int[] sizes = new int[] { 8, 9, 10, 11, 12, 13, 14, 16, 18, 20, 22, 24, 28, 32, 36, 48, 72 };
            foreach (var sz in sizes) {
                ListFontSize.Items.Add(sz);
            }

            string currentFamilyName = currentFamily.Source;
            if (ListFontFamily.Items.Contains(currentFamilyName)) {
                ListFontFamily.SelectedItem = currentFamilyName;
            } else {
                ListFontFamily.SelectedIndex = 0;
            }

            int currentSzInt = (int)Math.Round(currentSize);
            if (ListFontSize.Items.Contains(currentSzInt)) {
                ListFontSize.SelectedItem = currentSzInt;
            } else {
                ListFontSize.SelectedItem = 14;
            }

            UpdatePreview();
        }

        private void ListFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (ListFontFamily.SelectedItem != null) {
                SelectedFontFamily = new FontFamily(ListFontFamily.SelectedItem.ToString());
                UpdatePreview();
            }
        }

        private void ListFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (ListFontSize.SelectedItem != null) {
                SelectedFontSize = Convert.ToDouble(ListFontSize.SelectedItem);
                UpdatePreview();
            }
        }

        private void UpdatePreview() {
            if (TxtPreview != null) {
                TxtPreview.FontFamily = SelectedFontFamily;
                TxtPreview.FontSize = Math.Max(10, Math.Min(SelectedFontSize, 24)); // clamp preview size so it fits
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }
    }
}
