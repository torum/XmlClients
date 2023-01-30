using System.Windows.Input;
using FeedDesk.Contracts.Services;
using FeedDesk.Contracts.ViewModels;
using BlogWrite.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace FeedDesk.ViewModels;

public class EntryDetailsViewModel : ObservableRecipient, INavigationAware
{
    private EntryItem? _item;

    public EntryItem? Item
    {
        get => _item;
        set => SetProperty(ref _item, value);
    }

    public ICommand EntryViewExternalCommand
    {
        get;
    }

    private readonly INavigationService _navigationService;

    public ICommand GoBackCommand
    {
        get;
    }


    public IWebViewService WebViewService
    {
        get;
    }

    /*
    private static readonly string _html = @"
                <html>
                    <head>
                        <title></title>
                    </head>
                    <body style=""background-color:#212121;"">
                    </body>
                </html>";
    */

    private Uri? _source = null;// new("");
    public Uri? Source
    {
        get => _source;
        set => SetProperty(ref _source, value);
    }

    private bool _isLoading = true;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private bool _hasFailures;
    public bool HasFailures
    {
        get => _hasFailures;
        set => SetProperty(ref _hasFailures, value);
    }

    private string _entryContentText;
    public string EntryContentText
    {
        get => _entryContentText;
        set => SetProperty(ref _entryContentText, value);
    }

    private string _entryContentHTML;
    public string EntryContentHTML
    {
        get => _entryContentHTML;
        set => SetProperty(ref _entryContentHTML, value);
    }

    private bool _isContentText;
    public bool IsContentText
    {
        get => _isContentText;
        set => SetProperty(ref _isContentText, value);
    }

    private bool _isContentHTML;
    public bool IsContentHTML
    {
        get => _isContentHTML;
        set => SetProperty(ref _isContentHTML, value);
    }

    private static string WrapHtmlContent(string source, string? styles = null)
    {
        styles ??= @"
::-webkit-scrollbar { width: 17px; height: 3px;}
::-webkit-scrollbar-button {  background-color: #666; }
::-webkit-scrollbar-track {  background-color: #646464; box-shadow: 0 0 4px #aaa inset;}
::-webkit-scrollbar-track-piece { background-color: #212121;}
::-webkit-scrollbar-thumb { height: 50px; background-color: #666;}
::-webkit-scrollbar-corner { background-color: #646464;}}
::-webkit-resizer { background-color: #666;}

body {
	
	line-height: 1.75em;
	font-size: 13px;
	background-color: #222;
	color: #aaa;
}

p {
	font-size: 13px;
}

h1 {
	font-size: 30px;
	line-height: 34px;
}

h2 {
	font-size: 20px;
	line-height: 25px;
}

h3 {
	font-size: 16px;
	line-height: 27px;
	padding-top: 15px;
	padding-bottom: 15px;
	border-bottom: 1px solid #D8D8D8;
	border-top: 1px solid #D8D8D8;
}

hr {
	height: 1px;
	background-color: #d8d8d8;
	border: none;
	width: 100%;
	margin: 0px;
}

a[href] {
	color: #1e8ad6;
}

a[href]:hover {
	color: #3ba0e6;
}

img {
    width: 160;
    height: auto;
    margin: 6px 12px 12px 6px;
}

li {
	line-height: 1.5em;
}
                ";

        return String.Format(
            @"<html>
                    <head>
                        <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />

                        <!-- saved from url=(0014)about:internet -->

                        <style type='text/css'>
                            body {{ font: 12pt verdana; color: #101010; background: #cccccc; }}
                            table, td, th, tr {{ border: 1px solid black; border-collapse: collapse; }}
                        </style>

                        <!-- Custom style sheet -->
                        <style type='text/css'>{1}</style>
                    </head>
                    <body>{0}</body>
                </html>",
            source, styles);
    }

    public EntryDetailsViewModel(INavigationService navigationService, IWebViewService webViewService)
    {
        _navigationService = navigationService;
        WebViewService = webViewService;

        GoBackCommand = new RelayCommand(OnGoBack);
        EntryViewExternalCommand = new RelayCommand(OnEntryViewExternal);

    }

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is EntryItem item)
        {
            Item = item;

            if (Item.ContentType == EntryItem.ContentTypes.text)
            {
                IsContentText = true;
                EntryContentText = Item.Content;
            }
            else
            {
                IsContentText = false;
                EntryContentText = "";
            }

            if (Item.ContentType == EntryItem.ContentTypes.textHtml)
            {
                IsContentHTML = true;
                EntryContentHTML = Item.Content;
            }
            else
            {
                IsContentHTML = false;
                EntryContentHTML = "";
            }
        }

        WebViewService.NavigationCompleted += OnNavigationCompleted;
        WebViewService.CoreWebView2Initialized += OnCoreWebView2Initialized;

    }

    public void OnNavigatedFrom()
    {
        WebViewService.UnregisterEvents();
        WebViewService.NavigationCompleted -= OnNavigationCompleted;
        WebViewService.CoreWebView2Initialized -= OnCoreWebView2Initialized;
    }

    private void OnGoBack()
    {
        if (_navigationService.CanGoBack)
        {
            _navigationService.GoBack();
        }
    }

    private void OnNavigationCompleted(object? sender, CoreWebView2WebErrorStatus webErrorStatus)
    {
        IsLoading = false;
        //OnPropertyChanged(nameof(BrowserBackCommand));
        //OnPropertyChanged(nameof(BrowserForwardCommand));
        if (webErrorStatus != default)
        {
            HasFailures = true;
        }
    }

    private void OnCoreWebView2Initialized(object? sender, CoreWebView2InitializedEventArgs arg)
    {
        if (Item != null)
            WebViewService.NavigateToString(WrapHtmlContent(Item.Content));
    }

    private async void OnEntryViewExternal()
    {
        if (Item != null)
        {
            if (Item.AltHtmlUri != null)
            {
                await Windows.System.Launcher.LaunchUriAsync(Item.AltHtmlUri);
            }
        }
    }
}