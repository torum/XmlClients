/// 
/// 
/// BlogWrite 
///  - C#/WPF port of the original "BlogWrite" developed with Delphi.
/// https://github.com/torum/BlogWrite
/// 
/// 

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace BlogWrite.Models
{
    /// <summary>
    /// Base class for Treeview Node and Listview Item.
    /// </summary>
    abstract public class Node : INotifyPropertyChanged
    {
        private string _name;

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name == value)
                    return;

                _name = value;

                NotifyPropertyChanged("Name");
            }
        }

        protected Node(){}

        protected Node(string name)
        {
            Name = name;
        }

        #region == INotifyPropertyChanged ==

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            //this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            Application.Current.Dispatcher.Invoke(() =>
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        #endregion
    }

    /// <summary>
    /// Base class for Treeview Node.
    /// </summary>
    public class NodeTree : Node
    {
        private string _pathData = "M20,18H4V8H20M20,6H12L10,4H4C2.89,4 2,4.89 2,6V18A2,2 0 0,0 4,20H20A2,2 0 0,0 22,18V8C22,6.89 21.1,6 20,6Z";
        private NodeTree _parent;
        private bool _expanded;
        private ObservableCollection<NodeTree> _children = new ObservableCollection<NodeTree>();

        public string PathIcon
        {
            get
            {
                return _pathData;
            }
            protected set
            {
                if (_pathData == value)
                    return;
                _pathData = value;

                NotifyPropertyChanged("PathIcon");
            }
        }

        public bool Selected { get; set; }

        public bool Expanded
        {
            get
            {
                return _expanded;
            }
            set
            {
                if (_expanded == value)
                    return;

                _expanded = value;

                NotifyPropertyChanged("Expanded");
            }
        }

        public NodeTree Parent
        {
            get
            {
                return this._parent;
            }

            set
            {
                if (_parent == value)
                    return;


                _parent = value;

                NotifyPropertyChanged("Parent");
            }
        }

        public ObservableCollection<NodeTree> Children
        {
            get
            {
                return _children;
            }
            set
            {
                _children = value;

                NotifyPropertyChanged("Children");
            }
        }

        protected NodeTree(){}

        protected NodeTree(string name): base(name)
        {
            BindingOperations.EnableCollectionSynchronization(_children, new object());
        }

    }

}
