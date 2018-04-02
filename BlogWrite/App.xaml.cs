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

namespace BlogWrite
{
    /// <summary>
    /// App
    /// </summary>
    public partial class App : Application
    {
        /// <summary>The event mutex name.</summary>
        private const string UniqueEventName = "{b3e4fab6-0d7d-4e32-8cec-1fa2e99841d8}";

        /// <summary>The unique mutex name.</summary>
        private const string UniqueMutexName = "{546de69d-61fd-4de4-b1f9-5f06140ef8f2}";

        /// <summary>The event wait handle.</summary>
        private EventWaitHandle eventWaitHandle;

        /// <summary>The mutex.</summary>
        private Mutex mutex;

        /// <summary> Check and bring to front if already exists.</summary>
        private void AppOnStartup(object sender, StartupEventArgs e)
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

            string id = arg.Entry.ID;

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

                if (id == ((w as EditorWindow).DataContext as EditorViewModel).ID)
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

        public void LaunchServiceDiscoveryWindow(Window owner)
        {
            // TODO: Before opening the window, make sure no other window is open.
            // If a user minimize and restore, Modal window can get behind of the child window.

            var win = new ServiceDiscoveryWindow();
            win.DataContext = new ServiceDiscoveryViewModel();
            win.Owner = owner;
            win.ResizeMode = ResizeMode.NoResize;
            win.ShowDialog();
        }

    }
}
