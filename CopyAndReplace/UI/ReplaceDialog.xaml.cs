using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.VisualStudio.PlatformUI;

namespace CopyAndReplace.UI {
    /// <summary>
    /// Interaction logic for ReplaceDialog.xaml
    /// </summary>
    public partial class ReplaceDialog : DialogWindow {
        public ReplaceDialog() {
            this.InitializeComponent();
        }

        protected override void OnActivated(EventArgs e) {
            base.OnActivated(e);
            textReplacement.Focus();
            textReplacement.CaretIndex = textReplacement.Text.Length;
        }

        public ReplacementViewModel ViewModel {
            get { return (ReplacementViewModel)this.DataContext; }
            set { this.DataContext = value; }
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
        }
    }
}
