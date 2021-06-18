using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System.Collections.ObjectModel;

namespace BlogWrite.Models.Clients
{
    // Atom Feed Client - Implements Atom Syndication Format (reads Atom0.3 as well)
    class AtomFeedClient : BaseClient
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

                InvalidUriScheme(res.Error, entriesUrl.Scheme, "AtomFeedClient: GetEntries");
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
                        //xdoc.LoadXml(s);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine("LoadXml failed: " + e.Message);

                        ToDebugWindow("<< Invalid XML returned:"
                            + Environment.NewLine
                            + e.Message
                            + Environment.NewLine);

                        InvalidXml(res.Error, e.Message, "AtomFeedClient: GetEntries");
                        res.IsError = true;

                        return res;
                    }

                    // Atom Format
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
                    }
                    else
                    {
                        FormatUndetermined(res.Error, "AtomFeedClient:GetEntries");
                        res.IsError = true;

                        return res;
                    }
                }
                // HTTP non 200 status code
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

                    NonSuccessStatusCode(res.Error, HTTPResponseMessage.StatusCode.ToString(), "_HTTPConn.Client.GetAsync", "AtomFeedClient:GetEntries");
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

                HttpReqException(res.Error, e.Message, "_HTTPConn.Client.GetAsync", "AtomFeedClient:GetEntries");
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

                GenericException(res.Error, "", ErrorObject.ErrTypes.HTTP, "HTTP request error (Exception)", e.Message, "_HTTPConn.Client.GetAsync", "AtomFeedClient:GetEntries");
                res.IsError = true;

                return res;
            }

            return res;
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
                                                Debug.WriteLine("Exception @new Uri(altUri) @ AtomFeedClient Atom0.3" + "(" + entItem.Name + ")" + " : " + e.Message);
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
                                                Debug.WriteLine("Exception @new Uri(altUri) @ AtomFeedClient Atom0.3" + "(" + entItem.Name + ")" + " : " + e.Message);
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
                                            Debug.WriteLine("Exception @new Uri(altUri) @ AtomFeedClient Atom0.3" + "(" + entItem.Name + ")" + " : " + e.Message);
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

                if (!string.IsNullOrEmpty(sum.InnerText))
                {
                    //entry.SummaryPlainText = await StripHtmlTags(sum.InnerText);

                    entItem.SummaryPlainText = Truncate(sum.InnerText, 78);
                }
            }
            else
            {
                string s = entItem.Content;

                if (!string.IsNullOrEmpty(s))
                {
                    if (entItem.ContentType == EntryItem.ContentTypes.textHtml)
                    {
                        s = await StripHtmlTags(s);
                        entItem.SummaryPlainText = Truncate(s, 78);
                    }
                    else if (entItem.ContentType == EntryItem.ContentTypes.text)
                    {
                        entItem.SummaryPlainText = Truncate(s, 78);
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
            entItem.SummaryPlainText = entry.SummaryPlainText;
            entItem.Content = entry.Content;
            entItem.ContentType = entry.ContentType;
            // entItem.EntryBody = entry;

            entItem.Status = FeedEntryItem.ReadStatus.rsNew;

            if (entItem.ContentType == EntryItem.ContentTypes.textHtml)
            {
                // gets image Uri
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
                                    Debug.WriteLine("Exception @new Uri(editUri) @ AtomFeedClient Atom1.0" + "(" + entry.Name + ")" + " : " + e.Message);

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
                                                Debug.WriteLine("Exception @new Uri(altUri) @ AtomFeedClient Atom1.0" + "(" + entry.Name + ")" + " : " + e.Message);
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
                                                Debug.WriteLine("Exception @new Uri(altUri) @ AtomFeedClient Atom1.0" + "(" + entry.Name + ")" + " : " + e.Message);
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
                                            Debug.WriteLine("Exception @new Uri(altUri) @ AtomFeedClient Atom1.0" + "(" + entry.Name + ")" + " : " + e.Message);
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
                        Debug.WriteLine("Exception @XmlConvert.ToDateTime in the Atom 1.0 feed " + "(" + entry.Name + ")" + " : " + e.Message);
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

                if (!string.IsNullOrEmpty(sum.InnerText))
                {
                    //entry.SummaryPlainText = await StripHtmlTags(sum.InnerText);

                    entry.SummaryPlainText = Truncate(sum.InnerText, 78);
                }
            }
            else
            {
                string s = entry.Content;

                if (!string.IsNullOrEmpty(s))
                {
                    if (entry.ContentType == EntryFull.ContentTypes.textHtml)
                    {
                        s = await StripHtmlTags(s);
                        entry.SummaryPlainText = Truncate(s, 78);
                    }
                    else if (entry.ContentType == EntryFull.ContentTypes.text)
                    {
                        entry.SummaryPlainText = Truncate(s, 78);
                    }
                }
            }

            entry.Status = AtomEntry.EditStatus.esNormal;

            return entry;
        }
    }
}
