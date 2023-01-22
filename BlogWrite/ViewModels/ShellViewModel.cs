using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Xml;
using BlogWrite.Contracts.Services;
using BlogWrite.Models;
using BlogWrite.Services;
using BlogWrite.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Navigation;

namespace BlogWrite.ViewModels;

public class ShellViewModel : ObservableRecipient
{
    public ICommand MenuFileExitCommand
    {
        get;
    }

    public ICommand MenuSettingsCommand
    {
        get;
    }


    public ICommand MenuViewFeedsCommand
    {
        get;
    }

    public INavigationService NavigationService
    {
        get;
    }

    /*
    public INavigationViewService NavigationViewService
    {
        get;
    }
    */
    /*
    private object? _selected;
    public object? Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }
    */

    private bool _isBackEnabled;
    public bool IsBackEnabled
    {
        get => _isBackEnabled;
        set => SetProperty(ref _isBackEnabled, value);
    }

    public ShellViewModel(INavigationService navigationService) //, INavigationViewService navigationViewService
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        //NavigationViewService = navigationViewService;

        MenuFileExitCommand = new RelayCommand(OnMenuFileExit);
        MenuSettingsCommand = new RelayCommand(OnMenuSettings);
        MenuViewFeedsCommand = new RelayCommand(OnMenuViewFeeds);

    }

    private void OnNavigated(object sender, NavigationEventArgs e) //=> IsBackEnabled = NavigationService.CanGoBack;
    {
        IsBackEnabled = NavigationService.CanGoBack;

        /*
        if (e.SourcePageType == typeof(SettingsPage))
        {
            Selected = NavigationViewService.SettingsItem;
            return;
        }

        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
        */
    }

    private void OnMenuFileExit() => Application.Current.Exit();

    private void OnMenuSettings() => NavigationService.NavigateTo(typeof(SettingsViewModel).FullName!);


    private void OnMenuViewFeeds() => NavigationService.NavigateTo(typeof(FeedsViewModel).FullName!);

}
