using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace BlogWrite.Helpers
{

    public class TreeViewSelectedItemHelper : TreeView, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SelectedNodeProperty = DependencyProperty.Register("SelectedNode", typeof(Object), typeof(TreeViewSelectedItemHelper), new PropertyMetadata(null));

        public Object SelectedNode
        {
            get { return (Object)GetValue(SelectedNodeProperty); }
            set
            {
                SetValue(SelectedNodeProperty, value);
                NotifyPropertyChanged("SelectedNode");
            }
        }

        public TreeViewSelectedItemHelper() : base()
        {
            base.SelectedItemChanged += new RoutedPropertyChangedEventHandler<Object>(TreeViewSelectable_SelectedItemChanged);
        }

        private void TreeViewSelectable_SelectedItemChanged(Object sender, RoutedPropertyChangedEventArgs<Object> e)
        {
            this.SelectedNode = base.SelectedItem;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String aPropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(aPropertyName));
        }
    }
}
