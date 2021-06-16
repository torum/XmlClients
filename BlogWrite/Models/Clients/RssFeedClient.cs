using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Diagnostics;
using AngleSharp;
using BlogWrite.Common;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Collections.ObjectModel;

namespace BlogWrite.Models.Clients
{
    // RSS Feed Client - Implements RSS 2.0 and 1.0 as well.
    class RssFeedClient : BaseClient
    {
        public override async Task<HttpClientEntryItemCollectionResultWrapper> GetEntries(Uri entriesUrl, string feedId)
        {
            HttpClientEntryItemCollectionResultWrapper res = new HttpClientEntryItemCollectionResultWrapper();
            
            ObservableCollection<EntryItem> list = new ObservableCollection<EntryItem>();
            res.Entries = list;

            if (!(entriesUrl.Scheme.Equals("http") || entriesUrl.Scheme.Equals("https")))
            {
                ToDebugWindow("<< Invalid URI scheme:"
                    + Environment.NewLine
                    + entriesUrl.Scheme
                    + Environment.NewLine);

                InvalidUriScheme(res.Error, entriesUrl.Scheme, "RssFeedClient: GetEntries");
                res.IsError = true;

                return res;
            }

            try
            {
                var HTTPResponseMessage = await _HTTPConn.Client.GetAsync(entriesUrl);

                if (HTTPResponseMessage.IsSuccessStatusCode)
                {
                    string s = await HTTPResponseMessage.Content.ReadAsStringAsync();

                    ToDebugWindow(">> HTTP Request GET "
                        + entriesUrl.AbsoluteUri
                        + Environment.NewLine + Environment.NewLine
                        + "<< HTTP Response " + HTTPResponseMessage.StatusCode.ToString()
                        + Environment.NewLine
                        + s + Environment.NewLine);
                    
                    var source = await HTTPResponseMessage.Content.ReadAsStreamAsync();
                    
                    // Load XML
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

                        InvalidXml(res.Error, e.Message, "RssFeedClient: GetEntries");
                        res.IsError = true;

                        return res;
                    }

                    // RSS 2.0
                    if (xdoc.DocumentElement.LocalName.Equals("rss"))
                    {
                        XmlNodeList entryList;
                        entryList = xdoc.SelectNodes("//rss/channel/item");
                        if (entryList == null)
                        {
                            res.Entries = list;

                            return res;
                        }

                        foreach (XmlNode l in entryList)
                        {
                            EntryItem ent = new EntryItem("", feedId, this);
                            ent.Status = EntryItem.EntryStatus.esNormal;

                            FillEntryItemFromXmlRss(ent, l);

                            if (!string.IsNullOrEmpty(ent.EntryId))
                                list.Add(ent);
                        }
                    }
                    // RSS 1.0
                    else if (xdoc.DocumentElement.LocalName.Equals("RDF"))
                    {
                        XmlNamespaceManager NsMgr = new XmlNamespaceManager(xdoc.NameTable);
                        NsMgr.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
                        NsMgr.AddNamespace("rss", "http://purl.org/rss/1.0/");
                        NsMgr.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");

                        XmlNodeList entryList = xdoc.SelectNodes("//rdf:RDF/rss:item", NsMgr);
                        if (entryList == null)
                        {
                            res.Entries = list;

                            return res;
                        }

                        foreach (XmlNode l in entryList)
                        {
                            EntryItem ent = new EntryItem("", feedId, this);
                            ent.Status = EntryItem.EntryStatus.esNormal;

                            FillEntryItemFromXmlRdf(ent, l, NsMgr);

                            if (!string.IsNullOrEmpty(ent.EntryId))
                                list.Add(ent);
                        }
                    }
                    else
                    {
                        FormatUndetermined(res.Error, "RssFeedClient:GetEntries");
                        res.IsError = true;

                        return res;
                    }
                }
                // HTTP non 200 status code.
                else
                {
                    var contents = await HTTPResponseMessage.Content.ReadAsStringAsync();

                    if (contents != null)
                    {
                        ToDebugWindow(">> HTTP Request GET "
                            + entriesUrl.AbsoluteUri
                            + Environment.NewLine + Environment.NewLine
                            + "<< HTTP Response " + HTTPResponseMessage.StatusCode.ToString()
                            + Environment.NewLine
                            + contents + Environment.NewLine);
                    }

                    NonSuccessStatusCode(res.Error, HTTPResponseMessage.StatusCode.ToString(), "_HTTPConn.Client.GetAsync", "RssFeedClient:GetEntries");
                    res.IsError = true;

                    return res;
                }

            }
            // Internet connection errors
            catch (System.Net.Http.HttpRequestException e)
            {
                Debug.WriteLine("<< HttpRequestException: " + e.Message);

                ToDebugWindow(" << HttpRequestException: "
                    + Environment.NewLine
                    + e.Message
                    + Environment.NewLine);

                HttpReqException(res.Error, e.Message, "_HTTPConn.Client.GetAsync", "RssFeedClient:GetEntries");
                res.IsError = true;

                return res;
            }
            catch (Exception e)
            {
                Debug.WriteLine("HTTP error: " + e.Message);

                ToDebugWindow("<< HTTP error:"
                    + Environment.NewLine
                    + e.Message
                    + Environment.NewLine);

                GenericException(res.Error, "", ErrorObject.ErrTypes.HTTP, "HTTP request error (Exception)", e.Message, "_HTTPConn.Client.GetAsync", "RssFeedClient:GetEntries");
                res.IsError = true;

                return res;
            }

            return res;
        }

        private async void FillEntryItemFromXmlRss(EntryItem entItem, XmlNode entryNode)
        {
            XmlNode entryTitle = entryNode.SelectSingleNode("title");
            entItem.Name = (entryTitle != null) ? entryTitle.InnerText : "";

            XmlNode entryId = entryNode.SelectSingleNode("guid");
            entItem.EntryId = (entryId != null) ? entryId.InnerText : "";

            XmlNode entryLinkUri = entryNode.SelectSingleNode("link");
            try
            {
                if (entryLinkUri != null)
                    if (!string.IsNullOrEmpty(entryLinkUri.InnerText))
                        entItem.AltHtmlUri = new Uri(entryLinkUri.InnerText);
            }
            catch { }

            if (string.IsNullOrEmpty(entItem.EntryId))
                if (entItem.AltHtmlUri != null)
                    entItem.EntryId = entItem.AltHtmlUri.AbsoluteUri;

            XmlNode entryPudDate = entryNode.SelectSingleNode("pubDate");
            if (entryPudDate != null)
            {
                string s = entryPudDate.InnerText;
                if (!string.IsNullOrEmpty(s))
                {
                    try
                    {
                        DateTimeOffset dtf = DateTimeParser.ParseDateTimeRFC822(s);

                        entItem.Published = dtf.ToUniversalTime().DateTime;
                    }
                    catch {}
                }
            }

            // Force textHtml for RSS feed. Even though description was missing. (needs this for browser)
            entItem.ContentType = EntryItem.ContentTypes.textHtml;

            XmlNode sum = entryNode.SelectSingleNode("description");
            if (sum != null)
            {
                // Content
                entItem.Content = sum.InnerText;

                // Summary
                entItem.Summary = await StripStyleAttributes(sum.InnerText);
                
                if (!string.IsNullOrEmpty(sum.InnerText))
                {
                    entItem.SummaryPlainText = await StripHtmlTags(sum.InnerText);

                    entItem.SummaryPlainText = Truncate(entItem.SummaryPlainText, 78);
                }

                // gets image Uri
                entItem.ImageUri = await GetImageUriFromHtml(entItem.Content);
            }
        }

        private async void FillEntryItemFromXmlRdf(EntryItem entItem, XmlNode entryNode, XmlNamespaceManager NsMgr)
        {
            XmlNode entryTitle = entryNode.SelectSingleNode("rss:title", NsMgr);
            entItem.Name = (entryTitle != null) ? entryTitle.InnerText : "";

            XmlNode entryLinkUri = entryNode.SelectSingleNode("rss:link", NsMgr);
            try
            {
                if (entryLinkUri != null)
                    if (!string.IsNullOrEmpty(entryLinkUri.InnerText))
                        entItem.AltHtmlUri = new Uri(entryLinkUri.InnerText);
            }
            catch { }

            if (string.IsNullOrEmpty(entItem.EntryId))
                if (entItem.AltHtmlUri != null)
                    entItem.EntryId = entItem.AltHtmlUri.AbsoluteUri;

            XmlNode entryPudDate = entryNode.SelectSingleNode("dc:date", NsMgr);
            if (entryPudDate != null)
            {
                string s = entryPudDate.InnerText;
                if (!string.IsNullOrEmpty(s))
                {
                    try
                    {
                        entItem.Published = DateTime.Parse(s, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    }
                    catch { }
                }
            }

            // Force textHtml for RSS feed. Even though description was missing. (needs this for browser)
            entItem.ContentType = EntryItem.ContentTypes.textHtml;

            XmlNode sum = entryNode.SelectSingleNode("rss:description", NsMgr);
            if (sum != null)
            {
                // Content
                entItem.Content = sum.InnerText;

                // Summary
                entItem.Summary = await StripStyleAttributes(sum.InnerText);

                if (!string.IsNullOrEmpty(sum.InnerText))
                {
                    entItem.SummaryPlainText = await StripHtmlTags(sum.InnerText);

                    entItem.SummaryPlainText = Truncate(entItem.SummaryPlainText, 78);
                }

                // gets image Uri
                entItem.ImageUri = await GetImageUriFromHtml(entItem.Content);
            }
        }
    }
}
