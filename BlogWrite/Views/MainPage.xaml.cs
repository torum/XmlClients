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


        ViewModel.DebugOutput += (sender, arg) => { this.OnDebugOutput(arg); };

        ViewModel.DebugClear += () => this.OnDebugClear();
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


        /*
        var paragraph = new Paragraph();
        var run = new Run();

        run.Text = arg;

        paragraph.Inlines.Add(run);
        DebugTextBox.Blocks.Add(paragraph);
        */
    }

    public void OnDebugClear()
    {
        DebugTextBox.Text = string.Empty;    
    }
}
