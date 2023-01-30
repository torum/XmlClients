using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Xml;
using BlogWrite.Contracts.Services;
using BlogWrite.Contracts.ViewModels;
using BlogWrite.Core.Contracts.Services;
using BlogWrite.Core.Helpers;
using BlogWrite.Core.Models;
using BlogWrite.Core.Models.Clients;
using BlogWrite.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using Windows.UI.WebUI;
using WinRT.Interop;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;
using System.Text;
using System;
using System.Text.Encodings.Web;
using AngleSharp.Html.Parser;
using AngleSharp.Html;

namespace BlogWrite.ViewModels;

public partial class MainViewModel : ObservableRecipient, INavigationAware
{

    private static readonly string _html = @"
<!DOCTYPE html>                
<html>
  <head>
    <meta charset=""UTF-8"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
    <meta http-equiv=""Content-Type"" content=""text/html;charset=utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <link rel=""stylesheet"" href=""https://blogwrite/Web/style.css"">
    <script src=""https://blogwrite/Web/script.js""></script>
    <script>
        function setChangeListener (div, listener) {

            div.addEventListener(""blur"", listener);
            div.addEventListener(""keyup"", listener);
            div.addEventListener(""paste"", listener);
            div.addEventListener(""copy"", listener);
            div.addEventListener(""cut"", listener);
            div.addEventListener(""delete"", listener);
            div.addEventListener(""mouseup"", listener);

        }

        window.onload=function(){
            const mb = document.getElementById('mytextarea');
            mb.addEventListener('paste', OnPaste, false);
            //mb.addEventListener(""click"", handler2);

            setChangeListener(mb, function(event){
                //console.log(event);

                // Update HTML Source to native-side.
                window.chrome.webview.postMessage(mb.innerHTML);
            });

        }
        //document.getElementById('mytextarea').addEventListener('paste', OnPaste, false);


    </script>

  </head>

  <body>
      <div id=""mytextarea"" contenteditable=""true""><p>&nbsp;</p></div>


  </body>
</html>";

    private string _source = "";
    public string Source
    {
        get => _source;
        set => SetProperty(ref _source, value);
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

    #region == Flags ==

    private bool _isBackEnabled;
    public bool IsBackEnabled
    {
        get => _isBackEnabled;
        set => SetProperty(ref _isBackEnabled, value);
    }

    private bool _isDebugWindowEnabled = false;
    public bool IsDebugWindowEnabled
    {
        get => _isDebugWindowEnabled;
        set => SetProperty(ref _isDebugWindowEnabled, value);
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

    #endregion

    #region == Error ==

    private ErrorObject? _errorObj;
    public ErrorObject? ErrorObj
    {
        get => _errorObj;
        set => SetProperty(ref _errorObj, value);
    }

    #endregion

    #region == Debug Events ==

    //public event EventHandler<string>? DebugOutput;

    private string? _debuEventLog;
    public string? DebugEventLog
    {
        get => _debuEventLog;
        set => SetProperty(ref _debuEventLog, value);
    }

    private readonly Queue<string> debugEvents = new(101);

    public void OnDebugOutput(BaseClient sender, string data)
    {
        if (string.IsNullOrEmpty(data))
            return;

        if (!IsDebugWindowEnabled)
            return;

        if (!App.CurrentDispatcherQueue.HasThreadAccess)
        {
            App.CurrentDispatcherQueue.TryEnqueue(() =>
            {
                OnDebugOutput(sender, data);
            });
            return;
        }

        debugEvents.Enqueue(data);

        if (debugEvents.Count > 100)
            debugEvents.Dequeue();

        DebugEventLog = string.Join('\n', debugEvents.Reverse());

        /*
        if (!App.CurrentDispatcherQueue.HasThreadAccess)
        {
            App.CurrentDispatcherQueue.TryEnqueue(() =>
            {
                DebugOutput?.Invoke(this, Environment.NewLine + data + Environment.NewLine + Environment.NewLine);
            });
            return;
        }
        */

        //IsDebugTextHasText = true;
    }

    //public delegate void DebugClearEventHandler();
    //public event DebugClearEventHandler? DebugClear;

    #endregion

    #region == Services ==

    private readonly INavigationService _navigationService;

    public readonly IWebViewService WebViewService;

    #endregion

    public ICommand TestCommand
    {
        get;
    }

    public MainViewModel(INavigationService navigationService, IWebViewService webViewService)
    {
        _navigationService = navigationService;
        _navigationService.Navigated += OnNavigated;
        /*
        _fileDialogService = fileDialogService;
        _dataAccessService = dataAccessService;
        _feedClientService = feedClientService;
        _feedClientService.BaseClient.DebugOutput += OnDebugOutput;
        */
        WebViewService = webViewService;
        //_webViewService.Initialize();


        TestCommand = new RelayCommand(OnTest);

        /*
        InitializeFeedTree();
        InitializeDatabase();
        InitializeFeedClient();
        */
        //IsDebugWindowEnabled = true;
    }


    private async void OnTest()
    {
        var scriptResult = await WebViewService.CoreWebView2?.ExecuteScriptAsync("test();");
        //Debug.WriteLine(scriptResult);
        
        /*
        var res = await WebViewService.CoreWebView2.ExecuteScriptAsync(@"document.getElementById('mytextarea').innerHTML");
        //Debug.WriteLine(res);

        var json = JsonDocument.Parse(res);
        var text = json.RootElement.ToString();
        //Debug.WriteLine(text);

        var parser = new HtmlParser();
        var document = parser.ParseDocument(text);
        if (document.Body != null)
        {
            //document.Body.ToHtml(sw, new PrettyMarkupFormatter());
            var sb = new StringBuilder();
            if (document.Body.HasChildNodes)
            {
                for (var i = 0; i < document.Body.ChildNodes.Count(); i++)
                {
                    var sw = new StringWriter();
                    document.Body.ChildNodes[i].ToHtml(sw, new PrettyMarkupFormatter());
                    sb.Append(sw.ToString());
                }
            }
            Source = sb.ToString();
        }
        else
        {
            Source = "";
        }
        */
    }

    #region == INavigationService ==

    private void OnNavigated(object sender, NavigationEventArgs e) => IsBackEnabled = _navigationService.CanGoBack;

    public void OnNavigatedTo(object parameter)
    {
        WebViewService.NavigationCompleted += OnNavigationCompleted;
        WebViewService.CoreWebView2Initialized += OnCoreWebView2Initialized;
    }

    public void OnNavigatedFrom()
    {
        WebViewService.UnregisterEvents();
        WebViewService.NavigationCompleted -= OnNavigationCompleted;
        WebViewService.CoreWebView2Initialized -= OnCoreWebView2Initialized;
    }

    #endregion

    private async void OnNavigationCompleted(object? sender, CoreWebView2WebErrorStatus webErrorStatus)
    {
        IsLoading = false;
        //OnPropertyChanged(nameof(BrowserBackCommand));
        //OnPropertyChanged(nameof(BrowserForwardCommand));
        if (webErrorStatus != default)
        {
            HasFailures = true;
        }

        //var scriptResult = await WebViewService.CoreWebView2?.ExecuteScriptAsync("document.getElementById('mytextarea').style.fontWeight = `bold`");

    }

    private void OnCoreWebView2Initialized(object? sender, CoreWebView2InitializedEventArgs arg)
    {
        if (WebViewService.CoreWebView2 is null)
            return;

        WebViewService.CoreWebView2.DOMContentLoaded += OnCoreWebView2DOMContentLoaded;

        WebViewService.CoreWebView2.SetVirtualHostNameToFolderMapping(hostName: "blogwrite",
                                                       folderPath: "",
                                                       accessKind: CoreWebView2HostResourceAccessKind.Allow);

        WebViewService.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

        // not 
        WebViewService.CoreWebView2.PermissionRequested += CoreWebView2OnPermissionRequested;
        /*
        WebViewService.CoreWebView2.PermissionRequested += (_, args) =>
        {
            var uri = new Uri(args.Uri);
            Debug.WriteLine(uri.AbsoluteUri);

            if (args.PermissionKind == CoreWebView2PermissionKind.ClipboardRead)
            {
                args.State = CoreWebView2PermissionState.Allow;
            }
            else
            {
                args.State = CoreWebView2PermissionState.Default;
            }
        };
        */

        WebViewService.NavigateToString(_html);
        //WebViewService.CoreWebView2.Navigate("https://blogwrite/HTML/main.html");

        //WebViewService.CoreWebView2?.AddHostObjectToScript("model", _jsModel);
    }

    private void CoreWebView2OnPermissionRequested(object? sender, CoreWebView2PermissionRequestedEventArgs e)
    {
        Debug.WriteLine($"+++> Requested {e.PermissionKind} for uri {e.Uri}");
        var def = e.GetDeferral();
        if (e.PermissionKind == CoreWebView2PermissionKind.ClipboardRead)
        {
            e.State = CoreWebView2PermissionState.Allow;
        }
        else
        {
            e.State = CoreWebView2PermissionState.Default;
        }
        e.State = CoreWebView2PermissionState.Allow;
        e.Handled = true;
        def.Complete();
    }

    private void OnCoreWebView2DOMContentLoaded(object? sender, CoreWebView2DOMContentLoadedEventArgs args)
    {
        //Debug.Print($"WebView.CoreWebView2.DOMContentLoaded: {nameof(args.NavigationId)} = {args.NavigationId}");
    }

    private async void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        var msg = args.TryGetWebMessageAsString();// args.WebMessageAsJson;
        //Debug.WriteLine(msg);
        //var json = JsonDocument.Parse(msg);
        //var text = json.RootElement.ToString();
        //Debug.WriteLine(text);

        var parser = new HtmlParser();
        var document = await parser.ParseDocumentAsync(msg);//parser.ParseDocument(msg);
        if (document.Body != null)
        {
            //document.Body.ToHtml(sw, new PrettyMarkupFormatter());
            var sb = new StringBuilder();
            if (document.Body.HasChildNodes)
            {
                for (var i = 0; i < document.Body.ChildNodes.Count(); i++)
                {
                    var sw = new StringWriter();
                    document.Body.ChildNodes[i].ToHtml(sw, new PrettyMarkupFormatter());
                    sb.Append(sw.ToString());
                }
            }
            Source = sb.ToString();
        }
        else
        {
            Source = "";
        }
    }
}