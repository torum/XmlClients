using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Diagnostics;
using System.Xml.Linq;
using System.Xml;
using System.Collections.ObjectModel;
using System.Net;
using AngleSharp;
using AngleSharp.Html.Parser;
using AngleSharp.Xml.Parser;
using System.Security.Cryptography;
using System.Net.Http.Headers;

namespace BlogWrite.Models;

#region == Result Classes for Service Discovery ==

// Feed Link Class
public class FeedLink
{
    public enum FeedKinds
    {
        Atom,
        Rss,
        Unknown
    }

    public Uri FeedUri { get; set; }

    public string Title { get; set; }

    public Uri SiteUri { get; set; }

    public string SiteTitle { get; set; }

    public FeedKinds FeedKind { get; set; }

    public FeedLink(Uri feedUri, FeedKinds feedKind, string title, Uri siteUri, string siteTitle)
    {
        FeedUri = feedUri;
        FeedKind = feedKind;
        Title = title;
        SiteUri = siteUri;
        SiteTitle = siteTitle;
    }
}

// Base Searvice Document Link Class
public abstract class SearviceDocumentLink
{
    public SearviceDocumentLink()
    {

    }
}

public class RsdApi
{
    public string Name { get; set; }
    public string BlogID { get; set; }
    public bool Preferred { get; set; }
    public Uri ApiLink { get; set; }
}

// RsdLink: extends SearviceDocumentLink
public class RsdLink : SearviceDocumentLink
{
    public string Title { get; set; }

    public string EngineName { get; set; }

    public Uri HomePageLink { get; set; }

    public List<RsdApi> Apis { get; set; } = new();

    public RsdLink()
    {

    }
}

// AppLink: extends SearviceDocumentLink
public class AppLink : SearviceDocumentLink
{
    public NodeService NodeService { get; set; }

    public AppLink()
    {

    }
}

// Base class for Result.
abstract class ServiceResultBase
{

}

// Error Class that Holds ErrorInfo (BasedOn ServiceResultBase)
class ServiceResultErr : ServiceResultBase
{
    public string ErrTitle { get; set; }
    public string ErrDescription { get; set; }

    public ServiceResultErr(string et, string ed)
    {
        ErrTitle = et;
        ErrDescription = ed;
    }
}

// AuthRequired Class (BasedOn ServiceResultBase)
class ServiceResultAuthRequired : ServiceResultBase
{
    public Uri Addr { get; set; }

    public ServiceResultAuthRequired(Uri addr)
    {
        Addr = addr;
    }
}

// HTML Result Class that Holds Feeds and Service Links embedded in HTML. (BasedOn ServiceResultBase)
class ServiceResultHtmlPage : ServiceResultBase
{
    private ObservableCollection<FeedLink> _feeds = new();
    public ObservableCollection<FeedLink> Feeds
    {
        get { return _feeds; }
        set
        {

            if (_feeds == value)
                return;

            _feeds = value;
        }
    }

    private ObservableCollection<SearviceDocumentLink> _services = new();
    public ObservableCollection<SearviceDocumentLink> Services
    {
        get { return _services; }
        set
        {

            if (_services == value)
                return;

            _services = value;
        }
    }

    public ServiceResultHtmlPage()
    {

    }
}

// Feed Result Class That Holds Feed link info. (BasedOn ServiceResultBase)
class ServiceResultFeed : ServiceResultBase
{
    public FeedLink FeedlinkInfo;

    public ServiceResultFeed()
    {

    }
}

class ServiceResultRsd : ServiceResultBase
{
    public RsdLink Rsd;

    public ServiceResultRsd()
    {

    }
}

// Base Class for Service Result That Holds Feed link info. (BasedOn ServiceResultBase)
abstract class ServiceResult : ServiceResultBase
{
    public ServiceTypes ServiceType { get; set; }

    public Uri EndpointUri;

    public AuthTypes AuthType { get; set; } = AuthTypes.Wsse;

    public ServiceResult(ServiceTypes serviceType, Uri endpointUri, AuthTypes authType)
    {
        ServiceType = serviceType;
        EndpointUri = endpointUri;
        AuthType = authType;
    }
}

// AtomPub Service Result Class That Holds NodeService (SearviceDocumentLink info). (BasedOn ServiceResult)
class ServiceResultAtomPub : ServiceResult
{
    public NodeService AtomService { get; set; }

    public ServiceResultAtomPub(Uri endpointUri, AuthTypes authType, NodeService nodeService) : base(ServiceTypes.AtomPub, endpointUri, authType)
    {
        //Service = ServiceTypes.AtomPub;
        //EndpointUri = endpointUri;
        AtomService = nodeService;
    }
}

class ServiceResultAtomAPI : ServiceResult
{
    public ServiceResultAtomAPI(Uri endpointUri, AuthTypes authType) : base(ServiceTypes.AtomApi, endpointUri, authType)
    {
        //ServiceType = ServiceTypes.AtomApi;
        //EndpointUri = endpointUri;
    }
}

class ServiceResultXmlRpc : ServiceResult
{
    // XML-RPC specific blogid. 
    public string BlogID { get; set; }

    public ServiceResultXmlRpc(Uri endpointUri, AuthTypes authType) : base(ServiceTypes.XmlRpc, endpointUri, authType)
    {
        ServiceType = ServiceTypes.XmlRpc;
        EndpointUri = endpointUri;
    }
}

#endregion

// Service Discovery class.
class ServiceDiscovery
{
    private HttpClient? _httpClient;

    public ServiceDiscovery()
    {
        _httpClient = new HttpClient();
    }

    #region == Events ==

    public delegate void ServiceDiscoveryStatusUpdate(ServiceDiscovery sender, string data);

    public event ServiceDiscoveryStatusUpdate StatusUpdate;

    #endregion

    #region == Methods ==

    public async Task<ServiceResultBase> DiscoverService(Uri addr)
    {
        UpdateStatus(string.Format(">> HTTP GET " + addr.AbsoluteUri));

        try
        {
            var HTTPResponse = await _httpClient.GetAsync(addr);

            UpdateStatus(string.Format("<< HTTP status {0} returned.", HTTPResponse.StatusCode.ToString()));

            if (HTTPResponse.IsSuccessStatusCode)
            {
                if (HTTPResponse.Content == null)
                {
                    UpdateStatus("<< Content is emptty.");
                    ServiceResultErr re = new ServiceResultErr("Received no content.", "Content empty.");
                    return re;
                }

                string contenTypeString = HTTPResponse.Content.Headers.GetValues("Content-Type").FirstOrDefault();

                if (!string.IsNullOrEmpty(contenTypeString))
                {
                    UpdateStatus(string.Format("- Content-Type header is {0}", contenTypeString));

                    // HTML page.
                    if (contenTypeString.StartsWith("text/html"))
                    {
                        UpdateStatus("- Parsing the HTML document ...");

                        // HTML parse.
                        ServiceResultBase res = await ParseHtml(HTTPResponse.Content, addr);

                        if (res is ServiceResultHtmlPage)
                        {
                            if ((res as ServiceResultHtmlPage).Feeds.Count == 0)
                                UpdateStatus("- No feed link found.");

                            if ((res as ServiceResultHtmlPage).Services.Count == 0)
                                UpdateStatus("- No Service link found.");
                        }

                        return res;
                    }
                    else if (contenTypeString.StartsWith("text/xml") || contenTypeString.StartsWith("application/xml"))
                    {
                        UpdateStatus(string.Format("- Ambiguous Content-Type {0} returned. ", contenTypeString));

                        UpdateStatus("- Parsing XML document to determine the what this is ...");

                        // XML parse.
                        ServiceResultBase xml = await Task.Run(() => ParseXml(HTTPResponse.Content, addr));

                        return xml;
                    }
                    else if (contenTypeString.StartsWith("application/rss+xml"))
                    {
                        UpdateStatus("- Parsing RSS feed ...");

                        // XML parse.
                        ServiceResultBase feed = await Task.Run(() => ParseXml(HTTPResponse.Content, addr));

                        return feed;
                    }
                    else if (contenTypeString.StartsWith("application/rdf+xml"))
                    {
                        UpdateStatus("- Parsing RSS/RDF feed ...");

                        // XML parse.
                        ServiceResultBase feed = await Task.Run(() => ParseXml(HTTPResponse.Content, addr));

                        return feed;
                    }
                    //
                    else if (contenTypeString.StartsWith("application/atomsvc+xml"))
                    {
                        // This is the AtomPub endpoint.
                        /*
                        UpdateStatus("Found an Atom Publishing Protocol service document.");

                        ServiceResultAtomPub ap = new ServiceResultAtomPub();
                        ap.EndpointUri = addr;
                        ap.Service = ServiceTypes.AtomPub; ;
                        return ap;
                        */

                        UpdateStatus("- AtomPub endpoint (UNDERDEVELOPENT).");

                        ServiceResultErr re = new ServiceResultErr("AtomPub endpoint (UNDERDEVELOPENT).", string.Format("{0} is AtomPub endpoint. (UNDERDEVELOPENT)", contenTypeString));
                        return re;
                    }
                    else if (contenTypeString.StartsWith("application/rsd+xml"))
                    {
                        UpdateStatus("- Parsing RSD document ...");

                        var source = await HTTPResponse.Content.ReadAsStreamAsync();
                        var parser = new XmlParser();
                        var document = await parser.ParseDocumentAsync(source);

                        RsdLink rsd = ParseRsd(document);

                        ServiceResultRsd resRsd = new ServiceResultRsd();
                        resRsd.Rsd = rsd;

                        return (resRsd as ServiceResultBase);
                    }
                    else if (contenTypeString.StartsWith("application/atom+xml"))
                    {
                        // TODO:
                        // Possibly AtomApi endopoint. Or Atom Feed...

                        UpdateStatus("- Parsing Atom feed ...");

                        // RSS parse.
                        ServiceResultBase feed = await Task.Run(() => ParseXml(HTTPResponse.Content, addr));

                        return feed;
                        /*
                        UpdateStatus("- Atom (UNDERDEVELOPENT).");

                        ServiceResultErr re = new ServiceResultErr("Atom (UNDERDEVELOPENT).", string.Format("{0} is Atom. (UNDERDEVELOPENT)", contenTypeString));
                        return re;
                        */
                    }
                    else if (contenTypeString.StartsWith("application/x.atom+xml"))
                    {
                        /*
                        // TODO:
                        // Possibly AtomApi endopoint.
                        UpdateStatus("<< Old Atom format returned... ");

                        ServiceResultAtomAPI ap = new ServiceResultAtomAPI();
                        ap.EndpointUri = addr;
                        ap.Service = ServiceTypes.AtomApi;
                        return ap;
                        */
                        UpdateStatus("- AtomAPI (UNDERDEVELOPENT).");

                        ServiceResultErr re = new ServiceResultErr("AtomAPI (UNDERDEVELOPENT).", string.Format("{0} is AtomAPI. (UNDERDEVELOPENT)", contenTypeString));
                        return re;
                    }
                    else if (contenTypeString.StartsWith("application/x.atom+xml"))
                    {
                        /*
                        // TODO:
                        // Possibly AtomApi endopoint.
                        UpdateStatus("<< Old Atom format returned... ");

                        ServiceResultAtomAPI ap = new ServiceResultAtomAPI();
                        ap.EndpointUri = addr;
                        ap.Service = ServiceTypes.AtomApi;
                        return ap;
                        */
                        UpdateStatus("- AtomAPI (UNDERDEVELOPENT).");

                        ServiceResultErr re = new ServiceResultErr("AtomAPI (UNDERDEVELOPENT).", string.Format("{0} is AtomAPI. (UNDERDEVELOPENT)", contenTypeString));
                        return re;
                    }
                    else
                    {
                        UpdateStatus("- Unknown Content-Type returned.");

                        ServiceResultErr re = new ServiceResultErr("Received unsupported Content-Type.", string.Format("{0} is not supported.", contenTypeString));
                        return re;
                    }
                }
                else
                {
                    UpdateStatus("- No Content-Type returned. ");
                    ServiceResultErr re = new ServiceResultErr("Download failed", "No Content-Type reveived.");
                    return re;
                }
            }
            else
            {
                UpdateStatus("- Could not retrieve any document. ");

                //If 401 Unauthorized,
                // A user may or may not enter an AtomPub endpoint which require auth to get service document.
                if (HTTPResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    UpdateStatus("- Authorization is required. ");

                    ServiceResultAuthRequired rea = new ServiceResultAuthRequired(addr);
                    return rea;
                }
                else
                {
                    ServiceResultErr re = new ServiceResultErr("HTTP error.", "Could not retrieve any document.");
                    return re;
                }
            }

        }
        catch (System.Net.Http.HttpRequestException e)
        {
            UpdateStatus("<< HttpRequestException: " + e.Message);
            ServiceResultErr re = new ServiceResultErr("HTTP request error.", e.Message);
            return re;
        }
        catch (Exception e)
        {

            UpdateStatus("<< HTTP error: " + e.Message);
            ServiceResultErr re = new ServiceResultErr("HTTP error.", e.Message);
            return re;
        }
    }

    public async Task<ServiceResultBase> DiscoverServiceWithAuth(Uri addr, string userName, string apiKey, AuthTypes authType)
    {
        try
        {
            HttpRequestMessage webreq = new HttpRequestMessage(HttpMethod.Get, addr);

            if (authType == AuthTypes.Wsse)
            {
                // WSSE Auth header
                webreq.Headers.Add("Authorization", @"WSSE profile = ""UsernameToken""");
                webreq.Headers.Add("X-WSSE", MakeWSSEHeader(userName, apiKey));
            }
            else
            {
                // BASIC Auth
                string s = userName + ":" + apiKey;
                var byteArray = Encoding.UTF8.GetBytes(s);
                webreq.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }


            UpdateStatus(">> HTTP GET " + addr.AbsoluteUri);

            var HTTPResponse = await _httpClient.SendAsync(webreq);

            //var HTTPResponse = await _httpClient.GetAsync(addr);

            UpdateStatus(string.Format("<< HTTP status {0} returned.", HTTPResponse.StatusCode.ToString()));

            if (HTTPResponse.IsSuccessStatusCode)
            {
                if (HTTPResponse.Content == null)
                {
                    UpdateStatus("<< Content is emptty.");
                    ServiceResultErr re = new ServiceResultErr("Received no content.", "Content empty.");
                    return re;
                }

                string contenTypeString = HTTPResponse.Content.Headers.GetValues("Content-Type").FirstOrDefault();

                if (!string.IsNullOrEmpty(contenTypeString))
                {
                    UpdateStatus(string.Format("- Content-Type header is {0}", contenTypeString));

                    // HTML page.
                    if (contenTypeString.StartsWith("text/html"))
                    {
                        UpdateStatus("- HTML document returned. Something is wrong.");

                        // Error
                        ServiceResultErr re = new ServiceResultErr("API/Protocol authentication failed", "A HTML page returned.");
                        return re;
                    }
                    else if (contenTypeString.StartsWith("text/xml") || contenTypeString.StartsWith("application/xml"))
                    {
                        UpdateStatus(string.Format("- Ambiguous Content-Type {0} returned. ", contenTypeString));

                        UpdateStatus("- Parsing XML document to determine the what this is ...");

                        // XML parse.
                        ServiceResultBase xml = await Task.Run(() => ParseXml(HTTPResponse.Content, addr));

                        return xml;
                    }
                    else if (contenTypeString.StartsWith("application/rss+xml"))
                    {
                        UpdateStatus("- RSS feed returned. Something is wrong.");

                        // Error
                        ServiceResultErr re = new ServiceResultErr("API/Protocol authentication failed", "A RSS feed returned.");
                        return re;
                    }
                    else if (contenTypeString.StartsWith("application/rdf+xml"))
                    {
                        UpdateStatus("- RSS/RDF feed returned. Something is wrong.");

                        // Error
                        ServiceResultErr re = new ServiceResultErr("API/Protocol authentication failed", "A RSS/RDF feed returned.");
                        return re;
                    }
                    //
                    else if (contenTypeString.StartsWith("application/atomsvc+xml"))
                    {
                        // AtomPub endpoint.
                        UpdateStatus("- Atom Publishing Protocol Service document returned.");

                        ServiceResultBase ap = await Task.Run(() => ParseAtomServiceDocument(HTTPResponse.Content, addr, userName, apiKey, authType));
                        return ap;
                    }
                    else if (contenTypeString.StartsWith("application/rsd+xml"))
                    {
                        //await ParseRsd(HTTPResponse.Content);

                        UpdateStatus("- RSD (No need to be authenticated). Something went wrong.");

                        // Error
                        ServiceResultErr re = new ServiceResultErr("RSD (No need to be authenticated).", string.Format("{0} is RSD. (Something went wrong)", contenTypeString));
                        return re;
                    }
                    else if (contenTypeString.StartsWith("application/atom+xml"))
                    {
                        UpdateStatus("- Parsing Atom feed ...");

                        // XML parse.
                        ServiceResultBase feed = await Task.Run(() => ParseXml(HTTPResponse.Content, addr));

                        return feed;
                    }
                    else if (contenTypeString.StartsWith("application/x.atom+xml"))
                    {
                        /*
                        // TODO:
                        // Possibly AtomApi endopoint.
                        UpdateStatus("<< Old Atom format returned... ");

                        ServiceResultAtomAPI ap = new ServiceResultAtomAPI();
                        ap.EndpointUri = addr;
                        ap.Service = ServiceTypes.AtomApi;
                        return ap;
                        */
                        UpdateStatus("- AtomAPI (UNDERDEVELOPENT).");

                        ServiceResultErr re = new ServiceResultErr("AtomAPI (UNDERDEVELOPENT).", string.Format("{0} is AtomAPI. (UNDERDEVELOPENT)", contenTypeString));
                        return re;
                    }
                    else if (contenTypeString.StartsWith("application/x.atom+xml"))
                    {
                        /*
                        // TODO:
                        // Possibly AtomApi endopoint.
                        UpdateStatus("<< Old Atom format returned... ");

                        ServiceResultAtomAPI ap = new ServiceResultAtomAPI();
                        ap.EndpointUri = addr;
                        ap.Service = ServiceTypes.AtomApi;
                        return ap;
                        */
                        UpdateStatus("- AtomAPI (UNDERDEVELOPENT).");

                        ServiceResultErr re = new ServiceResultErr("AtomAPI (UNDERDEVELOPENT).", string.Format("{0} is AtomAPI. (UNDERDEVELOPENT)", contenTypeString));
                        return re;
                    }
                    else
                    {
                        UpdateStatus("- Unknown Content-Type returned.");

                        ServiceResultErr re = new ServiceResultErr("Received unsupported Content-Type.", string.Format("{0} is not supported.", contenTypeString));
                        return re;
                    }
                }
                else
                {
                    UpdateStatus("- No Content-Type returned. ");
                    ServiceResultErr re = new ServiceResultErr("Download failed", "No Content-Type reveived.");
                    return re;
                }
            }
            else
            {
                UpdateStatus("- Could not retrieve any document. ");

                //If 401 Unauthorized,
                // A user may or may not enter an AtomPub endpoint which require auth to get service document.
                if (HTTPResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    UpdateStatus("- Authorization failed. ");

                    ServiceResultAuthRequired rea = new ServiceResultAuthRequired(addr);
                    return rea;
                }
                else
                {
                    ServiceResultErr re = new ServiceResultErr("HTTP error.", "Could not retrieve any document.");
                    return re;
                }
            }

        }
        catch (System.Net.Http.HttpRequestException e)
        {
            UpdateStatus("<< HttpRequestException: " + e.Message);
            ServiceResultErr re = new ServiceResultErr("HTTP request error.", e.Message);
            return re;
        }
        catch (Exception e)
        {

            UpdateStatus("<< HTTP error: " + e.Message);
            ServiceResultErr re = new ServiceResultErr("HTTP error.", e.Message);
            return re;
        }
    }

    private async Task<ServiceResultBase> ParseHtml(HttpContent content, Uri addr)
    {
        ServiceResultHtmlPage res = new();

        //Use the default configuration for AngleSharp
        var config = Configuration.Default;

        //Create a new context for evaluating webpages with the given config
        var context = BrowsingContext.New(config);

        //Source
        var source = await content.ReadAsStreamAsync();

        //Create a virtual request to specify the document to load (here from our fixed string)
        var document = await context.OpenAsync(req => req.Content(source));

        // gets page title
        var elementTitle = document.QuerySelector("html > head > title");
        string siteTitle = "";
        if (elementTitle != null)
        {
            siteTitle = elementTitle.TextContent;

            UpdateStatus("- Webpage title found: " + siteTitle);
        }
        else
        {
            UpdateStatus("- Webpage title NOT found.");
        }

        var elements = document.QuerySelectorAll("link");

        foreach (var e in elements)
        {
            string re = e.GetAttribute("rel");
            string ty = e.GetAttribute("type");
            string hf = e.GetAttribute("href");
            string t = e.GetAttribute("title");

            if (!string.IsNullOrEmpty(re))
            {
                if (re.ToUpper() == "EDITURI")
                {
                    if (!string.IsNullOrEmpty(ty) && !string.IsNullOrEmpty(hf))
                    {
                        if (ty == "application/rsd+xml")
                        {
                            UpdateStatus(">> A link to RSD document found.");

                            Uri _rsdUrl = null;
                            try
                            {
                                _rsdUrl = new Uri(hf);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("Exception@ServiceDiscovery@ParseHTML on var _rsdUrl = new Uri(hf) : " + ex.Message);

                                UpdateStatus(">> A link to RSD document invalid :" + ex.Message);
                            }

                            if (_rsdUrl != null)
                            {
                                RsdLink rsd = await GetAndParseRsd(_rsdUrl);
                                if (rsd.Apis.Count > 0)
                                {
                                    res.Services.Add(rsd);
                                }
                            }
                        }
                    }
                }

                if (re.ToUpper() == "SERVICE")
                {
                    // TODO:
                }
                
                if (re.ToUpper() == "https://api.w.org/")
                {
                    // TODO:
                }

                if (re.ToUpper() == "ALTERNATE")
                {
                    if (!string.IsNullOrEmpty(ty) && !string.IsNullOrEmpty(hf))
                    {
                        if (ty == "application/atom+xml")
                        {
                            try
                            {
                                var _atomFeedUrl = new Uri(hf);

                                FeedLink fl = new(_atomFeedUrl, FeedLink.FeedKinds.Atom, t, addr, siteTitle);
                                

                                res.Feeds.Add(fl);

                                UpdateStatus("Found a link to an Atom feed.");
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("Exception@ServiceDiscovery@ParseHTML on var _atomFeedUrl = new Uri(hf) : " + ex.Message);
                            }
                        }
                        else if (ty == "application/rss+xml")
                        {
                            try
                            {
                                var _rssFeedUrl = new Uri(hf);

                                FeedLink fl = new(_rssFeedUrl, FeedLink.FeedKinds.Rss, t, addr, siteTitle);

                                res.Feeds.Add(fl);

                                UpdateStatus("Found a link to an RSS feed.");
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("Exception@ServiceDiscovery@ParseHTML on var _rssFeedUrl = new Uri(hf) : " + ex.Message);
                            }
                        }
                        else
                        {
                            //Debug.WriteLine("rel type: " + ty);
                        }
                    }
                }
            }
        }

        if (elements.Length == 0)
        {
            // If webpage is hosted on a free host like Byethost, it returns a empty html with a Javascript to test whether it is a bot or not.
            // <noscript>This site requires Javascript to work, please enable Javascript in your browser or use a browser with Javascript support</noscript>

            UpdateStatus("- No link found. If the webpage is hosted on a free host like Byethost, it returns a empty html with a Javascript to test whether it is a browser or not. In this case, we can't access the page.");
        }
        
        return res;
    }

    private async Task<ServiceResultBase> ParseXml(HttpContent content, Uri addr)
    {
        var source = await content.ReadAsStreamAsync();

        var parser = new XmlParser();
        var document = await parser.ParseDocumentAsync(source);

        bool isOK = false;
        string feedTitle="", siteLink = "";
        Uri siteUri = null;

        if (document != null)
        {
            if (document.DocumentElement != null)
            {
                // Possibly RSS 2.0
                if (document.DocumentElement.LocalName.Equals("rss"))
                {
                    string ver = document.DocumentElement.GetAttribute("version");
                    if (!string.IsNullOrEmpty(ver))
                    {
                        if (ver.Equals("2.0"))
                        {
                            UpdateStatus("RSS 2.0 feed detected.");
                            isOK = true;
                        }
                    }

                    // feed title
                    var elementTitle = document.QuerySelector("rss > channel > title");
                    if (elementTitle != null)
                    {
                        feedTitle = elementTitle.TextContent;

                        if (!string.IsNullOrEmpty(feedTitle))
                        {
                            UpdateStatus("Found a title for the RSS feed.");
                        }
                        else
                        {
                            feedTitle = "Empty RSS feed title";
                        }
                    }

                    //siteLink = document.DocumentElement.QuerySelector("channel > link").TextContent;
                    var sl = document.DocumentElement.QuerySelectorAll("channel > link");
                    if (sl != null)
                    {
                        foreach (var sle in sl)
                        {
                            if (string.IsNullOrEmpty(sle.NamespaceUri))
                            {
                                siteLink = sle.TextContent;
                                if (!string.IsNullOrEmpty(siteLink))
                                {
                                    try
                                    {
                                        siteUri = new Uri(siteLink);
                                    }
                                    catch { }
                                }
                                break;
                            }
                        }
                    }

                    if (isOK)
                    {
                        ServiceResultFeed rss = new ServiceResultFeed();
                        rss.FeedlinkInfo = new(addr, FeedLink.FeedKinds.Rss, feedTitle, siteUri, "");

                        return (rss as ServiceResultBase);
                    }
                }
                // Possibly RSS 1.0
                else if (document.DocumentElement.LocalName.Equals("RDF"))
                {
                    string ns = document.DocumentElement.GetAttribute("xmlns");

                    if (!string.IsNullOrEmpty(ns))
                    {
                        if (ns.Equals("http://purl.org/rss/1.0/"))
                        {
                            ns = document.DocumentElement.GetAttribute("xmlns:rdf");
                            if (!string.IsNullOrEmpty(ns))
                            {
                                if (ns.Equals("http://www.w3.org/1999/02/22-rdf-syntax-ns#"))
                                {
                                    UpdateStatus("RSS 1.0 feed detected.");
                                    isOK = true;
                                }
                            }
                        }
                    }

                    feedTitle = document.DocumentElement.QuerySelector("channel > title").TextContent;

                    if (!string.IsNullOrEmpty(feedTitle))
                    {
                        UpdateStatus("Found a title for the RSS feed.");
                    }
                    else
                    {
                        feedTitle = "Empty RSS feed title";
                    }

                    siteLink = document.DocumentElement.QuerySelector("channel > link").TextContent;
                    if (!string.IsNullOrEmpty(siteLink))
                    {
                        try
                        {
                            siteUri = new Uri(siteLink);
                        }
                        catch { }
                    }

                    if (isOK)
                    {
                        ServiceResultFeed rdf = new ServiceResultFeed();
                        rdf.FeedlinkInfo = new(addr, FeedLink.FeedKinds.Rss, feedTitle, siteUri, "");

                        return (rdf as ServiceResultBase);
                    }
                }
                // Possibly Atom 1.0 or 0.3 feed
                else if (document.DocumentElement.LocalName.Equals("feed"))
                {
                    string ns = document.DocumentElement.GetAttribute("xmlns");

                    if (!string.IsNullOrEmpty(ns))
                    {
                        if (ns.Equals("http://www.w3.org/2005/Atom") || ns.Equals("http://purl.org/atom/ns#"))
                        {
                            UpdateStatus("Atom feed detected.");
                            isOK = true;
                        }
                    }

                    // feed title
                    var elementTitle = document.QuerySelector("feed > title");
                    if (elementTitle != null)
                    {
                        feedTitle = elementTitle.TextContent;

                        if (!string.IsNullOrEmpty(feedTitle))
                        {
                            UpdateStatus("Found a title for the Atom feed.");
                        }
                        else
                        {
                            feedTitle = "Empty Atom feed title";
                        }
                    }

                    //siteLink = document.DocumentElement.QuerySelector("feed > link[href]").TextContent;
                    var sl = document.DocumentElement.QuerySelector("feed > link");
                    if (sl != null)
                    {
                        siteLink = sl.GetAttribute("href");
                        if (!string.IsNullOrEmpty(siteLink))
                        {
                            try
                            {
                                siteUri = new Uri(siteLink);
                            }
                            catch { }
                        }
                    }

                    if (isOK)
                    {
                        ServiceResultFeed atom = new ServiceResultFeed();
                        atom.FeedlinkInfo = new(addr, FeedLink.FeedKinds.Atom, feedTitle, siteUri, "");

                        return (atom as ServiceResultBase);
                    }
                }
                else if (document.DocumentElement.LocalName.Equals("rsd"))
                {
                    UpdateStatus("- Parsing RSD document ...");

                    RsdLink rsd = ParseRsd(document);

                    ServiceResultRsd resRsd = new ServiceResultRsd();
                    resRsd.Rsd = rsd;

                    return (resRsd as ServiceResultBase);
                }
                else
                {
                    UpdateStatus("- Unknown XML document ...");
                }
            }
        }

        ServiceResultErr ret = new ServiceResultErr("XML parse error.", "Could not parse the document.");

        return (ret as ServiceResultBase);
    }

    private async Task<ServiceResultBase> ParseAtomServiceDocument(HttpContent content, Uri addr, string userName, string apiKey, AuthTypes authType)
    {
        var source = await content.ReadAsStreamAsync();

        XmlDocument xdoc = new XmlDocument();
        try
        {
            XmlReader reader = XmlReader.Create(source);
            xdoc.Load(reader);
        }
        catch (Exception e)
        {
            UpdateStatus("<< XML parse error (Atom Service document): " + e.Message);

            ServiceResultErr xe = new ServiceResultErr("XML parse error.", "Could not parse the Atom Service document: " + e.Message);

            return (xe as ServiceResultBase);
        }

        XmlNamespaceManager atomNsMgr = new XmlNamespaceManager(xdoc.NameTable);
        atomNsMgr.AddNamespace("atom", "http://www.w3.org/2005/Atom");
        atomNsMgr.AddNamespace("app", "http://www.w3.org/2007/app");

        if ((xdoc.DocumentElement.Name == "app:service") || (xdoc.DocumentElement.Name == "service"))
        {
            NodeService account = new NodeService("New Service (Atom Publishing Protocol)", userName, apiKey, addr, ApiTypes.atAtomPub, ServiceTypes.AtomPub);

            account.EndPoint = addr;
            account.ServiceType = ServiceTypes.AtomPub;
            account.UserName = userName;
            account.UserPassword = apiKey;
            account.Api = ApiTypes.atAtomPub;
            account.AuthType = authType;

            XmlNodeList workspaceList = xdoc.DocumentElement.SelectNodes("app:workspace", atomNsMgr);
            //XmlNodeList workspaceList = xdoc.SelectNodes("//service/app:workspace", atomNsMgr);

            if (workspaceList != null)
            {
                foreach (XmlNode ws in workspaceList)
                {
                    NodeWorkspace workspace = new NodeWorkspace("Workspace Name");
                    workspace.IsExpanded = true;
                    workspace.Parent = account;

                    XmlNode wtl = ws.SelectSingleNode("atom:title", atomNsMgr);
                    if (wtl != null)
                    {
                        if (string.IsNullOrEmpty(wtl.InnerText))
                            workspace.Name = wtl.InnerText;
                    }
                    
                    XmlNodeList collectionList = ws.SelectNodes("app:collection", atomNsMgr);
                    if (collectionList != null)
                    {
                        foreach (XmlNode col in collectionList)
                        {
                            Uri colHrefUri = null;
                            if (col.Attributes["href"] != null)
                            {
                                string href = col.Attributes["href"].Value;
                                if (!string.IsNullOrEmpty(href))
                                {
                                    try
                                    {
                                        colHrefUri = new Uri(href);
                                    }
                                    catch { }
                                }
                            }

                            NodeAtomPubEntryCollection collection = new NodeAtomPubEntryCollection("Collection Name", colHrefUri, colHrefUri.AbsoluteUri);
                            collection.IsExpanded = true;
                            collection.Parent = workspace;

                            XmlNode ctl = col.SelectSingleNode("atom:title", atomNsMgr);
                            if (ctl != null)
                            {
                                if (!string.IsNullOrEmpty(ctl.InnerText))
                                    collection.Name = ctl.InnerText;
                            }

                            XmlNodeList accepts = col.SelectNodes("app:accept", atomNsMgr);
                            if (accepts != null)
                            {
                                foreach (XmlNode acp in accepts)
                                {
                                    string acpt = acp.InnerText;
                                    if (!string.IsNullOrEmpty(acpt))
                                    {
                                        collection.AcceptTypes.Add(acpt);

                                        if ((acpt == "application/atom+xml;type=entry") 
                                            || (acpt == "application/atom+xml"))
                                        {
                                            collection.IsAcceptEntry = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // default entry
                                collection.IsAcceptEntry = true;
                            }

                            XmlNodeList categoriesList = col.SelectNodes("app:categories", atomNsMgr); 
                            if (categoriesList != null)
                            {
                                foreach (XmlNode cats in categoriesList)
                                {
                                    NodeAtomPubCatetories categories = new NodeAtomPubCatetories("Categories");
                                    categories.IsExpanded = true;
                                    categories.Parent = collection;

                                    Uri catHrefUri = null;
                                    if (cats.Attributes["href"] != null)
                                    {
                                        string cathref = cats.Attributes["href"].Value;
                                        if (!string.IsNullOrEmpty(cathref))
                                        {
                                            try
                                            {
                                                catHrefUri = new Uri(cathref);

                                                categories.Href = catHrefUri;
                                            }
                                            catch { }
                                        }
                                    }

                                    if (cats.Attributes["fixed"] != null)
                                    {
                                        string catFix = cats.Attributes["fixed"].Value;
                                        if (!string.IsNullOrEmpty(catFix))
                                        {
                                            if (catFix == "yes")
                                            {
                                                categories.IsCategoryFixed = true;
                                            }
                                            else
                                            {
                                                categories.IsCategoryFixed = false;
                                            }
                                        }
                                    }

                                    XmlNodeList categoryList = cats.SelectNodes("atom:category", atomNsMgr);
                                    if (categoryList != null)
                                    {
                                        foreach (XmlNode cat in categoryList)
                                        {
                                            NodeAtomPubCategory category = new NodeAtomPubCategory("Category");
                                            category.IsExpanded = true;
                                            category.Parent = categories;

                                            if (cat.Attributes["term"] != null)
                                            {
                                                string term = cat.Attributes["term"].Value;
                                                if (!string.IsNullOrEmpty(term))
                                                {
                                                    category.Term = term;
                                                }
                                            }

                                            if (cat.Attributes["scheme"] != null)
                                            {
                                                string scheme = cat.Attributes["scheme"].Value;
                                                if (!string.IsNullOrEmpty(scheme))
                                                {
                                                    category.Scheme = scheme;
                                                }
                                            }

                                            categories.Children.Add(category);
                                        }
                                    }

                                    collection.Children.Add(categories);
                                }
                            }

                            workspace.Children.Add(collection);
                        }
                    }

                    account.Children.Add(workspace);
                }
            }

            ServiceResultAtomPub ap = new ServiceResultAtomPub(addr, authType, account);

            return (ap as ServiceResultBase);
        }

        ServiceResultErr ret = new ServiceResultErr("Fail to read service document.", "No service element found in the Atom Service document:" + xdoc.OuterXml.ToString());

        return (ret as ServiceResultBase);
    }

    private async Task<RsdLink> GetAndParseRsd(Uri addr)
    {
        UpdateStatus(string.Format(">> HTTP GET " + addr.AbsoluteUri));

        RsdLink rsdDoc = new();

        try
        {
            var HTTPResponse = await _httpClient.GetAsync(addr);

            UpdateStatus(string.Format("<< HTTP status {0} returned.", HTTPResponse.StatusCode.ToString()));

            if (HTTPResponse.IsSuccessStatusCode)
            {
                if (HTTPResponse.Content != null)
                {
                    var source = await HTTPResponse.Content.ReadAsStreamAsync();
                    var parser = new XmlParser();
                    var document = await parser.ParseDocumentAsync(source);

                    rsdDoc = ParseRsd(document);
                }
            }
            else
            {
                UpdateStatus("- Could not retrieve RSD document. ");
            }

        }
        catch (System.Net.Http.HttpRequestException e)
        {
            UpdateStatus("<< HttpRequestException: " + e.Message);
        }
        catch (Exception e)
        {
            UpdateStatus("<< HTTP error: " + e.Message);
        }

        return rsdDoc;
    }

    private RsdLink ParseRsd(AngleSharp.Xml.Dom.IXmlDocument document)
    {
        RsdLink rsdDoc = new();

        try
        {
            //var source = await content.ReadAsStreamAsync();
            //var parser = new XmlParser();
            //var document = await parser.ParseDocumentAsync(source);

            if (document != null)
            {
                if (document.DocumentElement != null)
                {
                    rsdDoc.EngineName = document.DocumentElement.QuerySelector("service > engineName").TextContent;

                    var homePageLink = document.DocumentElement.QuerySelector("service > homePageLink").TextContent;
                    if (!string.IsNullOrEmpty(homePageLink))
                    {
                        try
                        {
                            rsdDoc.HomePageLink = new Uri(homePageLink);
                        }
                        catch { }
                    }

                    var apis = document.DocumentElement.QuerySelectorAll("service > apis > api");
                    if (apis != null)
                    {
                        foreach (var api in apis)
                        {
                            var apiName = api.GetAttribute("name");
                            var apiBlogId = api.GetAttribute("blogID");
                            var apiPreferred = api.GetAttribute("preferred");
                            var apiLink = api.GetAttribute("apiLink");

                            RsdApi hoge = new();
                            hoge.Name = apiName;
                            hoge.BlogID = apiBlogId;
                            if (!string.IsNullOrEmpty(apiPreferred))
                            {
                                if (apiPreferred.ToLower() == "true")
                                {
                                    hoge.Preferred = true;
                                }
                            }
                            if (!string.IsNullOrEmpty(apiLink))
                            {
                                try
                                {
                                    hoge.ApiLink = new Uri(apiLink);
                                }
                                catch { }
                            }

                            rsdDoc.Apis.Add(hoge);
                        }
                    }

                    foreach (var fuga in rsdDoc.Apis)
                    {
                        if ((fuga.Name.ToLower() == "wordpress") && (fuga.Preferred))
                        {
                            UpdateStatus(string.Format("-  WordPress found."));
                        }
                        else
                        {
                            // TODO:
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            UpdateStatus("Load XML/RSD failed: " + e.Message);
        }

        return rsdDoc;
    }

    private async void UpdateStatus(string data)
    {
        await Task.Run(() => { StatusUpdate?.Invoke(this, data); });
    }

    #region == WSSE == 

    private string MakeWSSEHeader(string userName, string password)
    {
        string nonce = GenNounce(40);
        byte[] nonceBytes = Encoding.UTF8.GetBytes(nonce);
        string nonce64 = Convert.ToBase64String(nonceBytes);
        string createdString = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        string digest = GenDigest(password, createdString, nonce);
        string header = string.Format(
            @"UsernameToken Username=""{0}"", PasswordDigest=""{1}"", Nonce=""{2}"", Created=""{3}""",
            userName, digest, nonce64, createdString);
        return header;
    }

    private string GenDigest(string password, string created, string nonce)
    {
        byte[] digest;
        using (SHA1Managed sha1 = new SHA1Managed())
        {
            string digestText = nonce + created + password;
            byte[] digestBytes = Encoding.UTF8.GetBytes(digestText);
            digest = sha1.ComputeHash(digestBytes);
        }
        string digest64 = Convert.ToBase64String(digest);
        return digest64;
    }

    private string GenNounce(int length)
    {
        RNGCryptoServiceProvider rnd = new RNGCryptoServiceProvider();
        byte[] buffer = new byte[length];
        rnd.GetBytes(buffer);
        string nonce64 = Convert.ToBase64String(buffer);
        return nonce64;
    }

    #endregion

    #endregion
}

#region == Sample XML Code ==

// RSD - Really Simple Discovery: XML-RPC or AtomAPI or WP json
// Content-Type: application/rsd+xml

// RSD - Really Simple Discovery
// <link rel="EditURI" type="application/rsd+xml" title="RSD" href="http://1270.0.0.1/xmlrpc.php" />
/*
<?xml version="1.0" encoding="UTF-8"?>
<rsd version="1.0" xmlns="http://archipelago.phrasewise.com/rsd">
  <service>
    <engineName>WordPress</engineName>
    <engineLink>https://wordpress.org/</engineLink>
    <homePageLink>http://1270.0.0.1</homePageLink>
    <apis>
      <api name="WordPress" blogID="1" preferred="true" apiLink="http://1270.0.0.1/xmlrpc.php" />
      <api name="Movable Type" blogID="1" preferred="false" apiLink="http://1270.0.0.1/xmlrpc.php" />
      <api name="MetaWeblog" blogID="1" preferred="false" apiLink="http://1270.0.0.1xmlrpc.php" />
      <api name="Blogger" blogID="1" preferred="false" apiLink="http://1270.0.0.1/xmlrpc.php" />
      <api name="WP-API" blogID="1" preferred="false" apiLink="http://1270.0.0.1/wp-json/" />
    </apis>
  </service>
</rsd>
*/


// AtomPub Service Document
// Content-Type: application/atomsvc+xml
/*
<?xml version="1.0" encoding="UTF-8"?>
<service
    xmlns="http://www.w3.org/2007/app"
    xmlns:atom="http://www.w3.org/2005/Atom">
    <workspace>
        <atom:title>BlogTitle</atom:title>
        <collection href="https://livedoor.blogcms.jp/atompub/userid/article">
            <atom:title>BlogTitle - Entries</atom:title>
            <accept>application/atom+xml;type=entry</accept>
        
            <categories fixed="no" scheme="https://livedoor.blogcms.jp/atompub/userid/category">
            </categories>
        </collection>
        <collection href="https://livedoor.blogcms.jp/atompub/userid/image">
            <atom:title>BlogTitle - Images</atom:title>
            <accept>image/png</accept>
            <accept>image/jpeg</accept>
            <accept>image/gif</accept>
        </collection>
    </workspace>
</service>
*/

// AtomAPI at vox
// Content-Type: application/atom+xml
/*
<?xml version="1.0" encoding="utf-8"?>
<feed xmlns="http://purl.org/atom/ns#">
    <link xmlns="http://purl.org/atom/ns#" rel="service.post" href="http://www.vox.com/services/atom/svc=post/collection_id=6a00c2251f52cd549d00c2251f5478604a" title="blog" type="application/x.atom+xml"/>
    <link xmlns="http://purl.org/atom/ns#" rel="alternate" href="http://marumoto.vox.com/" title="blog" type="text/html"/>
    <link xmlns="http://purl.org/atom/ns#" rel="service.feed" href="http://www.vox.com/services/atom/svc=asset/6p00c2251f52cd549d" title="blog" type="application/atom+xml"/>
    <link xmlns="http://purl.org/atom/ns#" rel="service.upload" href="http://www.vox.com/services/atom/svc=asset" title="blog" type="application/atom+xml"/>
    <link xmlns="http://purl.org/atom/ns#" rel="replies" href="http://www.vox.com/services/atom/svc=asset/6p00c2251f52cd549d/type=Comment" title="blog" type="application/atom+xml"/>
</feed>
*/

// AtomAPI at livedoor http://cms.blog.livedoor.com/atom
// Content-Type: application/x.atom+xml
/*
<?xml version="1.0" encoding="UTF-8"?>
<feed xmlns="http://purl.org/atom/ns#">
    <link xmlns="http://purl.org/atom/ns#" type="application/x.atom+xml" rel="service.post" href="http://cms.blog.livedoor.com/atom/blog_id=95864" title="hepcat de　ブログ"/>
    <link xmlns="http://purl.org/atom/ns#" type="application/x.atom+xml" rel="service.feed" href="http://cms.blog.livedoor.com/atom/blog_id=95864" title="hepcat de　ブログ"/>
    <link xmlns="http://purl.org/atom/ns#" type="application/x.atom+xml" rel="service.categories" href="http://cms.blog.livedoor.com/atom/blog_id=95864/svc=categories" title="hepcat de　ブログ"/>
    <link xmlns="http://purl.org/atom/ns#" type="application/x.atom+xml" rel="service.upload" href="http://cms.blog.livedoor.com/atom/blog_id=95864/svc=upload" title="hepcat de　ブログ"/>
</feed>
*/

#endregion
