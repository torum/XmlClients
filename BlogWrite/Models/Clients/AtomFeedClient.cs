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
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlogWrite.Models.Clients
{
    /// <summary>
    /// AtomFeedClient 
    /// Implements Atom Syndication Format
    /// </summary>
    class AtomFeedClient : BaseClient
    {
        public Uri FeedUrl { get; }

        public AtomFeedClient(Uri feedUrl)
        {
            FeedUrl = feedUrl;
        }

        public override async Task<List<EntryItem>> GetEntries(Uri entriesUrl)
        {
            List<EntryItem> list = new List<EntryItem>();

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

                //System.Diagnostics.Debug.WriteLine("GET entries: " + s);
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
                    ent.Status = EntryItem.EntryStatus.esNormal;

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

        public void FillEntryItemFromXML(EntryItem entItem, XmlNode entryNode, XmlNamespaceManager atomNsMgr)
        {

            AtomEntry entry = CreateAtomEntryFromXML(entryNode, atomNsMgr);

            entItem.Name = entry.Name;
            //entItem.ID = entry.ID;
            entItem.EntryID = entry.EntryID;
            entItem.EditUri = entry.EditUri;
            entItem.AltHTMLUri = entry.AltHTMLUri;
            entItem.EntryBody = entry;

            entItem.Status = entry.Status;

        }

        private AtomEntry CreateAtomEntryFromXML(XmlNode entryNode, XmlNamespaceManager atomNsMgr)
        {

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

                    if (!string.IsNullOrEmpty(hrefAttr))
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

            /*
            //app:control/app:draft(yes/no)
            XmlNode entryDraft = entryNode.SelectSingleNode("app:control/app:draft", atomNsMgr);
            if (entryDraft == null) System.Diagnostics.Debug.WriteLine("app:draft: is null.");

            string draft = entryDraft?.InnerText;
            entry.IsDraft = (String.Compare(draft, "yes", true) == 0) ? true : false;
            */

            entry.Status = EntryItem.EntryStatus.esNormal;


            return entry;
        }

    }

}
