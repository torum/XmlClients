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
                    //+ s + Environment.NewLine);
                    );
                try
                {
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
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Invalid RSS/XML: " + e.Message);

                    ToDebugWindow("<< Invalid RSS/XML returned:"
                        + Environment.NewLine
                        + e.Message
                        + Environment.NewLine);

                    ClientErrorMessage = "Invalid RSS/XML returned: " + e.Message;

                    return list;
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

            return list;
        }


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
            
            string relAttr;
            string hrefAttr = "";
            string typeAttr;
            //Uri editUri = null;
            Uri altUri = null;

            foreach (SyndicationLink u in SynItem.Links)
            {
                relAttr = (u.RelationshipType != null) ? u.RelationshipType : "";
                hrefAttr = (u.Uri != null) ? u.Uri.AbsoluteUri : "";
                typeAttr = (u.MediaType != null) ? u.MediaType : "";

                /*
                if (!string.IsNullOrEmpty(hrefAttr))
                {
                    
                    switch (relAttr)
                    {
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
                */
            }

            AtomEntry entry = new AtomEntry("", this);

            entry.Name = (SynItem.Title.Text != null) ? SynItem.Title.Text : "";
            entry.EntryID = (SynItem.Id != null) ? SynItem.Id : "";
            //entry.EditUri = editUri;

            //Debug.WriteLine(hrefAttr);

            if (altUri == null)
            {
                if (!string.IsNullOrEmpty(hrefAttr))
                {
                    altUri = new Uri(hrefAttr);
                }
            }
            
            entry.AltHTMLUri = altUri;

            if (string.IsNullOrEmpty(entry.Content.ToString()))
            {
                //Debug.WriteLine("RSS Content null");

                // Content-Types for RSS is messed up. So let's ignore.
                /*
                string contype = SynItem.Summary.Type;

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
                */

                // force html
                entry.ContentType = EntryFull.ContentTypes.textHtml;
                
                if (SynItem.Summary != null)
                    entry.Content = SynItem.Summary.Text;

                if (!string.IsNullOrEmpty(entry.Content))
                {
                    entry.Content = await StripStyleAttributes(entry.Content);

                    /*
                    var context = BrowsingContext.New(Configuration.Default);
                    var document = await context.OpenAsync(req => req.Content(entry.Content));
                    //var blueListItemsLinq = document.QuerySelectorAll("*")
                    var ItemsLinq = document.All.Where(m => m.HasAttribute("style"));
                    foreach (var item in ItemsLinq)
                    {
                        item.RemoveAttribute("style");
                    }

                    Console.WriteLine(document.DocumentElement.OuterHtml);

                    entry.Content = document.DocumentElement.OuterHtml;
                    */
                }

            }
            else
            {
                //Debug.WriteLine("RSS Content NOT null");

                string contype = SynItem.Content.Type;
                if (!string.IsNullOrEmpty(contype))
                {

                    // Content-Types for RSS is messed up. So let's ignore.
                    /*
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
                    */
                }

                // force html
                entry.ContentType = EntryFull.ContentTypes.textHtml;

                entry.Content = SynItem.Content.ToString();
            }

            entry.Status = EntryItem.EntryStatus.esNormal;


            return entry;
        }

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

            Console.WriteLine(document.DocumentElement.OuterHtml);

            return document.DocumentElement.OuterHtml;

        }
    }
}
