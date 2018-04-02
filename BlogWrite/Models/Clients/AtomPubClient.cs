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

            NodeCollections blogs = await GetBlogs();
            foreach (var item in blogs.Children)
            {
                item.Parent = account;
                account.Children.Add(item);
            }

            account.Expanded = true;
            return account;
        }

        public override async Task<NodeCollections> GetBlogs()
        {
            NodeCollections blogs = new NodeCollections();

            var HTTPResponseMessage = await _HTTPConn.Client.GetAsync(_endpoint);

            if (HTTPResponseMessage.IsSuccessStatusCode)
            {
                string s = await HTTPResponseMessage.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine("GET blogs(collection): " + s);
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
                XmlDocument xdoc = new XmlDocument();
                try
                {
                    xdoc.LoadXml(s);
                }
                catch (Exception e)
                {
                    // TODO: 
                    System.Diagnostics.Debug.WriteLine("LoadXml failed: " + e.Message);
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

                    NodeCollection blog = new NodeCollection(accountTitle.InnerText);

                    NodeEntries entries = GetEntryNodesFromXML(n, atomNsMgr);
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

            return blogs;
        }

        private NodeEntries GetEntryNodesFromXML(XmlNode w, XmlNamespaceManager atomNsMgr)
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

                foreach (XmlNode a in acceptList)
                {
                    if (a.InnerText == "application/atom+xml;type=entry")
                    {
                        NodeEntry entry = new NodeEntry(title.InnerText, new Uri(hrefAttr));
                        entry.AcceptTypes.Add(a.InnerText);
                        entries.Children.Add(entry);
                    }
                    else
                    {
                        // TODO.
                        // application/atomcat+xml

                        System.Diagnostics.Debug.WriteLine("app:accept type " + a.InnerText + " not implemented (yet).");
                    }

                }

                if (entries.Children.Count > 0)
                    entries.Expanded = true;

            }
            return entries;
        }

        public override async Task<List<EntryItem>> GetEntries(Uri entriesUrl)
        {
            List<EntryItem> list = new List<EntryItem>();

            var HTTPResponseMessage = await _HTTPConn.Client.GetAsync(entriesUrl);

            string s = await HTTPResponseMessage.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine("GET entries: " + s);
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

            XmlDocument xdoc = new XmlDocument();
            try
            {
                xdoc.LoadXml(s);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("LoadXml failed: " + e.Message);
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

            return list;
        }

        public void FillEntryItemFromXML(EntryItem ent, XmlNode entryNode, XmlNamespaceManager atomNsMgr)
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
                    relAttr = u.Attributes["rel"].Value;
                    hrefAttr = u.Attributes["href"].Value;

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
                                    altUri = new Uri(hrefAttr);
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

            //updated
            //published
            //app:edited
            //summary


            //content
            //category

            ent.Name = (entryTitle != null) ? entryTitle.InnerText : "";
            ent.EntryID = (entryID != null) ? entryID.InnerText : "";
            ent.EditUri = editUri;
            ent.AltUri = altUri;


            //app:control/app:draft(yes/no)
            XmlNode entryDraft = entryNode.SelectSingleNode("app:control/app:draft", atomNsMgr);
            if (entryDraft == null) System.Diagnostics.Debug.WriteLine("app:draft: is null.");


            string draft = entryDraft?.InnerText;
            ent.IsDraft = (String.Compare(draft, "yes", true) == 0) ? true : false;
            ent.Status = ent.IsDraft ? EntryItem.EntryStatus.esDraft : EntryItem.EntryStatus.esNormal;


        }

        public override async Task<EntryFull> GetFullEntry(Uri entryUri)
        {
            // TODO: 
            // ETAG If-None-Match 

            var HTTPResponseMessage = await _HTTPConn.Client.GetAsync(entryUri);

            string s = await HTTPResponseMessage.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine("GET entry: " + s);

            XmlDocument xdoc = new XmlDocument();
            try
            {
                xdoc.LoadXml(s);
            }
            catch (Exception e)
            {
                // TODO: 
                System.Diagnostics.Debug.WriteLine("LoadXml failed: " + e.Message);
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

            AtomEntry entry = new AtomEntry("", this);

            FillEntryItemFromXML(entry, entryNode, atomNsMgr);

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


            //TODO: Save ETag
            //HTTPResponseMessage.Content

            /*
            // Hatena's formatted-content
            XmlNode formattedContent = entryNode.SelectSingleNode("hatena:formatted-content", atomNsMgr);
            if (formattedContent != null)
            {
                entry.FormattedContent = formattedContent.InnerText;
            }
            */

            return entry;
        }

        public override async Task<bool> UpdateEntry(EntryFull entry)
        {
            // TODO: For now
            if (!(entry is AtomEntry))
                return false;

            System.Diagnostics.Debug.WriteLine((entry as AtomEntry).AsXml());

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = entry.EditUri,
                Content = new StringContent((entry as AtomEntry).AsXml(), Encoding.UTF8, "application/atom+xml")
            };

            var response = await _HTTPConn.Client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine("updated. Status code is " + response.StatusCode.ToString());
                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Put failed. Status code is " + response.StatusCode.ToString());

                var contents = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine(contents);
                /* 
                 BadRequest 400 Cannot Change into Draft
                */

                return false;
            }

        }

        public override async Task<bool> PostEntry(EntryFull entry)
        {

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = entry.PostUri,
                Content = new StringContent((entry as AtomEntry).AsXml(), Encoding.UTF8, "application/atom+xml")
            };

            var response = await _HTTPConn.Client.SendAsync(request);

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

                    // TODO: load content body xml entry and get id and rel alt and such.


                    System.Diagnostics.Debug.WriteLine("created: " + entryUrl);
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Post IsSuccess, but Location header is null. ");
                    return false;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Post failed. Status code is " + response.StatusCode.ToString());

                var contents = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine(contents);
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


            if (response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine("deleted. Status code is " + response.StatusCode.ToString());
                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Delete failed. Status code is " + response.StatusCode.ToString());

                var contents = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine(contents);


                return false;
            }

            //return response.IsSuccessStatusCode ? true : false;

        }

    }
}
