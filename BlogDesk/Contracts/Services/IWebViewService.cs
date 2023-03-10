using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace BlogDesk.Contracts.Services;

public interface IWebViewService
{
    Uri? Source
    {
        get;
    }
    
    // Added
    public WebView2? WebView
    {
        get;
    }

    // Added
    public CoreWebView2? CoreWebView2
    {
        get;
    }

    bool CanGoBack
    {
        get;
    }

    bool CanGoForward
    {
        get;
    }

    event EventHandler<CoreWebView2WebErrorStatus>? NavigationCompleted;
    
    // Added
    event EventHandler<CoreWebView2InitializedEventArgs>? CoreWebView2Initialized;

    void Initialize(WebView2 webView);
    
    // Added
    void NavigateToString(string str);

    void GoBack();

    void GoForward();

    void Reload();

    void UnregisterEvents();
}
