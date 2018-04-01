/// 
/// 
/// BlogWrite 
///  - C#/WPF port of the original "BlogWrite" developed with Delphi.
/// https://github.com/torum/BlogWrite
/// 
/// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using mshtml;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Diagnostics;

namespace BlogWrite.Models
{
    /// <summary>
    /// 
    /// </summary>
    class ServiceResult
    {
        private ServiceDiscovery.ServiceTypes _service;


        public ServiceDiscovery.ServiceTypes ServiceType
        {
            get
            {
                return _service;
            }
        }

        public Uri EndpointUri;


        public string Err { get; set; }

        public ServiceResult(ServiceDiscovery.ServiceTypes type, Uri ep)
        {
            _service = type;
            EndpointUri = ep;
        }

        public ServiceResult()
        {
            _service = ServiceDiscovery.ServiceTypes.Unknown;
            EndpointUri = null;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    class ServiceDiscovery
    {
        private HttpClient _httpClient;
        private _serviceDocumentKind _serviceDocKind;
        private string _serviceDocUrl;
        private Uri _endpointUrl;

        private enum _serviceDocumentKind
        {
            RSD,
            AtomSrv,
            Unknown
        }

        private enum _rsdApiType
        {
            WordPress,      // WordPress XML-RPC
            MovableType,    // Movable Type XML-RPC
            MetaWeblog,     // Used with Movable Type XML-RPC API
            Blogger,        // Deprecated. May be used with Movable Type XML-RPC API
            WPAPI,          // WordPress REST Jason API
            AtomAPI         // Deprecated Atom 0.3 API
        }

        private enum _atomType
        {
            atomFeed,
            atomPub,
            atomAPI,
            atomGData
        }

        public enum ServiceTypes
        {
            AtomPub,
            AtomPub_Hatena,
            XmlRpc_WordPress,
            XmlRpc_MovableType,
            AtomApi,
            AtomApi_GData,
            Unknown
        }

        public ServiceDiscovery()
        {
            _httpClient = new HttpClient();

            _serviceDocKind = _serviceDocumentKind.Unknown;
        }

        #region == Methods ==

        public async Task<ServiceResult> DiscoverService(Uri addr)
        {
            ServiceResult sr = new ServiceResult();

            var HTTPResponseMessage = await _httpClient.GetAsync(addr);

            if (HTTPResponseMessage.IsSuccessStatusCode)
            {
                if (HTTPResponseMessage.Content == null)
                {
                    sr.Err = "Did not return any content. Content empty.";
                    return sr;
                }

                string contenTypeString =  HTTPResponseMessage.Content.Headers.GetValues("Content-Type").FirstOrDefault();

                if (!string.IsNullOrEmpty(contenTypeString))
                {
                    System.Diagnostics.Debug.WriteLine("GET Content-Type header is: " + contenTypeString);

                    if (contenTypeString.StartsWith("text/html"))
                    {

                        ParseHTML(HTTPResponseMessage.Content);

                        if (_serviceDocKind == _serviceDocumentKind.AtomSrv)
                        {
                            // return
                        }
                        else if (_serviceDocKind == _serviceDocumentKind.RSD)
                        {
                            //
                            ParseRSD(HTTPResponseMessage.Content);
                        }
                        else 
                        {
                            //could be xml-rpc endpoint
                            //http://torum.jp/ja/xmlrpc.php

                            // try post some method.

                        }

                    }
                    else if (contenTypeString.StartsWith("application/atomsvc+xml"))
                    {
                        _serviceDocKind = _serviceDocumentKind.AtomSrv;
                        _serviceDocUrl = addr.AbsoluteUri;

                        // return
                    }
                    else if (contenTypeString.StartsWith("application/rsd+xml"))
                    {
                        //
                        ParseRSD(HTTPResponseMessage.Content);
                    }

                }
                else
                {
                    sr.Err = "Content-Type did not match.";
                    return sr;
                }

            }

            return sr;
        }

        private async void ParseHTML(HttpContent content)
        {

            //Stream st = content.ReadAsStreamAsync().Result;

            string s = await content.ReadAsStringAsync();

            mshtml.HTMLDocument hd = new mshtml.HTMLDocument();
            mshtml.IHTMLDocument2 hdoc = (mshtml.IHTMLDocument2)hd;

            IPersistStreamInit ips = (IPersistStreamInit)hdoc;
            ips.InitNew();

            hdoc.designMode = "On";
            hdoc.clear();
            hdoc.write(s);
            hdoc.close();

            // In delphi, it's just
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
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
                
            }


        }

        private async void ParseRSD(HttpContent content)
        {
            //
        }

        #endregion
    }


    // RSD XML-RPC or AtomAPI
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







    // TStreamAdapter

    // https://msdn.microsoft.com/en-us/library/jj200585(v=vs.85).aspx

    public class StreamWrapper : Stream
    {
        private IStream m_stream;

        private void CheckDisposed()
        {
            if (m_stream == null)
            {
                throw new ObjectDisposedException("StreamWrapper");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (m_stream != null)
            {
                Marshal.ReleaseComObject(m_stream);
                m_stream = null;
            }
        }

        public StreamWrapper(IStream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException();
            }

            m_stream = stream;
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get
            {
                CheckDisposed();

                System.Runtime.InteropServices.ComTypes.STATSTG stat;
                m_stream.Stat(out stat, 1); //STATFLAG_NONAME

                return stat.cbSize;
            }
        }

        public override long Position
        {
            get
            {
                return Seek(0, SeekOrigin.Current);
            }
            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckDisposed();

            if (offset < 0 || count < 0 || offset + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            byte[] localBuffer = buffer;

            if (offset > 0)
            {
                localBuffer = new byte[count];
            }

            IntPtr bytesReadPtr = Marshal.AllocCoTaskMem(sizeof(int));

            try
            {
                m_stream.Read(localBuffer, count, bytesReadPtr);
                int bytesRead = Marshal.ReadInt32(bytesReadPtr);

                if (offset > 0)
                {
                    Array.Copy(localBuffer, 0, buffer, offset, bytesRead);
                }

                return bytesRead;
            }
            finally
            {
                Marshal.FreeCoTaskMem(bytesReadPtr);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            CheckDisposed();

            int dwOrigin;

            switch (origin)
            {
                case SeekOrigin.Begin:

                    dwOrigin = 0;   // STREAM_SEEK_SET
                    break;

                case SeekOrigin.Current:

                    dwOrigin = 1;   // STREAM_SEEK_CUR
                    break;

                case SeekOrigin.End:

                    dwOrigin = 2;   // STREAM_SEEK_END
                    break;

                default:

                    throw new ArgumentOutOfRangeException();

            }

            IntPtr posPtr = Marshal.AllocCoTaskMem(sizeof(long));

            try
            {
                m_stream.Seek(offset, dwOrigin, posPtr);
                return Marshal.ReadInt64(posPtr);
            }
            finally
            {
                Marshal.FreeCoTaskMem(posPtr);
            }
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }



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

