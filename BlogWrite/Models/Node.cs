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
    abstract public class NodeTree : Node
    {
        private string _pathData = "M20,18H4V8H20M20,6H12L10,4H4C2.89,4 2,4.89 2,6V18A2,2 0 0,0 4,20H20A2,2 0 0,0 22,18V8C22,6.89 21.1,6 20,6Z";
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

        private bool _IsSelected;
        public bool IsSelected
        {
            get
            {
                return _IsSelected;
            }
            set
            {
                if (_IsSelected == value)
                    return;

                _IsSelected = value;

                NotifyPropertyChanged("IsSelected");
            }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                if (_isExpanded == value)
                    return;

                _isExpanded = value;

                NotifyPropertyChanged("IsExpanded");
            }
        }

        private bool _isDragOver;
        public bool IsDragOver
        {
            get
            {
                return _isDragOver;
            }
            set
            {
                if (_isDragOver == value)
                    return;

                _isDragOver = value;

                NotifyPropertyChanged("IsDragOver");
            }
        }

        private bool _isBeforeDragSeparator;
        public bool IsBeforeDragSeparator
        {
            get
            {
                return _isBeforeDragSeparator;
            }
            set
            {
                if (_isBeforeDragSeparator == value)
                    return;

                _isBeforeDragSeparator = value;

                NotifyPropertyChanged("IsBeforeDragSeparator");
            }
        }

        private bool _isAfterDragSeparator;
        public bool IsAfterDragSeparator
        {
            get
            {
                return _isAfterDragSeparator;
            }
            set
            {
                if (_isAfterDragSeparator == value)
                    return;

                _isAfterDragSeparator = value;

                NotifyPropertyChanged("IsAfterDragSeparator");
            }
        }

        private NodeTree _parent;
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

        private ObservableCollection<NodeTree> _children = new ObservableCollection<NodeTree>();
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

        private bool ContainsChildLoop(ObservableCollection<NodeTree> childList, NodeTree ntc)
        {
            bool hasChild = false;

            foreach (var c in childList)
            {
                if (c == ntc)
                    return true;

                if (c.Children.Count > 0)
                {
                    if (ContainsChildLoop(c.Children, ntc))
                        return true;
                }
            }

            return hasChild;
        }

        public bool ContainsChild(NodeTree nt)
        {
            if (ContainsChildLoop(this.Children, nt))
                return true;
            else
                return false;

        }

    }
}
