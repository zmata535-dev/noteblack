using System;
using System.Windows;

namespace Noteblack {
    public enum SavePromptResult {
        Save,
        DontSave,
        Cancel
    }

    public partial class PromptSaveDialog : Window {
        public SavePromptResult Result { get; private set; }

        public PromptSaveDialog(string documentName) {
            InitializeComponent();
            Result = SavePromptResult.Cancel;
            TxtMessage.Text = string.Format("Do you want to save changes to {0}?", documentName);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e) {
            Result = SavePromptResult.Save;
            DialogResult = true;
            Close();
        }

        private void BtnDontSave_Click(object sender, RoutedEventArgs e) {
            Result = SavePromptResult.DontSave;
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) {
            Result = SavePromptResult.Cancel;
            DialogResult = false;
            Close();
        }
    }
}
