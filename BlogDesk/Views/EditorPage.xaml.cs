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
    public EditorWindow Window
    {
        get;
    }

    public EditorViewModel ViewModel
    {
        get;
    }

    public EditorPage()//EditorWindow window, EditorViewModel viewModel
    {
        Window = new EditorWindow();

        ViewModel = App.GetService<EditorViewModel>();
        //ViewModel = App.GetService<EditorViewModel>();
        //ViewModel = new EditorViewModel(new WebViewService(), new WebViewService(), new WebViewService());

        Window.ExtendsContentIntoTitleBar = true;

        Window.Content = this;

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

        // AppTitleBar needs InitializeComponent() beforehand.
        Window.SetTitleBar(AppTitleBar);

        Window.Activated += EditorWindow_Activated;
        Window.Closed += EditorWindow_Closed;

        // Init webview2
        //ViewModel.WebViewServiceRichEdit.Initialize(WebViewRichEdit);
        //ViewModel.WebViewServiceSourceEdit.Initialize(WebViewSourceEdit);
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

    public void EditorWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";

        AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];
        AppTitleBarIcon.Opacity = args.WindowActivationState == WindowActivationState.Deactivated ? 0.4 : 0.7;
        AppMenuBar.Opacity = args.WindowActivationState == WindowActivationState.Deactivated ? 0.4 : 0.7;
    }

    public void EditorWindow_Closed(object sender, WindowEventArgs args)
    {
        Window.BringToFront();

         // TODO:
        if (ViewModel.Closing())
        {
            // https://github.com/microsoft/microsoft-ui-xaml/issues/7336
            // already done this in viewmodel.
            //WebViewRichEdit.Close();
            //WebViewSourceEdit.Close();
            //WebViewPreviewBrowser.Close();
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
        /*
        if (WebViewRichEdit.Visibility == Visibility.Visible)
        {
            Debug.WriteLine("WebViewRichEdit.Focus on Page_Loaded");
            WebViewRichEdit.Focus(FocusState.Programmatic);
        }
        */
    }

    private async void OnWebView2RichEditSetFocus()
    {
        /*
        if (WebViewRichEdit.Visibility == Visibility.Visible)
        {
            // Required.
            await Task.Delay(100);

            Debug.WriteLine("OnWebView2RichEditSetFocus");
            WebViewRichEdit.Focus(FocusState.Programmatic);
        }
        */
    }

    private async void OnWebView2SourceEditSetFocus()
    {
        /*
        if (WebViewSourceEdit.Visibility == Visibility.Visible)
        {
            // Required.
            await Task.Delay(100);

            Debug.WriteLine("OnWebView2SourceEditSetFocus");
            WebViewSourceEdit.Focus(FocusState.Programmatic);
        }
        */
    }

    private async void OnWebView2PreviewBrowserSetFocus()
    {
        if (WebViewPreviewBrowser.Visibility == Visibility.Visible)
        {
            // Required.
            await Task.Delay(100);

            Debug.WriteLine("OnWebView2PreviewBrowserSetFocus");
            WebViewPreviewBrowser.Focus(FocusState.Programmatic);
        }
    }

}
