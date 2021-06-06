using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using BlogWrite.Models.Clients;

namespace BlogWrite.Models
{
    /// TODO:
    /// 
    /// need Atom NodeMediaCollection
    /// 

    /// <summary>
    /// class for EntryNode child (for treeview).
    /// </summary>
    public class NodeEntries : NodeTree
    {
        public NodeEntries() { }
    }

    /// <summary>
    /// class for EntryNode (for treeview).
    /// </summary>
    public class NodeEntryCollection : NodeTree
    {
        /// <summary>
        /// entries resource URI or xml-rpc URL for a blog.
        /// </summary>
        public Uri Uri { get; set; }

        // Constructor.
        public NodeEntryCollection(string name, Uri uri) : base(name)
        {
            Uri = uri;
            PathIcon = "M4,5V7H21V5M4,11H21V9H4M4,19H21V17H4M4,15H21V13H4V15Z";
        }

        public ObservableCollection<EntryItem> List { get; } = new ObservableCollection<EntryItem>();

        public BaseClient Client
        {
            get
            {
                if (this.Parent == null)
                    return null;

                if (this.Parent is NodeService)
                {
                    return (this.Parent as NodeService).Client;
                }

                if (this.Parent.Parent == null)
                    return null;

                if (!(this.Parent.Parent is NodeService))
                    return null;

                return (this.Parent.Parent as NodeService).Client;
            }
        }
    }

    public class NodeAtomPubEntryCollection : NodeEntryCollection
    {
        public Uri CategoriesUri { get; set; }

        public bool CategoryIsFixed { get; set; }

        public string CategoryScheme { get; set; }

        //TODO: enum supported AcceptTypes
        // "application/atom+xml"
        // "application/atom+xml;type=entry"
        // "application/atomcat+xml"
        public Collection<string> AcceptTypes = new Collection<string>();

        // Constructor.
        public NodeAtomPubEntryCollection(string name, Uri uri) : base(name, uri)
        {
            Uri = uri;
            PathIcon = "M4,5V7H21V5M4,11H21V9H4M4,19H21V17H4M4,15H21V13H4V15Z";
        }
    }

    public class NodeXmlRpcMTEntryCollection : NodeEntryCollection
    {
        // Constructor.
        public NodeXmlRpcMTEntryCollection(string name, Uri uri) : base(name, uri)
        {
            PathIcon = "M4,5V7H21V5M4,11H21V9H4M4,19H21V17H4M4,15H21V13H4V15Z";
        }
    }

    public class NodeXmlRpcWPEntryCollection : NodeEntryCollection
    {
        // Constructor.
        public NodeXmlRpcWPEntryCollection(string name, Uri uri) : base(name, uri)
        {
            PathIcon = "M4,5V7H21V5M4,11H21V9H4M4,19H21V17H4M4,15H21V13H4V15Z";
        }
    }

}
