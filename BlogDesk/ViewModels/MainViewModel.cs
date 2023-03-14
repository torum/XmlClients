using BlogDesk.Views;
using BlogWrite.Core.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Navigation;
using BlogDesk.Contracts.Services;
using BlogDesk.Helpers;
using BlogDesk.Services;
using Microsoft.UI.Xaml;

namespace BlogDesk.ViewModels;

public class MainViewModel : ObservableRecipient
{

    private ObservableCollection<EntryItem> _entries = new();
    public ObservableCollection<EntryItem> Entries
    {
        get => _entries;
        set => SetProperty(ref _entries, value);
    }

    private FeedEntryItem? _selectedListViewItem = null;
    public FeedEntryItem? SelectedListViewItem
    {
        get => _selectedListViewItem;
        set => SetProperty(ref _selectedListViewItem, value);
    }

    public ICommand NewEditorCommand
    {
        get;
    }
    public ICommand AddAccountCommand
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
    private readonly IAbstractFactory<EditorPage> _editorFactory;

    public MainViewModel(INavigationService navigationService, IAbstractFactory<EditorPage> editorFactory)
    {
        _navigationService = navigationService;
        _navigationService.Navigated += OnNavigated;

        _editorFactory = editorFactory;

        NewEditorCommand = new RelayCommand(OnNewEditor);
        AddAccountCommand = new RelayCommand(OnAddAccount);
    }

    private void OnNavigated(object sender, NavigationEventArgs e) => IsBackEnabled = _navigationService.CanGoBack;

    public void OnNavigatedTo(object parameter)
    {

    }

    public void OnNavigatedFrom()
    {

    }

    private void OnNewEditor()
    {
        CreateNewEditor();
    }

    public void CreateNewEditor()
    {
        var editor = _editorFactory.Create();

        var editorEindow = editor.Window;

        App.MainWindow.Closed += (s, a) =>
        {
            // TODO: when close is canceled.
            //editorEindow.CanClose
            editorEindow.Close();
        };

        editorEindow.Show();

        /*
        var window = new EditorWindow();
        var viewModel = App.GetService<EditorViewModel>();
        //var viewModel = new EditorViewModel(new WebViewService(), new WebViewService(), new WebViewService());

        //var editor = new EditorPage(window, viewModel);
        var editor = _editorFactory.Create();

        window.Content = editor;

        App.MainWindow.Closed += (s, a) =>
        {
            // TODO: when close is canceled.
            //window.CanClose
            window.Close();
        };

        await Task.Delay(200);

        window.Show();
        //window.Activate();
        */
    }

    private void OnAddAccount()
    {
        AddAccount();
    }

    public void AddAccount()
    {
        _navigationService.NavigateTo(typeof(AccountAddViewModel).FullName!);
    }
}
