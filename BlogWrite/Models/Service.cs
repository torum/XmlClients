/// 
/// 
/// BlogWrite 
///  - C#/WPF port of the original "BlogWrite" developed with Delphi.
/// https://github.com/torum/BlogWrite
/// 
/// 

using BlogWrite.Models.Clients;
using System;
using System.Xml;

namespace BlogWrite.Models
{

    public class NodeService : NodeTree
    {
        public Uri EndPoint {get;set;}
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public ApiTypes Api { get; set; }

        public enum ApiTypes
        {
            atAtomPub,
            atXMLRPC,
            atAtomAPI
        }

        public BlogClient Client { get; }
        public string ID { get; }

        public NodeService(string name, string username, string password, Uri endPoint, ApiTypes api) : base(name)
        {
            UserName = username;
            UserPassword = password;
            EndPoint = endPoint;
            PathIcon = "M12,19.2C9.5,19.2 7.29,17.92 6,16C6.03,14 10,12.9 12,12.9C14,12.9 17.97,14 18,16C16.71,17.92 14.5,19.2 12,19.2M12,5A3,3 0 0,1 15,8A3,3 0 0,1 12,11A3,3 0 0,1 9,8A3,3 0 0,1 12,5M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12C22,6.47 17.5,2 12,2Z";

            switch (api)
            {
                case ApiTypes.atAtomPub:
                    Client = new AtomPubClient(UserName, UserPassword, EndPoint);
                    break;
                case ApiTypes.atXMLRPC:
                    Client = new XmlRpcClient(UserName, UserPassword, EndPoint);
                    break;
            }
            Api = api;

            ID = Guid.NewGuid().ToString();
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
            if (accountList == null) return;

            foreach (XmlNode a in accountList)
            {
                XmlNodeList serviceList = a.SelectNodes("Service");
                if (serviceList == null)
                    return;

                foreach (XmlNode s in serviceList)
                {
                    var accountName = s.Attributes["Name"].Value;
                    var userName = s.Attributes["UserName"].Value;
                    var userPassword = s.Attributes["UserPassword"].Value;
                    var endpoint = s.Attributes["EndPoint"].Value;
                    string api = (s.Attributes["Api"] != null) ? s.Attributes["Api"].Value : "Atom";

                    var selecteds = string.IsNullOrEmpty(s.Attributes["Selected"].Value) ? "" : s.Attributes["Selected"].Value;
                    var expandeds = string.IsNullOrEmpty(s.Attributes["Expanded"].Value) ? "" : s.Attributes["Expanded"].Value;
                    bool isSelecteds = (selecteds == "true") ? true : false;
                    bool isExpandeds = (expandeds == "true") ? true : false;

                    if ((!string.IsNullOrEmpty(accountName))
                        && (!string.IsNullOrEmpty(userName))
                        && (!string.IsNullOrEmpty(userPassword))
                        && (!string.IsNullOrEmpty(endpoint)))
                    {
                        NodeService.ApiTypes at;
                        switch (api)
                        {
                            case "Atom":
                                at = NodeService.ApiTypes.atAtomPub;
                                break;
                            case "XML-RPC":
                                at = NodeService.ApiTypes.atXMLRPC;
                                break;
                            case "AtomAPI":
                                at = NodeService.ApiTypes.atAtomAPI;
                                break;
                            default:
                                at = NodeService.ApiTypes.atAtomPub;
                                break;
                        }

                        NodeService account = new NodeService(accountName, userName, userPassword, new Uri(endpoint), at);
                        account.Selected = isSelecteds;
                        account.Expanded = isExpandeds;
                        account.Parent = null;

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

                                NodeCollection blog = new NodeCollection(workspaceName);
                                blog.Selected = isSelectedw;
                                blog.Expanded = isExpandedw;
                                blog.Parent = account;

                                XmlNodeList collectionList = w.SelectNodes("Collection");
                                foreach (XmlNode c in collectionList)
                                {
                                    var collectionName = c.Attributes["Name"].Value;
                                    var collectionHref = c.Attributes["Href"].Value;
                                    var selectedc = string.IsNullOrEmpty(c.Attributes["Selected"].Value) ? "" : c.Attributes["Selected"].Value;
                                    var expandedc = string.IsNullOrEmpty(c.Attributes["Expanded"].Value) ? "" : c.Attributes["Expanded"].Value;
                                    bool isSelectedc = (selectedc == "true") ? true : false;
                                    bool isExpandedc = (expandedc == "true") ? true : false;
                                    if ((!string.IsNullOrEmpty(collectionName)) && (!string.IsNullOrEmpty(collectionHref)))
                                    {
                                        NodeEntry entry = new NodeEntry(collectionName, new Uri(collectionHref));
                                        entry.Selected = isSelectedc;
                                        entry.Expanded = isExpandedc;
                                        entry.Parent = blog;

                                        blog.Children.Add(entry);
                                    }

                                }

                                account.Children.Add(blog);
                            }

                        }

                        this.Children.Add(account);
                    }

                }
            }

        }

        public XmlDocument AsXmlDoc()
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);

            XmlElement root = doc.CreateElement(string.Empty, "Accounts", string.Empty);
            doc.AppendChild(root);

            foreach (var s in this.Children)
            {
                if (!(s is NodeService)) continue;

                XmlElement service = doc.CreateElement(string.Empty, "Service", string.Empty);

                XmlAttribute attrs = doc.CreateAttribute("Name");
                attrs.Value = (s).Name;
                service.SetAttributeNode(attrs);

                XmlAttribute attrsd = doc.CreateAttribute("Expanded");
                attrsd.Value = (s).Expanded ? "true" : "false";
                service.SetAttributeNode(attrsd);

                XmlAttribute attrss = doc.CreateAttribute("Selected");
                attrss.Value = (s).Selected ? "true" : "false";
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

                XmlAttribute atapi = doc.CreateAttribute("Api");
                switch (((NodeService)s).Api)
                {
                    case NodeService.ApiTypes.atAtomPub:
                        atapi.Value = "Atom";
                        service.SetAttributeNode(atapi);
                        break;
                    case NodeService.ApiTypes.atXMLRPC:
                        atapi.Value = "XML-RPC";
                        service.SetAttributeNode(atapi);
                        break;
                    case NodeService.ApiTypes.atAtomAPI:
                        atapi.Value = "AtomAPI";
                        service.SetAttributeNode(atapi);
                        break;
                }

                root.AppendChild(service);

                foreach (var w in s.Children)
                {
                    if (!(w is NodeCollection)) continue;

                    XmlElement workspace = doc.CreateElement(string.Empty, "Workspaces", string.Empty);

                    XmlAttribute attrw = doc.CreateAttribute("Name");
                    attrw.Value = (w).Name;
                    workspace.SetAttributeNode(attrw);

                    XmlAttribute attwd = doc.CreateAttribute("Expanded");
                    attwd.Value = (w).Expanded ? "true" : "false";
                    workspace.SetAttributeNode(attwd);

                    XmlAttribute attwp = doc.CreateAttribute("Selected");
                    attwp.Value = (w).Selected ? "true" : "false";
                    workspace.SetAttributeNode(attwp);

                    service.AppendChild(workspace);

                    foreach (var c in w.Children)
                    {
                        if (!(c is NodeEntry)) continue;

                        XmlElement collection = doc.CreateElement(string.Empty, "Collection", string.Empty);

                        XmlAttribute attrc = doc.CreateAttribute("Name");
                        attrc.Value = (c).Name;
                        collection.SetAttributeNode(attrc);

                        XmlAttribute attrcd = doc.CreateAttribute("Expanded");
                        attrcd.Value = (c).Expanded ? "true" : "false";
                        collection.SetAttributeNode(attrcd);

                        XmlAttribute attrcs = doc.CreateAttribute("Selected");
                        attrcs.Value = (c).Selected ? "true" : "false";
                        collection.SetAttributeNode(attrcs);

                        XmlAttribute attrch = doc.CreateAttribute("Href");
                        attrch.Value = ((NodeEntry)c).Uri.AbsoluteUri;
                        collection.SetAttributeNode(attrch);

                        workspace.AppendChild(collection);

                        foreach (var a in (c as NodeEntry).AcceptTypes)
                        {
                            XmlElement acceptType = doc.CreateElement(string.Empty, "Accept", string.Empty);
                            XmlText xt = doc.CreateTextNode(a);
                            acceptType.AppendChild(xt);

                            collection.AppendChild(acceptType);
                        }

                    }
                }
            }

            //System.Diagnostics.Debug.WriteLine(doc.OuterXml);

            return doc;

        }
    }

}
