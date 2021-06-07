using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Syndication;
using System.Xml;
using System.IO;
using System.Diagnostics;
using AngleSharp;

namespace BlogWrite.Models.Clients
{
    class RssFeedClient : BaseClient
    {
        public Uri FeedUrl { get; }

        public RssFeedClient(Uri feedUrl)
        {
            FeedUrl = feedUrl;
        }

        public override async Task<List<EntryItem>> GetEntries(Uri entriesUrl)
        {
            // Clear err msg.
            ClientErrorMessage = "";

            List<EntryItem> list = new List<EntryItem>();

            if (!(entriesUrl.Scheme.Equals("http") || entriesUrl.Scheme.Equals("https")))
            {
                ToDebugWindow("<< Invalid URI scheme:"
                    + Environment.NewLine
                    + entriesUrl.Scheme
                    + Environment.NewLine);

                ClientErrorMessage = "Invalid URI scheme (should be http or https): " + entriesUrl.Scheme;

                return list;
            }

            try
            {
                var HTTPResponseMessage = await _HTTPConn.Client.GetAsync(entriesUrl);

                if (HTTPResponseMessage.IsSuccessStatusCode)
                {
                    string s = await HTTPResponseMessage.Content.ReadAsStringAsync();

                    ToDebugWindow(">> HTTP Request GET "
                        //+ Environment.NewLine
                        + entriesUrl.AbsoluteUri
                        + Environment.NewLine + Environment.NewLine
                        + "<< HTTP Response " + HTTPResponseMessage.StatusCode.ToString()
                        + Environment.NewLine
                        + s + Environment.NewLine);

                    /*
                     * SyndicationFeed does not support RSS 1.0.
                     * 
                    TextReader tr = new StringReader(s);
                    XmlReader reader = XmlReader.Create(tr);
                    SyndicationFeed feed = SyndicationFeed.Load(reader);
                    tr.Close();
                    reader.Close();

                    foreach (SyndicationItem item in feed.Items)
                    {
                        EntryItem ent = new EntryItem("", this);
                        ent.Status = EntryItem.EntryStatus.esNormal;

                        FillEntryItemFromSynItem(ent, item);

                        list.Add(ent);
                    }
                    */

                    var source = await HTTPResponseMessage.Content.ReadAsStreamAsync();

                    XmlDocument xdoc = new XmlDocument();
                    try
                    {
                        XmlReader reader = XmlReader.Create(source);
                        xdoc.Load(reader);
                    }
                    catch (Exception e)
                    {
                        ToDebugWindow("<< Invalid XML document returned:"
                            + Environment.NewLine
                            + e.Message
                            + Environment.NewLine);

                        ClientErrorMessage = "Invalid XML document returned: " + e.Message;

                        return list;
                    }

                    if (xdoc.DocumentElement.LocalName.Equals("rss"))
                    {
                        XmlNodeList entryList;
                        entryList = xdoc.SelectNodes("//rss/channel/item");
                        if (entryList == null)
                            return list;

                        foreach (XmlNode l in entryList)
                        {
                            EntryItem ent = new EntryItem("", this);
                            ent.Status = EntryItem.EntryStatus.esNormal;

                            FillEntryItemFromXmlRss(ent, l);

                            list.Add(ent);
                        }
                    }
                    else if (xdoc.DocumentElement.LocalName.Equals("RDF"))
                    {
                        XmlNamespaceManager NsMgr = new XmlNamespaceManager(xdoc.NameTable);
                        NsMgr.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
                        NsMgr.AddNamespace("rss", "http://purl.org/rss/1.0/");
                        NsMgr.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");

                        XmlNodeList entryList = xdoc.SelectNodes("//rdf:RDF/rss:item", NsMgr);
                        if (entryList == null)
                            return list;

                        foreach (XmlNode l in entryList)
                        {
                            EntryItem ent = new EntryItem("", this);
                            ent.Status = EntryItem.EntryStatus.esNormal;

                            FillEntryItemFromXmlRdf(ent, l, NsMgr);

                            list.Add(ent);
                        }
                    }
                }
                else
                {
                    var contents = await HTTPResponseMessage.Content.ReadAsStringAsync();

                    if (contents != null)
                    {
                        ToDebugWindow(">> HTTP Request GET "
                            //+ Environment.NewLine
                            + entriesUrl.AbsoluteUri
                            + Environment.NewLine + Environment.NewLine
                            + "<< HTTP Response " + HTTPResponseMessage.StatusCode.ToString()
                            + Environment.NewLine
                            + contents + Environment.NewLine);
                    }

                    ClientErrorMessage = "HTTP request failed: " + HTTPResponseMessage.StatusCode.ToString();
                }

            }
            catch (System.Net.Http.HttpRequestException e)
            {
                Debug.WriteLine("<< HttpRequestException: " + e.Message);

                ToDebugWindow(" << HttpRequestException: "
                    + Environment.NewLine
                    + e.Message
                    + Environment.NewLine);

                ClientErrorMessage = "HTTP request error: " + e.Message;
            }
            catch (Exception e)
            {
                Debug.WriteLine("HTTP error: " + e.Message);

                ToDebugWindow("<< HTTP error:"
                    + Environment.NewLine
                    + e.Message
                    + Environment.NewLine);

                ClientErrorMessage = "HTTP error: " + e.Message;
            }

            return list;
        }

        public async void FillEntryItemFromXmlRss(EntryItem entItem, XmlNode entryNode)
        {

            AtomEntry entry = await CreateAtomEntryFromXmlRss(entryNode);

            entItem.Name = entry.Name;
            //entItem.ID = entry.ID;
            entItem.EntryID = entry.EntryID;
            entItem.AltHTMLUri = entry.AltHTMLUri;
            entItem.EntryBody = entry;

            entItem.Status = entry.Status;

        }

        public async void FillEntryItemFromXmlRdf(EntryItem entItem, XmlNode entryNode, XmlNamespaceManager NsMgr)
        {

            AtomEntry entry = await CreateAtomEntryFromXmlRdf(entryNode, NsMgr);

            entItem.Name = entry.Name;
            //entItem.ID = entry.ID;
            entItem.EntryID = entry.EntryID;
            entItem.AltHTMLUri = entry.AltHTMLUri;
            entItem.EntryBody = entry;

            entItem.Status = entry.Status;

        }

        private async Task<AtomEntry> CreateAtomEntryFromXmlRss(XmlNode entryNode)
        {

            XmlNode entryTitle = entryNode.SelectSingleNode("title");
            if (entryTitle == null)
            {
                //Debug.WriteLine("title: is null. ");
                //return;
            }

            XmlNode entryID = entryNode.SelectSingleNode("guid");
            if (entryID == null)
            {
                //Debug.WriteLine("guid: is null. ");
                //return;
            }

            XmlNodeList entryLinkUris = entryNode.SelectNodes("link");

            Uri altUri = null;
            if (entryLinkUris == null)
            {
                //Debug.WriteLine("link: is null. ");
                //continue;
            }
            else
            {
                // TODO:
                foreach (XmlNode u in entryLinkUris)
                {
                    try
                    {
                        altUri = new Uri(u.InnerText);
                        break;
                    }
                    catch
                    {
                        break;
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
            entry.AltHTMLUri = altUri;

            XmlNode cont = entryNode.SelectSingleNode("description");
            if (cont == null)
            {
                //Debug.WriteLine("description is null.");
            }
            else
            {
                entry.ContentType = EntryFull.ContentTypes.textHtml;
                entry.Content = await StripStyleAttributes(cont.InnerText);
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

        private async Task<AtomEntry> CreateAtomEntryFromXmlRdf(XmlNode entryNode, XmlNamespaceManager NsMgr)
        {

            XmlNode entryTitle = entryNode.SelectSingleNode("rss:title", NsMgr);
            if (entryTitle == null)
            {
                //Debug.WriteLine("title: is null. ");
                //return;
            }

            XmlNodeList entryLinkUris = entryNode.SelectNodes("rss:link", NsMgr);

            Uri altUri = null;
            if (entryLinkUris == null)
            {
                //Debug.WriteLine("link: is null. ");
                //continue;
            }
            else
            {
                // TODO:
                foreach (XmlNode u in entryLinkUris)
                {
                    try
                    {
                        altUri = new Uri(u.InnerText);
                        break;
                    }
                    catch
                    {
                        break;
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
            entry.AltHTMLUri = altUri;

            XmlNode cont = entryNode.SelectSingleNode("rss:description", NsMgr);
            if (cont == null)
            {
                //Debug.WriteLine("description is null.");
            }
            else
            {
                entry.ContentType = EntryFull.ContentTypes.textHtml;
                entry.Content = await StripStyleAttributes(cont.InnerText);
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

        // using System.ServiceModel.Syndication;
        // System.ServiceModel.Syndication class does not support RSS 1.0.
        /*
        public async void FillEntryItemFromSynItem(EntryItem entItem, SyndicationItem SynItem)
        {

            AtomEntry entry = await CreateAtomEntryFromSynItem(SynItem);

            entItem.Name = entry.Name;
            //entItem.ID = entry.ID;
            entItem.EntryID = entry.EntryID;
            entItem.EditUri = entry.EditUri;
            entItem.AltHTMLUri = entry.AltHTMLUri;
            entItem.EntryBody = entry;

            entItem.Status = entry.Status;

        }

        private async Task<AtomEntry> CreateAtomEntryFromSynItem(SyndicationItem SynItem)
        {
            AtomEntry entry = new AtomEntry("", this);

            entry.Name = (SynItem.Title.Text != null) ? SynItem.Title.Text : "";
            entry.EntryID = (SynItem.Id != null) ? SynItem.Id : "";

            foreach (SyndicationLink u in SynItem.Links)
            {
                entry.AltHTMLUri = u.Uri;
                break;
            }

            if (string.IsNullOrEmpty(entry.Content.ToString()))
            {
                //Debug.WriteLine("RSS Content null");

                // force html
                entry.ContentType = EntryFull.ContentTypes.textHtml;
                
                if (SynItem.Summary != null)
                    entry.Content = SynItem.Summary.Text;

                if (!string.IsNullOrEmpty(entry.Content))
                {
                    entry.Content = await StripStyleAttributes(entry.Content);
                }
            }
            else
            {
                //Debug.WriteLine("RSS Content NOT null");

                // force html
                entry.ContentType = EntryFull.ContentTypes.textHtml;

                entry.Content = SynItem.Content.ToString();
            }

            entry.Status = EntryItem.EntryStatus.esNormal;

            return entry;
        }
        */

        private async Task<string> StripStyleAttributes(string s)
        {
            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(s));
            //var blueListItemsLinq = document.QuerySelectorAll("*")
            var ItemsLinq = document.All.Where(m => m.HasAttribute("style"));
            foreach (var item in ItemsLinq)
            {
                item.RemoveAttribute("style");
            }

            //return document.DocumentElement.TextContent;
            return document.DocumentElement.InnerHtml;

        }
    }
}
