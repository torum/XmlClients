using System;
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

namespace BlogWrite.Models
{
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

        public FeedKinds FeedKind { get; set; }

        public FeedLink(Uri feedUri, FeedKinds feedKind, string title, Uri siteUri)
        {
            FeedUri = feedUri;
            FeedKind = feedKind;
            Title = title;
            SiteUri = siteUri;
        }
    }

    // Returns NodeService that holds AtomPub Service Document infomation.
    public class SearviceDocumentLink
    {
        public enum ServiceDocumentKinds
        {
            Feed,
            RSD,
            AtomSrv,
            AtomApi,
            Unknown
        }

        public Uri EndpointUri { get; set; }

        public ServiceDocumentKinds ServiceDocumentKind { get; set; }

        // XML-RPC specific blogid. 
        public string BlogID { get; set; }

        public SearviceDocumentLink(Uri fu, ServiceDocumentKinds fk)
        {
            EndpointUri = fu;
            ServiceDocumentKind = fk;
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
    class ServiceHtmlResult : ServiceResultBase
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

        public ServiceHtmlResult()
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

    // TODO:
    // ServiceResultXmlRpc_MT
    // ServiceResultXmlRpc_WP

}
