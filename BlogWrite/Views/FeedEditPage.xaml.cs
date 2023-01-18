using BlogWrite.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace BlogWrite.Views;

public sealed partial class FeedEditPage : Page
{
    public FeedEditViewModel ViewModel
    {
        get;
    }

    public FeedEditPage()
    {
        ViewModel = App.GetService<FeedEditViewModel>();
        this.InitializeComponent();
    }
}
