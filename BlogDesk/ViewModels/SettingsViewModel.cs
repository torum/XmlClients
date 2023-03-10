using System.Reflection;
using System.Windows.Input;
using BlogDesk.Contracts.Services;
using BlogDesk.Models;
using BlogWrite.Core.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel;

namespace BlogDesk.ViewModels;

public class SettingsViewModel : ObservableRecipient
{
    private readonly INavigationService _navigationService;
    private readonly IThemeSelectorService _themeSelectorService;
    private ElementTheme _elementTheme = ElementTheme.Default;
    private string _versionDescription;

    public ElementTheme ElementTheme
    {
        get => _elementTheme;
        set => SetProperty(ref _elementTheme, value);
    }

    public string VersionDescription
    {
        get => _versionDescription;
        set => SetProperty(ref _versionDescription, value);
    }

    public ICommand SwitchThemeCommand
    {
        get;
    }

    public ICommand GoBackCommand
    {
        get;
    }

    public SettingsViewModel(INavigationService navigationService, IThemeSelectorService themeSelectorService)
    {
        _navigationService = navigationService;
        _themeSelectorService = themeSelectorService;
        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();

        GoBackCommand = new RelayCommand(OnGoBack);
        /*
        SwitchThemeCommand = new RelayCommand<ElementTheme>(
            async (param) =>
            {
                if (ElementTheme != param)
                {
                    ElementTheme = param;
                    await _themeSelectorService.SetThemeAsync(param);
                }
            });
        */
        SwitchThemeCommand = new RelayCommand<ElementTheme>(
            async (param) =>
            {
                if (ElementTheme != param)
                {
                    ElementTheme = param;
                    await _themeSelectorService.SetThemeAsync(param);

                    var thm = ElementTheme.ToString().ToLower();
                    // send message to other windows (Editor windows)
                    WeakReferenceMessenger.Default.Send(new ThemeChangedMessage(thm));
                }
            });
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    private void OnGoBack()
    {
        if (_navigationService.CanGoBack)
        {
            _navigationService.GoBack();
        }
    }
}
