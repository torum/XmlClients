using BlogWrite.Core.Helpers;

namespace BlogDesk.Views;

public sealed partial class EditorWindow : WindowEx
{
    public EditorWindow()
    {
        InitializeComponent();

        //AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/BlogWrite.ico"));
        //Content = null;
        //Title = "AppDisplayName".GetLocalized();

    }
}
