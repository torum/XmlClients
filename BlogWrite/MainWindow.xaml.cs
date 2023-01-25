using BlogWrite.Core.Helpers;

namespace BlogWrite;

public sealed partial class MainWindow : WindowEx
{
    public MainWindow()
    {
        InitializeComponent();

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/BlogWrite.ico"));
        Content = null;
        Title = "AppDisplayName".GetLocalized();

    }
}
