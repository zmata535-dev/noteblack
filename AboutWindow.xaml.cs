using System;
using System.Windows;

namespace Noteblack {
    public partial class AboutWindow : Window {
        public AboutWindow() {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
