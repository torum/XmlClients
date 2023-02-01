using System.Text;
using System.Text.Json;
using System.Windows.Input;
using AngleSharp.Html;
using AngleSharp.Html.Parser;
using BlogWrite.Contracts.Services;
using BlogWrite.Contracts.ViewModels;
using BlogWrite.Core.Models;
using BlogWrite.Core.Models.Clients;
using BlogWrite.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using Windows.UI.ViewManagement;

namespace BlogWrite.ViewModels;

public partial class MainViewModel : ObservableRecipient, INavigationAware
{
    private readonly UISettings _uiSettings = new();
    private ElementTheme _elementTheme;

    private static readonly string _richEditHtml = @"
<!DOCTYPE html>                
<html>
  <head>
    <meta charset=""UTF-8"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
    <meta http-equiv=""Content-Type"" content=""text/html;charset=utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <link rel=""stylesheet"" href=""https://blogwrite/Web/richedit_style.css"">
    <script src=""https://blogwrite/Web/richedit_script.js""></script>
    <script>
        function setChangeListener (editor, listener) 
        {
            editor.addEventListener(""blur"", listener);
            editor.addEventListener(""keyup"", listener);
            //editor.addEventListener(""paste"", listener);
            //editor.addEventListener(""copy"", listener);
            editor.addEventListener(""cut"", listener);
            editor.addEventListener(""delete"", listener);
            editor.addEventListener(""mouseup"", listener);
        }

        window.onload=function()
        {
            const editor = document.getElementById('editor');

            document.addEventListener('DOMContentLoaded', () => { editor.focus(); });

            setChangeListener(editor, function(event){
                if (editor.innerHTML !== ''){
                }else{
                    editor.innerHTML = '<p>&nbsp;</p>';
                }

                // Update HTML Source to native-side.
                window.chrome.webview.postMessage(editor.innerHTML);
            });

            editor.addEventListener('paste', OnPaste, false);

        }
    </script>
<script>

</script>
  </head>

  <body>
      <div id=""editor"" contenteditable=""true"" autofocus><p>&nbsp;</p></div>
  </body>
</html>";

    private static readonly string _sourceEditHtml = @"
<!DOCTYPE html>                
<html>
  <head>
    <meta charset=""UTF-8"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
    <meta http-equiv=""Content-Type"" content=""text/html;charset=utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <link rel=""stylesheet"" href=""https://blogwrite/Web/sourceedit_style.css"">
    <script src=""https://blogwrite/Web/sourceedit_script.js""></script>
    <script>
        function setChangeListener (editor, listener) 
        {
            editor.addEventListener(""blur"", listener);
            editor.addEventListener(""keyup"", listener);
            editor.addEventListener(""paste"", listener);
            //editor.addEventListener(""copy"", listener);
            editor.addEventListener(""cut"", listener);
            editor.addEventListener(""delete"", listener);
            editor.addEventListener(""mouseup"", listener);
        }

        window.onload=function()
        {
            const editor = document.getElementById('editor');

            document.addEventListener('DOMContentLoaded', () => { editor.focus(); });

            setChangeListener(editor, function(event){
                if (editor.innerHTML !== ''){
                }else{
                    editor.innerHTML = '<p>&nbsp;</p>';
                }

                // Update HTML Source to native-side.
                window.chrome.webview.postMessage(editor.innerText);
            });

            editor.addEventListener('paste', OnPaste, false);

        }

        window.chrome.webview.addEventListener('message', arg => {
           document.getElementById(""editor"").innerText = arg.data;
        });
    </script>

  </head>

  <body>
      <div id=""editor"" contenteditable=""true"" autofocus><p>&nbsp;</p></div>
  </body>
</html>";

    private int _selectedTabindex;
    public int SelectedTabindex
    {
        get => _selectedTabindex;
        set 
        {
            SetProperty(ref _selectedTabindex, value);

            if (_selectedTabindex == 0)
            {
                // TODO: write source

                FocusRichEdit();
            }
            else if (_selectedTabindex == 1)
            {
                WriteToSourceEdit();

                FocusSourceEdit();
            }
            else if (_selectedTabindex == 2)
            {
                
            }
        }
    }

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

    private bool _isRichEditLoading = true;
    public bool IsRichEditLoading
    {
        get => _isRichEditLoading;
        set => SetProperty(ref _isRichEditLoading, value);
    }

    private bool _isSourceEditLoading = true;
    public bool IsSourceEditLoading
    {
        get => _isSourceEditLoading;
        set => SetProperty(ref _isSourceEditLoading, value);
    }

    private bool _hasRichEditFailures;
    public bool HasRichEditFailures
    {
        get => _hasRichEditFailures;
        set => SetProperty(ref _hasRichEditFailures, value);
    }

    private bool _hasSourceEditFailures;
    public bool HasSourceEditFailures
    {
        get => _hasSourceEditFailures;
        set => SetProperty(ref _hasSourceEditFailures, value);
    }

    private bool _isRichEditDOMLoaded = true;
    public bool IsRichEditDOMLoaded
    {
        get => _isRichEditDOMLoaded;
        set => SetProperty(ref _isRichEditDOMLoaded, value);
    }

    private bool _isSourceEditDOMLoaded = true;
    public bool IsSourceEditDOMLoaded
    {
        get => _isSourceEditDOMLoaded;
        set => SetProperty(ref _isSourceEditDOMLoaded, value);
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

    public readonly IWebViewService WebViewServiceRichEdit;
    public readonly IWebViewService WebViewServiceSourceEdit;

    #endregion

    public ICommand TestCommand
    {
        get;
    }

    public ICommand EditorExecFormatBoldCommand
    {
        get;
    }

    public ICommand EditorExecFormatItalicCommand
    {
        get;
    }

    public event EventHandler<string> WebView2RichEditSetFocus;

    public event EventHandler<string> WebView2SourceEditSetFocus;

    public MainViewModel(INavigationService navigationService, IWebViewService webViewServiceRichEdit, IWebViewService webViewServiceSourceEdit)
    {
        _navigationService = navigationService;
        _navigationService.Navigated += OnNavigated;

        WebViewServiceRichEdit = webViewServiceRichEdit;
        WebViewServiceSourceEdit = webViewServiceSourceEdit;

        TestCommand = new RelayCommand(OnTest);
        EditorExecFormatBoldCommand = new RelayCommand(OnEditorExecFormatBoldCommand);
        EditorExecFormatItalicCommand = new RelayCommand(OnEditorExecFormatItalicCommand);

        if (App.Current.RequestedTheme == ApplicationTheme.Dark)
        {
            _elementTheme = ElementTheme.Dark;
        }
        else if (App.Current.RequestedTheme == ApplicationTheme.Light)
        {
            _elementTheme = ElementTheme.Light;
        }

        WeakReferenceMessenger.Default.Register<ThemeChangedMessage>(this, (r, m) =>
        {
            Debug.WriteLine("MeThemeChangedMessagessage received: " + m.Value);
            var thm = m.Value;
            if (!string.IsNullOrEmpty(thm))
            {
                if (thm == "dark")
                {
                    _elementTheme = ElementTheme.Dark;
                }
                else if (thm == "light")
                {
                    _elementTheme = ElementTheme.Light;
                }
                else if (thm == "default")
                {
                    if (App.Current.RequestedTheme == ApplicationTheme.Dark)
                    {
                        _elementTheme = ElementTheme.Dark;
                    }
                    else if (App.Current.RequestedTheme == ApplicationTheme.Light)
                    {
                        _elementTheme = ElementTheme.Light;
                    }
                }

                ChangeRichEditTheme();
                ChangeSourceEditTheme();
            }
        });

        _uiSettings.ColorValuesChanged += ColorValuesChanged;
    }

    private async void ColorValuesChanged(UISettings sender, object args)
    {
        //var accentColor = sender.GetColorValue(UIColorType.Accent);
        //OR
        //Color accentColor = (Color)Resources["SystemAccentColor"];

        var backgroundColor = sender.GetColorValue(UIColorType.Background);
        var isDarkMode = backgroundColor == Colors.Black;

        if (isDarkMode)
        {
            //Debug.WriteLine("ColorValuesChanged: Dark");
            _elementTheme = ElementTheme.Dark;
        }
        else
        {
            //Debug.WriteLine("ColorValuesChanged: Light");
            _elementTheme = ElementTheme.Light;
        }

        // May fire multiple times...so
        await Task.Delay(2000);

        ChangeRichEditTheme();
        ChangeSourceEditTheme();
    }

    #region == INavigationService ==

    private void OnNavigated(object sender, NavigationEventArgs e) => IsBackEnabled = _navigationService.CanGoBack;

    public void OnNavigatedTo(object parameter)
    {
        WebViewServiceRichEdit.NavigationCompleted += OnRichEditWebView2NavigationCompleted;
        WebViewServiceRichEdit.CoreWebView2Initialized += OnRichEditCoreWebView2Initialized;

        WebViewServiceSourceEdit.NavigationCompleted += OnSourceEditWebView2NavigationCompleted;
        WebViewServiceSourceEdit.CoreWebView2Initialized += OnSourceEditCoreWebView2Initialized;
    }

    public void OnNavigatedFrom()
    {
        WebViewServiceRichEdit.NavigationCompleted -= OnRichEditWebView2NavigationCompleted;
        WebViewServiceRichEdit.CoreWebView2Initialized -= OnRichEditCoreWebView2Initialized;

        WebViewServiceSourceEdit.NavigationCompleted -= OnSourceEditWebView2NavigationCompleted;
        WebViewServiceSourceEdit.CoreWebView2Initialized -= OnSourceEditCoreWebView2Initialized;

        //WebViewService.UnregisterEvents();
    }

    #endregion


    #region == IWebViewService ==

    #region == WebViewRichEditService ==

    private void ChangeRichEditTheme()
    {
        if (!IsRichEditDOMLoaded)
        {
            return;
        }

        App.CurrentDispatcherQueue?.TryEnqueue(async () =>
        {
            string thm;
            if (_elementTheme == ElementTheme.Dark)
            {
                thm = "dark";
            }
            else
            {
                thm = "light";
            }
            await WebViewServiceRichEdit.CoreWebView2?.ExecuteScriptAsync($"toggleTheme(\"{thm}\");");

            Debug.WriteLine("ChangeRichEditTheme: " + thm);
        });
    }

    private async void OnEditorExecFormatBoldCommand()
    {
        var scriptResult = await WebViewServiceRichEdit.CoreWebView2?.ExecuteScriptAsync("document.execCommand(\"bold\", false);");
        Debug.WriteLine(scriptResult);
    }

    private async void OnEditorExecFormatItalicCommand()
    {
        var scriptResult = await WebViewServiceRichEdit.CoreWebView2?.ExecuteScriptAsync("document.execCommand(\"italic\", false);");
        Debug.WriteLine(scriptResult);
    }

    private async void FocusRichEdit()
    {
        if (IsRichEditDOMLoaded)
        {
            WebView2RichEditSetFocus?.Invoke(this, "");
            //await WebViewServiceRichEdit.CoreWebView2?.ExecuteScriptAsync($"focusEditor();");
            await WebViewServiceRichEdit.CoreWebView2?.ExecuteScriptAsync(@"document.getElementById('editor').focus();");
            //Debug.WriteLine("FocusEditor()");
        }
    }

    private async void OnTest()
    {
        var scriptResult = await WebViewServiceRichEdit.CoreWebView2?.ExecuteScriptAsync("test();");
        Debug.WriteLine(scriptResult);

        /*
        var res = await WebViewServiceRichEdit.CoreWebView2.ExecuteScriptAsync(@"document.getElementById('mytextarea').innerHTML");
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


    private void OnRichEditWebView2NavigationCompleted(object? sender, CoreWebView2WebErrorStatus webErrorStatus)
    {
        IsRichEditLoading = false;
        //OnPropertyChanged(nameof(BrowserBackCommand));
        //OnPropertyChanged(nameof(BrowserForwardCommand));
        if (webErrorStatus != default)
        {
            HasRichEditFailures = true;
        }
    }

    private void OnRichEditCoreWebView2Initialized(object? sender, CoreWebView2InitializedEventArgs arg)
    {
        if (WebViewServiceRichEdit.CoreWebView2 is null)
            return;

        //WebViewService.CoreWebView2?.AddHostObjectToScript("model", _jsModel);

        WebViewServiceRichEdit.CoreWebView2.SetVirtualHostNameToFolderMapping(hostName: "blogwrite", folderPath: "", accessKind: CoreWebView2HostResourceAccessKind.Allow);

        WebViewServiceRichEdit.CoreWebView2.DOMContentLoaded += OnRichEditCoreWebView2DOMContentLoaded;
        WebViewServiceRichEdit.CoreWebView2.WebMessageReceived += OnRichEditWebMessageReceived;
        // Not working because not supported.
        WebViewServiceRichEdit.CoreWebView2.PermissionRequested += OnRichEditCoreWebView2OnPermissionRequested;
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

        WebViewServiceRichEdit.NavigateToString(_richEditHtml);
        //WebViewService.CoreWebView2.Navigate("https://blogwrite/HTML/main.html");
    }

    private void OnRichEditCoreWebView2OnPermissionRequested(object? sender, CoreWebView2PermissionRequestedEventArgs e)
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

    private async void OnRichEditCoreWebView2DOMContentLoaded(object? sender, CoreWebView2DOMContentLoadedEventArgs args)
    {
        IsRichEditDOMLoaded = true;
        /*
        string thm;
        if (_elementTheme == ElementTheme.Dark)
        {
            thm = "dark";
        }
        else
        {
            thm = "light";
        }
        await WebViewService.CoreWebView2?.ExecuteScriptAsync($"toggleTheme({thm});");
        */
        ChangeRichEditTheme();

        var res = await WebViewServiceRichEdit.CoreWebView2.ExecuteScriptAsync(@"document.getElementById('editor').innerHTML");

        var json = JsonDocument.Parse(res);
        var text = json.RootElement.ToString();
        
        WriteToSource(text);

        //WebViewServiceRichEdit.CoreWebView2.OpenDevToolsWindow();
    }

    private async void OnRichEditWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        var msg = args.TryGetWebMessageAsString();// args.WebMessageAsJson;
        //Debug.WriteLine(msg);
        //var json = JsonDocument.Parse(msg);
        //var text = json.RootElement.ToString();
        //Debug.WriteLine(text);

        WriteToSource(msg);

        /*
        var scriptResult = await WebViewService.CoreWebView2.ExecuteScriptAsync(@"isSelectionInTag('B');");
        */
        var scriptResult = await WebViewServiceRichEdit.CoreWebView2.ExecuteScriptAsync(@"document.queryCommandState('bold');");
        if (scriptResult == "true")
        {
            Debug.WriteLine("BOLD");
        }
    }

    private async void WriteToSource(string source)
    {
        var parser = new HtmlParser();
        var document = await parser.ParseDocumentAsync(source);//parser.ParseDocument(msg);
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
                    var s = sw.ToString();
                    if (!string.IsNullOrEmpty(s))
                    {
                        sb.Append(s);
                    }
                }
            }
            Source = sb.ToString().Trim();
        }
        else
        {
            Source = "";
        }
    }

    #endregion

    #region == WebViewSourceEditService ==

    private void ChangeSourceEditTheme()
    {
        if (!IsSourceEditDOMLoaded)
        {
            return;
        }

        App.CurrentDispatcherQueue?.TryEnqueue(async () =>
        {
            string thm;
            if (_elementTheme == ElementTheme.Dark)
            {
                thm = "dark";
            }
            else
            {
                thm = "light";
            }
            await WebViewServiceSourceEdit.CoreWebView2?.ExecuteScriptAsync($"toggleTheme(\"{thm}\");");

            Debug.WriteLine("ChangeSourceEditTheme: " + thm);
        });
    }

    private async void FocusSourceEdit()
    {
        if (IsSourceEditDOMLoaded)
        {
            WebView2SourceEditSetFocus?.Invoke(this, "");
            //await WebViewServiceSourceEdit.CoreWebView2?.ExecuteScriptAsync($"focusEditor();");
            await WebViewServiceSourceEdit.CoreWebView2?.ExecuteScriptAsync(@"document.getElementById('editor').focus();");
            //Debug.WriteLine("FocusEditor()");
        }
    }

    private void OnSourceEditWebView2NavigationCompleted(object? sender, CoreWebView2WebErrorStatus webErrorStatus)
    {
        IsSourceEditLoading = false;
        //OnPropertyChanged(nameof(BrowserBackCommand));
        //OnPropertyChanged(nameof(BrowserForwardCommand));
        if (webErrorStatus != default)
        {
            HasSourceEditFailures = true;
        }
    }

    private void OnSourceEditCoreWebView2Initialized(object? sender, CoreWebView2InitializedEventArgs arg)
    {
        if (WebViewServiceSourceEdit.CoreWebView2 is null)
            return;

        //WebViewService.CoreWebView2?.AddHostObjectToScript("model", _jsModel);

        WebViewServiceSourceEdit.CoreWebView2.SetVirtualHostNameToFolderMapping(hostName: "blogwrite", folderPath: "", accessKind: CoreWebView2HostResourceAccessKind.Allow);

        WebViewServiceSourceEdit.CoreWebView2.DOMContentLoaded += OnSourceEditCoreWebView2DOMContentLoaded;
        WebViewServiceSourceEdit.CoreWebView2.WebMessageReceived += OnSourceEditWebMessageReceived;
        // Not working because not supported.
        WebViewServiceSourceEdit.CoreWebView2.PermissionRequested += OnSourceEditCoreWebView2OnPermissionRequested;
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

        WebViewServiceSourceEdit.NavigateToString(_sourceEditHtml);
        //WebViewService.CoreWebView2.Navigate("https://blogwrite/HTML/main.html");
    }

    private void OnSourceEditCoreWebView2OnPermissionRequested(object? sender, CoreWebView2PermissionRequestedEventArgs e)
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

    private void OnSourceEditCoreWebView2DOMContentLoaded(object? sender, CoreWebView2DOMContentLoadedEventArgs args)
    {
        IsSourceEditDOMLoaded = true;
        /*
        string thm;
        if (_elementTheme == ElementTheme.Dark)
        {
            thm = "dark";
        }
        else
        {
            thm = "light";
        }
        await WebViewService.CoreWebView2?.ExecuteScriptAsync($"toggleTheme({thm});");
        */
        ChangeSourceEditTheme();

        //var res = await WebViewServiceSourceEdit.CoreWebView2.ExecuteScriptAsync(@"document.getElementById('editor').innerHTML");

        //var json = JsonDocument.Parse(res);
        //var text = json.RootElement.ToString();

        //WriteToSource(text);

        //WebViewServiceRichEdit.CoreWebView2.OpenDevToolsWindow();
    }

    private void OnSourceEditWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        var msg = args.TryGetWebMessageAsString();// args.WebMessageAsJson;
        //Debug.WriteLine(msg);

        //WriteToSource(msg);
    }

    private void WriteToSourceEdit()
    {
        if (!IsSourceEditDOMLoaded)
            return;

        WebViewServiceSourceEdit.CoreWebView2?.PostWebMessageAsString(Source);

    }

    #endregion

    #endregion
}