using FeedDesk.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace FeedDesk.Views;

public sealed partial class FolderEditPage : Page
{
    public FolderEditViewModel ViewModel
    {
        get;
    }

    public FolderEditPage()
    {
        ViewModel = App.GetService<FolderEditViewModel>();
        this.InitializeComponent();
    }
}
