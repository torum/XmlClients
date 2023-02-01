using FeedDesk.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Windows.Storage;
using BlogWrite.Core.Models;
using AngleSharp.Dom;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Microsoft.UI.Xaml.Markup;

namespace FeedDesk.Views;

public sealed partial class FeedsPage : Page
{
    public FeedsViewModel ViewModel
    {
        get;
    }

    public FeedsPage()
    {
        ViewModel = App.GetService<FeedsViewModel>();
        try
        {
            InitializeComponent();
        }
        catch (XamlParseException parseException)
        {
            Debug.WriteLine($"Unhandled XamlParseException in FeedsPage: {parseException.Message}");
            foreach (var key in parseException.Data.Keys)
            {
                Debug.WriteLine("{Key}:{Value}", key.ToString(), parseException.Data[key]?.ToString());
            }
            throw;
        }

        //ViewModel.DebugOutput += (sender, arg) => { OnDebugOutput(arg); };
        //ViewModel.DebugClear += () => OnDebugClear();
    }

    public void OnDebugOutput(string arg)
    {
        // AppendText() is much faster than data binding.
        /*
        DebugTextBox.AppendText(arg);
        DebugTextBox.CaretIndex = DebugTextBox.Text.Length;
        DebugTextBox.ScrollToEnd();
        */

        DebugTextBox.Text = arg;
    }

    public void OnDebugClear()
    {
        DebugTextBox.Text = string.Empty;    
    }

    private void TreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
    {
        /*
        if (args.InvokedItem is NodeTree nt)
        {
            if (nt.Children.Count > 0)
                nt.IsExpanded = !nt.IsExpanded;
        }
        */
        /*
        if (args.InvokedItem is NodeTree nt)
        {
            if (nt.Children.Count > 0)
            {
                if (!nt.IsExpanded)
                    nt.IsExpanded = true;
            }
        }
        */
    }

    private void TreeViewItem_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (sender is TreeViewItem tvi)
        {
            if (tvi.IsSelected)
                tvi.IsExpanded = !tvi.IsExpanded;
        }
    }

    private async void ListViewEntryItem_DoubleTappedAsync(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        //
        if (ListViewEntryItem.SelectedItem != null)
        {
            if (ListViewEntryItem.SelectedItem is EntryItem item)

            if (item != null)
            {
                if (item.AltHtmlUri != null)
                {
                    await Windows.System.Launcher.LaunchUriAsync(item.AltHtmlUri);
                }
            }
        }
    }

    private void ListViewEntryItem_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement)
        {
            if (((FrameworkElement)e.OriginalSource).DataContext is FeedEntryItem item)
            {
                ListViewEntryItem.SelectedItem = item;
            }
        }
    }

    public static List<NodeTree> DraggedItems
    {
        get; set;
    }=
    new List<NodeTree>();

    private void TreeView_DragItemsStarting(TreeView sender, TreeViewDragItemsStartingEventArgs args)
    {
        // If busy downloading.
        //args.Cancel = true;

        if (args.Items.Count > 0)
        {
            if (args.Items[0] is not NodeTree)
            {
                args.Cancel = true;
                return;
            }

            // only allow feed and folders
            if ((args.Items[0] is NodeFeed) || (args.Items[0] is NodeFolder))
            {

            }
            else
            {
                args.Cancel = true;
                return;
            }
        }

        foreach (NodeTree item in args.Items)
        {
            DraggedItems.Add(item);
        }
    }

    private void TreeView_DragItemsCompleted(TreeView sender, TreeViewDragItemsCompletedEventArgs args)
    {
        foreach (NodeTree item in args.Items)
        {
            item.Parent = args.NewParentItem as NodeTree;
            
            // dropped on rootnodes which means NewParentItem is null.
            item.Parent ??= ViewModel.Root;
        }

        DraggedItems.Clear();

        ViewModel.SaveServiceXml();
    }

    private void TreeViewItem_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
    {
        if (sender is TreeViewItem item)
        {
            item.IsSelected = true;
        }
    }

}
