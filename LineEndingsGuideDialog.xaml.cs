using System;
using System.Windows;

namespace Noteblack {
    public partial class LineEndingsGuideDialog : Window {
        public LineEndingsGuideDialog() {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
