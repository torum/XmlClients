/// 
/// 
/// BlogWrite 
///  - C#/WPF port of the original "BlogWrite" developed with Delphi.
/// https://github.com/torum/BlogWrite
/// 
/// 
/// Atom Syndication Format:
///  https://tools.ietf.org/html/rfc4287
/// Atom Publishing protocol:
///  https://tools.ietf.org/html/rfc5023
///  
/// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BlogWrite.Models;
using BlogWrite.Models.Clients;

namespace BlogWrite.Models.Clients
{
    /// <summary>
    /// AtomPubClient 
    /// Implements Atom Publishing protocol
    /// </summary>
    class AtomPubClient : BlogClient
    {
        public AtomPubClient(string userName, string userPassword, Uri endpoint) : base(userName, userPassword, endpoint)
        {
            //TODO:
            _HTTPConn.Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", userName, userPassword))));
        }

        public override async Task<NodeService> GetAccount(string accountName)
        {
            NodeService account = new NodeService(accountName, _userName, _userPassword, _endpoint, ApiTypes.atAtomPub);

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

            var HTTPResponseMessage = await _HTTPConn.Client.GetAsync(_endpoint);

            if (HTTPResponseMessage.IsSuccessStatusCode)
            {
                string s = await HTTPResponseMessage.Content.ReadAsStringAsync();

                //System.Diagnostics.Debug.WriteLine("GET blogs(collection): " + s);

                ToDebugWindow(">> HTTP Request GET "
                    //+ Environment.NewLine
                    + _endpoint.AbsoluteUri
                    + Environment.NewLine + Environment.NewLine
                    + "<< HTTP Response " + HTTPResponseMessage.StatusCode.ToString()
                    + Environment.NewLine
                    + s + Environment.NewLine);

                /*
				    <?xml version="1.0" encoding="utf-8"?>
				    <service xmlns="http://www.w3.org/2007/app">
				      <workspace>
				        <atom:title xmlns:atom="http://www.w3.org/2005/Atom">hoge</atom:title>
				        <collection href="https://127.0.0.1/atom/entry">
				          <atom:title xmlns:atom="http://www.w3.org/2005/Atom">fuga</atom:title>
				          <accept>application/atom+xml;type=entry</accept>
				        </collection>
				      </workspace>
				    </service>
                 */

                /*
<?xml version="1.0" encoding="utf-8"?>
<service xmlns="http://www.w3.org/2007/app" xmlns:atom="http://www.w3.org/2005/Atom">
 <workspace>
   <atom:title>hoge Workspace</atom:title>
   <collection href="http://torum.jp/en/wp-app.php/service/posts">
     <atom:title>hoge Posts</atom:title>
     <accept>application/atom+xml;type=entry</accept>
     <categories href="http://hoge.jp/wp-app.php/service/categories" />
   </collection>
   <collection href="http://hoge.jp/wp-app.php/service/attachments">
     <atom:title>hoge Media</atom:title>
     <accept>image/*</accept><accept>audio/*</accept><accept>video/*</accept>
   </collection>
 </workspace>
</service>
                */

                string contenTypeString = HTTPResponseMessage.Content.Headers.GetValues("Content-Type").FirstOrDefault();

                if (!contenTypeString.StartsWith("application/atomsvc+xml"))
                {
                    System.Diagnostics.Debug.WriteLine("Content-Type is invalid: " + contenTypeString);

                    ToDebugWindow("<< Content-Type is invalid: " + contenTypeString
                        + Environment.NewLine
                        + "expecting " + "application/atomsvc+xml"
                        + Environment.NewLine);

                    return blogs;
                }

                XmlDocument xdoc = new XmlDocument();
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

                XmlNamespaceManager atomNsMgr = new XmlNamespaceManager(xdoc.NameTable);
                atomNsMgr.AddNamespace("atom", "http://www.w3.org/2005/Atom");
                atomNsMgr.AddNamespace("app", "http://www.w3.org/2007/app");

                XmlNodeList workspaceList;
                workspaceList = xdoc.SelectNodes("//app:service/app:workspace", atomNsMgr);
                if (workspaceList == null) return blogs;

                foreach (XmlNode n in workspaceList)
                {
                    XmlNode accountTitle = n.SelectSingleNode("atom:title", atomNsMgr);
                    if (accountTitle == null)
                    {
                        System.Diagnostics.Debug.WriteLine("atom:title is null. ");
                        continue;
                    }

                    NodeWorkspace blog = new NodeWorkspace(accountTitle.InnerText);

                    NodeEntries entries = await GetEntryNodesFromXML(n, atomNsMgr);
                    foreach (var item in entries.Children)
                    {
                        item.Parent = blog;
                        blog.Children.Add(item);
                    }

                    blog.Expanded = true;

                    blogs.Children.Add(blog);
                    blogs.Expanded = true;
                }

            }
            else
            {
                System.Diagnostics.Debug.WriteLine("GET Error blogs(collection): " + HTTPResponseMessage.StatusCode.ToString());

                var contents = await HTTPResponseMessage.Content.ReadAsStringAsync();

                if (contents != null)
                {
                    ToDebugWindow(">> HTTP Request GET (Failed)"
                        + Environment.NewLine
                        + _endpoint.AbsoluteUri
                        + Environment.NewLine + Environment.NewLine
                        + "<< HTTP Response " + HTTPResponseMessage.StatusCode.ToString()
                        + Environment.NewLine
                        + contents + Environment.NewLine);
                }
            }

            return blogs;
        }

        private async Task<NodeEntries> GetEntryNodesFromXML(XmlNode w, XmlNamespaceManager atomNsMgr)
        {
            NodeEntries entries = new NodeEntries();

            XmlNodeList collectionList = w.SelectNodes("app:collection", atomNsMgr);
            if (collectionList == null)
            {
                System.Diagnostics.Debug.WriteLine("app:collection is null.");
                return entries;
            }

            foreach (XmlNode n in collectionList)
            {
                var hrefAttr = n.Attributes["href"].Value;
                if (hrefAttr == null)
                {
                    System.Diagnostics.Debug.WriteLine("href Attr is null.");
                    continue;
                }
                XmlNode title = n.SelectSingleNode("atom:title", atomNsMgr);
                if (title == null)
                {
                    System.Diagnostics.Debug.WriteLine("atom:title is null.");
                    continue;
                }
                XmlNodeList acceptList = n.SelectNodes("app:accept", atomNsMgr);
                if (acceptList == null)
                {
                    System.Diagnostics.Debug.WriteLine("app:accept is null.");
                    continue;
                }

                NodeAtomPubEntryCollection entry = new NodeAtomPubEntryCollection(title.InnerText, new Uri(hrefAttr));

                foreach (XmlNode a in acceptList)
                {
                    entry.AcceptTypes.Add(a.InnerText);

                    if (a.InnerText == "application/atom+xml;type=entry")
                    {
                        //
                    }
                    else
                    {
                        // TODO:
                        // application/atomcat+xml

                        System.Diagnostics.Debug.WriteLine("app:accept type " + a.InnerText + " not implemented (yet).");
                    }

                }

                XmlNode cats = n.SelectSingleNode("app:categories", atomNsMgr);
                if (cats != null)
                {
                    // Look for category document.
                    if (cats.Attributes["href"] != null)
                    {
                        var hrefCat = cats.Attributes["href"];
                        try
                        {
                            entry.CategoriesUri = new Uri(hrefCat.Value);
                        }
                        catch { }
                    }

                    // Inline categories
                    XmlNodeList catList = cats.SelectNodes("atom:category", atomNsMgr);
                    foreach (XmlNode c in catList)
                    {
                        if (c.Attributes["term"] != null)
                        {
                            NodeCategory category = new NodeCategory(c.Attributes["term"].Value);
                            entry.Children.Add(category);
                        }
                    }

                }

                // Get category document.
                if (entry.CategoriesUri != null)
                {
                    List<NodeCategory> nc = await GetCategiries(entry.CategoriesUri);
                    foreach (NodeCategory c in nc)
                    {
                        entry.Children.Add(c);
                    }
                }

                entries.Children.Add(entry);

            }

            if (entries.Children.Count > 0)
                entries.Expanded = true;

            return entries;
        }

        public async Task<List<NodeCategory>> GetCategiries(Uri categoriesUrl)
        {
            List<NodeCategory> cats = new List<NodeCategory>();

            var HTTPResponseMessage = await _HTTPConn.Client.GetAsync(categoriesUrl);

            if (HTTPResponseMessage.IsSuccessStatusCode)
            {
                string s = await HTTPResponseMessage.Content.ReadAsStringAsync();

                ToDebugWindow(">> HTTP Request GET "
                    + Environment.NewLine
                    + categoriesUrl.AbsoluteUri
                    + Environment.NewLine + Environment.NewLine
                    + "<< HTTP Response " + HTTPResponseMessage.StatusCode.ToString()
                    + Environment.NewLine
                    + s + Environment.NewLine);

                /*
        <app:categories xmlns:app="http://www.w3.org/2007/app" xmlns="http://www.w3.org/2005/Atom" fixed="yes" scheme="http://torum.jp/en">
            <category term="Blogroll" />
            <category term="Software" />
            <category term="testCat" />
            <category term="Uncategorized" />
        </app:categories>
                 */

                string contenTypeString = HTTPResponseMessage.Content.Headers.GetValues("Content-Type").FirstOrDefault();

                if (!contenTypeString.StartsWith("application/atomcat+xml"))
                {
                    System.Diagnostics.Debug.WriteLine("Content-Type is invalid: " + contenTypeString);

                    ToDebugWindow("<< Content-Type is invalid: " + contenTypeString
                        + Environment.NewLine
                        + "expecting " + "application/atomcat+xml"
                        + Environment.NewLine);

                    return cats;
                }

                XmlDocument xdoc = new XmlDocument();
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

                XmlNamespaceManager atomNsMgr = new XmlNamespaceManager(xdoc.NameTable);
                atomNsMgr.AddNamespace("atom", "http://www.w3.org/2005/Atom");
                atomNsMgr.AddNamespace("app", "http://www.w3.org/2007/app");


                XmlNodeList categoryList;
                categoryList = xdoc.SelectNodes("//app:categories/atom:category", atomNsMgr);
                if (categoryList == null)
                    return cats;

                foreach (XmlNode c in categoryList)
                {
                    if (c.Attributes["term"] != null)
                    {
                        NodeCategory category = new NodeCategory(c.Attributes["term"].Value);
                        cats.Add(category);
                    }
                }
            }
            else
            {
                var contents = await HTTPResponseMessage.Content.ReadAsStringAsync();

                if (contents != null)
                {
                    ToDebugWindow(">> HTTP Request GET (Failed)"
                        + Environment.NewLine
                        + categoriesUrl.AbsoluteUri
                        + Environment.NewLine + Environment.NewLine
                        + "<< HTTP Response " + HTTPResponseMessage.StatusCode.ToString()
                        + Environment.NewLine
                        + contents + Environment.NewLine);
                }
            }

            return cats;
        }

        public override async Task<List<EntryItem>> GetEntries(Uri entriesUrl)
        {
            List<EntryItem> list = new List<EntryItem>();

            //System.Diagnostics.Debug.WriteLine("GetEntries Uri: " + entriesUrl.AbsoluteUri);
            var HTTPResponseMessage = await _HTTPConn.Client.GetAsync(entriesUrl);

            if (HTTPResponseMessage.IsSuccessStatusCode)
            {
                string s = await HTTPResponseMessage.Content.ReadAsStringAsync();

                ToDebugWindow(">> HTTP Request GET "
                    + Environment.NewLine
                    + entriesUrl.AbsoluteUri
                    + Environment.NewLine + Environment.NewLine
                    + "<< HTTP Response " + HTTPResponseMessage.StatusCode.ToString()
                    + Environment.NewLine
                    + s + Environment.NewLine);

                //System.Diagnostics.Debug.WriteLine("GET entries response: " + s);
                /*
       <?xml version="1.0" encoding="utf-8"?>
       <feed xmlns="http://www.w3.org/2005/Atom">
         <title type="text">dive into mark</title>
         <subtitle type="html">
           A &lt;em&gt;lot&lt;/em&gt; of effort
           went into making this effortless
         </subtitle>
         <updated>2005-07-31T12:29:29Z</updated>
         <id>tag:example.org,2003:3</id>
         <link rel="alternate" type="text/html"
          hreflang="en" href="http://example.org/"/>
         <link rel="self" type="application/atom+xml"
          href="http://example.org/feed.atom"/>
         <rights>Copyright (c) 2003, Mark Pilgrim</rights>
         <generator uri="http://www.example.com/" version="1.0">
           Example Toolkit
         </generator>
         <entry>
           <title>Atom draft-07 snapshot</title>
           <link rel="alternate" type="text/html"
            href="http://example.org/2005/04/02/atom"/>
           <link rel="enclosure" type="audio/mpeg" length="1337"
            href="http://example.org/audio/ph34r_my_podcast.mp3"/>
           <id>tag:example.org,2003:3.2397</id>
           <updated>2005-07-31T12:29:29Z</updated>
           <published>2003-12-13T08:29:29-04:00</published>
           <author>
             <name>Mark Pilgrim</name>
             <uri>http://example.org/</uri>
             <email>f8dy@example.com</email>
           </author>
           <contributor>
             <name>Sam Ruby</name>
           </contributor>
           <contributor>
             <name>Joe Gregorio</name>
           </contributor>
           <content type="xhtml" xml:lang="en"
            xml:base="http://diveintomark.org/">
             <div xmlns="http://www.w3.org/1999/xhtml">
               <p><i>[Update: The Atom draft is finished.]</i></p>
             </div>
           </content>
         </entry>
       </feed>
                */

                string contenTypeString = HTTPResponseMessage.Content.Headers.GetValues("Content-Type").FirstOrDefault();

                if (!contenTypeString.StartsWith("application/atom+xml"))
                {
                    System.Diagnostics.Debug.WriteLine("Content-Type is invalid: " + contenTypeString);

                    ToDebugWindow("<< Content-Type is invalid: " + contenTypeString
                        + Environment.NewLine
                        + "expecting " + "application/atom+xml"
                        + Environment.NewLine);

                    return list;
                }


                XmlDocument xdoc = new XmlDocument();
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

                XmlNamespaceManager atomNsMgr = new XmlNamespaceManager(xdoc.NameTable);
                atomNsMgr.AddNamespace("atom", "http://www.w3.org/2005/Atom");
                atomNsMgr.AddNamespace("app", "http://www.w3.org/2007/app");

                XmlNodeList entryList;
                entryList = xdoc.SelectNodes("//atom:feed/atom:entry", atomNsMgr);
                if (entryList == null)
                    return list;

                foreach (XmlNode l in entryList)
                {
                    EntryItem ent = new EntryItem("", this);

                    FillEntryItemFromXML(ent, l, atomNsMgr);

                    list.Add(ent);
                }
            }
            else
            {
                var contents = await HTTPResponseMessage.Content.ReadAsStringAsync();

                if (contents != null)
                {
                    ToDebugWindow(">> HTTP Request GET (Failed)"
                        + Environment.NewLine
                        + entriesUrl.AbsoluteUri
                        + Environment.NewLine + Environment.NewLine
                        + "<< HTTP Response " + HTTPResponseMessage.StatusCode.ToString()
                        + Environment.NewLine
                        + contents + Environment.NewLine);
                }
            }

            return list;
        }

        private void FillEntryItemFromXML(EntryItem entItem, XmlNode entryNode, XmlNamespaceManager atomNsMgr)
        {

            AtomEntry entry = CreateAtomEntryFromXML(entryNode, atomNsMgr);
            if (entry == null)
                return;

            entItem.Name = entry.Name;
            //entItem.ID = entry.ID;
            entItem.EntryID = entry.EntryID;
            entItem.EditUri = entry.EditUri;
            entItem.AltHTMLUri = entry.AltHTMLUri;
            entItem.EntryBody = entry;

            entItem.Status = entry.Status;
        }

        public override async Task<EntryFull> GetFullEntry(Uri entryUri, string nil)
        {
            // TODO: 
            // HTTP Head, if_modified_since or If-None-Match etag or something... then  Get;

            var HTTPResponseMessage = await _HTTPConn.Client.GetAsync(entryUri);

            if (HTTPResponseMessage.IsSuccessStatusCode)
            {
                string s = await HTTPResponseMessage.Content.ReadAsStringAsync();

                ToDebugWindow(">> HTTP Request GET "
                    + Environment.NewLine
                    + entryUri.AbsoluteUri
                    + Environment.NewLine + Environment.NewLine
                    + "<< HTTP Response " + HTTPResponseMessage.StatusCode.ToString()
                    + Environment.NewLine
                    + s + Environment.NewLine);

                string contenTypeString = HTTPResponseMessage.Content.Headers.GetValues("Content-Type").FirstOrDefault();

                if (!contenTypeString.StartsWith("application/atom+xml"))
                {
                    System.Diagnostics.Debug.WriteLine("Content-Type is invalid: " + contenTypeString);

                    ToDebugWindow("<< Content-Type is invalid: " + contenTypeString
                        + Environment.NewLine
                        + "expecting " + "application/atom+xml"
                        + Environment.NewLine);

                    return null;
                }

                XmlDocument xdoc = new XmlDocument();
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

                XmlNamespaceManager atomNsMgr = new XmlNamespaceManager(xdoc.NameTable);
                atomNsMgr.AddNamespace("atom", "http://www.w3.org/2005/Atom");
                atomNsMgr.AddNamespace("app", "http://www.w3.org/2007/app");
                atomNsMgr.AddNamespace("hatena", "http://www.hatena.ne.jp/info/xmlns#");

                XmlNode entryNode = xdoc.SelectSingleNode("//atom:entry", atomNsMgr);
                if (entryNode == null)
                {
                    System.Diagnostics.Debug.WriteLine("//atom:entry is null.");
                    return null;
                }

                XmlNode cont = entryNode.SelectSingleNode("atom:content", atomNsMgr);
                if (cont == null)
                {
                    System.Diagnostics.Debug.WriteLine("//atom:content is null.");
                    return null;
                }

                AtomEntry entry = CreateAtomEntryFromXML(entryNode, atomNsMgr);



                //TODO: Save ETag
                //HTTPResponseMessage.Content


                return entry;
            }
            else
            {
                var contents = await HTTPResponseMessage.Content.ReadAsStringAsync();

                if (contents != null)
                {
                    ToDebugWindow(">> HTTP Request GET (Failed)"
                        + Environment.NewLine
                        + _endpoint.AbsoluteUri
                        + Environment.NewLine + Environment.NewLine
                        + "<< HTTP Response " + HTTPResponseMessage.StatusCode.ToString()
                        + Environment.NewLine
                        + contents + Environment.NewLine);
                }

                return null;
            }
        }

        private AtomEntry CreateAtomEntryFromXML(XmlNode entryNode, XmlNamespaceManager atomNsMgr)
        {
            /*
			<entry>
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

            XmlNode entryTitle = entryNode.SelectSingleNode("atom:title", atomNsMgr);
            if (entryTitle == null)
            {
                System.Diagnostics.Debug.WriteLine("atom:title: is null. ");
                //return;
            }

            XmlNode entryID = entryNode.SelectSingleNode("atom:id", atomNsMgr);
            if (entryID == null)
            {
                System.Diagnostics.Debug.WriteLine("atom:id: is null. ");
                //return;
            }

            XmlNodeList entryLinkUris = entryNode.SelectNodes("atom:link", atomNsMgr);
            string relAttr;
            string hrefAttr;
            string typeAttr;
            Uri editUri = null;
            Uri altUri = null;
            if (entryLinkUris == null)
            {
                System.Diagnostics.Debug.WriteLine("atom:link: is null. ");
                //continue;
            }
            else
            {
                foreach (XmlNode u in entryLinkUris)
                {
                    relAttr = (u.Attributes["rel"] != null) ? u.Attributes["rel"].Value : "";
                    hrefAttr = (u.Attributes["href"] != null) ? u.Attributes["href"].Value : "";
                    typeAttr = (u.Attributes["type"] != null) ? u.Attributes["type"].Value : "";

                    if ((!string.IsNullOrEmpty(relAttr)) && (!string.IsNullOrEmpty(hrefAttr)))
                    {

                        switch (relAttr)
                        {
                            case "edit":
                                try
                                {
                                    editUri = new Uri(hrefAttr);
                                    break;
                                }
                                catch
                                {
                                    break;
                                }
                            case "alternate":
                                try
                                {
                                    if (!string.IsNullOrEmpty(typeAttr))
                                    {
                                        if (typeAttr == "text/html")
                                        {
                                            altUri = new Uri(hrefAttr);
                                        }
                                    }
                                    break;
                                }
                                catch
                                {
                                    break;
                                }
                            case "": //same as alternate
                                try
                                {
                                    if (!string.IsNullOrEmpty(typeAttr))
                                    {
                                        if (typeAttr == "text/html")
                                        {
                                            altUri = new Uri(hrefAttr);
                                        }
                                    }
                                    else
                                    {
                                        // I am not happy but let's assume it is html.
                                        altUri = new Uri(hrefAttr);
                                    }
                                    break;
                                }
                                catch
                                {
                                    break;
                                }
                        }
                    }
                }
            }

            // TODO:
            //updated
            //published
            //app:edited
            //summary
            //category

            AtomEntry entry = new AtomEntry("", this);
            // TODO:
            //AtomEntryHatena
            /*
            // Hatena's formatted-content
            XmlNode formattedContent = entryNode.SelectSingleNode("hatena:formatted-content", atomNsMgr);
            if (formattedContent != null)
            {
                entry.FormattedContent = formattedContent.InnerText;
            }
            */


            entry.Name = (entryTitle != null) ? entryTitle.InnerText : "";
            entry.EntryID = (entryID != null) ? entryID.InnerText : "";
            entry.EditUri = editUri;
            entry.AltHTMLUri = altUri;

            XmlNode cont = entryNode.SelectSingleNode("atom:content", atomNsMgr);
            if (cont == null)
            {
                System.Diagnostics.Debug.WriteLine("//atom:content is null.");
            }
            else
            {

                string contype = cont.Attributes["type"].Value;
                if (!string.IsNullOrEmpty(contype))
                {
                    entry.ContentTypeString = contype;

                    switch (contype)
                    {
                        case "text":
                            entry.ContentType = EntryFull.ContentTypes.text;
                            break;
                        case "html":
                            entry.ContentType = EntryFull.ContentTypes.textHtml;
                            break;
                        case "xhtml":
                            entry.ContentType = EntryFull.ContentTypes.textHtml;
                            break;
                        case "text/plain":
                            entry.ContentType = EntryFull.ContentTypes.text;
                            break;
                        case "text/html":
                            entry.ContentType = EntryFull.ContentTypes.textHtml;
                            break;
                        case "text/x-markdown":
                            entry.ContentType = EntryFull.ContentTypes.markdown;
                            break;
                        case "text/x-hatena-syntax":
                            entry.ContentType = EntryFull.ContentTypes.hatena;
                            break;
                        default:
                            entry.ContentType = EntryFull.ContentTypes.text;
                            break;
                    }
                }

                entry.Content = cont.InnerText;

            }

            //app:control/app:draft(yes/no)
            XmlNode entryDraft = entryNode.SelectSingleNode("app:control/app:draft", atomNsMgr);
            if (entryDraft == null) System.Diagnostics.Debug.WriteLine("app:draft: is null.");

            string draft = entryDraft?.InnerText;
            entry.IsDraft = (String.Compare(draft, "yes", true) == 0) ? true : false;

            entry.Status = entry.IsDraft ? EntryItem.EntryStatus.esDraft : EntryItem.EntryStatus.esNormal;


            return entry;
        }

        public override async Task<bool> UpdateEntry(EntryFull entry)
        {
            if (!(entry is AtomEntry))
                return false;

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = entry.EditUri,
                Content = new StringContent((entry as AtomEntry).AsUTF8Xml(), Encoding.UTF8, "application/atom+xml")
            };

            var response = await _HTTPConn.Client.SendAsync(request);

            ToDebugWindow(">> HTTP Request PUT "
                + Environment.NewLine
                + entry.EditUri.AbsoluteUri
                + Environment.NewLine
                + (entry as AtomEntry).AsUTF16Xml()
                + Environment.NewLine + Environment.NewLine
                + "<< HTTP Response " + response.StatusCode.ToString()
                + Environment.NewLine);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Put failed. Status code is " + response.StatusCode.ToString());

                var contents = await response.Content.ReadAsStringAsync();

                /* 
                 * Hatena AtomPub returns this.
                 BadRequest 400 Cannot Change into Draft
                */

                if (contents != null) {

                    System.Diagnostics.Debug.WriteLine(contents);

                    ToDebugWindow("<< HTTP Request PUT failed. HTTP Response content is:"
                    + Environment.NewLine
                    + contents
                    + Environment.NewLine);
                }

                return false;
            }

        }

        public override async Task<bool> PostEntry(EntryFull entry)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = entry.PostUri,
                Content = new StringContent((entry as AtomEntry).AsUTF8Xml(), Encoding.UTF8, "application/atom+xml")
            };

            var response = await _HTTPConn.Client.SendAsync(request);

            ToDebugWindow(">> HTTP Request POST "
                + Environment.NewLine
                + entry.EditUri.AbsoluteUri
                + Environment.NewLine
                + (entry as AtomEntry).AsUTF16Xml()
                + Environment.NewLine + Environment.NewLine
                + "<< HTTP Response " + response.StatusCode.ToString()
                + Environment.NewLine);

            /*
            XNamespace atom = "http://www.w3.org/2005/Atom";
            XNamespace app = "http://www.w3.org/2007/app";

            XDocument doc = new XDocument(new XElement(atom + "entry",
                                                    new XAttribute(XNamespace.Xmlns + "app", app.NamespaceName),
                                                    new XElement(atom + "title", "test title"),
                                                    new XElement(atom + "author",
                                                        new XElement(atom + "name", "me")),
                                                    new XElement(atom + "content",
                                                        new XAttribute("type", "text/plain"),
                                                        new XText("asdf")),
                                                    new XElement(atom + "category",
                                                        new XAttribute("term", "test categ")
                                                        ),
                                                    new XElement(app + "control",
                                                        new XElement(app + "draft", "yes")
                                                        )
                                                    )
                                         );

            System.Diagnostics.Debug.WriteLine("content xml: " + doc.ToString());
            */

            if (response.IsSuccessStatusCode)
            {
                Uri entryUrl = response.Headers.Location;

                if (entryUrl != null)
                {
                    entry.EditUri = entryUrl;

                    string contenTypeString = response.Content.Headers.GetValues("Content-Type").FirstOrDefault();

                    if (!contenTypeString.StartsWith("application/atom+xml"))
                    {
                        System.Diagnostics.Debug.WriteLine("Content-Type is invalid: " + contenTypeString);

                        ToDebugWindow("<< Content-Type is invalid: " + contenTypeString
                            + Environment.NewLine
                            + "expecting " + "application/atom+xml"
                            + Environment.NewLine);
                    }
                    else
                    {
                        // TODO: load content body xml entry and get id and rel alt and such.
                    }

                    //System.Diagnostics.Debug.WriteLine("created: " + entryUrl);

                    ToDebugWindow("<< HTTP Response Header - Location: " 
                        + Environment.NewLine
                        + entryUrl
                        + Environment.NewLine);

                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Post IsSuccess, but Location header is null. ");

                    ToDebugWindow("POST is successfull, but Location header is missing. "
                        + Environment.NewLine
                        + Environment.NewLine);

                    return false;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Post failed. Status code is " + response.StatusCode.ToString());

                var contents = await response.Content.ReadAsStringAsync();

                if (contents != null)
                {
                    System.Diagnostics.Debug.WriteLine(contents);

                    ToDebugWindow("POST failed. HTTP Response content is:"
                    + Environment.NewLine
                    + contents
                    + Environment.NewLine);
                }

                return false;
            }

        }

        public override async Task<bool> DeleteEntry(Uri editUri)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = editUri
            };

            var response = await _HTTPConn.Client.SendAsync(request);

            ToDebugWindow(">> HTTP Request DELETE "
                + Environment.NewLine
                + editUri.AbsoluteUri
                + Environment.NewLine + Environment.NewLine
                + "<< HTTP Response " + response.StatusCode.ToString()
                + Environment.NewLine);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Delete failed. Status code is " + response.StatusCode.ToString());

                var contents = await response.Content.ReadAsStringAsync();

                if (contents != null)
                {
                    System.Diagnostics.Debug.WriteLine(contents);

                    ToDebugWindow("DELETE failed. HTTP Response content is:"
                    + Environment.NewLine
                    + contents
                    + Environment.NewLine);
                }

                return false;
            }

        }

    }

}
