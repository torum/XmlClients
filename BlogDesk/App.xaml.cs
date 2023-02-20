using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json.Nodes;
using AngleSharp.Dom;
using BlogDesk.Activation;
using BlogDesk.Contracts.Services;
using BlogWrite.Core.Contracts.Services;
using BlogWrite.Core.Services;
using BlogWrite.Core.Helpers;
using BlogDesk.Models;
using BlogDesk.Notifications;
using BlogDesk.Services;
using BlogDesk.ViewModels;
using BlogDesk.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.Windows.ApplicationModel.Resources;

namespace BlogDesk;

public partial class App : Application
{
    //
    private static readonly string _appName = "BlogDesk";//_resourceLoader.GetString("AppName");
    private static readonly string _appDeveloper = "torum";
    private static readonly string _envDataFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    public static string AppDataFolder { get; } = _envDataFolder + System.IO.Path.DirectorySeparatorChar + _appDeveloper + System.IO.Path.DirectorySeparatorChar + _appName;
    public static string AppConfigFilePath { get; } = Path.Combine(AppDataFolder, _appName + ".config");

    // 
    private static readonly Microsoft.UI.Dispatching.DispatcherQueue _currentDispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
    public static Microsoft.UI.Dispatching.DispatcherQueue CurrentDispatcherQueue => _currentDispatcherQueue;

    //
    private static readonly ResourceLoader _resourceLoader = new();

    //
    public static WindowEx MainWindow { get; } = new MainWindow();

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

    public App()
    {
        //CultureInfo.CurrentUICulture = new CultureInfo( "ja-JP", false );
        //CultureInfo.CurrentUICulture = new CultureInfo("en-US", false);

        // Force theme
        //this.RequestedTheme = ApplicationTheme.Dark;
        //this.RequestedTheme = ApplicationTheme.Light;

        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers
            //services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();

            // Services
            //services.AddSingleton<IAppNotificationService, AppNotificationService>();
            //services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<IWebViewService, WebViewService>();
            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();
            //services.AddTransient<INavigationViewService, NavigationViewService>();

            // Core Services
            //services.AddSingleton<IFileService, FileService>();

            //services.AddTransient<IFileDialogService, FileDialogService>();
            //services.AddSingleton<IDataAccessService, DataAccessService>();
            //services.AddSingleton<IFeedClientService, FeedClientService>();

            // Views and ViewModels
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<SettingsPage>();

            services.AddTransient<EditorViewModel>();
            services.AddTransient<EditorPage>();

            services.AddTransient<AccountAddViewModel>();
            services.AddTransient<AccountAddPage>();

            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainPage>();

            services.AddSingleton<ShellPage>();
            services.AddSingleton<ShellViewModel>();


            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        //App.GetService<IAppNotificationService>().Initialize();

        UnhandledException += App_UnhandledException;

    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.

        Debug.Write("App_UnhandledException: " + e.Exception.ToString());
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        // Single instance.
        // https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/migrate-to-windows-app-sdk/guides/applifecycle
        var mainInstance = Microsoft.Windows.AppLifecycle.AppInstance.FindOrRegisterForKey("BlogDeskMain");
        // If the instance that's executing the OnLaunched handler right now
        // isn't the "main" instance.
        if (!mainInstance.IsCurrent)
        {
            // Redirect the activation (and args) to the "main" instance, and exit.
            var activatedEventArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();
            await mainInstance.RedirectActivationToAsync(activatedEventArgs);

            System.Diagnostics.Process.GetCurrentProcess().Kill();
            return;
        }
        else
        {
            // Otherwise, register for activation redirection
            Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().Activated += App_Activated;
        }

        // WinUIEx Storage option.
        if (!RuntimeHelper.IsMSIX)
        {
            // Create if not exists.
            System.IO.Directory.CreateDirectory(AppDataFolder);

            WinUIEx.WindowManager.PersistenceStorage = new FilePersistence(Path.Combine(AppDataFolder, "WinUIExPersistence.json"));
        }

        // Nortification example.
        //App.GetService<IAppNotificationService>().Show(string.Format("AppNotificationSamplePayload".GetLocalized(), AppContext.BaseDirectory));

        await App.GetService<IActivationService>().ActivateAsync(args);
    }

    private void App_Activated(object? sender, Microsoft.Windows.AppLifecycle.AppActivationArguments e)
    {
        CurrentDispatcherQueue?.TryEnqueue(() =>
        {
            MainWindow.Activate();
            MainWindow.BringToFront();
        });
    }

    #region == WinUIEx ==

    private class FilePersistence : IDictionary<string, object>
    {
        private readonly Dictionary<string, object> _data = new();
        private readonly string _file;

        public FilePersistence(string filename)
        {
            _file = filename;
            try
            {
                if (File.Exists(filename))
                {
                    var jo = System.Text.Json.Nodes.JsonObject.Parse(File.ReadAllText(filename)) as JsonObject;
                    foreach (var node in jo)
                    {
                        if (node.Value is JsonValue jvalue && jvalue.TryGetValue<string>(out string value))
                            _data[node.Key] = value;
                    }
                }
            }
            catch { }
        }
        private void Save()
        {
            JsonObject jo = new JsonObject();
            foreach (var item in _data)
            {
                if (item.Value is string s) // In this case we only need string support. TODO: Support other types
                    jo.Add(item.Key, s);
            }
            File.WriteAllText(_file, jo.ToJsonString());
        }
        public object this[string key] { get => _data[key]; set { _data[key] = value; Save(); } }

        public ICollection<string> Keys => _data.Keys;

        public ICollection<object> Values => _data.Values;

        public int Count => _data.Count;

        public bool IsReadOnly => false;

        public void Add(string key, object value)
        {
            _data.Add(key, value); Save();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            _data.Add(item.Key, item.Value); Save();
        }

        public void Clear()
        {
            _data.Clear(); Save();
        }

        public bool Contains(KeyValuePair<string, object> item) => _data.Contains(item);

        public bool ContainsKey(string key) => _data.ContainsKey(key);

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => throw new NotImplementedException(); 

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => throw new NotImplementedException(); 

        public bool Remove(string key) => throw new NotImplementedException(); 

        public bool Remove(KeyValuePair<string, object> item) => throw new NotImplementedException(); 

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value) => throw new NotImplementedException(); 

        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException(); 
    }

    #endregion
}
