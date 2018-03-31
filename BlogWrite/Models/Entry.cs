using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using BlogWrite.Models.Clients;

namespace BlogWrite.Models
{
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
    public class NodeEntry : NodeTree
    {
        public Uri Uri { get; set; }

        //TODO: enum AcceptType
        // "application/atom+xml"
        // "application/atom+xml;type=entry"
        // "application/atomcat+xml"
        public Collection<string> AcceptTypes = new Collection<string>();

        // Constructor.
        public NodeEntry(string name, Uri uri) : base(name)
        {
            Uri = uri;
            PathIcon = "M4,5V7H21V5M4,11H21V9H4M4,19H21V17H4M4,15H21V13H4V15Z";
        }

        public ObservableCollection<EntryItem> List { get; } = new ObservableCollection<EntryItem>();

        public BlogClient Client
        {
            get
            {
                if (this.Parent == null)
                    return null;
                if (this.Parent.Parent == null)
                    return null;
                if (!(this.Parent.Parent is NodeServies))
                    return null;

                return (this.Parent.Parent as NodeServies).Client;
            }
        }
    }

    /// <summary>
    /// class for Entry for listview index.
    /// </summary>
    public class EntryItem : Node
    {
        private string _esNew = "M13,9V3.5L18.5,9M6,2C4.89,2 4,2.89 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2H6Z";
        private string _esDraft = "M6,2C4.89,2 4,2.9 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2H6M13,3.5L18.5,9H13V3.5M12,11A3,3 0 0,1 15,14V15H16V19H8V15H9V14C9,12.36 10.34,11 12,11M12,13A1,1 0 0,0 11,14V15H13V14C13,13.47 12.55,13 12,13Z";
        private string _esNormal = "M13,9H18.5L13,3.5V9M6,2H14L20,8V20A2,2 0 0,1 18,22H6C4.89,22 4,21.1 4,20V4C4,2.89 4.89,2 6,2M15,18V16H6V18H15M18,14V12H6V14H18Z";
        private string _esUpdating = "M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M12,18C9.95,18 8.19,16.76 7.42,15H9.13C9.76,15.9 10.81,16.5 12,16.5A3.5,3.5 0 0,0 15.5,13A3.5,3.5 0 0,0 12,9.5C10.65,9.5 9.5,10.28 8.9,11.4L10.5,13H6.5V9L7.8,10.3C8.69,8.92 10.23,8 12,8A5,5 0 0,1 17,13A5,5 0 0,1 12,18Z";
        private EntryStatus _es;

        // System unique id used for file name or unique id for table in db. We auto-generate at constructer.
        public string ID { get; }
        // Entry's id provided by services.
        public string EntryID { get; set; }
        public NodeEntry NodeEntry { get; set; }

        public Uri PostUri { get; set; }
        public Uri EditUri { get; set; }
        public Uri AltUri { get; set; }

        // TODO:
        // created
        // last update

        // author

        public string Title
        {
            get
            {
                return this.Name;
            }
            set
            {
                if (this.Name == value)
                    return;

                this.Name = value;
                NotifyPropertyChanged(nameof(Title));
            }
        }

        public bool IsDraft { get; set; }

        public enum EntryStatus
        {
            esNew,
            esDraft,
            esNormal,
            esUpdating
        }

        public EntryStatus Status {
            get
            {
                return _es;
            }
            set
            {
                if (_es == value)
                    return;
                _es = value;

                // Update icon.
                NotifyPropertyChanged(nameof(Status));
                NotifyPropertyChanged(nameof(PathIcon));
            }
        }

        public string PathIcon
        {
            get
            {
                switch (Status)
                {
                    case EntryStatus.esNew:
                        return _esNew;
                    case EntryStatus.esDraft:
                        return _esDraft;
                    case EntryStatus.esNormal:
                        return _esNormal;
                    case EntryStatus.esUpdating:
                        return _esUpdating;
                    default: return _esNew;
                }
            }
        }

        public BlogClient Client { get; } = null;

        public EntryFull EntryBody { get; set; }

        // Constructor.
        public EntryItem(string title, BlogClient bc) : base(title)
        {
            Client = bc;
            ID = Guid.NewGuid().ToString();
            Status = EntryStatus.esNew;
        }
        
    }

    /// <summary>
    /// Base class for a Blog Entry class, which represents the whole Entry.
    /// </summary>
    public abstract class EntryFull : EntryItem
    {
        protected string _content = "";


        //type="text/html"
        //type="text/x-hatena-syntax"
        //type = "text/x-markdown"

        public enum ContentTypes
        {
            text,
            textHtml,
            markdown,
            hatena
        }

        public ContentTypes ContentType { get; set; }

        public string Content
        {
            get
            {
                return _content;
            }
            set
            {
                if (value == _content)
                    return;

                _content = value;

                NotifyPropertyChanged(nameof(Content));
            }
        }

        protected EntryFull(string title, BlogClient bc) : base(title, bc)
        {

        }

    }

    /// <summary>
    /// Atom Blog Entry class, which represents Atom Entry.
    /// </summary>
    public class AtomEntry : EntryFull
    {
        public string ContentTypeString { get; set; }

        public AtomEntry(string title, BlogClient bc) : base(title, bc)
        {

        }

        public XmlDocument AsXmlDoc()
        {
            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = xdoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            /*
			<entry xmlns="http://www.w3.org/2005/Atom" xmlns:app="http://www.w3.org/2007/app">
				<id>hoge</id>
				<link rel="edit" href="https://127.0.0.1/app/entry/17391345971628358314"/>
				<link rel="alternate" type="text/html" href="https://127.0.0.1/htm/entry/2018/03/22/221846"/>
				<author>
                    <name>hoge</name>
                </author>
				<title>test title</title>
				<updated>2018-03-22T22:18:46+09:00</updated>
				<published>2018-03-22T22:18:46+09:00</published>
				<app:edited>2018-03-22T22:18:46+09:00</app:edited>
				<summary type="text">asdf</summary>
				<content type="text/html">asdf</content>
				<hatena:formatted-content type="text/html" xmlns:hatena="http://www.hatena.ne.jp/info/xmlns#">&lt;a class=&quot;keyword&quot; href=&quot;http://d.hatena.ne.jp/keyword/asdf&quot;&gt;asdf&lt;/a&gt;</hatena:formatted-content>
				<category term="test" />
				<app:control>
					<app:draft>yes</app:draft>
				</app:control>
			  </entry>
             */

            XmlNamespaceManager atomNsMgr = new XmlNamespaceManager(xdoc.NameTable);
            atomNsMgr.AddNamespace("atom", "http://www.w3.org/2005/Atom");
            atomNsMgr.AddNamespace("app", "http://www.w3.org/2007/app");

            XmlElement rootNode = xdoc.CreateElement(string.Empty, "entry", "http://www.w3.org/2005/Atom");
            //rootNode.SetAttribute("xmlns", "http://www.w3.org/2005/Atom");
            rootNode.SetAttribute("xmlns:app", "http://www.w3.org/2007/app");
            xdoc.AppendChild(rootNode);

            XmlElement idNode = xdoc.CreateElement(string.Empty, "id", "http://www.w3.org/2005/Atom");
            XmlText idTextNode = xdoc.CreateTextNode(this.EntryID);
            idNode.AppendChild(idTextNode);
            rootNode.AppendChild(idNode);

            //link rel
            //link rel
            //author/name
            //updated
            //published
            //app:edited
            //summary

            //category

            XmlElement titleNode = xdoc.CreateElement(string.Empty, "title", "http://www.w3.org/2005/Atom");
            XmlText titleTextNode = xdoc.CreateTextNode(this.Name);
            titleNode.AppendChild(titleTextNode);
            rootNode.AppendChild(titleNode);

            XmlElement contentNode = xdoc.CreateElement(string.Empty, "content", "http://www.w3.org/2005/Atom");
            contentNode.SetAttribute("type", this.ContentTypeString);
            XmlText contentTextNode = xdoc.CreateTextNode(this.Content);
            contentNode.AppendChild(contentTextNode);
            rootNode.AppendChild(contentNode);

            XmlElement controlNode = xdoc.CreateElement("app", "control", "http://www.w3.org/2007/app");
            XmlElement draftNode = xdoc.CreateElement("app", "draft", "http://www.w3.org/2007/app");
            string yesOrNo = this.IsDraft ? "yes" : "no";
            XmlText draftTextNode = xdoc.CreateTextNode(yesOrNo);
            draftNode.AppendChild(draftTextNode);
            controlNode.AppendChild(draftNode);
            rootNode.AppendChild(controlNode);

            return xdoc;

        }

        public string AsXml()
        {
            var sb = new StringBuilder();
            using (var stringWriter = new StringWriterWithEncoding(sb, Encoding.UTF8))
            using (var xmlTextWriter = XmlWriter.Create(stringWriter))
            {
                XmlDocument xdoc = AsXmlDoc();

                xdoc.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                return stringWriter.GetStringBuilder().ToString();
            }
        }

    }

    /// <summary>
    /// Hatena Blog Atom Entry class.
    /// </summary>
    public class AtomEntryHatena : AtomEntry
    {
        protected string _formattedContent = "";

        public string FormattedContent
        {
            get
            {
                return _formattedContent;
            }
            set
            {
                if (value == _formattedContent)
                    return;

                _formattedContent = value;

                NotifyPropertyChanged(nameof(FormattedContent));
            }
        }

        public AtomEntryHatena(string title, BlogClient bc) : base(title, bc)
        {

        }

        //public XmlDocument AsXmlDoc(){ }

        //public string AsXml(){}

    }


    /// <summary>
    /// StringWriter With Encoding.
    /// </summary>
    public class StringWriterWithEncoding : StringWriter
    {
        public StringWriterWithEncoding(StringBuilder sb, Encoding encoding)
            : base(sb)
        {
            this.m_Encoding = encoding;
        }
        private readonly Encoding m_Encoding;
        public override Encoding Encoding
        {
            get
            {
                return this.m_Encoding;
            }
        }
    }

}
