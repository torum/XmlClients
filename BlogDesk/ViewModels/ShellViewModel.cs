using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Xml;
using BlogDesk.Contracts.Services;
using BlogDesk.Models;
using BlogDesk.Services;
using BlogDesk.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Navigation;
using Windows.UI.WindowManagement;

namespace BlogDesk.ViewModels;

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

    public ICommand MenuFileNewCommand
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
        MenuFileNewCommand = new RelayCommand(OnMenuFileNew);

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


    private void OnMenuFileNew()
    {
        EditorWindow window = new();
        /*
        this.Closed += (s, a) =>
        {
            window.Close();
        };
        */
        var editor = new EditorPage(window);
        window.Content = editor;
        window.Activate();
    }

}
