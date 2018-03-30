using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BlogWrite.Views;

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

            app.WindowList.Remove(this);

        }
    }
}
