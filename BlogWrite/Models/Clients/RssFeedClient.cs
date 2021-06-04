using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Syndication;
using System.Xml;
using System.IO;
using System.Diagnostics;

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
                    + s + Environment.NewLine);

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
                    System.Diagnostics.Debug.WriteLine("LoadXml failed: " + e.Message);

                    ToDebugWindow("<< Invalid XML returned:"
                        + Environment.NewLine
                        + e.Message
                        + Environment.NewLine);
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
            }

            return list;
        }


        public void FillEntryItemFromSynItem(EntryItem entItem, SyndicationItem SynItem)
        {

            AtomEntry entry = CreateAtomEntryFromSynItem(SynItem);

            entItem.Name = entry.Name;
            //entItem.ID = entry.ID;
            entItem.EntryID = entry.EntryID;
            entItem.EditUri = entry.EditUri;
            entItem.AltHTMLUri = entry.AltHTMLUri;
            entItem.EntryBody = entry;

            entItem.Status = entry.Status;

        }

        private AtomEntry CreateAtomEntryFromSynItem(SyndicationItem SynItem)
        {
            
            string relAttr;
            string hrefAttr;
            string typeAttr;
            //Uri editUri = null;
            Uri altUri = null;

            foreach (SyndicationLink u in SynItem.Links)
            {
                relAttr = (u.RelationshipType != null) ? u.RelationshipType : "";
                hrefAttr = (u.Uri != null) ? u.Uri.AbsoluteUri : "";
                typeAttr = (u.MediaType != null) ? u.MediaType : "";

                if (!string.IsNullOrEmpty(hrefAttr))
                {
                    
                    switch (relAttr)
                    {
                        /*
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
                         */
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

            AtomEntry entry = new AtomEntry("", this);

            entry.Name = (SynItem.Title.Text != null) ? SynItem.Title.Text : "";
            entry.EntryID = (SynItem.Id != null) ? SynItem.Id : "";
            //entry.EditUri = editUri;
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

    }
}
