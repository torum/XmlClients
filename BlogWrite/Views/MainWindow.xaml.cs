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
using BlogWrite.Models;

namespace BlogWrite.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region == Treeview D&D ==

        private enum InsertType
        {
            After,
            Before,
            Children,
            None
        }

        private InsertType _insertType = InsertType.None;

        private NodeTree _draggedItem, _targetItem;

        private Point _lastLeftMouseButtonDownPoint;

        private readonly HashSet<NodeTree> _changedBlocks = new HashSet<NodeTree>();

        #endregion

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

        #region == Treeview D&D ==

        // https://aonasuzutsuki.hatenablog.jp/entry/2020/10/01/170406

        private void TreeView_MouseDown (object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) { return; }
            this._lastLeftMouseButtonDownPoint = e.GetPosition(this.TreeViewMenu);
        }

        private void TreeView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
                return;

            try
            {
                var currentPosition = e.GetPosition(this.TreeViewMenu);

                if (Math.Abs(currentPosition.X - this._lastLeftMouseButtonDownPoint.X) <= 10.0 &&
                    Math.Abs(currentPosition.Y - this._lastLeftMouseButtonDownPoint.Y) <= 10.0)
                {
                    return;
                }

                _draggedItem = this.TreeViewMenu.SelectedItem as NodeTree;

                if (_draggedItem != null)
                {
                    if ((_draggedItem is NodeService) || (_draggedItem is NodeFolder) || (_draggedItem is NodeFeed))
                    {
                        DragDropEffects finalDropEffect = DragDrop.DoDragDrop(this.TreeViewMenu, this.TreeViewMenu.SelectedValue, DragDropEffects.Move);

                        //Checking target is not null and item is dragging(moving)
                        if ((finalDropEffect == DragDropEffects.Move) && (_targetItem != null))
                        {
                            // A Move drop was accepted
                            if (!_draggedItem.Name.Equals(_targetItem.Name))
                            {
                                MoveItem(_draggedItem, _targetItem);
                                _targetItem = null;
                                _draggedItem = null;
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
        }

        private void TreeView_DragOver(object sender, DragEventArgs e)
        {
            // 背景色やセパレータを元に戻します
            ResetDragOver(_changedBlocks);

            try
            {
                Point currentPosition = e.GetPosition(this.TreeViewMenu);

                if ((Math.Abs(currentPosition.X - this._lastLeftMouseButtonDownPoint.X) > 10.0) || (Math.Abs(currentPosition.Y - this._lastLeftMouseButtonDownPoint.Y) > 10.0))
                {
                    // Verify that this is a valid drop and then store the drop target
                    NodeTree item = GetNearestContainer(e.OriginalSource as UIElement);

                    if (CheckDropTarget(_draggedItem, item))
                    {
                        // カーソル要素がドラッグ中の要素の子要素にある時は何もする必要がないのでreturn
                        if (item.ContainsChild(_draggedItem))
                            return;
                        if (_draggedItem.ContainsChild(item))
                            return;
                        // Folderを別のFolder内にはドロップさせない
                        if ((_draggedItem is NodeFolder) && (item.Parent is NodeFolder))
                            return;
                        // ServiceをFolder内にドロップさせない。
                        if ((_draggedItem is NodeService) && (item.Parent is NodeFolder))
                            return;

                        var pos = e.GetPosition(e.OriginalSource as UIElement);
                        if (pos.Y > 0 && pos.Y < 10.0)
                        {
                            _insertType = InsertType.Before;

                            e.Effects = DragDropEffects.Move;

                            item.IsBeforeDragSeparator = true;

                            // 背景色などを変更したTreeViewItemInfoオブジェクトを_changedBlocksに追加
                            if (!_changedBlocks.Contains(item))
                                _changedBlocks.Add(item);
                        }
                        /*
                        else if (targetParentLast == targetElementInfo && currentPosition.Y < parentGrid.ActualHeight && currentPosition.Y > parentGrid.ActualHeight - 10.0)
                        {
                        //_insertType = InsertType.After;
                        //targetElementInfo.AfterSeparatorVisibility = Visibility.Visible;
                        //e.Effects = DragDropEffects.Move;
                        }
                        */
                        else
                        {
                            if ((_draggedItem is NodeService) && ((item is NodeFolder) || (item.Parent is NodeFolder)))
                                return;

                            if (item is NodeFolder)
                            {
                                _insertType = InsertType.Children;

                                e.Effects = DragDropEffects.Move;

                                // Change background color.
                                item.IsDragOver = true;

                                // 背景色などを変更したTreeViewItemInfoオブジェクトを_changedBlocksに追加
                                if (!_changedBlocks.Contains(item))
                                    _changedBlocks.Add(item);
                            }
                            else
                            {
                                e.Effects = DragDropEffects.None;
                            }
                        }
                    }
                    else
                    {
                        e.Effects = DragDropEffects.None;
                    }
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }

                e.Handled = true;
            }
            catch (Exception) { }
        }

        private void TreeView_Drop(object sender, DragEventArgs e)
        {
            // 背景色やセパレータを元に戻します
            ResetDragOver(_changedBlocks);

            try
            {
                e.Handled = true;
                e.Effects = DragDropEffects.None;

                // Verify that this is a valid drop and then store the drop target
                NodeTree TargetItem = GetNearestContainer(e.OriginalSource as UIElement);
                if (TargetItem != null && _draggedItem != null)
                {
                    // カーソル要素がドラッグ中の要素の子要素にある時は何もする必要がないのでreturn
                    if (TargetItem.ContainsChild(_draggedItem))
                        return;
                    if (_draggedItem.ContainsChild(TargetItem))
                        return;

                    _targetItem = TargetItem;
                    e.Effects = DragDropEffects.Move;

                }
            }
            catch (Exception) { }
        }

        private bool CheckDropTarget(NodeTree sourceItem, NodeTree targetItem)
        {
            bool isEqual = false;

            if ((sourceItem == null) || (targetItem == null))
                return isEqual;

            if (sourceItem.Name.Equals(targetItem.Name))
                return isEqual;

            // Only allow top level Node.
            if ((targetItem is NodeFolder) || (targetItem is NodeFeed) || (targetItem is NodeService))
                isEqual = true;

            return isEqual;
        }

        private NodeTree GetNearestContainer(UIElement element)
        {
            // Walk up the element tree to the nearest tree view item.
            TreeViewItem container = element as TreeViewItem;
            NodeTree NVContainer = null;

            while ((container == null) && (element != null))
            {
                element = VisualTreeHelper.GetParent(element) as UIElement;
                container = element as TreeViewItem;
            }
            if (container != null)
            {
                NVContainer = container.DataContext as NodeTree;
            }
            return NVContainer;
        }

        private void MoveItem(NodeTree sourceItem, NodeTree targetItem)
        {
            if (_insertType == InsertType.Children)
            {
                if (targetItem is NodeFolder)
                {
                    //Asking user wether he want to drop the dragged TreeViewItem here or not
                    if (MessageBox.Show("Would you like to drop " + sourceItem.Name + " into " + targetItem.Name + "", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            if ((sourceItem.Parent as NodeTree).Children.Remove(sourceItem))
                            { 
                                targetItem.Children.Add(sourceItem);
                                sourceItem.Parent = targetItem;
                                targetItem.IsExpanded = true;
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
            else if (_insertType == InsertType.Before)
            {
                if ((targetItem is NodeFolder) || (targetItem is NodeFeed) || (targetItem is NodeService))
                {
                    //Asking user wether he want to drop the dragged TreeViewItem here or not
                    if (MessageBox.Show("Would you like to insert " + sourceItem.Name + " before " + targetItem.Name + "", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            if (sourceItem.Parent.Children.Remove(sourceItem))
                            {
                                int inx = targetItem.Parent.Children.IndexOf(targetItem);

                                targetItem.Parent.Children.Insert(inx, sourceItem);
                                sourceItem.Parent = targetItem.Parent;
                                targetItem.IsExpanded = true;

                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
        }

        //--- 変更されたセパレータ、背景色を元に戻します
        private static void ResetDragOver(ICollection<NodeTree> collection)
        {
            var list = collection.ToList();
            foreach (var item in list)
            {
                item.IsDragOver = false;
                item.IsBeforeDragSeparator = false;
                item.IsAfterDragSeparator = false;

                collection.Remove(item);
            }
        }

        #endregion

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
