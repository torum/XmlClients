using BlogWrite.ViewModels;
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
using BlogWrite.Models;
using AngleSharp.Dom;

namespace BlogWrite.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();

        ViewModel.DebugOutput += (sender, arg) => { OnDebugOutput(arg); };
        ViewModel.DebugClear += () => OnDebugClear();
    }

    public void OnDebugOutput(string arg)
    {
        // AppendText() is much faster than data binding.
        /*
        DebugTextBox.AppendText(arg);
        DebugTextBox.CaretIndex = DebugTextBox.Text.Length;
        DebugTextBox.ScrollToEnd();
        */

        DebugTextBox.Text = DebugTextBox.Text + arg;
    }

    public void OnDebugClear()
    {
        DebugTextBox.Text = string.Empty;    
    }

    private void TreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
    {
        var node = args.InvokedItem as TreeViewNode;

        if (node is null)
        {
            Debug.WriteLine("node is null...");
            return;
        }

        if (node.Content is NodeFeed item)
        {
            item.IsExpanded = !item.IsExpanded;
        }

        //node.IsExpanded = !node.IsExpanded;
    }
}
