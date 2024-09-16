using BlogDesk.Views;
using XmlClients.Core.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Navigation;
using BlogDesk.Contracts.Services;
using BlogDesk.Helpers;
using BlogDesk.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace BlogDesk.ViewModels;

public class MainViewModel : ObservableRecipient
{

    private ObservableCollection<EntryItem> _entries = new();
    public ObservableCollection<EntryItem> Entries
    {
        get => _entries;
        set => SetProperty(ref _entries, value);
    }

    private FeedEntryItem? _selectedListViewItem = null;
    public FeedEntryItem? SelectedListViewItem
    {
        get => _selectedListViewItem;
        set => SetProperty(ref _selectedListViewItem, value);
    }

    public ICommand NewEditorCommand
    {
        get;
    }

    public ICommand AddAccountCommand
    {
        get;
    }

    private bool _isBackEnabled;
    public bool IsBackEnabled
    {
        get => _isBackEnabled;
        set => SetProperty(ref _isBackEnabled, value);
    }

    private readonly INavigationService _navigationService;
    private readonly IAbstractFactory<EditorPage> _editorFactory;

    internal readonly IWebViewService WebViewServiceEditor;

    public MainViewModel(INavigationService navigationService, IWebViewService webViewServiceEditor, IAbstractFactory<EditorPage> editorFactory)
    {
        _navigationService = navigationService;
        _navigationService.Navigated += OnNavigated;

        WebViewServiceEditor = webViewServiceEditor;
        WebViewServiceEditor.CoreWebView2Initialized += OnEditorCoreWebView2Initialized;

        _editorFactory = editorFactory;

        NewEditorCommand = new RelayCommand(OnNewEditor);
        AddAccountCommand = new RelayCommand(OnAddAccount);

    }

    private void OnNavigated(object sender, NavigationEventArgs e) => IsBackEnabled = _navigationService.CanGoBack;

    public void OnNavigatedTo(object parameter)
    {

    }

    public void OnNavigatedFrom()
    {

    }



    private static readonly string _indexHtml = @"
<!DOCTYPE html> 
<html> 
<head> 
    <meta http-equiv=""Content-Type"" content=""text/html;charset=utf-8"" /> 
    <link rel=""stylesheet"" 
          data-name=""vs/editor/editor.main"" 
          href=""https://BlogDesk/Web/MonacoEditor/min/vs/editor/editor.main.css"" /> 
    <style> 
        html, body { height: 100%; width: 100%; margin: 0;} 
        #container { height: 100%; width: 100%;}
    </style> 
</head> 
<body> 
    <div id=""container""></div> 
    <script src=""https://BlogDesk/Web/MonacoEditor/min/vs/loader.js""></script> 
    <script> 
        require.config({ paths: { 'vs': 'https://BlogDesk/Web/MonacoEditor/min/vs' } }); 
    </script> 
    <script src=""https://BlogDesk/Web/MonacoEditor/min/vs/editor/editor.main.nls.js""></script> 
    <script src=""https://BlogDesk/Web/MonacoEditor/min/vs/editor/editor.main.js""></script> 
    <script> 
        var editor = monaco.editor.create(document.getElementById('container'), { 
            value: '<p></p>\n',
            lineNumbers: ""on"",
            readOnly: false,
            roundedSelection: false,
            scrollBeyondLastLine: false,
            theme: ""vs-dark"",
            automaticLayout: true,
            language: 'html' 
        }); 
    </script> 
</body> 
</html>";

    private void OnEditorCoreWebView2Initialized(object? sender, CoreWebView2InitializedEventArgs arg)
    {
        if (WebViewServiceEditor.CoreWebView2 is null)
        {
            return;
        }

        //await WebViewServiceEditor.CoreWebView2.
        //WebViewServiceEditor.CoreWebView2.Navigate(Path.Combine("file:", Directory.GetCurrentDirectory(), "bin", "monaco", "index.html"));
        try
        {
            //WebViewService.CoreWebView2?.AddHostObjectToScript("model", _jsModel);

            WebViewServiceEditor.CoreWebView2.SetVirtualHostNameToFolderMapping(hostName: "BlogDesk", folderPath: "", accessKind: CoreWebView2HostResourceAccessKind.Allow);

            //WebViewServiceEditor.CoreWebView2.DOMContentLoaded += OnRichEditCoreWebView2DOMContentLoaded;
            //WebViewServiceEditor.CoreWebView2.WebMessageReceived += OnRichEditWebMessageReceived;
            //// Not working because not supported.
            //WebViewServiceEditor.CoreWebView2.PermissionRequested += OnRichEditCoreWebView2OnPermissionRequested;

            WebViewServiceEditor.NavigateToString(_indexHtml);
            //WebViewServiceEditor.CoreWebView2.Navigate("https://BlogDesk/Web/MonacoEditor/index.html");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("OnRichEditCoreWebView2Initialized: " + ex);

            throw;
        }

    }

    private void OnNewEditor()
    {
        CreateNewEditor();
    }

    public void CreateNewEditor()
    {
        var editor = _editorFactory.Create();

        var editorEindow = editor.Window;

        App.MainWindow.Closed += (s, a) =>
        {
            // TODO: when close is canceled.
            //editorEindow.CanClose
            editorEindow.Close();
        };

        editorEindow.Show();

        /*
        var window = new EditorWindow();
        var viewModel = App.GetService<EditorViewModel>();
        //var viewModel = new EditorViewModel(new WebViewService(), new WebViewService(), new WebViewService());

        //var editor = new EditorPage(window, viewModel);
        var editor = _editorFactory.Create();

        window.Content = editor;

        App.MainWindow.Closed += (s, a) =>
        {
            // TODO: when close is canceled.
            //window.CanClose
            window.Close();
        };

        await Task.Delay(200);

        window.Show();
        //window.Activate();
        */
    }

    private void OnAddAccount()
    {
        AddAccount();
    }

    public void AddAccount()
    {
        _navigationService.NavigateTo(typeof(AccountAddViewModel).FullName!);
    }
}
