namespace BlogWrite.Views
{
    /// <summary>
    /// EditorWindow.xaml code behind.
    /// </summary>
    public partial class EditorWindow
    {
        public EditorWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App app = App.Current as App;

            app.RemoveEditorWindow(this);

        }
    }
}
