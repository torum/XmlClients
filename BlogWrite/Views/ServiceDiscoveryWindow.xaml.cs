using BlogWrite.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        private void TextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender != null)
            {
                if (sender is TextBox)
                {
                    if ((sender as TextBox).Visibility == Visibility.Visible)
                    {
                        (sender as TextBox).Focus();
                        Keyboard.Focus((sender as TextBox));
                    }
                }
            }
        }

    }
}
