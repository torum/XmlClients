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
    // Feed Client - reads Atom 1.0. 0.3, RSS 2.0 and 1.0.
    public class FeedClient : BaseClient
    {
        public override async Task<HttpClientEntryItemCollectionResultWrapper> GetEntries(Uri entriesUrl, string feedId)
        {
            HttpClientEntryItemCollectionResultWrapper res = new HttpClientEntryItemCollectionResultWrapper();
            
            List<EntryItem> list = new List<EntryItem>();
            res.Entries = list;

            if (!(entriesUrl.Scheme.Equals("http") || entriesUrl.Scheme.Equals("https")))
            {
                ToDebugWindow("<< Invalid URI scheme:"
                    + Environment.NewLine
                    + entriesUrl.Scheme
                    + Environment.NewLine);

                InvalidUriScheme(res.Error, entriesUrl.Scheme, "FeedClient: GetEntries");
                res.IsError = true;

                return res;
            }

            try
            {
                var HTTPResponseMessage = await _HTTPConn.Client.GetAsync(entriesUrl);

                if (HTTPResponseMessage.IsSuccessStatusCode)
                {
                    //string s = await HTTPResponseMessage.Content.ReadAsStringAsync();
                    /*
                    ToDebugWindow(">> HTTP Request: GET "
                        + entriesUrl.AbsoluteUri
                        + Environment.NewLine
                        + "<< HTTP Response " + HTTPResponseMessage.StatusCode.ToString()
                        //+ Environment.NewLine + s + Environment.NewLine);
                        + Environment.NewLine);
                    */

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
                        ToDebugWindow("<< Invalid XML document returned: " + entriesUrl.AbsoluteUri
                            + Environment.NewLine
                            + e.Message
                            + Environment.NewLine); ;

                        InvalidXml(res.Error, e.Message, "FeedClient: GetEntries");
                        res.IsError = true;

                        return res;
                    }

                    // RSS 2.0
                    if (xdoc.DocumentElement.LocalName.Equals("rss"))
                    {
                        XmlNamespaceManager NsMgr = new XmlNamespaceManager(xdoc.NameTable);
                        NsMgr.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");

                        XmlNodeList entryList;
                        entryList = xdoc.SelectNodes("//rss/channel/item");
                        if (entryList == null)
                        {
                            res.Entries = list;

                            return res;
                        }

                        foreach (XmlNode l in entryList)
                        {
                            FeedEntryItem ent = new FeedEntryItem("", feedId, this);

                            FillEntryItemFromXmlRss(ent, l, NsMgr);

                            if (!string.IsNullOrEmpty(ent.EntryId))
                                list.Add(ent);
                        }

                        // 
                        //await GetImages(list);
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
                            FeedEntryItem ent = new FeedEntryItem("", feedId, this);

                            FillEntryItemFromXmlRdf(ent, l, NsMgr);

                            if (!string.IsNullOrEmpty(ent.EntryId))
                                list.Add(ent);
                        }

                        // 
                        //await GetImages(list);
                    }
                    else if (xdoc.DocumentElement.LocalName.Equals("feed"))
                    {

                        if (xdoc.DocumentElement.NamespaceURI.Equals("http://www.w3.org/2005/Atom"))
                        {
                            XmlNamespaceManager atomNsMgr = new XmlNamespaceManager(xdoc.NameTable);
                            atomNsMgr.AddNamespace("atom", "http://www.w3.org/2005/Atom");
                            atomNsMgr.AddNamespace("app", "http://www.w3.org/2007/app");

                            XmlNodeList entryList;
                            entryList = xdoc.SelectNodes("//atom:feed/atom:entry", atomNsMgr);
                            if (entryList == null)
                            {
                                res.Entries = list;

                                return res;
                            }

                            foreach (XmlNode l in entryList)
                            {
                                FeedEntryItem ent = new FeedEntryItem("", feedId, this);
                                //ent.Status = EditEntryItem.EditStatus.esNormal;

                                FillEntryItemFromXmlAtom10(ent, l, atomNsMgr, feedId);

                                if (!string.IsNullOrEmpty(ent.EntryId))
                                    list.Add(ent);
                            }

                            // 
                            //await GetImages(list);
                        }
                        // Old Atom 0.3
                        else if (xdoc.DocumentElement.NamespaceURI.Equals("http://purl.org/atom/ns#"))
                        {
                            XmlNamespaceManager atomNsMgr = new XmlNamespaceManager(xdoc.NameTable);
                            atomNsMgr.AddNamespace("atom", "http://purl.org/atom/ns#");
                            atomNsMgr.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");

                            XmlNodeList entryList;
                            entryList = xdoc.SelectNodes("//atom:feed/atom:entry", atomNsMgr);
                            if (entryList == null)
                            {
                                res.Entries = list;

                                return res;
                            }

                            foreach (XmlNode l in entryList)
                            {
                                FeedEntryItem ent = new FeedEntryItem("", feedId, this);
                                //ent.Status = EditEntryItem.EditStatus.esNormal;

                                FillEntryItemFromXmlAtom03(ent, l, atomNsMgr);

                                if (!string.IsNullOrEmpty(ent.EntryId))
                                    list.Add(ent);
                            }

                            // 
                            //await GetImages(list);

                            ToDebugWindow("<< Old version of Atom feed format detected. Update recommended: " + entriesUrl.AbsoluteUri
                                + Environment.NewLine);
                        }
                        else
                        {
                            ToDebugWindow("<< FormatUndetermined @FeedClient:GetEntries@xdoc.DocumentElement.NamespaceURI.Equals"
                                + Environment.NewLine);

                            FormatUndetermined(res.Error, "FeedClient:GetEntries");
                            res.IsError = true;

                            return res;
                        }
                    }
                    else
                    {
                        ToDebugWindow("<< FormatUndetermined @FeedClient:GetEntries:xdoc.DocumentElement.LocalName/NamespaceURI"
                            + Environment.NewLine);

                        FormatUndetermined(res.Error, "FeedClient:GetEntries");
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
                        ToDebugWindow(">> HTTP Request: GET "
                            + entriesUrl.AbsoluteUri
                            + Environment.NewLine
                            + "<< HTTP Response " + HTTPResponseMessage.StatusCode.ToString()
                            + Environment.NewLine
                            + contents + Environment.NewLine);
                    }

                    NonSuccessStatusCode(res.Error, HTTPResponseMessage.StatusCode.ToString(), "_HTTPConn.Client.GetAsync", "FeedClient:GetEntries");
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

                HttpReqException(res.Error, e.Message, "_HTTPConn.Client.GetAsync", "FeedClient:GetEntries");
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

                GenericException(res.Error, "", ErrorObject.ErrTypes.HTTP, "HTTP request error (Exception)", e.Message, "_HTTPConn.Client.GetAsync", "FeedClient:GetEntries");
                res.IsError = true;

                return res;
            }

            return res;
        }

        private async void FillEntryItemFromXmlRss(FeedEntryItem entItem, XmlNode entryNode, XmlNamespaceManager NsMgr)
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
            catch (Exception e)
            {
                ToDebugWindow(">> Exception @FeedClient@FillEntryItemFromXmlRss:new Uri()"
                    + Environment.NewLine +
                    "RSS feed entry (" + entItem.Name + ") contain invalid entry Uri: " + e.Message +
                    Environment.NewLine);
            }

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
                    catch (Exception e)
                    {
                       // Debug.WriteLine("Exception @ParseDateTimeRFC822 in the RSS 2.0 feed " + "("+ entItem.Name  + ")" + " : " + e.Message);

                        ToDebugWindow(">> Exception @FeedClient@FillEntryItemFromXmlRss:ParseDateTimeRFC822()"
                            + Environment.NewLine +
                            "RSS feed entry(" + entItem.Name + ") contain invalid entry pubDate (DateTimeRFC822 expected): " + e.Message +
                            Environment.NewLine);
                    }
                }
            }

            string entryAuthor = "";
            XmlNodeList entryAuthors = entryNode.SelectNodes("dc:creator", NsMgr);
            if (entryAuthors != null)
            {
                foreach (XmlNode auth in entryAuthors)
                {
                    if (string.IsNullOrEmpty(entryAuthor))
                        entryAuthor = auth.InnerText;
                    else
                        entryAuthor += "/" + auth.InnerText;
                }
            }

            if (string.IsNullOrEmpty(entryAuthor))
            {
                if (entItem.AltHtmlUri != null)
                    entryAuthor = entItem.AltHtmlUri.Host;
            }

            entItem.Author = entryAuthor;

            // gets imageUri from enclosure
            XmlNode enclosure = entryNode.SelectSingleNode("enclosure");
            if (enclosure != null)
            {
                if (enclosure.Attributes["url"] != null)
                {
                    string urlImage = enclosure.Attributes["url"].Value;

                    if (enclosure.Attributes["type"] != null)
                    {
                        string imageType = enclosure.Attributes["type"].Value;

                        if ((imageType == "image/jpg") || (imageType == "image/jpeg")  || (imageType == "image/png") || (imageType == "image/gif"))
                        {
                            try
                            {
                                entItem.ImageUri = new Uri(urlImage);
                            }
                            catch (Exception e)
                            {
                                ToDebugWindow(">> Exception @FeedClient@FillEntryItemFromXmlRss:new Uri()"
                                    + Environment.NewLine +
                                    "RSS feed entry (" + entItem.Name + ") contain invalid entry > enclosure@link Uri: " + e.Message +
                                    Environment.NewLine);
                            }
                        }
                    }
                }
            }

            // Force textHtml for RSS feed. Even though description was missing. (needs this for browser)
            entItem.ContentType = EntryItem.ContentTypes.textHtml;

            XmlNode sum = entryNode.SelectSingleNode("description");
            if (sum != null)
            {
                // Content
                entItem.Content = await StripStyleAttributes(sum.InnerText);

                if (!string.IsNullOrEmpty(entItem.Content))
                {
                    // Summary
                    entItem.Summary = await StripHtmlTags(entItem.Content);
                    //entItem.Summary = Truncate(entItem.Summary, 230);

                    //entItem.SummaryPlainText = Truncate(entItem.Summary, 78);

                    // gets image Uri
                    if (entItem.ImageUri == null)
                        entItem.ImageUri = await GetImageUriFromHtml(entItem.Content);
                }
            }

            entItem.Status = FeedEntryItem.ReadStatus.rsNew;
        }

        private async void FillEntryItemFromXmlRdf(FeedEntryItem entItem, XmlNode entryNode, XmlNamespaceManager NsMgr)
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
            catch (Exception e)
            {
                Debug.WriteLine("Exception @new Uri(entryLinkUri.InnerText)" + "(" + entItem.Name + ")" + " : " + e.Message);

                ToDebugWindow(">> Exception @RssFeedClient@FillEntryItemFromXmlRdf:new Uri()"
                    + Environment.NewLine +
                    "RSS feed entry (" + entItem.Name + ") contain invalid entry Uri: " + e.Message +
                    Environment.NewLine);
            }

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
                    catch (Exception e)
                    {
                        //Debug.WriteLine("Exception @DateTime.Parse in the RSS 1.0 feed " + "(" + entItem.Name + ")" + " : " + e.Message);

                        ToDebugWindow(">> Exception @FeedClient@FillEntryItemFromXmlRdf: DateTime.Parse()"
                            + Environment.NewLine +
                            "RSS feed entry(" + entItem.Name + ") contain invalid entry dc:date: " + e.Message +
                            Environment.NewLine);
                    }
                }
            }

            string entryAuthor = "";
              XmlNodeList entryAuthors = entryNode.SelectNodes("dc:creator", NsMgr);
            if (entryAuthors != null)
            {
                foreach (XmlNode auth in entryAuthors)
                {
                    if (string.IsNullOrEmpty(entryAuthor))
                        entryAuthor = auth.InnerText;
                    else
                        entryAuthor += "/" + auth.InnerText;
                }
            }

            if (string.IsNullOrEmpty(entryAuthor))
            {
                if (entItem.AltHtmlUri != null)
                    entryAuthor = entItem.AltHtmlUri.Host;
            }

            entItem.Author = entryAuthor;

            // Force textHtml for RSS feed. Even though description was missing. (needs this for browser)
            entItem.ContentType = EntryItem.ContentTypes.textHtml;

            XmlNode sum = entryNode.SelectSingleNode("rss:description", NsMgr);
            if (sum != null)
            {
                // Content
                entItem.Content = await StripStyleAttributes(sum.InnerText);

                if (!string.IsNullOrEmpty(entItem.Content))
                {
                    // Summary
                    entItem.Summary = await StripHtmlTags(entItem.Content);

                    //entItem.SummaryPlainText = Truncate(entItem.SummaryPlainText, 78);
                }

                // gets image Uri
                entItem.ImageUri = await GetImageUriFromHtml(entItem.Content);
            }

            entItem.Status = FeedEntryItem.ReadStatus.rsNew;
        }

        private async void FillEntryItemFromXmlAtom03(FeedEntryItem entItem, XmlNode entryNode, XmlNamespaceManager atomNsMgr)
        {
            XmlNode entryTitle = entryNode.SelectSingleNode("atom:title", atomNsMgr);
            if (entryTitle != null)
            {
                entItem.Name = entryTitle.InnerText;
            }

            XmlNode entryID = entryNode.SelectSingleNode("atom:id", atomNsMgr);
            if (entryID != null)
            {
                entItem.EntryId = entryID.InnerText;
            }

            XmlNodeList entryLinkUris = entryNode.SelectNodes("atom:link", atomNsMgr);
            string relAttr;
            string hrefAttr;
            string typeAttr;

            Uri altUri = null;

            if (entryLinkUris != null)
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
                            case "service.post":
                                try
                                {
                                    //editUri = new Uri(hrefAttr);
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
                                            try
                                            {
                                                altUri = new Uri(hrefAttr);
                                            }
                                            catch (Exception e)
                                            {
                                                Debug.WriteLine("Exception @new Uri(altUri) @ FeedClient Atom0.3" + "(" + entItem.Name + ")" + " : " + e.Message);
                                            }
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
                                            try
                                            {
                                                altUri = new Uri(hrefAttr);
                                            }
                                            catch (Exception e)
                                            {
                                                Debug.WriteLine("Exception @new Uri(altUri) @ FeedClient Atom0.3" + "(" + entItem.Name + ")" + " : " + e.Message);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            // I am not happy but let's assume it is html.
                                            altUri = new Uri(hrefAttr);
                                        }
                                        catch (Exception e)
                                        {
                                            Debug.WriteLine("Exception @new Uri(altUri) @ FeedClient Atom0.3" + "(" + entItem.Name + ")" + " : " + e.Message);
                                        }
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

            //entItem.EditUri = editUri;
            entItem.AltHtmlUri = altUri;

            XmlNode entryPublished = entryNode.SelectSingleNode("atom:issued", atomNsMgr);
            if (entryPublished != null)
            {
                if (!string.IsNullOrEmpty(entryPublished.InnerText))
                {
                    try
                    {
                        entItem.Published = XmlConvert.ToDateTime(entryPublished.InnerText, XmlDateTimeSerializationMode.Local);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Exception @XmlConvert.ToDateTime in the Atom 0.3 feed " + "(" + entItem.Name + ")" + " : " + e.Message);
                    }
                }
            }

            string entryAuthor = "";
            XmlNodeList entryAuthors = entryNode.SelectNodes("atom:author", atomNsMgr);
            if (entryAuthors != null)
            {
                foreach (XmlNode auth in entryAuthors)
                {
                    XmlNode authName = auth.SelectSingleNode("atom:name", atomNsMgr);
                    if (authName != null)
                    {
                        if (string.IsNullOrEmpty(entryAuthor))
                            entryAuthor = authName.InnerText;
                        else
                            entryAuthor += "/" + authName.InnerText;
                    }
                }
            }

            if (string.IsNullOrEmpty(entryAuthor))
            {
                if (altUri != null)
                    entryAuthor = altUri.Host;
            }

            entItem.Author = entryAuthor;

            XmlNode cont = entryNode.SelectSingleNode("atom:content", atomNsMgr);
            if (cont == null)
            {
                string contype = cont.Attributes["type"].Value;
                if (!string.IsNullOrEmpty(contype))
                {
                    //entItem.ContentTypeString = contype;

                    switch (contype)
                    {
                        case "text":
                            entItem.ContentType = EntryItem.ContentTypes.text;
                            break;
                        case "html":
                            entItem.ContentType = EntryItem.ContentTypes.textHtml;
                            break;
                        case "xhtml":
                            entItem.ContentType = EntryItem.ContentTypes.textHtml;
                            break;
                        case "text/plain":
                            entItem.ContentType = EntryItem.ContentTypes.text;
                            break;
                        case "text/html":
                            entItem.ContentType = EntryItem.ContentTypes.textHtml;
                            break;
                        case "text/x-markdown":
                            entItem.ContentType = EntryItem.ContentTypes.markdown;
                            break;
                        case "text/x-hatena-syntax":
                            entItem.ContentType = EntryItem.ContentTypes.hatena;
                            break;
                        default:
                            entItem.ContentType = EntryItem.ContentTypes.text;
                            break;
                    }
                }

                entItem.Content = cont.InnerText;
            }

            XmlNode sum = entryNode.SelectSingleNode("atom:summary", atomNsMgr);
            if (sum != null)
            {
                entItem.Summary = await StripStyleAttributes(sum.InnerText);
                //entry.ContentType = EntryFull.ContentTypes.textHtml;

                if (!string.IsNullOrEmpty(entItem.Summary))
                {
                    entItem.Summary = await StripHtmlTags(entItem.Summary);

                    //entItem.SummaryPlainText = Truncate(sum.InnerText, 78);
                }
            }
            else
            {
                string s = entItem.Content;

                if (!string.IsNullOrEmpty(s))
                {
                    if (entItem.ContentType == EntryItem.ContentTypes.textHtml)
                    {
                        entItem.Summary = await StripHtmlTags(s);
                        //entItem.SummaryPlainText = Truncate(s, 78);
                    }
                    else if (entItem.ContentType == EntryItem.ContentTypes.text)
                    {
                        entItem.Summary = s;
                        //entItem.SummaryPlainText = Truncate(s, 78);
                    }
                }
            }

            entItem.Status = FeedEntryItem.ReadStatus.rsNew;

            if (entItem.ContentType == EntryItem.ContentTypes.textHtml)
            {
                // gets image Uri
                entItem.ImageUri = await GetImageUriFromHtml(entItem.Content);
            }
        }

        private async void FillEntryItemFromXmlAtom10(FeedEntryItem entItem, XmlNode entryNode, XmlNamespaceManager atomNsMgr, string feedId)
        {
            // TODO:
            AtomEntry entry = await CreateAtomEntryFromXmlAtom(entryNode, atomNsMgr, feedId);

            entItem.Name = entry.Name;
            //entItem.ID = entry.ID;
            entItem.EntryId = entry.EntryId;
            //entItem.EditUri = entry.EditUri;
            entItem.AltHtmlUri = entry.AltHtmlUri;
            entItem.Published = entry.Published;
            entItem.Author = entry.Author;
            entItem.Summary = entry.Summary;
            //entItem.SummaryPlainText = entry.SummaryPlainText;
            entItem.Content = entry.Content;
            entItem.ContentType = entry.ContentType;
            // entItem.EntryBody = entry;

            entItem.Status = FeedEntryItem.ReadStatus.rsNew;

            if (entItem.ContentType == EntryItem.ContentTypes.textHtml)
            {
                // gets image Uri
                if (entItem.ImageUri == null)
                    entItem.ImageUri = await GetImageUriFromHtml(entItem.Content);
            }
        }

        // TODO:
        private async Task<AtomEntry> CreateAtomEntryFromXmlAtom(XmlNode entryNode, XmlNamespaceManager atomNsMgr, string feedId)
        {
            AtomEntry entry = new AtomEntry("", feedId, this);

            XmlNode entryTitle = entryNode.SelectSingleNode("atom:title", atomNsMgr);
            if (entryTitle != null)
            {
                entry.Name = entryTitle.InnerText;
            }

            XmlNode entryID = entryNode.SelectSingleNode("atom:id", atomNsMgr);
            if (entryID != null)
            {
                entry.EntryId = entryID.InnerText;
            }

            XmlNodeList entryLinkUris = entryNode.SelectNodes("atom:link", atomNsMgr);
            string relAttr;
            string hrefAttr;
            string typeAttr;
            Uri editUri = null;
            Uri altUri = null;
            if (entryLinkUris != null)
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

                                catch (Exception e)
                                {
                                    Debug.WriteLine("Exception @new Uri(editUri) @ FeedClient Atom1.0" + "(" + entry.Name + ")" + " : " + e.Message);

                                    ToDebugWindow(">> Exception @FeedClient@CreateAtomEntryFromXmlAtom@ new Uri(editUri)"
                                        + Environment.NewLine +
                                        "Atom feed entry(" + entry.Name + ") contain invalid entry atom:editUri: " + e.Message +
                                        Environment.NewLine);

                                    break;
                                }
                            case "alternate":
                                try
                                {
                                    if (!string.IsNullOrEmpty(typeAttr))
                                    {
                                        if (typeAttr == "text/html")
                                        {
                                            try
                                            {
                                                altUri = new Uri(hrefAttr);
                                            }
                                            catch (Exception e)
                                            {
                                                Debug.WriteLine("Exception @new Uri(altUri) @ FeedClient Atom1.0" + "(" + entry.Name + ")" + " : " + e.Message);

                                                ToDebugWindow(">> Exception @FeedClient@CreateAtomEntryFromXmlAtom@ new Uri(altUri)"
                                                    + Environment.NewLine +
                                                    "Atom feed entry(" + entry.Name + ") contain invalid entry atom:altUri: " + e.Message +
                                                    Environment.NewLine);
                                            }
                                        }
                                    }
                                    else if (string.IsNullOrEmpty(typeAttr))
                                    {
                                        try
                                        {
                                            // let's assume it is html.
                                            altUri = new Uri(hrefAttr);
                                        }
                                        catch (Exception e)
                                        {
                                            Debug.WriteLine("Exception @new Uri(altUri) @ FeedClient Atom1.0" + "(" + entry.Name + ")" + " : " + e.Message);

                                            ToDebugWindow(">> Exception @FeedClient@CreateAtomEntryFromXmlAtom@ new Uri(altUri)"
                                                + Environment.NewLine +
                                                "Atom feed entry(" + entry.Name + ") contain invalid entry atom:altUri: " + e.Message +
                                                Environment.NewLine);
                                        }
                                    }
                                    break;
                                }
                                catch
                                {
                                    break;
                                }
                            case "enclosure":
                                try
                                {
                                    if (!string.IsNullOrEmpty(typeAttr))
                                    {
                                        if ((typeAttr == "image/jpg") || (typeAttr == "image/jpeg") || (typeAttr == "image/png") || (typeAttr == "image/gif"))
                                        {
                                            try
                                            {
                                                entry.ImageUri = new Uri(hrefAttr);
                                            }
                                            catch (Exception e)
                                            {
                                                ToDebugWindow(">> Exception @FeedClient@CreateAtomEntryFromXmlAtom:new Uri()"
                                                    + Environment.NewLine +
                                                    "Atom feed entry (" + entry.Name + ") contain invalid entry > enclosure@link Uri: " + e.Message +
                                                    Environment.NewLine);
                                            }
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
                                            try
                                            {
                                                altUri = new Uri(hrefAttr);
                                            }
                                            catch (Exception e)
                                            {
                                                Debug.WriteLine("Exception @new Uri(altUri) @ FeedClient Atom1.0" + "(" + entry.Name + ")" + " : " + e.Message);

                                                ToDebugWindow(">> Exception @FeedClient@CreateAtomEntryFromXmlAtom@ new Uri(altUri)"
                                                    + Environment.NewLine +
                                                    "Atom feed entry(" + entry.Name + ") contain invalid entry atom:altUri: " + e.Message +
                                                    Environment.NewLine);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            // I am not happy but let's assume it is html.
                                            altUri = new Uri(hrefAttr);
                                        }
                                        catch (Exception e)
                                        {
                                            Debug.WriteLine("Exception @new Uri(altUri) @ FeedClient Atom1.0" + "(" + entry.Name + ")" + " : " + e.Message);

                                            ToDebugWindow(">> Exception @FeedClient@CreateAtomEntryFromXmlAtom@ new Uri(altUri)"
                                                + Environment.NewLine +
                                                "Atom feed entry(" + entry.Name + ") contain invalid entry atom:altUri: " + e.Message +
                                                Environment.NewLine);
                                        }
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

            entry.EditUri = editUri;
            entry.AltHtmlUri = altUri;

            XmlNode entryPublished = entryNode.SelectSingleNode("atom:published", atomNsMgr);
            if (entryPublished != null)
            {
                if (!string.IsNullOrEmpty(entryPublished.InnerText))
                {
                    try
                    {
                        entry.Published = XmlConvert.ToDateTime(entryPublished.InnerText, XmlDateTimeSerializationMode.Utc);
                    }
                    catch (Exception e)
                    {
                        //Debug.WriteLine("Exception @XmlConvert.ToDateTime in the Atom 1.0 feed " + "(" + entry.Name + ")" + " : " + e.Message);

                        ToDebugWindow(">> Exception @FeedClient@CreateAtomEntryFromXmlAtom: XmlConvert.ToDateTime()"
                            + Environment.NewLine +
                            "Atom feed entry(" + entry.Name + ") contain invalid entry atom:published: " + e.Message +
                            Environment.NewLine);
                    }
                }
            }

            string entryAuthor = "";
            XmlNodeList entryAuthors = entryNode.SelectNodes("atom:author", atomNsMgr);
            if (entryAuthors != null)
            {
                foreach (XmlNode auth in entryAuthors)
                {
                    XmlNode authName = auth.SelectSingleNode("atom:name", atomNsMgr);
                    if (authName != null)
                    {
                        if (string.IsNullOrEmpty(entryAuthor))
                            entryAuthor = authName.InnerText;
                        else
                            entryAuthor += "/" + authName.InnerText;
                    }
                }
            }

            if (string.IsNullOrEmpty(entryAuthor))
            {
                if (altUri != null)
                    entryAuthor = altUri.Host;
            }

            entry.Author = entryAuthor;

            /*
<?xml version="1.0" encoding="utf-8"?>
<feed xmlns="http://www.w3.org/2005/Atom">
    <title type="text">dive into mark</title>
    <subtitle type="html"> A &lt;em&gt;lot&lt;/em&gt; of effort went into making this effortless</subtitle>
    <updated>2005-07-31T12:29:29Z</updated>
    <id>tag:example.org,2003:3</id>
    <link rel="alternate" type="text/html" hreflang="en" href="http://example.org/"/>
    <link rel="self" type="application/atom+xml" href="http://example.org/feed.atom"/>
    <rights>Copyright (c) 2003, Mark Pilgrim</rights>
    <generator uri="http://www.example.com/" version="1.0">Example Toolkit</generator>
    <entry>
        <title>Atom draft-07 snapshot</title>
        <link rel="alternate" type="text/html" href="http://example.org/2005/04/02/atom"/>
        <link rel="enclosure" type="audio/mpeg" length="1337" href="http://example.org/audio/ph34r_my_podcast.mp3"/>
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
        <content type="xhtml" xml:lang="en" xml:base="http://diveintomark.org/">
            <div xmlns="http://www.w3.org/1999/xhtml"><p><i>[Update: The Atom draft is finished.]</i></p></div>
        </content>
    </entry>
</feed>
            */

            // TODO:
            //updated
            //app:edited
            //category

            /*
            //app:control/app:draft(yes/no)
            XmlNode entryDraft = entryNode.SelectSingleNode("app:control/app:draft", atomNsMgr);
            if (entryDraft == null) System.Diagnostics.Debug.WriteLine("app:draft: is null.");

            string draft = entryDraft?.InnerText;
            entry.IsDraft = (String.Compare(draft, "yes", true) == 0) ? true : false;
            */

            /*
            XmlNode entryUpdated = entryNode.SelectSingleNode("atom:updated", atomNsMgr);
            if (entryUpdated != null)
            {
                if (!string.IsNullOrEmpty(entryUpdated.InnerText))
                {
                     = XmlConvert.ToDateTime(entryUpdated.InnerText, XmlDateTimeSerializationMode.Utc);
                }
            }
            */

            //AtomEntryHatena
            /*
            // Hatena's formatted-content
            XmlNode formattedContent = entryNode.SelectSingleNode("hatena:formatted-content", atomNsMgr);
            if (formattedContent != null)
            {
                entry.FormattedContent = formattedContent.InnerText;
            }
            */

            XmlNode cont = entryNode.SelectSingleNode("atom:content", atomNsMgr);
            if (cont != null)
            {
                string contype = cont.Attributes["type"].Value;
                if (!string.IsNullOrEmpty(contype))
                {
                    entry.ContentTypeString = contype;

                    switch (contype)
                    {
                        case "text":
                            entry.ContentType = EntryItem.ContentTypes.text;
                            break;
                        case "html":
                            entry.ContentType = EntryItem.ContentTypes.textHtml;
                            break;
                        case "xhtml":
                            entry.ContentType = EntryItem.ContentTypes.textHtml;
                            break;
                        case "text/plain":
                            entry.ContentType = EntryItem.ContentTypes.text;
                            break;
                        case "text/html":
                            entry.ContentType = EntryItem.ContentTypes.textHtml;
                            break;
                        case "text/x-markdown":
                            entry.ContentType = EntryItem.ContentTypes.markdown;
                            break;
                        case "text/x-hatena-syntax":
                            entry.ContentType = EntryItem.ContentTypes.hatena;
                            break;
                        default:
                            entry.ContentType = EntryItem.ContentTypes.text;
                            break;
                    }
                }

                entry.Content = cont.InnerText;
            }

            XmlNode sum = entryNode.SelectSingleNode("atom:summary", atomNsMgr);
            if (sum != null)
            {
                entry.Summary = await StripStyleAttributes(sum.InnerText);
                //entry.ContentType = EntryFull.ContentTypes.textHtml;

                if (!string.IsNullOrEmpty(entry.Summary))
                {
                    entry.Summary = await StripHtmlTags(entry.Summary);

                    //entItem.SummaryPlainText = Truncate(sum.InnerText, 78);
                }
            }
            else
            {
                string s = entry.Content;

                if (!string.IsNullOrEmpty(s))
                {
                    if (entry.ContentType == EntryItem.ContentTypes.textHtml)
                    {
                        entry.Summary = await StripHtmlTags(s);
                        //entItem.SummaryPlainText = Truncate(s, 78);
                    }
                    else if (entry.ContentType == EntryItem.ContentTypes.text)
                    {
                        entry.Summary = s;
                        //entItem.SummaryPlainText = Truncate(s, 78);
                    }
                }
            }

            entry.Status = AtomEntry.EditStatus.esNormal;

            return entry;
        }
    }
}
