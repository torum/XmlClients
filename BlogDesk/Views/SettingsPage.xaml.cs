using BlogDesk.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace BlogDesk.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel
    {
        get;
    }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
    }
}
