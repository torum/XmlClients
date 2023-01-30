using BlogWrite.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
        InitializeComponent();

        ViewModel.WebViewService.Initialize(WebViewEditor);
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Delay(100);
        WebViewEditor.Focus(FocusState.Programmatic);
    }
}
