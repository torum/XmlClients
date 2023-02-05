using BlogWrite.Core.Helpers;

namespace FeedDesk;

public sealed partial class MainWindow : WindowEx
{
    public MainWindow()
    {
        InitializeComponent();

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "FeedDesk3.ico"));
        Content = null;
        Title = "AppDisplayName".GetLocalized();

    }
}
