using BlogWrite.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace BlogWrite.Views;

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
