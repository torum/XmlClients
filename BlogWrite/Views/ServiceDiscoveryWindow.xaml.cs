using BlogWrite.ViewModels;
using System;
using System.Windows;

namespace BlogWrite.Views
{
    /// <summary>
    /// ServiceDiscovery.xaml
    /// </summary>
    public partial class ServiceDiscoveryWindow : Window
    {
        public ServiceDiscoveryWindow()
        {
            InitializeComponent();

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
