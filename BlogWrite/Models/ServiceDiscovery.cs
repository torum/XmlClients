/// 
/// 
/// BlogWrite 
///  - C#/WPF port of the original "BlogWrite" developed with Delphi.
/// https://github.com/torum/BlogWrite
/// 
/// 
/// Atom Publishing Protocol - Service document
/// https://tools.ietf.org/html/rfc5023
///  
/// Really Simple Discovery (RSD) 
/// https://cyber.harvard.edu/blogs/gems/tech/rsd.html
/// 

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

namespace BlogWrite.Models
{
    /// <summary>
    /// Service Discovery Result class.
    /// </summary>
    abstract class ServiceResultBase
    {
        
    }

    class ServiceResultErr : ServiceResultBase
    {
        public string Err { get; set; }

        public ServiceResultErr(string e)
        {
            Err = e;
        }
    }

    class ServiceResultAuthRequired : ServiceResultBase
    {
        public Uri Addr { get; set; }

        public ServiceResultAuthRequired(Uri addr)
        {
            Addr = addr;
        }
    }

    class ServiceResultAtomFeed : ServiceResultBase
    {
        public Uri AtomFeedUrl;

        public ServiceResultAtomFeed(Uri addr)
        {
            AtomFeedUrl = addr;
        }
    }

    abstract class ServiceResult : ServiceResultBase
    {
        public ServiceTypes Service { get; set; }

        public Uri EndpointUri;

        public ServiceResult(ServiceTypes type, Uri ep)
        {
            Service = type;
            EndpointUri = ep;
        }

        public ServiceResult()
        {
            Service = ServiceTypes.Unknown;
            EndpointUri = null;
        }
    }

    class ServiceResultAtomPub: ServiceResult
    {
        
    }

    class ServiceResultAtomAPI : ServiceResult
    {

    }

    class ServiceResultXmlRpc: ServiceResult
    {
        // XML-RPC specific blogid. 
        public string BlogID { get; set; }
    }

    /// <summary>
    /// Service Discovery class.
    /// </summary>
    class ServiceDiscovery
    {
        private HttpClient _httpClient;
        private _serviceDocumentKind _serviceDocKind;
        private string _serviceDocUrl;
        private Uri _endpointUrl;
        private Uri _atomFeedUrl;
        private string _blogId = "";
        private ServiceTypes _serviceTypes;

        private enum _serviceDocumentKind
        {
            RSD,
            AtomSrv,
            AtomApi,
            Unknown
        }

        /*
        private enum _rsdApiType
        {
            WordPress,      // WordPress XML-RPC
            MovableType,    // Movable Type XML-RPC
            MetaWeblog,     // Used with Movable Type XML-RPC API
            Blogger,        // Deprecated. May be used with Movable Type XML-RPC API
            WordPressJsonRestApi,  // WordPress Jason REST API
            AtomAPI         // Deprecated Atom 0.3 API
        }
        */

        /*
        private enum _atomType
        {
            atomFeed,
            atomPub,
            atomAPI,
            atomGData
        }
        */

        public ServiceDiscovery()
        {
            _httpClient = new HttpClient();

            _serviceDocKind = _serviceDocumentKind.Unknown;

            _serviceTypes = ServiceTypes.Unknown;
        }

        #region == Events ==

        public delegate void ServiceDiscoveryStatusUpdate(ServiceDiscovery sender, string data);

        public event ServiceDiscoveryStatusUpdate StatusUpdate;

        #endregion

        #region == Methods ==

        public async Task<ServiceResultBase> DiscoverService(Uri addr)
        {
            // Initialize variables.
            _serviceDocKind = _serviceDocumentKind.Unknown;
            _serviceDocUrl = null;
            _endpointUrl =null;
            _atomFeedUrl = null;
            _blogId = "";
            _serviceTypes= ServiceTypes.Unknown;

            UpdateStatus(">> Trying to access given URL...");

            var HTTPResponse = await _httpClient.GetAsync(addr);

            if (HTTPResponse.IsSuccessStatusCode)
            {
                if (HTTPResponse.Content == null)
                {
                    UpdateStatus("<< Received no content.");
                    ServiceResultErr re = new ServiceResultErr("Did not return any content. Content empty.");
                    return re;
                }

                string contenTypeString = HTTPResponse.Content.Headers.GetValues("Content-Type").FirstOrDefault();

                if (!string.IsNullOrEmpty(contenTypeString))
                {
                    Debug.WriteLine("GET Content-Type header is: " + contenTypeString);

                    if (contenTypeString.StartsWith("text/html"))
                    {
                        UpdateStatus("<< Returned a HTML webpage.");

                        bool x = await ParseHTML(HTTPResponse.Content);

                        if (_serviceDocKind == _serviceDocumentKind.AtomSrv)
                        {
                            ServiceResultAtomPub ap = new ServiceResultAtomPub();
                            ap.EndpointUri = new Uri(_serviceDocUrl);
                            ap.Service = ServiceTypes.AtomPub;
                            return ap;
                        }
                        else if (_serviceDocKind == _serviceDocumentKind.RSD)
                        {
                            bool y = await GetRSD();
                            
                            if ((_serviceTypes == ServiceTypes.XmlRpc_WordPress) ||
                                    (_serviceTypes == ServiceTypes.XmlRpc_MovableType) &&
                                    (_endpointUrl !=null))
                            {
                                ServiceResultXmlRpc xp = new ServiceResultXmlRpc();
                                xp.Service = _serviceTypes;
                                xp.EndpointUri = _endpointUrl;
                                xp.BlogID = _blogId;
                                return xp;
                            }
                            else
                            {
                                UpdateStatus("Could not determin service type. [WordPress,MovableType] not found.");
                                ServiceResultErr re = new ServiceResultErr("Could not determin service type.");
                                return re;
                            }
                        }
                        else
                        {

                            UpdateStatus("Could not find any service document from HTML webpage.");

                            if (_atomFeedUrl != null)
                            {
                                UpdateStatus("Atom feed link is present.");

                                ServiceResultAtomFeed ap = new ServiceResultAtomFeed(_atomFeedUrl);
                                return ap;
                            }
                            else
                            {
                                UpdateStatus(">> Did not find a service document link.");

                                // Could be xml-rpc endpoint.


                                // TODO: 
                                // A user entered xml-rpc endpoint.
                                // Try POST some method.
                                UpdateStatus("TODO: Could not determine API from the HTML webpage.");
                                // For now.
                                ServiceResultErr re = new ServiceResultErr("Could not determine API from the HTML webpage.");
                                return re;

                                //UpdateStatus(">> Trying to test a few things...");
                            }
                        }
                    }
                    else if (contenTypeString.StartsWith("application/atomsvc+xml"))
                    {
                        // This is the AtomPub endpoint.

                        UpdateStatus("Found an Atom Publishing Protocol service document.");

                        ServiceResultAtomPub ap = new ServiceResultAtomPub();
                        ap.EndpointUri = addr;
                        ap.Service = ServiceTypes.AtomPub; ;
                        return ap;

                    }
                    else if (contenTypeString.StartsWith("application/rsd+xml"))
                    {
                        bool y = await GetRSD();

                        if (((_serviceTypes == ServiceTypes.XmlRpc_WordPress) || (_serviceTypes == ServiceTypes.XmlRpc_MovableType)) 
                            && (_endpointUrl != null))
                        {
                            ServiceResultXmlRpc xp = new ServiceResultXmlRpc();
                            xp.Service = _serviceTypes;
                            xp.EndpointUri = _endpointUrl;
                            xp.BlogID = _blogId;
                            return xp;
                        }
                        else
                        {
                            UpdateStatus("Could not determin service type. [WordPress,MovableType] not found.");
                            ServiceResultErr re = new ServiceResultErr("Could not determin service type.");
                            return re;
                        }
                    }
                    else if (contenTypeString.StartsWith("application/atom+xml"))
                    {
                        // TODO:
                        // Possibly AtomApi endopoint. Or Atom Feed...

                        UpdateStatus("<< Atom format returned...");

                        ServiceResultAtomFeed ap = new ServiceResultAtomFeed(addr);
                        return ap;

                    }
                    else if (contenTypeString.StartsWith("application/x.atom+xml"))
                    {
                        // TODO:
                        // Possibly AtomApi endopoint.
                        UpdateStatus("<< Old Atom format returned... ");

                        ServiceResultAtomAPI ap = new ServiceResultAtomAPI();
                        ap.EndpointUri = addr;
                        ap.Service = ServiceTypes.AtomApi;
                        return ap;
                    }
                    else
                    {
                        UpdateStatus("<< Unknown Content-Type returned. " + contenTypeString + " is not supported.");
                        ServiceResultErr re = new ServiceResultErr("Content-Type unknown.");
                        return re;
                    }

                }
                else
                {
                    UpdateStatus("<< No Content-Type returned. ");
                    ServiceResultErr re = new ServiceResultErr("Content-Type did not match.");
                    return re;
                }

            }
            else
            {
                UpdateStatus("<< HTTP error: " + HTTPResponse.StatusCode.ToString());
                UpdateStatus("Could not retrieve any service document. ");

                //TODO: If 401 Unauthorized,
                // A user may or may not enter an AtomPub endpoint which require auth to get service document.

                ServiceResultAuthRequired rea = new ServiceResultAuthRequired(addr);
                return rea;
            }

            //UpdateStatus(Environment.NewLine + "Finished.");
            //Debug.WriteLine("EndpointUri: " + _endpointUrl + " Service: " + _serviceTypes.ToString());

        }

        private async Task<bool> ParseHTML(HttpContent content)
        {
            /*
            UpdateStatus(">> Trying to parse a HTML document...");

            //Stream st = content.ReadAsStreamAsync().Result;

            string s = await content.ReadAsStringAsync();

            UpdateStatus(">> Loading a HTML document...");

            mshtml.HTMLDocument hd = new mshtml.HTMLDocument();
            mshtml.IHTMLDocument2 hdoc = (mshtml.IHTMLDocument2)hd;

            IPersistStreamInit ips = (IPersistStreamInit)hdoc;
            ips.InitNew();

            hdoc.designMode = "On";
            hdoc.clear();
            hdoc.write(s);
            hdoc.close();

            // In Delphi, it's just
            // hdoc:= CreateComObject(Class_HTMLDocument) as IHTMLDocument2;
            // (hdoc as IPersistStreamInit).Load(TStreamAdapter.Create(Response.Stream));

            int i = 0;
            while (hdoc.readyState != "complete")
            {
                await Task.Delay(100);
                if (i > 500)
                {
                    throw new Exception(string.Format("The document {0} timed out while loading", "IHTMLDocument2"));
                }
                i++;
            }

            if (hdoc.readyState == "complete")
            {

                UpdateStatus(">> Checking HTML link tags...");

                IHTMLElementCollection ElementCollection = hdoc.all;
                foreach (var e in ElementCollection)
                {
                    if ((e as IHTMLElement).tagName == "LINK")
                    {
                        string re = (e as IHTMLElement).getAttribute("rel", 0);
                        if (!string.IsNullOrEmpty(re)) {
                            if (re.ToUpper() == "EDITURI")
                            {
                                string hf = (e as IHTMLElement).getAttribute("href", 0);
                                if (!string.IsNullOrEmpty(hf))
                                {
                                    _serviceDocKind = _serviceDocumentKind.RSD;
                                    _serviceDocUrl = hf;

                                    Debug.WriteLine("ServiceDocumentKind is: RSD " + _serviceDocUrl);

                                    UpdateStatus("Found a link to a RSD documnet.");

                                }
                            }
                            else if (re.ToUpper() == "SERVICE")
                            {
                                string ty = (e as IHTMLElement).getAttribute("type", 0);
                                if (!string.IsNullOrEmpty(ty))
                                {
                                    if (ty == "application/atomsvc+xml")
                                    {
                                        string hf = (e as IHTMLElement).getAttribute("href", 0);
                                        if (!string.IsNullOrEmpty(hf))
                                        {
                                            _serviceDocKind = _serviceDocumentKind.AtomSrv;
                                            _serviceDocUrl = hf;

                                            Debug.WriteLine("ServiceDocumentKind is: AtomSrv " + _serviceDocUrl);

                                            UpdateStatus("Found a link to an Atom service documnet.");
                                        }
                                    }
                                }

                            }
                            else if (re == "https://api.w.org/")
                            {
                                string hf = (e as IHTMLElement).getAttribute("href", 0);
                                if (!string.IsNullOrEmpty(hf))
                                {
                                    //_serviceDocKind = _serviceDocumentKind.AtomSrv;
                                    //_serviceDocUrl = hf;

                                    Debug.WriteLine("Found a link to WP REST API: " + hf);

                                    UpdateStatus("Found a link to WordPress JSON REST API.");
                                }

                            }
                            else if (re.ToUpper() == "ALTERNATE")
                            {
                                string ty = (e as IHTMLElement).getAttribute("type", 0);
                                if (!string.IsNullOrEmpty(ty))
                                {
                                    if (ty == "application/atom+xml")
                                    {
                                        string hf = (e as IHTMLElement).getAttribute("href", 0);
                                        if (!string.IsNullOrEmpty(hf))
                                        {
                                            Debug.WriteLine("Atom feed found.");
                                            try
                                            {
                                                _atomFeedUrl = new Uri(hf);
                                            }
                                            catch { }

                                            UpdateStatus("Found a link to an Atom feed.");
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
                
            }
            */
            return true;
        }

        private async Task<bool> GetRSD()
        {
            UpdateStatus(">> Trying to access the RSD document...");
            
            var HTTPResponse = await _httpClient.GetAsync(_serviceDocUrl);

            if (!HTTPResponse.IsSuccessStatusCode)
            {
                UpdateStatus("<< HTTP error: " + HTTPResponse.StatusCode.ToString());
                UpdateStatus("Failed to retrive the RSD document.");
                return false;
            }
                
            if (HTTPResponse.Content == null)
            {
                UpdateStatus("<< Returned no content.");
                return false;
            }
            
            UpdateStatus(">> Loading a RSD document...");

            var st = await HTTPResponse.Content.ReadAsStreamAsync();
            if (st == null)
                return false;

            XmlDocument xdoc = new XmlDocument();
            try
            {
                xdoc.Load(st);
            }
            catch (Exception e)
            {
                UpdateStatus("Load RSD failed.  Xml document error: " + e.Message);

                Debug.WriteLine("LoadXml failed: " + e.Message);
            }

            // RSD: XML-RPC or AtomAPI or WP json
            // Content-Type: application/rsd+xml

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

            UpdateStatus(">> Trying to parse the RSD documnet...");

            XmlNamespaceManager NsMgr = new XmlNamespaceManager(xdoc.NameTable);
            NsMgr.AddNamespace("rsd", "http://archipelago.phrasewise.com/rsd");

            XmlNodeList apis = xdoc.SelectNodes("//rsd:rsd/rsd:service/rsd:apis/rsd:api", NsMgr);
            if (apis == null)
                return false;

            var api = "";
            foreach (XmlNode a in apis)
            {
                var apiLink = a.Attributes["apiLink"].Value;
                if (!string.IsNullOrEmpty(apiLink))
                {
                    var nm = "";
                    
                    var pref = a.Attributes["preferred"].Value;
                    if (pref.ToLower() == "true")
                    {
                        api = apiLink;
                        
                        nm = a.Attributes["name"].Value;
                        if (!string.IsNullOrEmpty(nm))
                        {
                            if (nm.ToLower() == "wordpress")
                            {
                                _serviceTypes = ServiceTypes.XmlRpc_WordPress;

                                var id = a.Attributes["blogID"].Value;
                                if (!string.IsNullOrEmpty(id)) {
                                    _blogId = id;
                                }

                                UpdateStatus("Found WordPress API.");

                                break;
                            }
                            else if (nm.ToLower() == "movable type")
                            {
                                _serviceTypes = ServiceTypes.XmlRpc_MovableType;

                                var id = a.Attributes["blogID"].Value;
                                if (!string.IsNullOrEmpty(id))
                                {
                                    _blogId = id;
                                }

                                UpdateStatus("Found Movable Type API.");

                                break;
                            }
                        }
                    }

                    api = apiLink;

                    nm = a.Attributes["name"].Value;
                    if (!string.IsNullOrEmpty(nm))
                    {
                        if (nm.ToLower() == "wordpress")
                        {
                            _serviceTypes = ServiceTypes.XmlRpc_WordPress;

                            var id = a.Attributes["blogID"].Value;
                            if (!string.IsNullOrEmpty(id))
                            {
                                _blogId = id;
                            }

                            UpdateStatus("Found WordPress API.");

                            break;
                        }
                        else if (nm.ToLower() == "movable type")
                        {
                            _serviceTypes = ServiceTypes.XmlRpc_MovableType;

                            var id = a.Attributes["blogID"].Value;
                            if (!string.IsNullOrEmpty(id))
                            {
                                _blogId = id;
                            }

                            UpdateStatus("Found Movable Type API.");

                            break;
                        }
                    }

                    
                }
            }

            if (!string.IsNullOrEmpty(api))
            {
                _endpointUrl = new Uri(api);
                return true;
            }
            else
            {
                return false;
            }

        }

        private async void UpdateStatus(string data)
        {
            await Task.Run(() => { StatusUpdate?.Invoke(this, data); });
        }

        #endregion
    }


    ////////////////////////////////////////////////////////////////////////////////


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

    ////////////////////////////////////////////////////////////////////////////////



    // IPersistStreamInit

    [ComImport(), Guid("0000010c-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPersist
    {
        void GetClassID(Guid pClassId);
    }
    [ComImport(), Guid("7FD52380-4E07-101B-AE2D-08002B2EC713"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPersistStreamInit : IPersist
    {
        void GetClassID([In, Out] ref Guid pClassId);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig()]
        int IsDirty();
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig()]
        void Load(IStream pStm); //System.Runtime.InteropServices.ComTypes.IStream
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig()]
        void Save(IStream pStm, [In, MarshalAs(UnmanagedType.Bool)] bool fClearDirty);//System.Runtime.InteropServices.ComTypes.IStream
        void GetMaxSize([Out]long pCbSize);
        [return: MarshalAs(UnmanagedType.I4)]
        [PreserveSig()]
        void InitNew();
    }
}


