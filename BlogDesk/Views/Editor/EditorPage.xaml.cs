using BlogDesk.ViewModels;
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

        ViewModel = App.GetService<EditorViewModel>();

        try
        {
            InitializeComponent();
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

        ViewModel.WebViewServiceRichEdit.Initialize(WebViewRichEdit);
        ViewModel.WebViewServiceSourceEdit.Initialize(WebViewSourceEdit);
        ViewModel.WebViewServicePreviewBrowser.Initialize(WebViewPreviewBrowser);

        ViewModel.WebView2RichEditSetFocus += (sender, arg) => { this.OnWebView2RichEditSetFocus(arg); };
        ViewModel.WebView2SourceEditSetFocus += (sender, arg) => { this.OnWebView2SourceEditSetFocus(arg); };
        ViewModel.WebView2PreviewBrowserSetFocus += (sender, arg) => { this.OnWebView2PreviewBrowserSetFocus(arg); };

        ViewModel.ThemeChanged += (sender, arg) => { this.OnThemeChanged(arg); };

        //ViewModel.WindowClosing += (sender, arg) => { this.OnWindowClosing(arg); };

        EditorWindow.ExtendsContentIntoTitleBar = true;
        EditorWindow.SetTitleBar(AppTitleBar);
        EditorWindow.Activated += EditorWindow_Activated;
        EditorWindow.Closed += EditorWindow_Closed;
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
        //Debug.WriteLine("EditorWindow_Closed");

        WebViewRichEdit.Close();
        WebViewSourceEdit.Close();
        WebViewPreviewBrowser.Close();
    }

    private void OnThemeChanged(ElementTheme arg)
    {
        this.RequestedTheme = arg;
    }
    

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        // Required.
        await Task.Delay(100);

        WebViewRichEdit.Focus(FocusState.Programmatic);
    }

    private async void OnWebView2RichEditSetFocus(string arg)
    {
        if (WebViewRichEdit.Visibility == Visibility.Visible)
        {
            // Required.
            await Task.Delay(100);

            WebViewRichEdit.Focus(FocusState.Programmatic);
            //Debug.WriteLine("OnWebView2EditorSetFocus");
        }
    }

    private async void OnWebView2SourceEditSetFocus(string arg)
    {
        if (WebViewSourceEdit.Visibility == Visibility.Visible)
        {
            // Required.
            await Task.Delay(100);

            WebViewSourceEdit.Focus(FocusState.Programmatic);
            //Debug.WriteLine("OnWebView2EditorSetFocus");
        }
    }

    private async void OnWebView2PreviewBrowserSetFocus(string arg)
    {
        if (WebViewPreviewBrowser.Visibility == Visibility.Visible)
        {
            // Required.
            await Task.Delay(100);

            WebViewPreviewBrowser.Focus(FocusState.Programmatic);
            //Debug.WriteLine("OnWebView2EditorSetFocus");
        }
    }

}
