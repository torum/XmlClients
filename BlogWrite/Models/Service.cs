using BlogWrite.Models.Clients;
using System;
using System.Collections.ObjectModel;
using System.Xml;

namespace BlogWrite.Models
{
    // List of recognized services.
    public enum ServiceTypes
    {
        AtomPub,
        AtomPub_Hatena,
        Feed,
        XmlRpc_WordPress,
        XmlRpc_MovableType,
        AtomApi,
        Unknown
    }

    // List of APIs
    public enum ApiTypes
    {
        atAtomPub,
        atAtomFeed,
        atRssFeed,
        //atXMLRPC,
        atXMLRPC_MovableType,
        atXMLRPC_WordPress,
        //atWPJson,
        //atAtomAPI,
        atUnknown
    }

    // Web Searvice (AtomPub, XML-RPC, etc)
    public class NodeService : NodeTree
    {
        // Service provider logos icon (svg based path data)
        
        // WordPress
        //private string _pathIconWordPress = "M3.42,12C3.42,10.76 3.69,9.58 4.16,8.5L8.26,19.72C5.39,18.33 3.42,15.4 3.42,12M17.79,11.57C17.79,12.3 17.5,13.15 17.14,14.34L16.28,17.2L13.18,8L14.16,7.9C14.63,7.84 14.57,7.16 14.11,7.19C14.11,7.19 12.72,7.3 11.82,7.3L9.56,7.19C9.1,7.16 9.05,7.87 9.5,7.9L10.41,8L11.75,11.64L9.87,17.27L6.74,8L7.73,7.9C8.19,7.84 8.13,7.16 7.67,7.19C7.67,7.19 6.28,7.3 5.38,7.3L4.83,7.29C6.37,4.96 9,3.42 12,3.42C14.23,3.42 16.27,4.28 17.79,5.67H17.68C16.84,5.67 16.24,6.4 16.24,7.19C16.24,7.9 16.65,8.5 17.08,9.2C17.41,9.77 17.79,10.5 17.79,11.57M12.15,12.75L14.79,19.97L14.85,20.09C13.96,20.41 13,20.58 12,20.58C11.16,20.58 10.35,20.46 9.58,20.23L12.15,12.75M19.53,7.88C20.2,9.11 20.58,10.5 20.58,12C20.58,15.16 18.86,17.93 16.31,19.41L18.93,11.84C19.42,10.62 19.59,9.64 19.59,8.77L19.53,7.88M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,21.54C17.26,21.54 21.54,17.26 21.54,12C21.54,6.74 17.26,2.46 12,2.46C6.74,2.46 2.46,6.74 2.46,12C2.46,17.26 6.74,21.54 12,21.54Z";
        // Hatena
        //private string _pathIconHatena = "M 50.099609 0 C 22.499609 4.7369516e-015 0 22.499609 0 50.099609 C 4.7369516e-015 77.699609 22.499609 100.19922 50.099609 100.19922 C 77.699609 100.19922 100.09922 77.699609 100.19922 50.099609 C 100.19922 22.499609 77.699609 0 50.099609 0 z M 50.099609 6.4003906 C 74.199609 6.4003906 93.800781 25.999609 93.800781 50.099609 C 93.800781 74.199609 74.199609 93.800781 50.099609 93.800781 C 25.999609 93.800781 6.4003906 74.199609 6.4003906 50.099609 C 6.4003906 25.999609 25.999609 6.4003906 50.099609 6.4003906 z M 49 12.699219 C 48.2 15.499219 46.800781 20.300781 44.300781 25.300781 C 40.500781 33.100781 35.699219 39.900391 35.699219 39.900391 L 38.800781 81.699219 C 38.800781 81.699219 41.699609 84.900391 50.099609 84.900391 C 58.499609 84.900391 61.400391 81.699219 61.400391 81.699219 L 64.5 39.900391 C 64.5 39.900391 59.700391 33.100781 55.900391 25.300781 C 53.500391 20.300781 51.999219 15.499219 51.199219 12.699219 L 51.199219 48.199219 C 52.399219 48.699219 53.300781 49.799219 53.300781 51.199219 C 53.300781 52.999219 51.8 54.5 50 54.5 C 48.2 54.5 46.699219 52.999219 46.699219 51.199219 C 46.699219 49.699219 47.7 48.499609 49 48.099609 L 49 12.699219 z";
        // AtomPub
        //private string _pathIconAtomPub = "M12,11A1,1 0 0,1 13,12A1,1 0 0,1 12,13A1,1 0 0,1 11,12A1,1 0 0,1 12,11M4.22,4.22C5.65,2.79 8.75,3.43 12,5.56C15.25,3.43 18.35,2.79 19.78,4.22C21.21,5.65 20.57,8.75 18.44,12C20.57,15.25 21.21,18.35 19.78,19.78C18.35,21.21 15.25,20.57 12,18.44C8.75,20.57 5.65,21.21 4.22,19.78C2.79,18.35 3.43,15.25 5.56,12C3.43,8.75 2.79,5.65 4.22,4.22M15.54,8.46C16.15,9.08 16.71,9.71 17.23,10.34C18.61,8.21 19.11,6.38 18.36,5.64C17.62,4.89 15.79,5.39 13.66,6.77C14.29,7.29 14.92,7.85 15.54,8.46M8.46,15.54C7.85,14.92 7.29,14.29 6.77,13.66C5.39,15.79 4.89,17.62 5.64,18.36C6.38,19.11 8.21,18.61 10.34,17.23C9.71,16.71 9.08,16.15 8.46,15.54M5.64,5.64C4.89,6.38 5.39,8.21 6.77,10.34C7.29,9.71 7.85,9.08 8.46,8.46C9.08,7.85 9.71,7.29 10.34,6.77C8.21,5.39 6.38,4.89 5.64,5.64M9.88,14.12C10.58,14.82 11.3,15.46 12,16.03C12.7,15.46 13.42,14.82 14.12,14.12C14.82,13.42 15.46,12.7 16.03,12C15.46,11.3 14.82,10.58 14.12,9.88C13.42,9.18 12.7,8.54 12,7.97C11.3,8.54 10.58,9.18 9.88,9.88C9.18,10.58 8.54,11.3 7.97,12C8.54,12.7 9.18,13.42 9.88,14.12M18.36,18.36C19.11,17.62 18.61,15.79 17.23,13.66C16.71,14.29 16.15,14.92 15.54,15.54C14.92,16.15 14.29,16.71 13.66,17.23C15.79,18.61 17.62,19.11 18.36,18.36Z";
        // AtomFeed
        //private string _pathIconAtomFeed = "M6.18,15.64A2.18,2.18 0 0,1 8.36,17.82C8.36,19 7.38,20 6.18,20C5,20 4,19 4,17.82A2.18,2.18 0 0,1 6.18,15.64M4,4.44A15.56,15.56 0 0,1 19.56,20H16.73A12.73,12.73 0 0,0 4,7.27V4.44M4,10.1A9.9,9.9 0 0,1 13.9,20H11.07A7.07,7.07 0 0,0 4,12.93V10.1Z";
        // Blogger
        //private string _pathIconBlogger = "M14,13H9.95A1,1 0 0,0 8.95,14A1,1 0 0,0 9.95,15H14A1,1 0 0,0 15,14A1,1 0 0,0 14,13M9.95,10H12.55A1,1 0 0,0 13.55,9A1,1 0 0,0 12.55,8H9.95A1,1 0 0,0 8.95,9A1,1 0 0,0 9.95,10M16,9V10A1,1 0 0,0 17,11A1,1 0 0,1 18,12V15A3,3 0 0,1 15,18H9A3,3 0 0,1 6,15V8A3,3 0 0,1 9,5H13A3,3 0 0,1 16,8M20,2H4C2.89,2 2,2.89 2,4V20A2,2 0 0,0 4,22H20A2,2 0 0,0 22,20V4C22,2.89 21.1,2 20,2Z";

        public Uri EndPoint {get;set;}

        public string UserName { get; set; }

        public string UserPassword { get; set; }

        public ApiTypes Api { get; set; }

        public ServiceTypes ServiceType { get; set; }

        public BaseClient Client { get; }

        public string ID { get; }

        public NodeService(string name, string username, string password, Uri endPoint, ApiTypes api, ServiceTypes serviceType) : base(name)
        {
            // Default account icon
            PathIcon = "M12,19.2C9.5,19.2 7.29,17.92 6,16C6.03,14 10,12.9 12,12.9C14,12.9 17.97,14 18,16C16.71,17.92 14.5,19.2 12,19.2M12,5A3,3 0 0,1 15,8A3,3 0 0,1 12,11A3,3 0 0,1 9,8A3,3 0 0,1 12,5M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12C22,6.47 17.5,2 12,2Z";

            EndPoint = endPoint;

            Api = api;

            ServiceType = serviceType;

            ID = Guid.NewGuid().ToString();

            UserName = username;

            UserPassword = password;

            switch (api)
            {
                case ApiTypes.atAtomPub:
                    Client = new AtomPubClient(UserName, UserPassword, EndPoint);
                    break;
                case ApiTypes.atXMLRPC_MovableType:
                    Client = new XmlRpcMTClient(UserName, UserPassword, EndPoint);
                    break;
                //case ApiTypes.atXMLRPC_WordPress:
                //    Client = new XmlRpcWPClient(UserName, UserPassword, EndPoint);
                //    break;
                case ApiTypes.atAtomFeed:
                    Client = new AtomFeedClient();
                    break;
                case ApiTypes.atRssFeed:
                    Client = new RssFeedClient();
                    break;
                    //TODO: WP, AtomAPI
            }

        }
        
        public NodeService(string name, Uri endPoint, ApiTypes api, ServiceTypes serviceType) : base(name)
        { 
            // Default account icon
            PathIcon = "M12,19.2C9.5,19.2 7.29,17.92 6,16C6.03,14 10,12.9 12,12.9C14,12.9 17.97,14 18,16C16.71,17.92 14.5,19.2 12,19.2M12,5A3,3 0 0,1 15,8A3,3 0 0,1 12,11A3,3 0 0,1 9,8A3,3 0 0,1 12,5M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12C22,6.47 17.5,2 12,2Z";

            EndPoint = endPoint;

            Api = api;
            
            ServiceType = serviceType;

            ID = Guid.NewGuid().ToString();

            switch (api)
            {
                case ApiTypes.atAtomPub:
                    Client = new AtomPubClient(UserName, UserPassword, EndPoint);
                    break;
                case ApiTypes.atXMLRPC_MovableType:
                    Client = new XmlRpcMTClient(UserName, UserPassword, EndPoint);
                    break;
                //case ApiTypes.atXMLRPC_WordPress:
                //    Client = new XmlRpcWPClient(UserName, UserPassword, EndPoint);
                //    break;
                case ApiTypes.atAtomFeed:
                    Client = new AtomFeedClient();
                    break;
                case ApiTypes.atRssFeed:
                    Client = new RssFeedClient();
                    break;

                    //TODO: WP, AtomAPI
            }
        }
    }

    // Base class for NodeAtomFeed and NodeRssFeed
    abstract public class NodeFeed : NodeService
    {
        public string SiteTitle { get; set; }

        public Uri SiteUri { get; set; }

        public enum DownloadStatus
        {
            normal,
            loading,
            error
        }

        private static string _defaultPathIcon = "M6.18,15.64A2.18,2.18 0 0,1 8.36,17.82C8.36,19 7.38,20 6.18,20C5,20 4,19 4,17.82A2.18,2.18 0 0,1 6.18,15.64M4,4.44A15.56,15.56 0 0,1 19.56,20H16.73A12.73,12.73 0 0,0 4,7.27V4.44M4,10.1A9.9,9.9 0 0,1 13.9,20H11.07A7.07,7.07 0 0,0 4,12.93V10.1Z";
        private static string _loadingPathIcon = "M2 12C2 16.97 6.03 21 11 21C13.39 21 15.68 20.06 17.4 18.4L15.9 16.9C14.63 18.25 12.86 19 11 19C4.76 19 1.64 11.46 6.05 7.05C10.46 2.64 18 5.77 18 12H15L19 16H19.1L23 12H20C20 7.03 15.97 3 11 3C6.03 3 2 7.03 2 12Z";
        private static string _loadErrorPathIcon = "M2 12C2 17 6 21 11 21C13.4 21 15.7 20.1 17.4 18.4L15.9 16.9C14.6 18.3 12.9 19 11 19C4.8 19 1.6 11.5 6.1 7.1S18 5.8 18 12H15L19 16H19.1L23 12H20C20 7 16 3 11 3S2 7 2 12M10 15H12V17H10V15M10 7H12V13H10V7";

        private DownloadStatus _status;
        public DownloadStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (_status == value)
                    return;

                _status = value;

                if (_status == DownloadStatus.normal)
                {
                    PathIcon = _defaultPathIcon;
                }
                else if (_status == DownloadStatus.loading)
                {
                    PathIcon = _loadingPathIcon;
                }
                else if (_status == DownloadStatus.error)
                {
                    PathIcon = _loadErrorPathIcon;
                }
                NotifyPropertyChanged("PathIcon");

                NotifyPropertyChanged("Status");
            }
        }

        public ObservableCollection<EntryItem> List { get; } = new ObservableCollection<EntryItem>();

        public NodeFeed(string name, Uri feedUrl, ApiTypes api) : base(name, feedUrl, api, ServiceTypes.Feed)
        {
            PathIcon = "M6.18,15.64A2.18,2.18 0 0,1 8.36,17.82C8.36,19 7.38,20 6.18,20C5,20 4,19 4,17.82A2.18,2.18 0 0,1 6.18,15.64M4,4.44A15.56,15.56 0 0,1 19.56,20H16.73A12.73,12.73 0 0,0 4,7.27V4.44M4,10.1A9.9,9.9 0 0,1 13.9,20H11.07A7.07,7.07 0 0,0 4,12.93V10.1Z";
        }
    }

    // Atom Feed
    public class NodeAtomFeed : NodeFeed
    {
        public NodeAtomFeed(string name, Uri feedUrl) : base(name, feedUrl, ApiTypes.atAtomFeed)
        {
            Api = ApiTypes.atAtomFeed;
            ServiceType = ServiceTypes.Feed;
        }
    }

    // RSS Feed
    public class NodeRssFeed : NodeFeed
    {
        public NodeRssFeed(string name, Uri feedUrl) : base(name, feedUrl, ApiTypes.atRssFeed)
        {
            Api = ApiTypes.atRssFeed;
            ServiceType = ServiceTypes.Feed;
        }
    }

    // Folder
    public class NodeFolder : NodeTree
    {
        public NodeFolder(string name) : base(name)
        {
            PathIcon = "M5,3H19A2,2 0 0,1 21,5V19A2,2 0 0,1 19,21H5A2,2 0 0,1 3,19V5A2,2 0 0,1 5,3M7.5,15A1.5,1.5 0 0,0 6,16.5A1.5,1.5 0 0,0 7.5,18A1.5,1.5 0 0,0 9,16.5A1.5,1.5 0 0,0 7.5,15M6,10V12A6,6 0 0,1 12,18H14A8,8 0 0,0 6,10M6,6V8A10,10 0 0,1 16,18H18A12,12 0 0,0 6,6Z";
        }
    }

    /// <summary>
    /// Container class for Treeview.
    /// </summary>
    public class ServiceTreeBuilder : NodeTree
    {
        public ServiceTreeBuilder() { }

        public void LoadXmlDoc(XmlDocument doc)
        {
            if (doc == null)
                return;
            if (doc.DocumentElement == null)
                return;

            XmlNodeList accountList;
            accountList = doc.SelectNodes("//Accounts");
            if (accountList == null) 
                return;

            foreach (XmlNode a in accountList)
            {
                // Loop through the top level childs.
                XmlNodeList serviceList = a.ChildNodes;
                if (serviceList == null)
                    continue;

                foreach (XmlNode s in serviceList)
                {
                    if (s.LocalName.Equals("Service"))
                    {
                        var accountName = s.Attributes["Name"].Value;

                        var userName = s.Attributes["UserName"].Value;
                        var userPassword = s.Attributes["UserPassword"].Value;
                        var endpoint = s.Attributes["EndPoint"].Value;
                        string api = (s.Attributes["Api"] != null) ? s.Attributes["Api"].Value : "Unknown"; //
                        string tp = (s.Attributes["Type"] != null) ? s.Attributes["Type"].Value : "Unknown";

                        var selecteds = string.IsNullOrEmpty(s.Attributes["Selected"].Value) ? "" : s.Attributes["Selected"].Value;
                        var expandeds = string.IsNullOrEmpty(s.Attributes["Expanded"].Value) ? "" : s.Attributes["Expanded"].Value;
                        bool isSelecteds = (selecteds == "true") ? true : false;
                        bool isExpandeds = (expandeds == "true") ? true : false;

                        ServiceTypes stp;
                        switch (tp)
                        {
                            case "AtomPub":
                                stp = ServiceTypes.AtomPub;
                                break;
                            case "AtomPub_Hatena":
                                stp = ServiceTypes.AtomPub_Hatena;
                                break;
                            case "Feed":
                                stp = ServiceTypes.Feed;
                                break;
                            case "XML-RPC_MovableType":
                                stp = ServiceTypes.XmlRpc_MovableType;
                                break;
                            case "XML-RPC_WordPress":
                                stp = ServiceTypes.XmlRpc_WordPress;
                                break;
                            // other
                            default:
                                stp = ServiceTypes.Unknown;
                                break;
                        }

                        ApiTypes at;
                        switch (api)
                        {
                            case "AtomPub":
                                at = ApiTypes.atAtomPub;
                                break;
                            case "AtomFeed":
                                at = ApiTypes.atAtomFeed;
                                break;
                            case "RssFeed":
                                at = ApiTypes.atRssFeed;
                                break;
                            //case "XML-RPC":
                            //    at = ApiTypes.atXMLRPC_MovableType;
                            //    break;
                            case "XML-RPC_MovableType":
                                at = ApiTypes.atXMLRPC_MovableType;
                                break;
                            case "XML-RPC_WordPress":
                                at = ApiTypes.atXMLRPC_WordPress;
                                break;
                            //case "AtomAPI":
                            //    at = ApiTypes.atAtomAPI;
                            //    break;
                            default:
                                at = ApiTypes.atUnknown;
                                break;
                        }

                        if (stp == ServiceTypes.Feed)
                        {
                            /*
                            if ((!string.IsNullOrEmpty(accountName)) && (!string.IsNullOrEmpty(endpoint)))
                            {
                                if (at == ApiTypes.atAtomFeed)
                                {
                                    NodeAtomFeed account = new NodeAtomFeed(accountName, new Uri(endpoint));

                                    account.IsSelected = isSelecteds;
                                    account.IsExpanded = isExpandeds;
                                    account.Parent = this;

                                    account.ServiceType = ServiceTypes.Feed;
                                    account.Api = at;

                                    this.Children.Add(account);
                                }
                                else if (at == ApiTypes.atRssFeed)
                                {
                                    NodeRssFeed account = new NodeRssFeed(accountName, new Uri(endpoint));
                                    account.IsSelected = isSelecteds;
                                    account.IsExpanded = isExpandeds;
                                    account.Parent = this;

                                    account.ServiceType = ServiceTypes.Feed;
                                    account.Api = at;

                                    this.Children.Add(account);
                                }
                            }
                            */
                            continue;
                        }

                        if ((!string.IsNullOrEmpty(accountName)) && (!string.IsNullOrEmpty(userName)) && (!string.IsNullOrEmpty(userPassword)) && (!string.IsNullOrEmpty(endpoint)))
                        {
                            NodeService account = new NodeService(accountName, userName, userPassword, new Uri(endpoint), at, stp);
                            account.IsSelected = isSelecteds;
                            account.IsExpanded = isExpandeds;
                            account.Parent = this;

                            account.ServiceType = stp;
                            account.Api = at;

                            XmlNodeList workspaceList = s.SelectNodes("Workspaces");
                            foreach (XmlNode w in workspaceList)
                            {
                                var workspaceName = w.Attributes["Name"].Value;
                                var selectedw = string.IsNullOrEmpty(w.Attributes["Selected"].Value) ? "" : w.Attributes["Selected"].Value;
                                var expandedw = string.IsNullOrEmpty(w.Attributes["Expanded"].Value) ? "" : w.Attributes["Expanded"].Value;
                                bool isSelectedw = (selectedw == "true") ? true : false;
                                bool isExpandedw = (expandedw == "true") ? true : false;

                                if (!string.IsNullOrEmpty(workspaceName))
                                {

                                    NodeWorkspace blog = new NodeWorkspace(workspaceName);
                                    blog.IsSelected = isSelectedw;
                                    blog.IsExpanded = isExpandedw;
                                    blog.Parent = account;

                                    XmlNodeList collectionList = w.SelectNodes("Collection");
                                    foreach (XmlNode c in collectionList)
                                    {
                                        var collectionName = c.Attributes["Name"].Value;
                                        var selectedc = string.IsNullOrEmpty(c.Attributes["Selected"].Value) ? "" : c.Attributes["Selected"].Value;
                                        var expandedc = string.IsNullOrEmpty(c.Attributes["Expanded"].Value) ? "" : c.Attributes["Expanded"].Value;
                                        bool isSelectedc = (selectedc == "true") ? true : false;
                                        bool isExpandedc = (expandedc == "true") ? true : false;

                                        string collectionHref = (c.Attributes["Href"] != null) ? c.Attributes["Href"].Value : "";

                                        if ((!string.IsNullOrEmpty(collectionName)) && (!string.IsNullOrEmpty(collectionHref)))
                                        {
                                            NodeEntryCollection entry = null;

                                            switch ((blog.Parent as NodeService).Api)
                                            {
                                                case ApiTypes.atAtomPub:
                                                    entry = new NodeAtomPubEntryCollection(collectionName, new Uri(collectionHref));
                                                    break;
                                                //case ApiTypes.atXMLRPC:
                                                //    break;
                                                case ApiTypes.atXMLRPC_MovableType:
                                                    entry = new NodeXmlRpcMTEntryCollection(collectionName, new Uri(collectionHref));
                                                    break;
                                                case ApiTypes.atXMLRPC_WordPress:
                                                    entry = new NodeXmlRpcWPEntryCollection(collectionName, new Uri(collectionHref));
                                                    break;
                                                    //case ApiTypes.atAtomAPI:
                                                    //    break;
                                            }

                                            if (entry == null)
                                                continue;

                                            //TODO:
                                            //AcceptTypeps, CategoriesUri 


                                            entry.IsSelected = isSelectedc;
                                            entry.IsExpanded = isExpandedc;
                                            entry.Parent = blog;


                                            XmlNodeList categoryList = c.SelectNodes("Category");
                                            foreach (XmlNode t in categoryList)
                                            {
                                                var categoryName = t.Attributes["Name"].Value;
                                                var selectedt = string.IsNullOrEmpty(t.Attributes["Selected"].Value) ? "" : t.Attributes["Selected"].Value;
                                                var expandedt = string.IsNullOrEmpty(t.Attributes["Expanded"].Value) ? "" : t.Attributes["Expanded"].Value;
                                                bool isSelectedt = (selectedc == "true") ? true : false;
                                                bool isExpandedt = (expandedc == "true") ? true : false;

                                                if (!string.IsNullOrEmpty(categoryName))
                                                {

                                                    NodeCategory category = null;

                                                    switch ((blog.Parent as NodeService).Api)
                                                    {
                                                        case ApiTypes.atAtomPub:
                                                            category = new NodeAtomPubCategory(categoryName);
                                                            break;
                                                        //case ApiTypes.atXMLRPC:
                                                        //    break;
                                                        case ApiTypes.atXMLRPC_MovableType:
                                                            category = new NodeXmlRpcMTCategory(categoryName);
                                                            break;
                                                        case ApiTypes.atXMLRPC_WordPress:
                                                            category = new NodeXmlRpcWPCategory(categoryName);
                                                            break;
                                                            //case ApiTypes.atAtomAPI:
                                                            //    break;
                                                    }

                                                    if (category == null)
                                                        return;

                                                    //


                                                    category.IsSelected = isSelectedc;
                                                    category.IsExpanded = isExpandedc;
                                                    category.Parent = entry;

                                                    entry.Children.Add(category);
                                                }
                                            }

                                            blog.Children.Add(entry);
                                        }

                                    }

                                    account.Children.Add(blog);
                                }

                            }

                            this.Children.Add(account);
                        }
                    }
                    else if (s.LocalName.Equals("Feed"))
                    {
                        NodeFeed feed = LoadXmlChildFeed(s);

                        if (feed != null)
                            this.Children.Add(feed);

                    }
                    else if (s.LocalName.Equals("Folder"))
                    {
                        var folderName = s.Attributes["Name"].Value;

                        if (!string.IsNullOrEmpty(folderName))
                        {
                            var selecteds = string.IsNullOrEmpty(s.Attributes["Selected"].Value) ? "" : s.Attributes["Selected"].Value;
                            var expandeds = string.IsNullOrEmpty(s.Attributes["Expanded"].Value) ? "" : s.Attributes["Expanded"].Value;
                            bool isSelecteds = (selecteds == "true") ? true : false;
                            bool isExpandeds = (expandeds == "true") ? true : false;

                            NodeFolder folder = new NodeFolder(folderName);
                            folder.IsSelected = isSelecteds;
                            folder.IsExpanded = isExpandeds;
                            folder.Parent = this;


                            XmlNodeList feedList = s.SelectNodes("Feed");
                            foreach (XmlNode f in feedList)
                            {
                                NodeFeed feed = LoadXmlChildFeed(f);

                                if (feed != null)
                                    folder.Children.Add(feed);

                            }

                            this.Children.Add(folder);
                        }
                    }

                }

                break;
            }
        }

        private NodeFeed LoadXmlChildFeed(XmlNode node)
        {
            var feedName = node.Attributes["Name"].Value;

            if (!string.IsNullOrEmpty(feedName))
            {
                var selecteds = string.IsNullOrEmpty(node.Attributes["Selected"].Value) ? "" : node.Attributes["Selected"].Value;
                var expandeds = string.IsNullOrEmpty(node.Attributes["Expanded"].Value) ? "" : node.Attributes["Expanded"].Value;
                bool isSelectedf = (selecteds == "true") ? true : false;
                bool isExpandedf = (expandeds == "true") ? true : false;

                var endpoint = node.Attributes["EndPoint"].Value;
                string api = (node.Attributes["Api"] != null) ? node.Attributes["Api"].Value : "Unknown";

                string siteTitle = "";
                var attr = node.Attributes["SiteTitle"];
                if (attr != null)
                {
                    siteTitle = string.IsNullOrEmpty(node.Attributes["SiteTitle"].Value) ? "" : node.Attributes["SiteTitle"].Value;
                }

                Uri siteUri = null;
                attr = node.Attributes["SiteUri"];
                if (attr != null)
                {
                    var siteLink = string.IsNullOrEmpty(node.Attributes["SiteUri"].Value) ? "" : node.Attributes["SiteUri"].Value;
                    
                    if (!string.IsNullOrEmpty(siteLink))
                    {
                        try
                        {
                            siteUri = new Uri(siteLink);
                        }
                        catch { }
                    }
                }


                ApiTypes at;
                switch (api)
                {
                    case "AtomFeed":
                        at = ApiTypes.atAtomFeed;
                        break;
                    case "RssFeed":
                        at = ApiTypes.atRssFeed;
                        break;
                    default:
                        at = ApiTypes.atUnknown;
                        break;
                }

                if (!string.IsNullOrEmpty(endpoint))
                {
                    if (at == ApiTypes.atAtomFeed)
                    {
                        NodeAtomFeed feed = new NodeAtomFeed(feedName, new Uri(endpoint));

                        feed.IsSelected = isSelectedf;
                        feed.IsExpanded = isExpandedf;
                        feed.Parent = this;

                        feed.ServiceType = ServiceTypes.Feed;
                        feed.Api = at;

                        feed.SiteTitle = siteTitle;
                        feed.SiteUri = siteUri;

                        //this.Children.Add(feed);
                        return feed;
                    }
                    else if (at == ApiTypes.atRssFeed)
                    {
                        NodeRssFeed feed = new NodeRssFeed(feedName, new Uri(endpoint));
                        feed.IsSelected = isSelectedf;
                        feed.IsExpanded = isExpandedf;
                        feed.Parent = this;

                        feed.ServiceType = ServiceTypes.Feed;
                        feed.Api = at;

                        feed.SiteTitle = siteTitle;
                        feed.SiteUri = siteUri;

                        //this.Children.Add(feed);
                        return feed;
                    }
                }


            }

            return null;
        }

        public XmlDocument AsXmlDoc()
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);

            XmlElement root = doc.CreateElement(string.Empty, "Accounts", string.Empty);
            doc.AppendChild(root);

            foreach (var s in this.Children)
            {
                if (s is NodeService)
                {
                    if (s is NodeFeed)
                    {
                        XmlElement feed = AsXmlFeedElement(doc, (s as NodeFeed));

                        root.AppendChild(feed);
                    }
                    else 
                    {
                        XmlElement service = doc.CreateElement(string.Empty, "Service", string.Empty);

                        XmlAttribute attrs = doc.CreateAttribute("Name");
                        attrs.Value = (s).Name;
                        service.SetAttributeNode(attrs);

                        XmlAttribute attrsd = doc.CreateAttribute("Expanded");
                        attrsd.Value = (s).IsExpanded ? "true" : "false";
                        service.SetAttributeNode(attrsd);

                        XmlAttribute attrss = doc.CreateAttribute("Selected");
                        attrss.Value = (s).IsSelected ? "true" : "false";
                        service.SetAttributeNode(attrss);

                        XmlAttribute attrsn = doc.CreateAttribute("UserName");
                        attrsn.Value = ((NodeService)s).UserName;
                        service.SetAttributeNode(attrsn);

                        XmlAttribute attrsp = doc.CreateAttribute("UserPassword");
                        attrsp.Value = ((NodeService)s).UserPassword;
                        service.SetAttributeNode(attrsp);

                        XmlAttribute attrse = doc.CreateAttribute("EndPoint");
                        attrse.Value = ((NodeService)s).EndPoint.AbsoluteUri;
                        service.SetAttributeNode(attrse);

                        XmlAttribute atstp = doc.CreateAttribute("Type");
                        switch (((NodeService)s).ServiceType)
                        {
                            case ServiceTypes.AtomPub:
                                atstp.Value = "AtomPub";
                                service.SetAttributeNode(atstp);
                                break;
                            case ServiceTypes.AtomPub_Hatena:
                                atstp.Value = "AtomPub_Hatena";
                                service.SetAttributeNode(atstp);
                                break;
                            case ServiceTypes.Feed:
                                atstp.Value = "Feed";
                                service.SetAttributeNode(atstp);
                                break;
                            case ServiceTypes.XmlRpc_MovableType:
                                atstp.Value = "XML-RPC_MovableType";
                                service.SetAttributeNode(atstp);
                                break;
                            case ServiceTypes.XmlRpc_WordPress:
                                atstp.Value = "XML-RPC_WordPress";
                                service.SetAttributeNode(atstp);
                                break;
                            // other...
                            case ServiceTypes.Unknown:
                                atstp.Value = "Unknown";
                                service.SetAttributeNode(atstp);
                                break;

                        }

                        XmlAttribute atapi = doc.CreateAttribute("Api");
                        switch (((NodeService)s).Api)
                        {
                            case ApiTypes.atAtomPub:
                                atapi.Value = "AtomPub";
                                service.SetAttributeNode(atapi);
                                break;
                            case ApiTypes.atAtomFeed:
                                atapi.Value = "AtomFeed";
                                service.SetAttributeNode(atapi);
                                break;
                            case ApiTypes.atRssFeed:
                                atapi.Value = "RssFeed";
                                service.SetAttributeNode(atapi);
                                break;
                            //case ApiTypes.atXMLRPC:
                            //    atapi.Value = "XML-RPC";
                            //    service.SetAttributeNode(atapi);
                            //    break;
                            case ApiTypes.atXMLRPC_MovableType:
                                atapi.Value = "XML-RPC_MovableType";
                                service.SetAttributeNode(atapi);
                                break;
                            case ApiTypes.atXMLRPC_WordPress:
                                atapi.Value = "XML-RPC_WordPress";
                                service.SetAttributeNode(atapi);
                                break;
                            case ApiTypes.atUnknown:
                                atapi.Value = "Unknown";
                                service.SetAttributeNode(atapi);
                                break;
                                //case ApiTypes.atAtomAPI:
                                //    atapi.Value = "AtomAPI";
                                //    service.SetAttributeNode(atapi);
                                //    break;
                        }

                        root.AppendChild(service);

                        foreach (var w in s.Children)
                        {
                            if (!(w is NodeWorkspace)) continue;

                            XmlElement workspace = doc.CreateElement(string.Empty, "Workspaces", string.Empty);

                            XmlAttribute attrw = doc.CreateAttribute("Name");
                            attrw.Value = (w).Name;
                            workspace.SetAttributeNode(attrw);

                            XmlAttribute attwd = doc.CreateAttribute("Expanded");
                            attwd.Value = (w).IsExpanded ? "true" : "false";
                            workspace.SetAttributeNode(attwd);

                            XmlAttribute attwp = doc.CreateAttribute("Selected");
                            attwp.Value = (w).IsSelected ? "true" : "false";
                            workspace.SetAttributeNode(attwp);

                            service.AppendChild(workspace);

                            foreach (var c in w.Children)
                            {
                                if (!(c is NodeEntryCollection)) continue;

                                XmlElement collection = doc.CreateElement(string.Empty, "Collection", string.Empty);

                                XmlAttribute attrc = doc.CreateAttribute("Name");
                                attrc.Value = (c).Name;
                                collection.SetAttributeNode(attrc);

                                XmlAttribute attrcd = doc.CreateAttribute("Expanded");
                                attrcd.Value = (c).IsExpanded ? "true" : "false";
                                collection.SetAttributeNode(attrcd);

                                XmlAttribute attrcs = doc.CreateAttribute("Selected");
                                attrcs.Value = (c).IsSelected ? "true" : "false";
                                collection.SetAttributeNode(attrcs);

                                XmlAttribute attrch = doc.CreateAttribute("Href");
                                attrch.Value = ((NodeEntryCollection)c).Uri.AbsoluteUri;
                                collection.SetAttributeNode(attrch);


                                workspace.AppendChild(collection);

                                if (c is NodeAtomPubEntryCollection)
                                {
                                    foreach (var a in (c as NodeAtomPubEntryCollection).AcceptTypes)
                                    {
                                        XmlElement acceptType = doc.CreateElement(string.Empty, "Accept", string.Empty);
                                        XmlText xt = doc.CreateTextNode(a);
                                        acceptType.AppendChild(xt);

                                        collection.AppendChild(acceptType);
                                    }
                                }

                                foreach (var t in (c as NodeEntryCollection).Children)
                                {
                                    if (!(t is NodeCategory)) continue;

                                    XmlElement category = doc.CreateElement(string.Empty, "Category", string.Empty);

                                    XmlAttribute attrt = doc.CreateAttribute("Name");
                                    attrt.Value = (t).Name;
                                    category.SetAttributeNode(attrt);

                                    XmlAttribute attrtd = doc.CreateAttribute("Expanded");
                                    attrtd.Value = (t).IsExpanded ? "true" : "false";
                                    category.SetAttributeNode(attrtd);

                                    XmlAttribute attrts = doc.CreateAttribute("Selected");
                                    attrts.Value = (t).IsSelected ? "true" : "false";
                                    category.SetAttributeNode(attrts);

                                    collection.AppendChild(category);

                                    if (t is NodeAtomPubCategory)
                                    {
                                        //
                                        //XmlAttribute attrtt = doc.CreateAttribute("Term");
                                        //attrtt.Value = ((NodeCategory)t).Term;
                                        //category.SetAttributeNode(attrtt);
                                    }
                                    else if (t is NodeXmlRpcMTCategory)
                                    {
                                        //

                                    }
                                    else if (t is NodeXmlRpcWPCategory)
                                    {
                                        //

                                    }


                                }


                            }
                        }
                    }
                }
                else if (s is NodeFolder)
                {
                    XmlElement folder = doc.CreateElement(string.Empty, "Folder", string.Empty);

                    XmlAttribute attrd = doc.CreateAttribute("Name");
                    attrd.Value = (s).Name;
                    folder.SetAttributeNode(attrd);

                    attrd = doc.CreateAttribute("Expanded");
                    attrd.Value = (s).IsExpanded ? "true" : "false";
                    folder.SetAttributeNode(attrd);

                    attrd = doc.CreateAttribute("Selected");
                    attrd.Value = (s).IsSelected ? "true" : "false";
                    folder.SetAttributeNode(attrd);

                    root.AppendChild(folder);

                    foreach (var fd in s.Children)
                    {
                        if (!(fd is NodeFeed)) continue;

                        XmlElement feed = AsXmlFeedElement(doc, (fd as NodeFeed));

                        folder.AppendChild(feed);
                    }
                }

            }

            //System.Diagnostics.Debug.WriteLine(doc.OuterXml);

            return doc;
        }

        private XmlElement AsXmlFeedElement(XmlDocument doc, NodeFeed fd)
        {
            XmlElement feed = doc.CreateElement(string.Empty, "Feed", string.Empty);

            XmlAttribute attrf = doc.CreateAttribute("Name");
            attrf.Value = (fd).Name;
            feed.SetAttributeNode(attrf);

            attrf = doc.CreateAttribute("Expanded");
            attrf.Value = (fd).IsExpanded ? "true" : "false";
            feed.SetAttributeNode(attrf);

            attrf = doc.CreateAttribute("Selected");
            attrf.Value = (fd).IsSelected ? "true" : "false";
            feed.SetAttributeNode(attrf);

            attrf = doc.CreateAttribute("EndPoint");
            attrf.Value = (fd as NodeFeed).EndPoint.AbsoluteUri;
            feed.SetAttributeNode(attrf);

            attrf = doc.CreateAttribute("Type");
            attrf.Value = "Feed";
            feed.SetAttributeNode(attrf);

            attrf = doc.CreateAttribute("Api");
            switch ((fd as NodeFeed).Api)
            {
                case ApiTypes.atAtomFeed:
                    attrf.Value = "AtomFeed";
                    feed.SetAttributeNode(attrf);
                    break;
                case ApiTypes.atRssFeed:
                    attrf.Value = "RssFeed";
                    feed.SetAttributeNode(attrf);
                    break;
                case ApiTypes.atUnknown:
                    attrf.Value = "Unknown";
                    feed.SetAttributeNode(attrf);
                    break;
            }

            attrf = doc.CreateAttribute("SiteTitle");
            attrf.Value = (fd as NodeFeed).SiteTitle;
            feed.SetAttributeNode(attrf);

            attrf = doc.CreateAttribute("SiteUri");
            if ((fd as NodeFeed).SiteUri != null)
                attrf.Value = (fd as NodeFeed).SiteUri.AbsoluteUri;
            feed.SetAttributeNode(attrf);

            return feed;
        }
    }
}
