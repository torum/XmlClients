using BlogDesk.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace BlogDesk.Views;

public sealed partial class AccountAddPage : Page
{
    public AccountAddViewModel ViewModel
    {
        get;
    }

    public AccountAddPage()
    {
        ViewModel = App.GetService<AccountAddViewModel>();
        InitializeComponent();
    }
}
