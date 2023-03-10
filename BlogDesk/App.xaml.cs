using BlogDesk.Activation;
using BlogDesk.Contracts.Services;
using BlogDesk.Services;
using BlogDesk.ViewModels;
using BlogDesk.Views;
using BlogWrite.Core.Contracts.Services;
using BlogWrite.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

namespace BlogDesk;

public partial class App : Application
{
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    // DispatcherQueue
    private static readonly Microsoft.UI.Dispatching.DispatcherQueue _currentDispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
    public static Microsoft.UI.Dispatching.DispatcherQueue CurrentDispatcherQueue => _currentDispatcherQueue;

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers

            // Services
            //services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<IWebViewService, WebViewService>();
            services.AddSingleton<INavigationViewService, NavigationViewService>();
            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Core Services
            //services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<IAutoDiscoveryService, AutoDiscoveryService>();

            // Views and ViewModels
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<SettingsPage>();
            services.AddSingleton<WebViewViewModel>();
            services.AddSingleton<WebViewPage>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainPage>();
            services.AddSingleton<ShellPage>();
            services.AddSingleton<ShellViewModel>();
            services.AddSingleton<AccountAddPage>();
            services.AddSingleton<AccountAddViewModel>();
            //services.AddTransient<EditorViewModel>();
            //services.AddTransient<EditorPage>();

            // Configuration
            //services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.

        Debug.WriteLine("App_UnhandledException", e.Message + $"StackTrace: {e.Exception.StackTrace}, Source: {e.Exception.Source}");
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        // 
        var manager = WinUIEx.WindowManager.Get(MainWindow);
        manager.PersistenceId = "MainWindowPersistanceId";
        manager.Backdrop = new WinUIEx.MicaSystemBackdrop();

        await App.GetService<IActivationService>().ActivateAsync(args);
    }
}
