using BlogWrite.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace BlogWrite.Views
{
    /// <summary>
    /// MainWindow.xaml code behind.
    /// </summary>
    public partial class MainWindow 
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;

            if (this.DataContext is MainViewModel vm)
            {
                App app = App.Current as App;

                vm.LaunchServiceDiscovery += (sender, arg) => { app.LaunchServiceDiscoveryWindow(this); };

                vm.OpenEditorView += (sender, arg) => { app.CreateOrBringToFrontEditorWindow(arg); };

                vm.OpenEditorNewView += (sender, arg) => { app.CreateNewEditorWindow(arg); };

            }
        }

        public void BringToForeground()
        {
            if (this.WindowState == WindowState.Minimized || this.Visibility == Visibility.Hidden)
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            }

            this.Activate();
            //this.Topmost = true;
            //this.Topmost = false;
            this.Focus();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Load window possition.
            if ((Properties.Settings.Default.MainWindow_Left != 0)
                && (Properties.Settings.Default.MainWindow_Top != 0)
                && (Properties.Settings.Default.MainWindow_Width != 0)
                && (Properties.Settings.Default.MainWindow_Height != 0)
                )
            {
                this.Left = Properties.Settings.Default.MainWindow_Left;
                this.Top = Properties.Settings.Default.MainWindow_Top;
                this.Width = Properties.Settings.Default.MainWindow_Width;
                this.Height = Properties.Settings.Default.MainWindow_Height;
            }
        }

        // TODO: Listview colum header click auto sort.
        // https://stackoverflow.com/questions/994148/best-way-to-make-wpf-listview-gridview-sort-on-column-header-clicking
        // Hey, it's "View" matter.

        /*
        up
        M7.41,15.41L12,10.83L16.59,15.41L18,14L12,8L6,14L7.41,15.41Z

        down
        M7.41,8.58L12,13.17L16.59,8.58L18,10L12,16L6,10L7.41,8.58Z

         */

        private void EntryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ListView))
                return;

            (sender as ListView).ScrollIntoView((sender as ListView).SelectedItem);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // TODO: When MainWindow try to close itself, confirm to close all the child windows. 

            App app = App.Current as App;

            // Now, use "app.Windows" this time.
            foreach (var w in app.Windows)
            {
                if (!(w is EditorWindow))
                    continue;
                
                if ((w as EditorWindow).DataContext == null)
                    continue;

                if (!((w as EditorWindow).DataContext is EditorViewModel))
                    continue;

                (w as EditorWindow).Close();
            }

            // Save window pos.
            if (WindowState == WindowState.Normal)
            {
                Properties.Settings.Default.MainWindow_Left = this.Left;
                Properties.Settings.Default.MainWindow_Top = this.Top;
                Properties.Settings.Default.MainWindow_Height = this.Height;
                Properties.Settings.Default.MainWindow_Width = this.Width;
            }

            Properties.Settings.Default.Save();
        }
    }

}
