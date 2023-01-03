using System.Xml.Linq;
using BlogWrite.Contracts.Services;
using BlogWrite.ViewModels;

using Microsoft.UI.Xaml;

namespace BlogWrite.Activation;

public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly INavigationService _navigationService;

    public DefaultActivationHandler(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        // None of the ActivationHandlers has handled the activation.
        return _navigationService.Frame?.Content == null;
    }

    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        _navigationService.NavigateTo(typeof(MainViewModel).FullName!, args.Arguments);


        #region == Load settings ==

        // set fallback, default
        double width = 1007;//App.MainWindow.GetAppWindow().Size.Width;
        double height = 600;//App.MainWindow.GetAppWindow().Size.Height;

        // Create if not exists.
        System.IO.Directory.CreateDirectory(App.AppDataFolder);

        if (System.IO.File.Exists(App.AppConfigFilePath))
        {
            var xdoc = XDocument.Load(App.AppConfigFilePath);

            //Debug.WriteLine(xdoc.ToString());

            // Main window
            if (App.MainWindow != null)
            {
                // Main Window element
                var mainWindow = xdoc.Root?.Element("MainWindow");
                if (mainWindow != null)
                {
                    /*
                    var hoge = mainWindow.Attribute("top");
                    if (hoge != null)
                    {
                        (sender as Window).Top = double.Parse(hoge.Value);
                    }
                    */
                    /*
                    hoge = mainWindow.Attribute("left");
                    if (hoge != null)
                    {
                        (sender as Window).Left = double.Parse(hoge.Value);
                    }
                    */
                    var hoge = mainWindow.Attribute("height");
                    if (hoge != null)
                    {
                        height = double.Parse(hoge.Value);
                    }

                    hoge = mainWindow.Attribute("width");
                    if (hoge != null)
                    {
                        width = double.Parse(hoge.Value);
                    }
                    /*
                    hoge = mainWindow.Attribute("state");
                    if (hoge != null)
                    {
                        if (hoge.Value == "Maximized")
                        {
                            (sender as Window).WindowState = WindowState.Maximized;
                        }
                        else if (hoge.Value == "Normal")
                        {
                            (sender as Window).WindowState = WindowState.Normal;
                        }
                        else if (hoge.Value == "Minimized")
                        {
                            (sender as Window).WindowState = WindowState.Normal;
                        }
                    }
                    */

                }

            }
            /*
            // Options
            var opts = xdoc.Root.Element("Opts");
            if (opts != null)
            {
                var xvalue = opts.Attribute("IsChartTooltipVisible");
                if (xvalue != null)
                {
                    if (!string.IsNullOrEmpty(xvalue.Value))
                    {
                        if (xvalue.Value == "True")
                            MainVM.IsChartTooltipVisible = true;
                        else

                            MainVM.IsChartTooltipVisible = false;
                    }
                }
            }
            */

        }

        #endregion
        
        App.MainWindow.Width = width;
        App.MainWindow.Height= height;

        //
        //App.MainWindow.CenterOnScreen();

        await Task.CompletedTask;
    }
}
