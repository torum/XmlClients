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
using Microsoft.Web.WebView2.Core;
using System.IO;

namespace BlogWrite.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CoreWebView2Environment _env;

        private static string html = @"
                <html>
                    <head>
                        <title></title>
                    </head>
                    <body style=""background-color:#212121;"">
                    </body>
                </html>";

        #region == Treeview D&D ==

        private enum InsertType
        {
            After,
            Before,
            Children,
            None
        }

        private InsertType _insertType = InsertType.None;

        private NodeTree _draggingItem, _targetItem;

        private Point _lastLeftMouseButtonDownPoint;

        private readonly HashSet<NodeTree> _changedBlocks = new HashSet<NodeTree>();

        private bool IsRenamingInProgress;

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            InitializeWebView2Async();

            Loaded += (this.DataContext as MainViewModel).OnWindowLoaded;
            Closing += (this.DataContext as MainViewModel).OnWindowClosing;

            RestoreButton.Visibility = Visibility.Collapsed;
            MaxButton.Visibility = Visibility.Visible;

            if (this.DataContext is MainViewModel vm)
            {
                if (vm != null)
                {
                    vm.DebugWindowShowHide += () => this.OnDebugWindowShowHide();
                    vm.DebugWindowShowHide2 += (sender, arg) => OnDebugWindowShowHide2(arg);

                    vm.DebugOutput += (sender, arg) => { this.OnDebugOutput(arg); };

                    vm.DebugClear += () => this.OnDebugClear();

                    vm.WriteHtmlToContentPreviewBrowser += (sender, arg) => { this.OnWriteHtmlToContentPreviewBrowser(arg); };
                    
                    vm.NavigateUrlToContentPreviewBrowser += (sender, arg) => { this.OnNavigateUrlToContentPreviewBrowser(arg); };

                    vm.OpenServiceDiscoveryView += (sender, arg) => { this.OnCreateServiceDiscoveryWindow(this); };

                    vm.ContentsBrowserWindowShowHide += () => this.OnContentsBrowserWindowShowHide();

                    vm.ContentsBrowserWindowShowHide2 += (sender, arg) => this.OnContentsBrowserWindowShowHide2(arg);

                    App app = App.Current as App;
                    if (app != null)
                    {

                        vm.OpenEditorView += (sender, arg) => { app.CreateOrBringToFrontEditorWindow(arg); };

                        vm.OpenEditorNewView += (sender, arg) => { app.CreateNewEditorWindow(arg); };

                    }
                }
            }

        }

        private async void InitializeWebView2Async()
        {
            // I really do hate smooth-scrolling.
            var op = new CoreWebView2EnvironmentOptions("--disable-smooth-scrolling");

            _env = await CoreWebView2Environment.CreateAsync(userDataFolder: System.IO.Path.Combine(System.IO.Path.GetTempPath(), "BlogWrite"), options:op);

            await ListViewContentPreviewWebBrowser.EnsureCoreWebView2Async(_env);

            ListViewContentPreviewWebBrowser.CoreWebView2InitializationCompleted += ListViewContentPreviewWebBrowser_InitializationCompleted;
            
            await CardViewContentPreviewWebBrowser.EnsureCoreWebView2Async(_env);

            CardViewContentPreviewWebBrowser.CoreWebView2InitializationCompleted += CardViewContentPreviewWebBrowser_InitializationCompleted;

        }

        private void ListViewContentPreviewWebBrowser_InitializationCompleted(object sender, EventArgs e)
        {
            //ListViewContentPreviewWebBrowser.CoreWebView2.Settings.UserAgent = "";

            ListViewContentPreviewWebBrowser.NavigateToString(html);
        }

        private void CardViewContentPreviewWebBrowser_InitializationCompleted(object sender, EventArgs e)
        {
            //CardViewContentPreviewWebBrowser.CoreWebView2.Settings.UserAgent = "";

            CardViewContentPreviewWebBrowser.NavigateToString(html);
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
            var win = new ServiceDiscoveryWindow();
            win.DataContext = new ServiceDiscoveryViewModel();

            var vm = (win.DataContext as ServiceDiscoveryViewModel);
            vm.RegisterFeed += (sender, arg) => OnRegisterFeed(arg);
            vm.RegisterService += (sender, arg) => OnRegisterService(arg);
            vm.CloseAction = new Action(win.Close);

            win.Owner = this;//owner;
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

        public void OnRegisterService(RegisterServiceEventArgs arg)
        {
            if (this.DataContext == null)
                return;

            if (this.DataContext is not MainViewModel)
                return;

            (this.DataContext as MainViewModel).AddService(arg.nodeService);
        }

        public async void OnWriteHtmlToContentPreviewBrowser(string arg)
        {
            await ListViewContentPreviewWebBrowser.EnsureCoreWebView2Async(_env);

            ListViewContentPreviewWebBrowser.NavigateToString(arg);
        }

        public async void OnNavigateUrlToContentPreviewBrowser(Uri arg)
        {
            if (arg == null)
                return;

            if (ViewTab.SelectedIndex == 0)
            {
                if (GridCardViewContentPreviewWebBrowser.Visibility != Visibility.Visible)
                {
                    GridCardViewContentPreviewWebBrowser.Visibility = Visibility.Visible;
                }

                await CardViewContentPreviewWebBrowser.EnsureCoreWebView2Async(_env);

                CardViewContentPreviewWebBrowser.Source = arg;

            }
            else if (ViewTab.SelectedIndex == 1)
            {
                if (GridListViewContentPreviewWebBrowser.Visibility != Visibility.Visible)
                {
                    // Re-set Listview, splitter and browser heights.
                    GridListView.RowDefinitions[1].Height = new GridLength(3, GridUnitType.Star);
                    GridListView.RowDefinitions[2].Height = new GridLength(8);
                    GridListView.RowDefinitions[3].Height = new GridLength(5, GridUnitType.Star);

                    SplitterListViewContentPreviewWebBrowser.Visibility = Visibility.Visible;
                    GridListViewContentPreviewWebBrowser.Visibility = Visibility.Visible;
                }

                await ListViewContentPreviewWebBrowser.EnsureCoreWebView2Async(_env);

                ListViewContentPreviewWebBrowser.Source = arg;
            }

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
            /*

            */
        }

        public void OnDebugWindowShowHide2(bool on)
        {
            if (on)
            {
                LayoutGrid.RowDefinitions[2].Height = new GridLength(3, GridUnitType.Star);

                LayoutGrid.RowDefinitions[3].Height = new GridLength(8);
                LayoutGrid.RowDefinitions[4].Height = new GridLength(1, GridUnitType.Star);

                DebugWindowGridSplitter.Visibility = Visibility.Visible;
                DebugWindow.Visibility = Visibility.Visible;
            }
            else
            {
                LayoutGrid.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);

                LayoutGrid.RowDefinitions[3].Height = new GridLength(0);
                LayoutGrid.RowDefinitions[4].Height = new GridLength(0);

                DebugWindowGridSplitter.Visibility = Visibility.Collapsed;
                DebugWindow.Visibility = Visibility.Collapsed;
            }
            /*

            */
        }

        public void OnContentsBrowserWindowShowHide()
        {
            /*
            if (GridRightBottom.Visibility == Visibility.Visible)
            {
                OnContentsBrowserWindowShowHide2(false);
            }
            else
            {
                OnContentsBrowserWindowShowHide2(true);
            }
            */
        }

        public void OnContentsBrowserWindowShowHide2(bool on)
        {
            if (on)
            {
                if (ViewTab.SelectedIndex == 0)
                {

                }
                else if (ViewTab.SelectedIndex == 1)
                {

                }
            }
            else
            {
                if (ViewTab.SelectedIndex == 0)
                {
                    if (GridCardViewContentPreviewWebBrowser.Visibility == Visibility.Visible)
                    {
                        GridCardViewContentPreviewWebBrowser.Visibility = Visibility.Collapsed;
                    }

                }
                else if (ViewTab.SelectedIndex == 1)
                {
                    if (GridListViewContentPreviewWebBrowser.Visibility == Visibility.Visible)
                    {
                        SplitterListViewContentPreviewWebBrowser.Visibility = Visibility.Collapsed;
                        GridListViewContentPreviewWebBrowser.Visibility = Visibility.Collapsed;

                        // Sets Listview, splitter and browser heights.
                        GridListView.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
                        GridListView.RowDefinitions[2].Height = new GridLength(0);
                        GridListView.RowDefinitions[3].Height = new GridLength(0);
                    }
                }
            }
        }

        private void TreeViewMenuItemShowInfo_Click(object sender, RoutedEventArgs e)
        {
            NodeTree targetItem = TreeViewMenu.SelectedItem as NodeTree;

            if (targetItem == null)
                return;

            if ((targetItem is NodeFeed) || (targetItem is NodeService))
            {
                if (targetItem is NodeFeed)
                {
                    var dialog = new InfoWindow()
                    {
                        Owner = this,
                        Width = 600,
                        Height = 500,
                        Title = "Info Window",
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };

                    dialog.InfoFeedTitleTextBox.Text = (targetItem as NodeFeed).Name;
                    dialog.InfoFeedUrlTextBox.Text = (targetItem as NodeFeed).EndPoint.AbsoluteUri;
                    dialog.InfoSiteTitleTextBox.Text = (targetItem as NodeFeed).SiteTitle;
                    if ((targetItem as NodeFeed).SiteUri != null)
                        dialog.InfoSiteUrlTextBox.Text = (targetItem as NodeFeed).SiteUri.AbsoluteUri;

                    dialog.ShowDialog();
                }
            }
        }

        private void CardViewListview_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (CardViewListview.Items.Count > 0)
            {
                CardViewListview.ScrollIntoView(CardViewListview.Items[0]);
            }
        }

        private void ListViewListView_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (ListViewListView.Items.Count > 0)
            {
                ListViewListView.ScrollIntoView(ListViewListView.Items[0]);
            }
        }

        #region == TreeviewItem Delete ==

        private void TreeViewMenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            NodeTree targetItem = (TreeViewMenu.SelectedItem as NodeTree);

            if (targetItem == null)
                return;

            if ((targetItem is NodeFolder) || (targetItem is NodeFeed) || (targetItem is NodeService))
            {
                if (MessageBox.Show(string.Format("Are you sure you want delete {0}?", targetItem.Name), "Comfirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (targetItem.Parent.Children.Remove(targetItem))
                        {
                            
                        }
                    }
                    catch (Exception) { }
                }
            }
        }

        #endregion

        #region == TreeviewItem In-place Renaming ==

        public static TreeViewItem ContainerFromItemRecursive(ItemContainerGenerator root, object item)
        {
            var treeViewItem = root.ContainerFromItem(item) as TreeViewItem;
            if (treeViewItem != null)
                return treeViewItem;
            foreach (var subItem in root.Items)
            {
                treeViewItem = root.ContainerFromItem(subItem) as TreeViewItem;
                if (treeViewItem != null)
                {
                    var search = ContainerFromItemRecursive(treeViewItem.ItemContainerGenerator, item);
                    if (search != null)
                        return search;
                }
            }
            return null;
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private void TreeViewMenuItemRename_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckRenameOK(TreeViewMenu.SelectedItem as NodeTree))
                return;

            TreeViewItem item = ContainerFromItemRecursive(TreeViewMenu.ItemContainerGenerator, TreeViewMenu.SelectedItem);
            if (item == null) 
                return;

            //item.Focus();

            TextBox renameTextBox = FindVisualChild<TextBox>(item as DependencyObject);
            if (renameTextBox != null)
            {
                IsRenamingInProgress = true;

                renameTextBox.Visibility = Visibility.Visible;

                renameTextBox.Focus();
                renameTextBox.SelectAll();

            }
        }

        private void RenameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateTreeViewItemName();
        }

        private void RenameTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            // TODO: Save on Enter key
            if (e.Key == Key.Return)
            {
                // NOT GOOD if IME is on.
                //UpdateTreeViewItemName();
            }
        }

        private void UpdateNodeNameButton_Click(object sender, RoutedEventArgs e)
        {
            // This ensures "Enter" key press event even if IME is on. 

            UpdateTreeViewItemName();
        }

        private void UpdateTreeViewItemName()
        {

            TreeViewItem item = ContainerFromItemRecursive(TreeViewMenu.ItemContainerGenerator, TreeViewMenu.SelectedItem);
            if (item == null)
                return;

            item.Focus();

            TextBox renameTextBox = FindVisualChild<TextBox>(item as DependencyObject);
            if (renameTextBox != null)
            {
                renameTextBox.Visibility = Visibility.Collapsed;

                // Didn't need this. It turned out XAML binding took care of updating.
                /*
                NodeTree tvm = TreeViewMenu.SelectedItem as NodeTree;
                if (tvm != null)
                {
                    var s = renameTextBox.Text.Trim();
                    if (!string.IsNullOrEmpty(s))
                    {
                        if (tvm.Name != s)
                        {
                            tvm.Name = s;
                        }
                    }
                }
                */
                IsRenamingInProgress = false;
            }
        }

        private bool CheckRenameOK(NodeTree nd)
        {
            if ((nd is NodeFeed) || (nd is NodeFolder) || (nd is NodeService))
                return true;
            else
                return false;
        }

        #region == Right click select ==
        
        private void TreeViewItem_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsRenamingInProgress)
                return;

            //NodeTree item = GetNearestContainer(e.OriginalSource as UIElement);
            TreeViewItem item = GetParentObjectEx<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (item != null)
            {
                // the current node has focus
                item.Focus();
                item.IsSelected = true;

                // no longer handle the operating system
                e.Handled = true;

            }
        }


        public TreeViewItem GetParentObjectEx<TreeViewItem>(DependencyObject obj) where TreeViewItem : FrameworkElement
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            while (parent != null)
            {
                if (parent is TreeViewItem)
                {
                    return (TreeViewItem)parent;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        #endregion

        #endregion

        #region == Treeview Drag & Drop ==

        // https://aonasuzutsuki.hatenablog.jp/entry/2020/10/01/170406

        private void TreeView_MouseDown (object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            this._lastLeftMouseButtonDownPoint = e.GetPosition(this.TreeViewMenu);
        }

        private void TreeView_MouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void TreeView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
                return;

            if (IsRenamingInProgress)
                return;

            try
            {
                var currentPosition = e.GetPosition(this.TreeViewMenu);

                if (Math.Abs(currentPosition.X - this._lastLeftMouseButtonDownPoint.X) <= 10.0 &&
                    Math.Abs(currentPosition.Y - this._lastLeftMouseButtonDownPoint.Y) <= 10.0)
                {
                    return;
                }

                _draggingItem = this.TreeViewMenu.SelectedItem as NodeTree;

                if (_draggingItem != null)
                {
                    if ((_draggingItem is NodeService) || (_draggingItem is NodeFolder) || (_draggingItem is NodeFeed))
                    {
                        DragDropEffects finalDropEffect = DragDrop.DoDragDrop(this.TreeViewMenu, this.TreeViewMenu.SelectedValue, DragDropEffects.Move);

                        //Checking target is not null and item is dragging(moving)
                        if ((finalDropEffect == DragDropEffects.Move) && (_targetItem != null))
                        {
                            // A Move drop was accepted
                            if (!_draggingItem.Name.Equals(_targetItem.Name))
                            {
                                MoveItem(_draggingItem, _targetItem);
                                _targetItem = null;
                                _draggingItem = null;
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

            if (IsRenamingInProgress)
                return;

            try
            {
                Point currentPosition = e.GetPosition(this.TreeViewMenu);

                if ((Math.Abs(currentPosition.X - this._lastLeftMouseButtonDownPoint.X) > 10.0) || (Math.Abs(currentPosition.Y - this._lastLeftMouseButtonDownPoint.Y) > 10.0))
                {
                    // Verify that this is a valid drop and then store the drop target
                    NodeTree item = GetNearestContainer(e.OriginalSource as UIElement);

                    if (CheckDropTarget(_draggingItem, item))
                    {
                        // カーソル要素がドラッグ中の要素の子要素にある時は何もする必要がないのでreturn
                        if (item.ContainsChild(_draggingItem))
                            return;
                        if (_draggingItem.ContainsChild(item))
                            return;
                        // Folderを別のFolder内にはドロップさせない
                        if ((_draggingItem is NodeFolder) && (item.Parent is NodeFolder))
                            return;
                        // ServiceをFolder内にドロップさせない。
                        if ((_draggingItem is NodeService) && (_draggingItem is not NodeFeed) && (item.Parent is NodeFolder))
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
                            if (_draggingItem is NodeService)
                            {
                                if (_draggingItem is not NodeFeed)
                                {
                                    if ((item is NodeFolder) || (item.Parent is NodeFolder))
                                    {
                                        e.Effects = DragDropEffects.None;
                                        return;
                                    }
                                }
                            }

                            if ((_draggingItem is NodeFolder) && (item is NodeFolder))
                            {
                                e.Effects = DragDropEffects.None;
                                return;
                            }

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

            e.Handled = true;

            if (IsRenamingInProgress)
                return;

            try
            {

                // Verify that this is a valid drop and then store the drop target
                NodeTree TargetItem = GetNearestContainer(e.OriginalSource as UIElement);
                if (TargetItem != null && _draggingItem != null)
                {
                    // カーソル要素がドラッグ中の要素の子要素にある時は何もする必要がないのでreturn
                    if (TargetItem.ContainsChild(_draggingItem))
                        return;
                    if (_draggingItem.ContainsChild(TargetItem))
                        return;

                    _targetItem = TargetItem;
                    e.Effects = DragDropEffects.Move;

                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
            }
            catch (Exception) { }
        }

        private bool CheckDropTarget(NodeTree sourceItem, NodeTree targetItem)
        {
            bool isEqual = false;

            if ((sourceItem == null) || (targetItem == null))
                return false;

            if (sourceItem.Name.Equals(targetItem.Name))
                return false;

            if (IsRenamingInProgress)
                return false;

            // Only allow top level Node.
            if ((targetItem is NodeFolder) || (targetItem is NodeFeed) || (targetItem is NodeService))
                isEqual = true;

            // ServiceをFolder内にドロップさせない。
            if ((_draggingItem is NodeService) && (_draggingItem is not NodeFeed) && (targetItem.Parent is NodeFolder))
                isEqual = false;

            if ((_draggingItem is NodeFolder) && (targetItem.Parent is NodeFolder))
                isEqual = false;

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
                    //if (MessageBox.Show("Would you like to drop " + sourceItem.Name + " into " + targetItem.Name + "", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    //{
                    //}
                    try
                    {
                        if ((sourceItem.Parent as NodeTree).Children.Remove(sourceItem))
                        {
                            targetItem.Children.Add(sourceItem);
                            sourceItem.Parent = targetItem;
                            targetItem.IsExpanded = true;
                        }
                    }
                    catch (Exception) { }
                }
            }
            else if (_insertType == InsertType.Before)
            {
                if ((targetItem is NodeFolder) || (targetItem is NodeFeed) || (targetItem is NodeService))
                {
                    //Asking user wether he want to drop the dragged TreeViewItem here or not
                    //if (MessageBox.Show("Would you like to insert " + sourceItem.Name + " before " + targetItem.Name + "", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    //{
                    //}
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
                    catch (Exception) { }
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
