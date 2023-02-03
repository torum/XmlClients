using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BlogDesk.Contracts.Services;
using BlogDesk.Contracts.ViewModels;
using BlogDesk.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Navigation;

namespace BlogDesk.ViewModels;

public partial class MainViewModel : ObservableRecipient, INavigationAware
{
    public ICommand MenuFileNewEditorCommand
    {
        get;
    }

    private bool _isBackEnabled;
    public bool IsBackEnabled
    {
        get => _isBackEnabled;
        set => SetProperty(ref _isBackEnabled, value);
    }

    private readonly INavigationService _navigationService;

    public MainViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        _navigationService.Navigated += OnNavigated;


        MenuFileNewEditorCommand = new RelayCommand(OnMenuFileNewEditor);
    }

    private void OnNavigated(object sender, NavigationEventArgs e) => IsBackEnabled = _navigationService.CanGoBack;

    public void OnNavigatedTo(object parameter)
    {

    }

    public void OnNavigatedFrom()
    {

    }

    private void OnMenuFileNewEditor()
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
