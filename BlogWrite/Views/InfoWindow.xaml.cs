using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BlogWrite.Views
{
    /// <summary>
    /// InfoWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class InfoWindow : Window
    {
        public InfoWindow()
        {
            InitializeComponent();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void InfoSiteUrlOpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(InfoSiteUrlTextBox.Text.Trim()))
                return;

            ProcessStartInfo psi = new ProcessStartInfo(InfoSiteUrlTextBox.Text.Trim());
            psi.UseShellExecute = true;
            Process.Start(psi);

            e.Handled = true;
        }

        private void InfoFeedUrlOpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(InfoFeedUrlTextBox.Text.Trim()))
                return;

            ProcessStartInfo psi = new ProcessStartInfo(InfoFeedUrlTextBox.Text.Trim());
            psi.UseShellExecute = true;
            Process.Start(psi);

            e.Handled = true;
        }
    }
}
