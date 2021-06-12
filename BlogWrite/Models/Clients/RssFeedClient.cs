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
using BlogWrite.Common;

namespace BlogWrite.Models.Clients
{
    class RssFeedClient : BaseClient
    {
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

                        ClientErrorMessage = "Invalid XML document returned: " + e.Message;

                        return list;
                    }

                    // RSS 2.0
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
                    // RSS 1.0
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
                // HTTP non 200 status code.
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
            // Internet connection errors
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
                    catch { }
                }
            }

            XmlNode sum = entryNode.SelectSingleNode("description");
            if (sum != null)
            {
                entItem.Summary = await StripStyleAttributes(sum.InnerText);

                if (!string.IsNullOrEmpty(sum.InnerText))
                {
                    entItem.SummaryPlainText = await StripHtmlTags(sum.InnerText);

                    entItem.SummaryPlainText = Truncate(entItem.SummaryPlainText, 78);
                }
            }
        }

        public async void FillEntryItemFromXmlRdf(EntryItem entItem, XmlNode entryNode, XmlNamespaceManager NsMgr)
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

            XmlNode sum = entryNode.SelectSingleNode("rss:description", NsMgr);
            if (sum != null)
            {
                entItem.Summary = await StripStyleAttributes(sum.InnerText);
                //entry.ContentType = EntryFull.ContentTypes.textHtml;

                if (!string.IsNullOrEmpty(sum.InnerText))
                {
                    entItem.SummaryPlainText = await StripHtmlTags(sum.InnerText);

                    entItem.SummaryPlainText = Truncate(entItem.SummaryPlainText, 78);
                }
            }
        }
    }
}
