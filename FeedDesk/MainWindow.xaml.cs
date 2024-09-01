using BlogWrite.Core.Helpers;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Windows.Storage;

namespace FeedDesk;

public sealed partial class MainWindow : WindowEx
{
    public MainWindow()
    {
        InitializeComponent();

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "FeedDesk3.ico"));
        Content = null;
        Title = "AppDisplayName".GetLocalized();



        // SystemBackdrop
        if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
        {
            //manager.Backdrop = new WinUIEx.AcrylicSystemBackdrop();
            if (RuntimeHelper.IsMSIX)
            {
                // Load preference from localsetting.
                if (ApplicationData.Current.LocalSettings.Values.TryGetValue(App.BackdropSettingsKey, out var obj))
                {
                    var s = (string)obj;
                    if (s == SystemBackdropOption.Acrylic.ToString())
                    {
                        SystemBackdrop = new DesktopAcrylicBackdrop();
                    }
                    else if (s == SystemBackdropOption.Mica.ToString())
                    {
                        SystemBackdrop = new MicaBackdrop()
                        {
                            Kind = MicaKind.Base
                        };
                    }
                    else
                    {
                        SystemBackdrop = new DesktopAcrylicBackdrop();
                    }
                }
                else
                {
                    // default acrylic.
                    SystemBackdrop = new DesktopAcrylicBackdrop();
                }
            }
            else
            {
                // just for me.
                SystemBackdrop = new DesktopAcrylicBackdrop();
            }

        }
        else if (Microsoft.UI.Composition.SystemBackdrops.MicaController.IsSupported())
        {
            SystemBackdrop = new MicaBackdrop()
            {
                Kind = MicaKind.Base
            };
        }
        else
        {
            // Memo: Without Backdrop, theme setting's theme is not gonna have any effect( "system default" will be used). So the setting is disabled.
        }
    }
}
