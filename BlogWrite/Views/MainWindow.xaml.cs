using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Globalization;
using System.Diagnostics;
using BlogWrite.ViewModels;

namespace BlogWrite.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += (this.DataContext as MainViewModel).OnWindowLoaded;
            Closing += (this.DataContext as MainViewModel).OnWindowClosing;

            RestoreButton.Visibility = Visibility.Collapsed;
            MaxButton.Visibility = Visibility.Visible;

            if (this.DataContext is MainViewModel vm)
            {
                if (vm != null)
                {
                    vm.DebugWindowShowHide += () => this.OnDebugWindowShowHide();

                    vm.DebugOutput += (sender, arg) => { this.OnDebugOutput(arg); };

                    vm.DebugClear += () => this.OnDebugClear();

                    vm.WriteHtmlToContentPreviewBrowser += (sender, arg) => { this.OnWriteHtmlToContentPreviewBrowser(arg); };

                    vm.OpenServiceDiscoveryView += (sender, arg) => { this.OnCreateServiceDiscoveryWindow(this); };


                    App app = App.Current as App;
                    if (app != null)
                    {

                        vm.OpenEditorView += (sender, arg) => { app.CreateOrBringToFrontEditorWindow(arg); };

                        vm.OpenEditorNewView += (sender, arg) => { app.CreateNewEditorWindow(arg); };

                    }
                }
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
            this.Focus();
        }
        
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var html = @"
                <html>
                    <head>
                        <title></title>
                    </head>
                    <body style=""background-color:#212121;"">
                    </body>
                </html>";

            await ContentPreviewWebBrowser.EnsureCoreWebView2Async();

            ContentPreviewWebBrowser.NavigateToString(html);
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            // TODO: When MainWindow try to close itself, confirm to close all the child windows. 

            App app = App.Current as App;
            if (app == null)
                return;

            // Now, use "app.Windows" this time.
            foreach (var w in app.Windows)
            {
                // Editor
                if (!(w is EditorWindow))
                    continue;

                if ((w as EditorWindow).DataContext == null)
                    continue;

                if (!((w as EditorWindow).DataContext is EditorViewModel))
                    continue;

                (w as EditorWindow).Close();
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                RestoreButton.Visibility = Visibility.Collapsed;
                MaxButton.Visibility = Visibility.Visible;
            }
            else if (this.WindowState == WindowState.Maximized)
            {
                RestoreButton.Visibility = Visibility.Visible;
                MaxButton.Visibility = Visibility.Collapsed;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        private void MaxButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        
        private void EntryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ListView))
                return;

            (sender as ListView).ScrollIntoView((sender as ListView).SelectedItem);
        }

        public void OnCreateServiceDiscoveryWindow(Window owner)
        {
            // TODO: Before opening the window, make sure no other window is open.
            // If a user minimize and restore, Modal window can get behind of the child window.

            var win = new ServiceDiscoveryWindow();
            win.DataContext = new ServiceDiscoveryViewModel();

            var vm = (win.DataContext as ServiceDiscoveryViewModel);
            vm.RegisterFeed += (sender, arg) => OnRegisterFeed(arg);
            vm.CloseAction = new Action(win.Close);

            win.Owner = owner;
            win.ShowDialog();
        }

        public void OnRegisterFeed(RegisterFeedEventArgs arg)
        {
            if (this.DataContext == null)
                return;

            if (this.DataContext is not MainViewModel)
                return;

            (this.DataContext as MainViewModel).AddFeed(arg.FeedLinkData);
        }

        public void OnWriteHtmlToContentPreviewBrowser(string arg)
        {
            // 
            ContentPreviewWebBrowser.NavigateToString(arg);
        }

        public void OnDebugOutput(string arg)
        {
            // AppendText() is much faster than data binding.
            DebugTextBox.AppendText(arg);

            DebugTextBox.CaretIndex = DebugTextBox.Text.Length;
            DebugTextBox.ScrollToEnd();
        }

        public void OnDebugClear()
        {
            DebugTextBox.Clear();
        }

        public void OnDebugWindowShowHide()
        {
            if (DebugWindowGridSplitter.Visibility == Visibility.Visible)
            {
                LayoutGrid.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);

                LayoutGrid.RowDefinitions[3].Height = new GridLength(0);
                LayoutGrid.RowDefinitions[4].Height = new GridLength(0);

                DebugWindowGridSplitter.Visibility = Visibility.Collapsed;
                DebugWindow.Visibility = Visibility.Collapsed;
            }
            else
            {
                LayoutGrid.RowDefinitions[2].Height = new GridLength(3, GridUnitType.Star);

                LayoutGrid.RowDefinitions[3].Height = new GridLength(8);
                LayoutGrid.RowDefinitions[4].Height = new GridLength(1, GridUnitType.Star);

                DebugWindowGridSplitter.Visibility = Visibility.Visible;
                DebugWindow.Visibility = Visibility.Visible;
            }
        }

        #region == MAXIMIZE時のタスクバー被りのFix ==
        // https://engy.us/blog/2020/01/01/implementing-a-custom-window-title-bar-in-wpf/

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            ((HwndSource)PresentationSource.FromVisual(this)).AddHook(HookProc);
        }

        public static IntPtr HookProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_GETMINMAXINFO)
            {
                // We need to tell the system what our size should be when maximized. Otherwise it will cover the whole screen,
                // including the task bar.
                MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

                // Adjust the maximized size and position to fit the work area of the correct monitor
                IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

                if (monitor != IntPtr.Zero)
                {
                    MONITORINFO monitorInfo = new MONITORINFO();
                    monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                    GetMonitorInfo(monitor, ref monitorInfo);
                    RECT rcWorkArea = monitorInfo.rcWork;
                    RECT rcMonitorArea = monitorInfo.rcMonitor;
                    mmi.ptMaxPosition.X = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
                    mmi.ptMaxPosition.Y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top) - 4; // -4を付加した。てっぺんをクリックしても反応がなかったから。
                    mmi.ptMaxSize.X = Math.Abs(rcWorkArea.Right - rcWorkArea.Left);
                    mmi.ptMaxSize.Y = Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top) + 4; // 付加した分の補正。
                }

                Marshal.StructureToPtr(mmi, lParam, true);
            }

            return IntPtr.Zero;
        }

        private const int WM_GETMINMAXINFO = 0x0024;

        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr handle, uint flags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                this.Left = left;
                this.Top = top;
                this.Right = right;
                this.Bottom = bottom;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }


        #endregion

    }
}
