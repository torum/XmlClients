using XmlClients.Core.Helpers;

namespace BlogDesk;

public sealed partial class MainWindow : WindowEx
{
    public MainWindow()
    {
        InitializeComponent();

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/BlogDesk.ico"));
        Content = null;
        Title = "AppDisplayName".GetLocalized();
    }
}
