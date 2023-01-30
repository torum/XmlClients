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
using BlogWrite.Core.Models;
using AngleSharp.Dom;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Microsoft.Web.WebView2.Core;

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

        ViewModel.WebViewService.Initialize(WebView);


        //ViewModel.DebugOutput += (sender, arg) => { OnDebugOutput(arg); };
        //ViewModel.DebugClear += () => OnDebugClear();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Delay(100);
        WebView.Focus(FocusState.Programmatic);
    }
}
