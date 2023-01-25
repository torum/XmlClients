using FeedDesk.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace FeedDesk.Views;

public sealed partial class FolderAddPage : Page
{
    public FolderAddViewModel ViewModel
    {
        get;
    }

    public FolderAddPage()
    {
        ViewModel = App.GetService<FolderAddViewModel>();
        this.InitializeComponent();
    }
}
