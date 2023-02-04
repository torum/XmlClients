using System.Text;
using System.Text.Json;
using System.Windows.Input;
using AngleSharp.Html;
using AngleSharp.Html.Parser;
using BlogDesk.Contracts.Services;
using BlogDesk.Contracts.ViewModels;
using BlogWrite.Core.Models;
using BlogWrite.Core.Models.Clients;
using BlogDesk.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using Windows.UI.ViewManagement;
using BlogWrite.Core.Helpers;
using BlogDesk.Views;

namespace BlogDesk.ViewModels;

public partial class EditorViewModel : ObservableRecipient
{

    public ICommand MenuFileExitCommand
    {
        get;
    }


    public ICommand MenuFileNewCommand
    {
        get;
    }

    private readonly UISettings _uiSettings = new();

    private ElementTheme _theme;
    public ElementTheme Theme
    {
        get => _theme;
        set => _theme = value;
    }

    private static readonly string _richEditHtml = @"
<!DOCTYPE html>                
<html>
  <head>
    <meta charset=""UTF-8"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
    <meta http-equiv=""Content-Type"" content=""text/html;charset=utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <link rel=""stylesheet"" href=""https://blogdesk/Web/richedit_style.css"">
    <script src=""https://blogdesk/Web/richedit_script.js""></script>
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

        window.chrome.webview.addEventListener('message', arg => {
           document.getElementById('editor').innerHTML = arg.data;
        });
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
    <link rel=""stylesheet"" href=""https://blogdesk/Web/sourceedit_style.css"">
    <script src=""https://blogdesk/Web/sourceedit_script.js""></script>
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

    private static readonly string _previewBrowserHtml = @"
<!DOCTYPE html>                
<html>
  <head>
    <meta charset=""UTF-8"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
    <meta http-equiv=""Content-Type"" content=""text/html;charset=utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <link rel=""stylesheet"" href=""https://blogdesk/Web/preview_style.css"">
    <script src=""https://blogdesk/Web/preview_script.js""></script>
    <script>
        window.chrome.webview.addEventListener('message', arg => {
           document.getElementById(""editor"").innerHTML = arg.data;
        });
    </script>
  </head>

  <body>
      <div id=""editor"" autofocus><p>&nbsp;</p></div>
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
                WriteToRichEdit();

                FocusRichEdit();
            }
            else if (_selectedTabindex == 1)
            {
                WriteToSourceEdit();

                FocusSourceEdit();
            }
            else if (_selectedTabindex == 2)
            {
                WriteToPreviewBrowser();

                FocusPreviewBrowser();
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

    private bool _isPreviewBrowserLoading = true;
    public bool IsPreviewBrowserLoading
    {
        get => _isPreviewBrowserLoading;
        set => SetProperty(ref _isPreviewBrowserLoading, value);
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

    private bool _hasPreviewBrowserFailures;
    public bool HasPreviewBrowserFailures
    {
        get => _hasPreviewBrowserFailures;
        set => SetProperty(ref _hasPreviewBrowserFailures, value);
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

    private bool _isPreviewBrowserDOMLoaded = true;
    public bool IsPreviewBrowserDOMLoaded
    {
        get => _isPreviewBrowserDOMLoaded;
        set => SetProperty(ref _isPreviewBrowserDOMLoaded, value);
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

    //private readonly INavigationChildService _navigationService;

    public readonly IWebViewService WebViewServiceRichEdit;
    public readonly IWebViewService WebViewServiceSourceEdit;
    public readonly IWebViewService WebViewServicePreviewBrowser;

    #endregion

    #region == Commands ==

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

    #endregion

    #region == Events ==

    public event EventHandler<string> WebView2RichEditSetFocus;

    public event EventHandler<string> WebView2SourceEditSetFocus;

    public event EventHandler<string> WebView2PreviewBrowserSetFocus;

    //public event EventHandler<string> WindowClosing;

    public event EventHandler<ElementTheme> ThemeChanged;

    #endregion

    public EditorViewModel(IWebViewService webViewServiceRichEdit, IWebViewService webViewServiceSourceEdit, IWebViewService webViewServicePreviewBrowser)
    {
        WebViewServiceRichEdit = webViewServiceRichEdit;
        WebViewServiceSourceEdit = webViewServiceSourceEdit;
        WebViewServicePreviewBrowser = webViewServicePreviewBrowser;

        MenuFileExitCommand = new RelayCommand(OnMenuFileExit);
        MenuFileNewCommand = new RelayCommand(OnMenuFileNew);

        TestCommand = new RelayCommand(OnTest);
        EditorExecFormatBoldCommand = new RelayCommand(OnEditorExecFormatBoldCommand);
        EditorExecFormatItalicCommand = new RelayCommand(OnEditorExecFormatItalicCommand);

        // This does not consider the theme which is changed/specified in settings. Need to be set in SetTheme() from codebehind.
        if (App.Current.RequestedTheme == ApplicationTheme.Dark)
        {
            _theme = ElementTheme.Dark;
        }
        else if (App.Current.RequestedTheme == ApplicationTheme.Light)
        {
            _theme = ElementTheme.Light;
        }
        else
        {
            _theme = ElementTheme.Dark;
        }

        WeakReferenceMessenger.Default.Register<ThemeChangedMessage>(this, (r, m) =>
        {
            Debug.WriteLine("MeThemeChangedMessagessage received: " + m.Value);
            var thm = m.Value;
            if (!string.IsNullOrEmpty(thm))
            {
                if (thm == "dark")
                {
                    _theme = ElementTheme.Dark;
                }
                else if (thm == "light")
                {
                    _theme = ElementTheme.Light;
                }
                else if (thm == "default")
                {
                    if (App.Current.RequestedTheme == ApplicationTheme.Dark)
                    {
                        _theme = ElementTheme.Dark;
                    }
                    else if (App.Current.RequestedTheme == ApplicationTheme.Light)
                    {
                        _theme = ElementTheme.Light;
                    }
                }

                ChangeRichEditTheme();
                ChangeSourceEditTheme();
                ChangePreviewBrowserTheme();

                ThemeChanged?.Invoke(this, _theme);
            }
        });
        

        // System theme changed.
        _uiSettings.ColorValuesChanged += SystemUISettingColorValuesChanged;


        WebViewServiceRichEdit.NavigationCompleted += OnRichEditWebView2NavigationCompleted;
        WebViewServiceRichEdit.CoreWebView2Initialized += OnRichEditCoreWebView2Initialized;

        WebViewServiceSourceEdit.NavigationCompleted += OnSourceEditWebView2NavigationCompleted;
        WebViewServiceSourceEdit.CoreWebView2Initialized += OnSourceEditCoreWebView2Initialized;

        WebViewServicePreviewBrowser.NavigationCompleted += OnPreviewBrowserWebView2NavigationCompleted;
        WebViewServicePreviewBrowser.CoreWebView2Initialized += OnPreviewBrowserCoreWebView2Initialized;

    }

    private void OnMenuFileExit()
    {
        //
    }

    private void OnMenuFileNew()
    {
        //
    }

    public void SetTheme(ElementTheme theme)
    {
        _theme = theme;
    }

    public bool Closing()
    {
        //Debug.WriteLine("EditorViewModel Closing");

        // TODO: check if dirty.

        // TODO:
        WebViewServiceRichEdit.UnregisterEvents();
        WebViewServiceRichEdit.CoreWebView2.DOMContentLoaded -= OnRichEditCoreWebView2DOMContentLoaded;
        WebViewServiceRichEdit.CoreWebView2.WebMessageReceived -= OnRichEditWebMessageReceived;
        WebViewServiceRichEdit.CoreWebView2.PermissionRequested -= OnRichEditCoreWebView2OnPermissionRequested;

        WebViewServiceSourceEdit.UnregisterEvents();
        WebViewServiceSourceEdit.CoreWebView2.DOMContentLoaded -= OnSourceEditCoreWebView2DOMContentLoaded;
        WebViewServiceSourceEdit.CoreWebView2.WebMessageReceived -= OnSourceEditWebMessageReceived;
        WebViewServiceSourceEdit.CoreWebView2.PermissionRequested -= OnSourceEditCoreWebView2OnPermissionRequested;

        WebViewServicePreviewBrowser.UnregisterEvents();
        WebViewServicePreviewBrowser.CoreWebView2.DOMContentLoaded -= OnPreviewBrowserCoreWebView2DOMContentLoaded;
        WebViewServicePreviewBrowser.CoreWebView2.NavigationStarting -= OnPreviewBrowserCoreWebView2NavigationStarting;
        WebViewServicePreviewBrowser.CoreWebView2.FrameNavigationStarting -= OnPreviewBrowserCoreWebView2NavigationStarting;

        _uiSettings.ColorValuesChanged -= SystemUISettingColorValuesChanged;

        WeakReferenceMessenger.Default.UnregisterAll(this);

        return true;
    }

    #region == IWebViewServices ==

    #region == WebViewRichEdit ==

    private void ChangeRichEditTheme()
    {
        if (!IsRichEditDOMLoaded)
        {
            return;
        }

        App.CurrentDispatcherQueue?.TryEnqueue(async () =>
        {
            string thm;
            if (_theme == ElementTheme.Dark)
            {
                thm = "dark";
            }
            else
            {
                thm = "light";
            }
            await WebViewServiceRichEdit.CoreWebView2?.ExecuteScriptAsync($"toggleTheme(\"{thm}\");");

            //Debug.WriteLine("ChangeRichEditTheme: " + thm);
        });
    }

    private async void OnEditorExecFormatBoldCommand()
    {
        var scriptResult = await WebViewServiceRichEdit.CoreWebView2?.ExecuteScriptAsync("document.execCommand(\"bold\", false);");
        Debug.WriteLine(scriptResult);

        var res = await WebViewServiceRichEdit.CoreWebView2.ExecuteScriptAsync(@"document.getElementById('editor').innerHTML");

        var json = JsonDocument.Parse(res);
        var text = json.RootElement.ToString();

        WriteToSource(text);
    }

    private async void OnEditorExecFormatItalicCommand()
    {
        var scriptResult = await WebViewServiceRichEdit.CoreWebView2?.ExecuteScriptAsync("document.execCommand(\"italic\", false);");
        Debug.WriteLine(scriptResult);

        var res = await WebViewServiceRichEdit.CoreWebView2.ExecuteScriptAsync(@"document.getElementById('editor').innerHTML");

        var json = JsonDocument.Parse(res);
        var text = json.RootElement.ToString();

        WriteToSource(text);
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

        WebViewServiceRichEdit.CoreWebView2.SetVirtualHostNameToFolderMapping(hostName: "blogdesk", folderPath: "", accessKind: CoreWebView2HostResourceAccessKind.Allow);

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
        //WebViewService.CoreWebView2.Navigate("https://blogdesk/HTML/main.html");
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

    private void WriteToRichEdit()
    {
        if (!IsRichEditDOMLoaded)
            return;

        WebViewServiceRichEdit.CoreWebView2?.PostWebMessageAsString(Source);
    }

    #endregion

    #region == WebViewSourceEdit ==

    private void ChangeSourceEditTheme()
    {
        if (!IsSourceEditDOMLoaded)
        {
            return;
        }

        App.CurrentDispatcherQueue?.TryEnqueue(async () =>
        {
            string thm;
            if (_theme == ElementTheme.Dark)
            {
                thm = "dark";
            }
            else
            {
                thm = "light";
            }
            await WebViewServiceSourceEdit.CoreWebView2?.ExecuteScriptAsync($"toggleTheme(\"{thm}\");");

            //Debug.WriteLine("ChangeSourceEditTheme: " + thm);
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

        WebViewServiceSourceEdit.CoreWebView2.SetVirtualHostNameToFolderMapping(hostName: "blogdesk", folderPath: "", accessKind: CoreWebView2HostResourceAccessKind.Allow);

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
        //WebViewService.CoreWebView2.Navigate("https://blogdesk/HTML/main.html");
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

        // TODO:
        Source = msg;
    }

    private void WriteToSourceEdit()
    {
        if (!IsSourceEditDOMLoaded)
            return;

        WebViewServiceSourceEdit.CoreWebView2?.PostWebMessageAsString(Source);
    }

    #endregion

    #region == WebViewPreviewBrowser ==

    private void ChangePreviewBrowserTheme()
    {
        if (!IsSourceEditDOMLoaded)
        {
            return;
        }

        App.CurrentDispatcherQueue?.TryEnqueue(async () =>
        {
            string thm;
            if (_theme == ElementTheme.Dark)
            {
                thm = "dark";
            }
            else
            {
                thm = "light";
            }
            await WebViewServicePreviewBrowser.CoreWebView2?.ExecuteScriptAsync($"toggleTheme(\"{thm}\");");

            //Debug.WriteLine("ChangePreviewBrowserTheme: " + thm);
        });
    }

    private async void FocusPreviewBrowser()
    {
        if (IsPreviewBrowserDOMLoaded)
        {
            WebView2PreviewBrowserSetFocus?.Invoke(this, "");
            //await WebViewServiceSourceEdit.CoreWebView2?.ExecuteScriptAsync($"focusEditor();");
            await WebViewServicePreviewBrowser.CoreWebView2?.ExecuteScriptAsync(@"document.getElementById('editor').focus();");
            //Debug.WriteLine("FocusEditor()");
        }
    }

    private void OnPreviewBrowserWebView2NavigationCompleted(object? sender, CoreWebView2WebErrorStatus webErrorStatus)
    {
        IsPreviewBrowserLoading = false;
        //OnPropertyChanged(nameof(BrowserBackCommand));
        //OnPropertyChanged(nameof(BrowserForwardCommand));
        if (webErrorStatus != default)
        {
            HasPreviewBrowserFailures = true;
        }
    }

    private void OnPreviewBrowserCoreWebView2Initialized(object? sender, CoreWebView2InitializedEventArgs arg)
    {
        if (WebViewServicePreviewBrowser.CoreWebView2 is null)
            return;

        //WebViewService.CoreWebView2?.AddHostObjectToScript("model", _jsModel);

        WebViewServicePreviewBrowser.CoreWebView2.SetVirtualHostNameToFolderMapping(hostName: "blogdesk", folderPath: "", accessKind: CoreWebView2HostResourceAccessKind.Allow);

        WebViewServicePreviewBrowser.CoreWebView2.DOMContentLoaded += OnPreviewBrowserCoreWebView2DOMContentLoaded;
        WebViewServicePreviewBrowser.CoreWebView2.NavigationStarting += OnPreviewBrowserCoreWebView2NavigationStarting;
        WebViewServicePreviewBrowser.CoreWebView2.FrameNavigationStarting += OnPreviewBrowserCoreWebView2NavigationStarting;

        WebViewServicePreviewBrowser.NavigateToString(_previewBrowserHtml);
    }

    private void OnPreviewBrowserCoreWebView2DOMContentLoaded(object? sender, CoreWebView2DOMContentLoadedEventArgs args)
    {
        IsPreviewBrowserDOMLoaded = true;

        ChangePreviewBrowserTheme();

        //WebViewServiceRichEdit.CoreWebView2.OpenDevToolsWindow();
    }

    private async void OnPreviewBrowserCoreWebView2NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs args)
    {
        if (!string.IsNullOrEmpty(args.Uri))
        {
            if (args.Uri.StartsWith("http"))
            {
                args.Cancel = true;

                Uri? uri = null;
                try
                {
                    uri = new Uri(args.Uri);
                }
                catch(Exception ex) { Debug.WriteLine("Invalid Uri: " + args.Uri + "\n" + ex); }

                if (uri != null)
                {
                    await Windows.System.Launcher.LaunchUriAsync(uri);
                }
            }
            else if (args.Uri.StartsWith("data:text/html;charset=utf-8;base64"))
            {
                // NavigateToString..
            }
            else
            {
                Debug.WriteLine("Preview browser navigation canceled: " + args.Uri);
                args.Cancel = true;
            }
        }

                
    }

    private void WriteToPreviewBrowser()
    {
        if (!IsSourceEditDOMLoaded)
            return;

        WebViewServicePreviewBrowser.CoreWebView2?.PostWebMessageAsString(Source);

    }

    #endregion

    #endregion


    private async void SystemUISettingColorValuesChanged(UISettings sender, object args)
    {
        //var accentColor = sender.GetColorValue(UIColorType.Accent);
        //OR
        //Color accentColor = (Color)Resources["SystemAccentColor"];

        var backgroundColor = sender.GetColorValue(UIColorType.Background);
        var isDarkMode = backgroundColor == Colors.Black;

        if (isDarkMode)
        {
            //Debug.WriteLine("ColorValuesChanged: Dark");
            _theme = ElementTheme.Dark;
        }
        else
        {
            //Debug.WriteLine("ColorValuesChanged: Light");
            _theme = ElementTheme.Light;
        }

        // May fire multiple times...so
        await Task.Delay(2000);

        ChangeRichEditTheme();
        ChangeSourceEditTheme();
        ChangePreviewBrowserTheme();
    }

}