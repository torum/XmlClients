using BlogDesk.Contracts.Services;
using BlogDesk.Services;
using BlogDesk.ViewModels;
using BlogWrite.Core.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;

namespace BlogDesk.Views;

public sealed partial class EditorPage : Page
{
    public EditorViewModel ViewModel
    {
        get;
    }

    public EditorWindow EditorWindow
    {
        get;
    }

    public EditorPage(EditorWindow window)
    {
        EditorWindow = window;

        EditorWindow.ExtendsContentIntoTitleBar = true;
        EditorWindow.Activated += EditorWindow_Activated;
        EditorWindow.Closed += EditorWindow_Closed;

        ViewModel = new EditorViewModel(new WebViewService(), new WebViewService(), new WebViewService());//App.GetService<EditorViewModel>();

        try
        {
            InitializeComponent();

            // AppTitleBar needs InitializeComponent() beforehand.
            EditorWindow.SetTitleBar(AppTitleBar);
        }
        catch (XamlParseException parseException)
        {
            Debug.WriteLine($"Unhandled XamlParseException in MainPage: {parseException.Message}");
            foreach (var key in parseException.Data.Keys)
            {
                Debug.WriteLine("{Key}:{Value}", key.ToString(), parseException.Data[key]?.ToString());
            }
            throw;
        }
        // Init webview2
        ViewModel.WebViewServiceRichEdit.Initialize(WebViewRichEdit);
        ViewModel.WebViewServiceSourceEdit.Initialize(WebViewSourceEdit);
        ViewModel.WebViewServicePreviewBrowser.Initialize(WebViewPreviewBrowser);

        // Focus control
        ViewModel.WebView2RichEditSetFocus += (sender, arg) => { OnWebView2RichEditSetFocus(); };
        ViewModel.WebView2SourceEditSetFocus += (sender, arg) => { OnWebView2SourceEditSetFocus(); };
        ViewModel.WebView2PreviewBrowserSetFocus += (sender, arg) => { OnWebView2PreviewBrowserSetFocus(); };

        // Theme change event from message received in the viewmodel.
        ViewModel.ThemeChanged += (sender, arg) => { OnThemeChanged(arg); };

        // This is for the case where theme is changed in settings page.
        if (App.MainWindow.Content is FrameworkElement rootElement)
        {
            if (RequestedTheme != rootElement.RequestedTheme)
            {
                OnThemeChanged(rootElement.RequestedTheme);

                ViewModel.SetTheme(rootElement.RequestedTheme);
            }
        }
    }

    private void EditorWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";

        AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];
        AppTitleBarIcon.Opacity = args.WindowActivationState == WindowActivationState.Deactivated ? 0.4 : 0.7;
        AppMenuBar.Opacity = args.WindowActivationState == WindowActivationState.Deactivated ? 0.4 : 0.7;
    }

    private void EditorWindow_Closed(object sender, WindowEventArgs args)
    {
        // TODO:

        if (ViewModel.Closing())
        {
            WebViewRichEdit.Close();
            WebViewSourceEdit.Close();
            WebViewPreviewBrowser.Close();
        }
        else
        {
            // Cancel
            //args.Handled = true;
        }
    }

    private void OnThemeChanged(ElementTheme arg)
    {
        RequestedTheme = arg;

        // not good...
        //TitleBarHelper.UpdateTitleBar(RequestedTheme, EditorWindow);
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        // Required.
        await Task.Delay(100);

        if (WebViewRichEdit.Visibility == Visibility.Visible)
        {
            WebViewRichEdit.Focus(FocusState.Programmatic);
        }
    }

    private async void OnWebView2RichEditSetFocus()
    {
        if (WebViewRichEdit.Visibility == Visibility.Visible)
        {
            // Required.
            await Task.Delay(100);

            WebViewRichEdit.Focus(FocusState.Programmatic);
        }
    }

    private async void OnWebView2SourceEditSetFocus()
    {
        if (WebViewSourceEdit.Visibility == Visibility.Visible)
        {
            // Required.
            await Task.Delay(100);

            WebViewSourceEdit.Focus(FocusState.Programmatic);
        }
    }

    private async void OnWebView2PreviewBrowserSetFocus()
    {
        if (WebViewPreviewBrowser.Visibility == Visibility.Visible)
        {
            // Required.
            await Task.Delay(100);

            WebViewPreviewBrowser.Focus(FocusState.Programmatic);
        }
    }

}
