using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using BlogWrite.Views;
using BlogWrite.ViewModels;
using System.Globalization;
using Microsoft.Web.WebView2.Core;
using System.Diagnostics;
using System.Text;
using System.IO;

namespace BlogWrite
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {   
        private bool _mutexOn = true;

        /// <summary>The event mutex name.</summary>
        private const string UniqueEventName = "{be34355c-f95c-44e7-b6a5-7f8f324c9e17}";

        /// <summary>The unique mutex name.</summary>
        private const string UniqueMutexName = "{2cc0287f-c110-4a4c-9759-959dde34a154}";

        /// <summary>The event wait handle.</summary>
        private EventWaitHandle eventWaitHandle;

        /// <summary>The mutex.</summary>
        private Mutex mutex;

        /// <summary> Check and bring to front if already exists.</summary>
        private void AppOnStartup(object sender, StartupEventArgs e)
        {
            if (!IsWebViewVersionInstalled())
            {
                // show dialog.
                var dialog = new WebView2RuntimeInstall()
                {
                    Owner = null,
                    Width = 800,
                    Height = 400,
                    Title = "Microsoft Edge WebView2 Runtime Installation",
                };

                dialog.ShowDialog();

                this.Shutdown();
            }

            // テスト用
            //ChangeTheme("DefaultTheme");
            //ChangeTheme("LightTheme");

            // For testing only. Don't forget to comment this out if you uncomment.
            //BlogWrite.Properties.Resources.Culture = CultureInfo.GetCultureInfo("en-US"); //or ja-JP etc

            if (_mutexOn)
            {
                this.mutex = new Mutex(true, UniqueMutexName, out bool isOwned);
                this.eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);

                // So, R# would not give a warning that this variable is not used.
                GC.KeepAlive(this.mutex);

                if (isOwned)
                {
                    // Spawn a thread which will be waiting for our event
                    var thread = new Thread(
                        () =>
                        {
                            while (this.eventWaitHandle.WaitOne())
                            {
                                Current.Dispatcher.BeginInvoke(
                                    (Action)(() => ((MainWindow)Current.MainWindow).BringToForeground()));
                            }
                        });

                    // It is important mark it as background otherwise it will prevent app from exiting.
                    thread.IsBackground = true;

                    thread.Start();
                    return;
                }

                // Notify other instance so it could bring itself to foreground.
                this.eventWaitHandle.Set();

                // Terminate this instance.
                this.Shutdown();
            }
        }

        /// <summary> Hold a list of windows here.</summary>
        public List<Window> WindowList = new List<Window>();

        /// <summary> Create an Editor Window.</summary>
        public void CreateNewEditorWindow(BlogEntryEventArgs arg)
        {
            if (arg.Entry == null)
                return;

            var win = new EditorWindow();
            win.DataContext = new EditorViewModel(arg.Entry);

            App app = App.Current as App;
            app.WindowList.Add(win);

            // We can't use Show() or set win.Owner = this. 
            // Try minimized and resotre a child window then close it. An owner window minimizes itself.
            //win.Owner = this;
            //win.Show();
            win.ShowInTaskbar = true;
            win.ShowActivated = true;
            win.Visibility = Visibility.Visible;
            win.Activate();
        }

        /// <summary> Create or BringToFront an Editor Window.</summary>
        public void CreateOrBringToFrontEditorWindow(BlogEntryEventArgs arg)
        {
            if (arg.Entry == null)
                return;

            string id = arg.Entry.Id;

            App app = App.Current as App;

            // The key is to use app.WindowList here.
            //foreach (var w in app.Windows)
            foreach (var w in app.WindowList)
            {
                if (!(w is EditorWindow))
                    continue;

                if ((w as EditorWindow).DataContext == null)
                    continue;

                if (!((w as EditorWindow).DataContext is EditorViewModel))
                    continue;

                if (id == ((w as EditorWindow).DataContext as EditorViewModel).Id)
                {
                    //w.Activate();

                    if ((w as EditorWindow).WindowState == WindowState.Minimized || (w as Window).Visibility == Visibility.Hidden)
                    {
                        //w.Show();
                        (w as EditorWindow).Visibility = Visibility.Visible;
                        (w as EditorWindow).WindowState = WindowState.Normal;
                    }

                    (w as EditorWindow).Activate();
                    //(w as EditorWindow).Topmost = true;
                    //(w as EditorWindow).Topmost = false;
                    (w as EditorWindow).Focus();

                    return;
                }
            }

            var win = new EditorWindow
            {
                DataContext = new EditorViewModel(arg.Entry)
            };

            app.WindowList.Add(win);

            // We can't use Show() or set win.Owner = this. 
            // Try minimized and resotre a child window then close it. An owner window minimizes itself.
            //win.Owner = this;
            //win.Show();
            win.ShowInTaskbar = true;
            win.ShowActivated = true;
            win.Visibility = Visibility.Visible;
            win.Activate();
        }

        /// <summary> Remove an Editor Window from WindowList.</summary>
        public void RemoveEditorWindow(EditorWindow editor)
        {
            WindowList.Remove(editor);
        }

        // Theme change
        public void ChangeTheme(string themeName)
        {
            System.Diagnostics.Debug.WriteLine(themeName);

            ResourceDictionary _themeDict = Application.Current.Resources.MergedDictionaries.FirstOrDefault(x => x.Source == new Uri("pack://application:,,,/Themes/DefaultTheme.xaml"));
            if (_themeDict != null)
            {
                _themeDict.Clear();
            }
            else
            {
                // 新しいリソース・ディクショナリを追加
                _themeDict = new ResourceDictionary();
                Application.Current.Resources.MergedDictionaries.Add(_themeDict);
            }

            // テーマをリソース・ディクショナリのソースに指定
            string themeUri = String.Format("pack://application:,,,/Themes/{0}.xaml", themeName);
            _themeDict.Source = new Uri(themeUri);

        }

        // WebView2 runtime check
        private bool IsWebViewVersionInstalled()
        {
            try
            {
                string verNo = CoreWebView2Environment.GetAvailableBrowserVersionString();

                //Debug.WriteLine("versionNo:" + verNo);//versionNo: 91.0.864.41

                Version ver = new Version(verNo); //

                Version asmVerNo = typeof(CoreWebView2Environment).Assembly.GetName().Version;

                if (ver.Build >= asmVerNo.Build)
                    return true;

            }
            catch { }

            return false;
        }

        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private StringBuilder Errortxt = new StringBuilder();
        public bool IsSaveErrorLog;
        public string LogFilePath;

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var exception = e.Exception as Exception;

            System.Diagnostics.Debug.WriteLine("App_DispatcherUnhandledException: " + exception.Message);

            AppendErrorLog("App_DispatcherUnhandledException", exception.Message);

            SaveErrorLog();

            e.Handled = false;
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var exception = e.Exception.InnerException as Exception;

            System.Diagnostics.Debug.WriteLine("TaskScheduler_UnobservedTaskException: " + exception.Message);

            AppendErrorLog("TaskScheduler_UnobservedTaskException", exception.Message);
            // save
            SaveErrorLog();

            e.SetObserved();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;

            if (exception is TaskCanceledException)
            {
                // can ignore.
                System.Diagnostics.Debug.WriteLine("CurrentDomain_UnhandledException (TaskCanceledException): " + exception.Message);

                AppendErrorLog("CurrentDomain_UnhandledException (TaskCanceledException)", exception.Message);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("CurrentDomain_UnhandledException: " + exception.Message);

                AppendErrorLog("CurrentDomain_UnhandledException", exception.Message);

                // save
                SaveErrorLog();
            }

            // TODO:
            //Environment.Exit(1);
        }

        public void AppendErrorLog(string errorTxt, string kindTxt)
        {
            DateTime dt = DateTime.Now;
            string nowString = dt.ToString("yyyy/MM/dd HH:mm:ss");

            Errortxt.AppendLine(nowString + " - " + kindTxt + " - " + errorTxt);
        }

        public void SaveErrorLog()
        {
            if (!IsSaveErrorLog)
                return;

            if (string.IsNullOrEmpty(LogFilePath))
                return;

            string s = Errortxt.ToString();
            if (!string.IsNullOrEmpty(s))
                File.WriteAllText(LogFilePath, s);
        }

    }
}
