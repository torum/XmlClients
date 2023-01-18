using BlogWrite.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace BlogWrite.Views;

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
