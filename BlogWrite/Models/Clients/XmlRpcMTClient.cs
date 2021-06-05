/// 
/// 
/// BlogWrite 
///  - C#/WPF port of the original "BlogWrite" developed with Delphi.
/// https://github.com/torum/BlogWrite
/// 
///  XML-RPC Movable Type API, MetaWeblog API, Blogger API
///  https://codex.wordpress.org/XML-RPC_MovableType_API
///  https://codex.wordpress.org/XML-RPC_MetaWeblog_API
///  https://codex.wordpress.org/XML-RPC_Blogger_API
///  
///  TODO:
///  Categories.
///  Fault code.
///  

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;

namespace BlogWrite.Models.Clients
{
    /// <summary>
    /// XmlRpcClient 
    /// Implements: 
    ///   XML-RPC Movable Type API, MetaWeblog API, Blogger API 
    /// Uses following methods:
    ///  metaWeblog.getUsersBlogs
    ///  metaWeblog.getRecentPosts
    ///  metaWeblog.getPost
    ///  metaWeblog.newPost
    ///  metaWeblog.editPost
    ///  metaWeblog.deletePost
    ///  metaWeblog.getCategories
    ///  metaWeblog.newMediaObject
    ///  mt.getRecentPostTitles
    ///  mt.supportedMethods
    ///  mt.supportedTextFilters
    ///  mt.publishPost
    ///  mt.getCategoryList
    ///  mt.getPostCategories
    ///  mt.setPostCategories
    /// </summary>
    class XmlRpcMTClient : BlogClient
    {
        public XmlRpcMTClient(string userName, string userPassword, Uri endpoint) : base(userName, userPassword, endpoint)
        {

        }

        public override async Task<NodeService> GetAccount(string accountName)
        {
            NodeService account = new NodeService(accountName, _userName, _userPassword, _endpoint, ApiTypes.atXMLRPC_MovableType, ServiceTypes.XmlRpc_MovableType);

            NodeWorkspaces blogs = await GetBlogs();

            foreach (var item in blogs.Children)
            {
                item.Parent = account;
                account.Children.Add(item);
            }

            account.Expanded = true;

            return account;
        }

        public override async Task<NodeWorkspaces> GetBlogs()
        {
            NodeWorkspaces blogs = new NodeWorkspaces();

            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = xdoc.CreateXmlDeclaration("1.0", "UTF-8", null);

            XmlElement objRootNode, objMethodNode, objParamsNode, objParamNode, objValueNode, objTypeNode;
            XmlText xt;

            objRootNode = xdoc.CreateElement(string.Empty, "methodCall", string.Empty);
            xdoc.AppendChild(objRootNode);

                objMethodNode = xdoc.CreateElement(string.Empty, "methodName", string.Empty);
                    xt = xdoc.CreateTextNode("metaWeblog.getUsersBlogs"); // blogger.getUsersBlogs
                    objMethodNode.AppendChild(xt);
                    objRootNode.AppendChild(objMethodNode);

                objParamsNode = xdoc.CreateElement(string.Empty, "params", string.Empty);

                    objParamNode = xdoc.CreateElement(string.Empty, "param", string.Empty);
                    objParamsNode.AppendChild(objParamNode);

                        objValueNode = xdoc.CreateElement(string.Empty, "value", string.Empty);
                        objParamNode.AppendChild(objValueNode);

                            objTypeNode = xdoc.CreateElement(string.Empty, "string", string.Empty);
                            objValueNode.AppendChild(objTypeNode);

                    objParamNode = xdoc.CreateElement(string.Empty, "param", string.Empty);
                    objParamsNode.AppendChild(objParamNode);

                        objValueNode = xdoc.CreateElement(string.Empty, "value", string.Empty);
                        objParamNode.AppendChild(objValueNode);

                            objTypeNode = xdoc.CreateElement(string.Empty, "string", string.Empty);
                            xt = xdoc.CreateTextNode(_userName); 
                            objTypeNode.AppendChild(xt);
                            objValueNode.AppendChild(objTypeNode);

                    objParamNode = xdoc.CreateElement(string.Empty, "param", string.Empty);
                    objParamsNode.AppendChild(objParamNode);

                        objValueNode = xdoc.CreateElement(string.Empty, "value", string.Empty);
                        objParamNode.AppendChild(objValueNode);

                            objTypeNode = xdoc.CreateElement(string.Empty, "string", string.Empty);
                            xt = xdoc.CreateTextNode(_userPassword); 
                            objTypeNode.AppendChild(xt);
                            objValueNode.AppendChild(objTypeNode);

            objRootNode.AppendChild(objParamsNode);

            //System.Diagnostics.Debug.WriteLine("GET blogs(getUsersBlogs): " + AsXml(xdoc));

            ToDebugWindow(">> HTTP Request POST "
                //+ Environment.NewLine
                + _endpoint.AbsoluteUri
                + Environment.NewLine
                + AsUTF16Xml(xdoc)
                + Environment.NewLine);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = _endpoint,
                Content = new StringContent(AsUTF8Xml(xdoc), Encoding.UTF8, "text/xml")
            };

            var response = await _HTTPConn.Client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string contenTypeString = response.Content.Headers.GetValues("Content-Type").FirstOrDefault();

                if (!contenTypeString.StartsWith("text/xml"))
                {
                    System.Diagnostics.Debug.WriteLine("Content-Type is invalid: " + contenTypeString);

                    ToDebugWindow("<< HTTP Response Content-Type is invalid: " + contenTypeString
                        + Environment.NewLine
                        + "expecting " + "text/xml"
                        + Environment.NewLine);

                    return blogs;
                }

                string s = await response.Content.ReadAsStringAsync();

                ToDebugWindow("<< HTTP Response " + response.StatusCode.ToString()
                + Environment.NewLine
                + s 
                + Environment.NewLine);

                try
                {
                    xdoc.LoadXml(s);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("LoadXml failed: " + e.Message);

                    ToDebugWindow("<< Invalid XML returned:"
                        + Environment.NewLine
                        + e.Message
                        + Environment.NewLine);

                    return blogs;
                }


                // TODO: Parse error response.


                // //methodResponse/fault
                // or
                // //methodResponse/params

                /*

    // fault response   
    <methodResponse>
        <fault>
            <value>
                <struct>
                    <member>
                        <name>faultCode</name>
                        <value>
                            <int>400</int>
                        </value>
                    </member>
                    <member>
                        <name>faultString</name>
                        <value>
                            <string>Insufficient arguments passed to this XML-RPC method.</string>
                        </value>
                    </member>
                </struct>
            </value>
        </fault>
    </methodResponse>

                */

                XmlNodeList blogList;
                blogList = xdoc.SelectNodes("//methodResponse/params/param/value/array/data/value");
                if (blogList == null)
                    return blogs;

                foreach (XmlNode b in blogList)
                {
                    NodeWorkspace blog = new NodeWorkspace("Blog");

                    XmlNodeList memberList = b.SelectNodes("struct/member");
                    if (memberList == null)
                        continue;

                    NodeEntries entries = new NodeEntries();
                    bool isAdmin = false;
                    bool isPrimary = false;
                    Uri url = null;
                    string blogid = "";
                    string blogName = "";
                    Uri xmlrpc = null;

                    foreach (XmlNode m in memberList)
                    {

                        XmlNodeList valueList = m.ChildNodes;
                        string name = "";
                        string value = "";
                        foreach (XmlNode v in valueList)
                        {
                            if (v.Name == "name")
                            {
                                name = v.InnerText;
                            }
                            if (v.Name == "value")
                            {
                                value = v.InnerText;
                            }
                        }
                        
                        if (name == "isAdmin")
                        {
                            if (value != "0")
                            {
                                isAdmin = true;
                            }
                        }

                        if (name == "isPrimary")
                        {
                            if (value != "0")
                            {
                                isPrimary = true;
                            }
                        }

                        if (name == "url")
                        {
                            try
                            {
                                url = new Uri(value);
                            }
                            catch { }
                        }

                        if (name == "blogid")
                        {
                            blogid = value;
                        }

                        if (name == "blogName")
                        {
                            blogName = value;
                        }

                        if (name == "xmlrpc")
                        {
                            try
                            {
                                xmlrpc = new Uri(value);
                            }
                            catch { }
                        }

                    }

                    //System.Diagnostics.Debug.WriteLine("blogName: " + blogName);

                    if (!string.IsNullOrEmpty(blogName))
                    {
                        if (xmlrpc == null)
                        {
                            xmlrpc = _endpoint;
                        }

                        NodeXmlRpcMTEntryCollection entry = new NodeXmlRpcMTEntryCollection(blogName, xmlrpc);

                        // TODO: blogid, isAdmin, etc


                        // Categories
                        List<NodeCategory> cats = await GetCategiries(xmlrpc, blogid);

                        foreach (NodeCategory c in cats)
                        {
                            entry.Children.Add(c);
                        }

                        //blogid, blogName are required. 
                        //url is optional.

                        // wp has isAdmin,
                        // for multisite, xmlrpc, isPrimary.

                        //make sure compare them case insensitive.

                        entry.Parent = blog;
                        blog.Children.Add(entry);

                    }

                    blog.Expanded = true;

                    blogs.Children.Add(blog);
                }

                //blogs.Expanded = true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("get blogs failed (HTTP error): " );

                var contents = await response.Content.ReadAsStringAsync();

                if (contents != null)
                {
                    ToDebugWindow(">> HTTP Request POST (Failed)"
                        + Environment.NewLine
                        + _endpoint.AbsoluteUri
                        + Environment.NewLine + Environment.NewLine
                        + "<< HTTP Response " + response.StatusCode.ToString()
                        + Environment.NewLine
                        + contents + Environment.NewLine);
                }
            }

            return blogs;
        }

        public async Task<List<NodeCategory>> GetCategiries(Uri categoriesUrl, string blogid)
        {
            List<NodeCategory> cats = new List<NodeCategory>();

            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = xdoc.CreateXmlDeclaration("1.0", "UTF-8", null);

            XmlElement objRootNode, objMethodNode, objParamsNode, objParamNode, objValueNode, objTypeNode;
            XmlText xt;

            objRootNode = xdoc.CreateElement(string.Empty, "methodCall", string.Empty);
            xdoc.AppendChild(objRootNode);

            objMethodNode = xdoc.CreateElement(string.Empty, "methodName", string.Empty);
            xt = xdoc.CreateTextNode("metaWeblog.getCategories");
            objMethodNode.AppendChild(xt);
            objRootNode.AppendChild(objMethodNode);

            objParamsNode = xdoc.CreateElement(string.Empty, "params", string.Empty);

            objParamNode = xdoc.CreateElement(string.Empty, "param", string.Empty);
            objParamsNode.AppendChild(objParamNode);

            objValueNode = xdoc.CreateElement(string.Empty, "value", string.Empty);
            objParamNode.AppendChild(objValueNode);

            objTypeNode = xdoc.CreateElement(string.Empty, "int", string.Empty);
            xt = xdoc.CreateTextNode(blogid);
            objTypeNode.AppendChild(xt);
            objValueNode.AppendChild(objTypeNode);

            objParamNode = xdoc.CreateElement(string.Empty, "param", string.Empty);
            objParamsNode.AppendChild(objParamNode);

            objValueNode = xdoc.CreateElement(string.Empty, "value", string.Empty);
            objParamNode.AppendChild(objValueNode);

            objTypeNode = xdoc.CreateElement(string.Empty, "string", string.Empty);
            xt = xdoc.CreateTextNode(_userName);
            objTypeNode.AppendChild(xt);
            objValueNode.AppendChild(objTypeNode);

            objParamNode = xdoc.CreateElement(string.Empty, "param", string.Empty);
            objParamsNode.AppendChild(objParamNode);

            objValueNode = xdoc.CreateElement(string.Empty, "value", string.Empty);
            objParamNode.AppendChild(objValueNode);

            objTypeNode = xdoc.CreateElement(string.Empty, "string", string.Empty);
            xt = xdoc.CreateTextNode(_userPassword);
            objTypeNode.AppendChild(xt);
            objValueNode.AppendChild(objTypeNode);

            objRootNode.AppendChild(objParamsNode);

            //System.Diagnostics.Debug.WriteLine("GetCategiries: " + AsUTF16Xml(xdoc));

            ToDebugWindow(">> HTTP Request POST "
                //+ Environment.NewLine
                + categoriesUrl.AbsoluteUri
                + Environment.NewLine
                + AsUTF16Xml(xdoc)
                + Environment.NewLine);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = categoriesUrl,
                Content = new StringContent(AsUTF8Xml(xdoc), Encoding.UTF8, "text/xml")
            };

            var response = await _HTTPConn.Client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string contenTypeString = response.Content.Headers.GetValues("Content-Type").FirstOrDefault();

                if (!contenTypeString.StartsWith("text/xml"))
                {
                    System.Diagnostics.Debug.WriteLine("Content-Type is invalid: " + contenTypeString);

                    ToDebugWindow("<< HTTP Response Content-Type is invalid: " + contenTypeString
                        + Environment.NewLine
                        + "expecting " + "text/xml"
                        + Environment.NewLine);

                    return cats;
                }

                string s = await response.Content.ReadAsStringAsync();

                //System.Diagnostics.Debug.WriteLine("GetCategiries response: " + s);

                ToDebugWindow("<< HTTP Response " + response.StatusCode.ToString()
                + Environment.NewLine
                + s
                + Environment.NewLine);

                try
                {
                    xdoc.LoadXml(s);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("LoadXml failed: " + e.Message);

                    ToDebugWindow("<< Invalid XML returned:"
                        + Environment.NewLine
                        + e.Message
                        + Environment.NewLine);

                    return cats;
                }

                XmlNodeList catList;
                catList = xdoc.SelectNodes("//methodResponse/params/param/value/array/data/value");
                if (catList == null)
                    return cats;

                foreach (XmlNode cal in catList)
                {
                    XmlNodeList memberList = cal.SelectNodes("struct/member");
                    if (memberList == null)
                        continue;

                    string categoryName = "";
                    string categoryId = "";
                    string parentId = "";
                    string description = "";
                    string categoryDescription = "";
                    Uri htmlUrl = null;
                    Uri rssUrl = null;
                    
                    foreach (XmlNode m in memberList)
                    {

                        XmlNodeList valueList = m.ChildNodes;
                        string name = "";
                        string value = "";
                        foreach (XmlNode v in valueList)
                        {
                            if (v.Name == "name")
                            {
                                name = v.InnerText;
                            }
                            if (v.Name == "value")
                            {
                                value = v.InnerText;
                            }
                        }

                        if (name == "categoryName")
                        {
                            categoryName = value;
                        }

                        if (name == "categoryId")
                        {
                            categoryId = value;
                        }

                        if (name == "parentId")
                        {
                            parentId = value;
                        }

                        if (name == "description")
                        {
                            description = value;
                        }

                        if (name == "categoryDescription")
                        {
                            categoryDescription = value;
                        }

                        if (name == "htmlUrl")
                        {
                            try
                            {
                                htmlUrl = new Uri(value);
                            }
                            catch { }
                        }

                        if (name == "rssUrl")
                        {
                            try
                            {
                                rssUrl = new Uri(value);
                            }
                            catch { }
                        }


                    }
                    
                    if (!string.IsNullOrEmpty(categoryName))
                    {
                        NodeXmlRpcMTCategory category = new NodeXmlRpcMTCategory(categoryName);
                        category.CategoryId = categoryId;
                        category.ParentId = parentId;
                        category.Description = description;
                        category.CategoryDescription = categoryDescription;
                        category.HtmlUrl = htmlUrl;
                        category.RssUrl = rssUrl;

                        cats.Add(category);
                    }

                }


            }
            else
            {
                var contents = await response.Content.ReadAsStringAsync();

                if (contents != null)
                {
                    ToDebugWindow(">> HTTP Request POST (Failed)"
                        + Environment.NewLine
                        + categoriesUrl.AbsoluteUri
                        + Environment.NewLine + Environment.NewLine
                        + "<< HTTP Response " + response.StatusCode.ToString()
                        + Environment.NewLine
                        + contents + Environment.NewLine);
                }

                //return cats;
            }

            return cats;
        }

        public override async Task<List<EntryItem>> GetEntries(Uri entryUri)
        {
            List<EntryItem> list = new List<EntryItem>();

            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = xdoc.CreateXmlDeclaration("1.0", "UTF-8", null);

            XmlElement objRootNode, objMethodNode, objParamsNode, objParamNode, objValueNode, objTypeNode;
            XmlText xt;

            objRootNode = xdoc.CreateElement(string.Empty, "methodCall", string.Empty);
            xdoc.AppendChild(objRootNode);

            objMethodNode = xdoc.CreateElement(string.Empty, "methodName", string.Empty);
            xt = xdoc.CreateTextNode("metaWeblog.getRecentPosts");
            objMethodNode.AppendChild(xt);
            objRootNode.AppendChild(objMethodNode);

            objParamsNode = xdoc.CreateElement(string.Empty, "params", string.Empty);

            objParamNode = xdoc.CreateElement(string.Empty, "param", string.Empty);
            objParamsNode.AppendChild(objParamNode);

            objValueNode = xdoc.CreateElement(string.Empty, "value", string.Empty);
            objParamNode.AppendChild(objValueNode);

            objTypeNode = xdoc.CreateElement(string.Empty, "string", string.Empty);
            objValueNode.AppendChild(objTypeNode);

            objParamNode = xdoc.CreateElement(string.Empty, "param", string.Empty);
            objParamsNode.AppendChild(objParamNode);

            objValueNode = xdoc.CreateElement(string.Empty, "value", string.Empty);
            objParamNode.AppendChild(objValueNode);

            objTypeNode = xdoc.CreateElement(string.Empty, "string", string.Empty);
            xt = xdoc.CreateTextNode(_userName);
            objTypeNode.AppendChild(xt);
            objValueNode.AppendChild(objTypeNode);

            objParamNode = xdoc.CreateElement(string.Empty, "param", string.Empty);
            objParamsNode.AppendChild(objParamNode);

            objValueNode = xdoc.CreateElement(string.Empty, "value", string.Empty);
            objParamNode.AppendChild(objValueNode);

            objTypeNode = xdoc.CreateElement(string.Empty, "string", string.Empty);
            xt = xdoc.CreateTextNode(_userPassword);
            objTypeNode.AppendChild(xt);
            objValueNode.AppendChild(objTypeNode);

            objRootNode.AppendChild(objParamsNode);

            //System.Diagnostics.Debug.WriteLine("GET blogs(getUsersBlogs): " + AsXml(xdoc));

            ToDebugWindow(">> HTTP Request POST "
                //+ Environment.NewLine
                + entryUri.AbsoluteUri
                + Environment.NewLine
                + AsUTF16Xml(xdoc)
                + Environment.NewLine);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = entryUri,
                Content = new StringContent(AsUTF8Xml(xdoc), Encoding.UTF8, "text/xml")
            };

            try
            {
                var response = await _HTTPConn.Client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string contenTypeString = response.Content.Headers.GetValues("Content-Type").FirstOrDefault();

                    if (!contenTypeString.StartsWith("text/xml"))
                    {
                        System.Diagnostics.Debug.WriteLine("Content-Type is invalid: " + contenTypeString);

                        ToDebugWindow("<< HTTP Response Content-Type is invalid: " + contenTypeString
                            + Environment.NewLine
                            + "expecting " + "text/xml"
                            + Environment.NewLine);

                        return list;
                    }

                    string s = await response.Content.ReadAsStringAsync();

                    ToDebugWindow("<< HTTP Response " + response.StatusCode.ToString()
                    + Environment.NewLine
                    + s
                    + Environment.NewLine);

                    try
                    {
                        xdoc.LoadXml(s);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine("LoadXml failed: " + e.Message);

                        ToDebugWindow("<< Invalid XML returned:"
                            + Environment.NewLine
                            + e.Message
                            + Environment.NewLine);

                        return list;
                    }

                    XmlNodeList entryList;
                    entryList = xdoc.SelectNodes("//methodResponse/params/param/value/array/data/value");
                    if (entryList == null)
                        return list;

                    foreach (XmlNode l in entryList)
                    {
                        EntryItem ent = new EntryItem("", this);

                        FillEntryItemFromXML(ent, l, entryUri);

                        list.Add(ent);
                    }
                }

            }
            catch (Exception e)
            {
                // TODO:
                Debug.WriteLine("Exception@(XmlRpcMTClient)GetEntries : " + e.Message);
            }

            return list;
        }

        private void FillEntryItemFromXML(EntryItem entItem, XmlNode entryNode, Uri xmlrpcUri)
        {

            MTEntry entry = CreateMTEntryFromXML(entryNode);
            if (entry == null)
                return;

            // multisite has independent endpoint. So we set it here.
            entry.EditUri = xmlrpcUri;
            entry.PostUri = xmlrpcUri;

            entItem.Name = entry.Name;
            //entItem.ID = entry.ID;
            entItem.EntryID = entry.EntryID;
            entItem.EditUri = entry.EditUri;
            entItem.PostUri = entry.PostUri;
            entItem.AltHTMLUri = entry.AltHTMLUri;
            entItem.EntryBody = entry;

            entItem.Status = entry.Status;

        }

        public override  async Task<EntryFull> GetFullEntry(Uri entryUri, string postid) 
        {
            if (string.IsNullOrEmpty(postid))
                throw new InvalidOperationException("XML-RPC requires postid");

            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = xdoc.CreateXmlDeclaration("1.0", "UTF-8", null);

            XmlElement objRootNode, objMethodNode, objParamsNode, objParamNode, objValueNode, objTypeNode;
            XmlText xt;

            objRootNode = xdoc.CreateElement(string.Empty, "methodCall", string.Empty);
            xdoc.AppendChild(objRootNode);

            objMethodNode = xdoc.CreateElement(string.Empty, "methodName", string.Empty);
            xt = xdoc.CreateTextNode("metaWeblog.getPost");
            objMethodNode.AppendChild(xt);
            objRootNode.AppendChild(objMethodNode);

            objParamsNode = xdoc.CreateElement(string.Empty, "params", string.Empty);

            objParamNode = xdoc.CreateElement(string.Empty, "param", string.Empty);
            objParamsNode.AppendChild(objParamNode);

            objValueNode = xdoc.CreateElement(string.Empty, "value", string.Empty);
            objParamNode.AppendChild(objValueNode);

            objTypeNode = xdoc.CreateElement(string.Empty, "int", string.Empty);
            xt = xdoc.CreateTextNode(postid);
            objTypeNode.AppendChild(xt);
            objValueNode.AppendChild(objTypeNode);

            objParamNode = xdoc.CreateElement(string.Empty, "param", string.Empty);
            objParamsNode.AppendChild(objParamNode);

            objValueNode = xdoc.CreateElement(string.Empty, "value", string.Empty);
            objParamNode.AppendChild(objValueNode);

            objTypeNode = xdoc.CreateElement(string.Empty, "string", string.Empty);
            xt = xdoc.CreateTextNode(_userName);
            objTypeNode.AppendChild(xt);
            objValueNode.AppendChild(objTypeNode);

            objParamNode = xdoc.CreateElement(string.Empty, "param", string.Empty);
            objParamsNode.AppendChild(objParamNode);

            objValueNode = xdoc.CreateElement(string.Empty, "value", string.Empty);
            objParamNode.AppendChild(objValueNode);

            objTypeNode = xdoc.CreateElement(string.Empty, "string", string.Empty);
            xt = xdoc.CreateTextNode(_userPassword);
            objTypeNode.AppendChild(xt);
            objValueNode.AppendChild(objTypeNode);

            objRootNode.AppendChild(objParamsNode);

            //System.Diagnostics.Debug.WriteLine("GET GetFullEntry: " + AsXml(xdoc));

            ToDebugWindow(">> HTTP Request POST "
                //+ Environment.NewLine
                + entryUri.AbsoluteUri
                + Environment.NewLine
                + AsUTF16Xml(xdoc)
                + Environment.NewLine);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = entryUri,
                Content = new StringContent(AsUTF8Xml(xdoc), Encoding.UTF8, "text/xml")
            };

            var response = await _HTTPConn.Client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string contenTypeString = response.Content.Headers.GetValues("Content-Type").FirstOrDefault();

                if (!contenTypeString.StartsWith("text/xml"))
                {
                    System.Diagnostics.Debug.WriteLine("Content-Type is invalid: " + contenTypeString);

                    ToDebugWindow("<< HTTP Response Content-Type is invalid: " + contenTypeString
                        + Environment.NewLine
                        + "expecting " + "text/xml"
                        + Environment.NewLine);

                    return null;
                }

                string s = await response.Content.ReadAsStringAsync();

                ToDebugWindow("<< HTTP Response " + response.StatusCode.ToString()
                + Environment.NewLine
                + s
                + Environment.NewLine);

                //System.Diagnostics.Debug.WriteLine("GetFullEntry response: " + s);

                try
                {
                    xdoc.LoadXml(s);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("LoadXml failed: " + e.Message);

                    ToDebugWindow("<< Invalid XML returned:"
                        + Environment.NewLine
                        + e.Message
                        + Environment.NewLine);

                    return null;
                }

            }

            XmlNode entryNode = xdoc.SelectSingleNode("//methodResponse/params/param/value");
            if (entryNode == null)
                return null;

            MTEntry entry = CreateMTEntryFromXML(entryNode);

            return entry;
        }

        private MTEntry CreateMTEntryFromXML(XmlNode entryNode)
        {

            XmlNodeList memberList = entryNode.SelectNodes("struct/member");
            if (memberList == null)
                return null;

            Uri url = null;
            string postid = "";
            string title = "";
            string description = "";

            foreach (XmlNode m in memberList)
            {
                XmlNodeList valueList = m.ChildNodes;
                string name = "";
                string value = "";
                foreach (XmlNode v in valueList)
                {
                    if (v.Name == "name")
                    {
                        name = v.InnerText;
                    }
                    if (v.Name == "value")
                    {
                        value = v.InnerText;
                    }
                }

                //System.Diagnostics.Debug.WriteLine("name: " + name + " - value: " + value);

                if (name == "postid")
                {
                    postid = value;
                }

                if (name == "title")
                {
                    title = value;
                }

                if (name == "link")
                {
                    try
                    {
                        url = new Uri(value);
                    }
                    catch { }
                }
                if (name == "permaLink")
                {
                    try
                    {
                        url = new Uri(value);
                    }
                    catch { }
                }

                if (name == "description")
                {
                    description = value;
                }


                //TODO:

                //categories
                /*
      <member>
        <name>categories</name>
        <value>
          <array>
            <data>
              <value><string>Blogroll</string></value>
              <value><string>Uncategorized</string></value>
             */

                /*
name: postid - value: 50
name: description - value: hogehoge
name: title - value: test2
name: link - value: http://hoge.jp/en/blog/2018/04/06/test2/
name: permaLink - value: http://hoge.jp/en/blog/2018/04/06/test2/
           
name: categories - value: BlogrollUncategorized 
            
name: dateCreated - value: 20180406T05:17:44
name: date_modified - value: 20180406T05:17:44
name: date_modified_gmt - value: 20180406T05:17:44

name: mt_allow_comments - value: 0
name: mt_allow_pings - value: 1
name: mt_keywords - value: 
name: mt_excerpt - value: 
name: mt_text_more - value: 
name: wp_more_text - value: 

name: userid - value: 1

name: wp_slug - value: test2
name: wp_password - value: 
name: wp_author_id - value: 1
name: wp_author_display_name - value: torum
name: date_created_gmt - value: 20180406T05:17:44
name: post_status - value: publish
name: custom_fields - value: id67key_edit_lastvalue1id66key_edit_lockvalue1522991724:1
name: wp_post_format - value: standard

name: sticky - value: 0
name: wp_post_thumbnail - value: 
             
             */


            }

            MTEntry entry = new MTEntry(title, this);
            entry.AltHTMLUri = url;
            entry.EntryID = postid;
            //entry.EditUri = _endpoint; No, don't. Multisite has multiple endpoints for each blog.

            entry.Content = description;

            //TODO: MT doesn't have this flag? need to check.
            entry.IsDraft =  false;
            entry.Status = EntryItem.EntryStatus.esNormal;

            return entry;
        }

        public override async Task<bool> UpdateEntry(EntryFull entry)
        {
            XmlDocument xdoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = xdoc.CreateXmlDeclaration("1.0", "UTF-8", null);

            XmlElement objRootNode, objMethodNode, objParamsNode, objParamNode, objValueNode, objTypeNode;
            XmlText xt;

            objRootNode = xdoc.CreateElement(string.Empty, "methodCall", string.Empty);
            xdoc.AppendChild(objRootNode);

            objMethodNode = xdoc.CreateElement(string.Empty, "methodName", string.Empty);
            xt = xdoc.CreateTextNode("metaWeblog.editPost");
            objMethodNode.AppendChild(xt);
            objRootNode.AppendChild(objMethodNode);

            objParamsNode = xdoc.CreateElement(string.Empty, "params", string.Empty);

            objParamNode = xdoc.CreateElement(string.Empty, "param", string.Empty);
            objParamsNode.AppendChild(objParamNode);

            objValueNode = xdoc.CreateElement(string.Empty, "value", string.Empty);
            objParamNode.AppendChild(objValueNode);

            objTypeNode = xdoc.CreateElement(string.Empty, "int", string.Empty);
            xt = xdoc.CreateTextNode(entry.EntryID);
            objTypeNode.AppendChild(xt);
            objValueNode.AppendChild(objTypeNode);

            objParamNode = xdoc.CreateElement(string.Empty, "param", string.Empty);
            objParamsNode.AppendChild(objParamNode);

            objValueNode = xdoc.CreateElement(string.Empty, "value", string.Empty);
            objParamNode.AppendChild(objValueNode);

            objTypeNode = xdoc.CreateElement(string.Empty, "string", string.Empty);
            xt = xdoc.CreateTextNode(_userName);
            objTypeNode.AppendChild(xt);
            objValueNode.AppendChild(objTypeNode);

            objParamNode = xdoc.CreateElement(string.Empty, "param", string.Empty);
            objParamsNode.AppendChild(objParamNode);

            objValueNode = xdoc.CreateElement(string.Empty, "value", string.Empty);
            objParamNode.AppendChild(objValueNode);

            objTypeNode = xdoc.CreateElement(string.Empty, "string", string.Empty);
            xt = xdoc.CreateTextNode(_userPassword);
            objTypeNode.AppendChild(xt);
            objValueNode.AppendChild(objTypeNode);

            //
            XmlElement structNode = xdoc.CreateElement(string.Empty, "struct", string.Empty);
            XmlElement memberNode = xdoc.CreateElement(string.Empty, "member", string.Empty);

            XmlElement nameNode = xdoc.CreateElement(string.Empty, "name", string.Empty);
            //XmlText ntn = xdoc.CreateTextNode();
            XmlElement valueNode = xdoc.CreateElement(string.Empty, "value", string.Empty);
            XmlElement stringNode = xdoc.CreateElement(string.Empty, "string", string.Empty);
            //XmlText vtn = xdoc.CreateTextNode();

            //struct/member/name
            //struct/member/value
            //struct/member/value/string

            objParamNode = xdoc.CreateElement(string.Empty, "param", string.Empty);
            objParamsNode.AppendChild(objParamNode);

            objValueNode = xdoc.CreateElement(string.Empty, "value", string.Empty);
            objParamNode.AppendChild(objValueNode);

            objTypeNode = xdoc.CreateElement(string.Empty, "bool", string.Empty);
            string isPublish = entry.IsDraft ? "0" : "1";
            xt = xdoc.CreateTextNode(isPublish);
            objTypeNode.AppendChild(xt);
            objValueNode.AppendChild(objTypeNode);

            objRootNode.AppendChild(objParamsNode);

            System.Diagnostics.Debug.WriteLine("metaWeblog.editPost: " + AsUTF16Xml(xdoc));

            ToDebugWindow(">> HTTP Request POST "
                //+ Environment.NewLine
                + entry.EditUri.AbsoluteUri
                + Environment.NewLine
                + AsUTF16Xml(xdoc)
                + Environment.NewLine);

            return true;

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = entry.EditUri,
                Content = new StringContent(AsUTF8Xml(xdoc), Encoding.UTF8, "text/xml")
            };

            var response = await _HTTPConn.Client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string contenTypeString = response.Content.Headers.GetValues("Content-Type").FirstOrDefault();

                if (!contenTypeString.StartsWith("text/xml"))
                {
                    System.Diagnostics.Debug.WriteLine("Content-Type is invalid: " + contenTypeString);

                    ToDebugWindow("<< HTTP Response Content-Type is invalid: " + contenTypeString
                        + Environment.NewLine
                        + "expecting " + "text/xml"
                        + Environment.NewLine);

                    return false;
                }

                string s = await response.Content.ReadAsStringAsync();

                //System.Diagnostics.Debug.WriteLine("metaWeblog.editPost response: " + s);

                ToDebugWindow("<< HTTP Response " + response.StatusCode.ToString()
                + Environment.NewLine
                + s
                + Environment.NewLine);

                try
                {
                    xdoc.LoadXml(s);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("LoadXml failed: " + e.Message);

                    ToDebugWindow("<< Invalid XML returned:"
                        + Environment.NewLine
                        + e.Message
                        + Environment.NewLine);

                    return false;
                }



            }

            return true;

        }

        public override async Task<bool> PostEntry(EntryFull entry)
        {
            //metaWeblog.newPost


            return true;

        }

        public override async Task<bool> DeleteEntry(Uri editUri)
        {
            //metaWeblog.deletePost


            return true;

        }



    }

}


/*
<?xml version="1.0" encoding="UTF-8"?>
<methodResponse>
  <params>
    <param>
      <value>

        <array>
          <data>

  <value>
    <struct>
      <member>
        <name>dateCreated</name>
        <value><dateTime.iso8601>20180406T05:17:44</dateTime.iso8601></value>
      </member>
      <member><name>userid</name><value><string>1</string></value></member>
      <member><name>postid</name><value><string>50</string></value></member>
      <member><name>description</name><value><string>hogehoge</string></value></member>
      <member><name>title</name><value><string>test2</string></value></member>
      <member><name>link</name><value><string>http://hoge.jp/en/blog/2018/04/06/test2/</string></value></member>
      <member><name>permaLink</name><value><string>http://hoge.jp/en/blog/2018/04/06/test2/</string></value></member>
      <member><name>categories</name>
        <value>
          <array>
            <data>
              <value><string>Blogroll</string></value>
              <value><string>Uncategorized</string></value>
            </data>
          </array>
        </value>
      </member>
      <member><name>mt_excerpt</name><value><string></string></value></member>
      <member><name>mt_text_more</name><value><string></string></value></member>
      <member><name>wp_more_text</name><value><string></string></value></member>
      <member><name>mt_allow_comments</name><value><int>0</int></value></member>
      <member><name>mt_allow_pings</name><value><int>1</int></value></member>
      <member><name>mt_keywords</name><value><string></string></value></member>
      <member><name>wp_slug</name><value><string>test2</string></value></member>
      <member><name>wp_password</name><value><string></string></value></member>
      <member><name>wp_author_id</name><value><string>1</string></value></member>
      <member><name>wp_author_display_name</name><value><string>torum</string></value></member>
      <member><name>date_created_gmt</name><value><dateTime.iso8601>20180406T05:17:44</dateTime.iso8601></value></member>
      <member><name>post_status</name><value><string>publish</string></value></member>
      <member><name>custom_fields</name>
        <value>
          <array>
            <data>
              <value>
                <struct>
                  <member><name>id</name><value><string>67</string></value></member>
                  <member><name>key</name><value><string>_edit_last</string></value></member>
                  <member><name>value</name><value><string>1</string></value></member>
                </struct>
              </value>
              <value>
                <struct>
                  <member><name>id</name><value><string>66</string></value></member>
                  <member><name>key</name><value><string>_edit_lock</string></value></member>
                  <member><name>value</name><value><string>1522991724:1</string></value></member>
                </struct>
              </value>
            </data>
          </array>
        </value>
      </member>
      <member><name>wp_post_format</name><value><string>standard</string></value></member>
      <member><name>date_modified</name><value><dateTime.iso8601>20180406T05:17:44</dateTime.iso8601></value></member>
      <member><name>date_modified_gmt</name><value><dateTime.iso8601>20180406T05:17:44</dateTime.iso8601></value></member>
      <member><name>sticky</name><value><boolean>0</boolean></value></member>
      <member><name>wp_post_thumbnail</name><value><string></string></value></member>
    </struct>
  </value>

  <value><struct>
  <member><name>dateCreated</name><value><dateTime.iso8601>20180405T02:08:25</dateTime.iso8601></value></member>
  <member><name>userid</name><value><string>1</string></value></member>
  <member><name>postid</name><value><string>47</string></value></member>
  <member><name>description</name><value><string>test body</string></value></member>
  <member><name>title</name><value><string>This is a test</string></value></member>
  <member><name>link</name><value><string>http://hoge.jp/en/blog/2018/04/05/this-is-a-test/</string></value></member>
  <member><name>permaLink</name><value><string>http://hoge.jp/en/blog/2018/04/05/this-is-a-test/</string></value></member>
  <member><name>categories</name><value><array><data>
  <value><string>Software</string></value>
  <value><string>testCat</string></value>
</data></array></value></member>
  <member><name>mt_excerpt</name><value><string></string></value></member>
  <member><name>mt_text_more</name><value><string></string></value></member>
  <member><name>wp_more_text</name><value><string></string></value></member>
  <member><name>mt_allow_comments</name><value><int>0</int></value></member>
  <member><name>mt_allow_pings</name><value><int>1</int></value></member>
  <member><name>mt_keywords</name><value><string></string></value></member>
  <member><name>wp_slug</name><value><string>this-is-a-test</string></value></member>
  <member><name>wp_password</name><value><string></string></value></member>
  <member><name>wp_author_id</name><value><string>1</string></value></member>
  <member><name>wp_author_display_name</name><value><string>torum</string></value></member>
  <member><name>date_created_gmt</name><value><dateTime.iso8601>20180405T02:08:25</dateTime.iso8601></value></member>
  <member><name>post_status</name><value><string>publish</string></value></member>
  <member><name>custom_fields</name><value><array><data>
  <value><struct>
  <member><name>id</name><value><string>62</string></value></member>
  <member><name>key</name><value><string>_edit_last</string></value></member>
  <member><name>value</name><value><string>1</string></value></member>
</struct></value>
  <value><struct>
  <member><name>id</name><value><string>63</string></value></member>
  <member><name>key</name><value><string>_edit_lock</string></value></member>
  <member><name>value</name><value><string>1522893995:1</string></value></member>
</struct></value>
</data></array></value></member>
  <member><name>wp_post_format</name><value><string>standard</string></value></member>
  <member><name>date_modified</name><value><dateTime.iso8601>20180405T02:08:25</dateTime.iso8601></value></member>
  <member><name>date_modified_gmt</name><value><dateTime.iso8601>20180405T02:08:25</dateTime.iso8601></value></member>
  <member><name>sticky</name><value><boolean>0</boolean></value></member>
  <member><name>wp_post_thumbnail</name><value><string></string></value></member>
</struct></value>

          </data>
        </array>

      </value>
    </param>
  </params>
</methodResponse>


*/










