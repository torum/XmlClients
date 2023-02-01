using BlogWrite.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

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


        ViewModel.WebView2RichEditSetFocus += (sender, arg) => { this.OnWebView2RichEditSetFocus(arg); };
        ViewModel.WebView2SourceEditSetFocus += (sender, arg) => { this.OnWebView2SourceEditSetFocus(arg); };
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
}
