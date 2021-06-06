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
