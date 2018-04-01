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

namespace BlogWrite.Models.Clients
{
    public class DiscoveryClient
    {
        private HTTPConnection _HTTPConn;

        private ServiceDocumentKind _serviceDocKind;
        private string _serviceDocUrl;

        public enum ServiceDocumentKind
        {
            RSD,
            AtomSrv,
            Unknown
        }

        
        public enum RsdApiType
        {
            WordPress,
            MovableType,
            MetaWeblog,
            Blogger,
            WPAPI,
            AtomAPI
        }

        public DiscoveryClient()
        {

            _HTTPConn = HTTPConnection.Instance;

            _serviceDocKind = ServiceDocumentKind.Unknown;

        }

        public async void DiscoverService(Uri addr)
        {
            var HTTPResponseMessage = await _HTTPConn.Client.GetAsync(addr);

            if (HTTPResponseMessage.IsSuccessStatusCode)
            {
                if (HTTPResponseMessage.Content == null)
                    return;

                string contenTypeString =  HTTPResponseMessage.Content.Headers.GetValues("Content-Type").FirstOrDefault();

                if (!string.IsNullOrEmpty(contenTypeString))
                {
                    System.Diagnostics.Debug.WriteLine("GET Content-Type header is: " + contenTypeString);
                    //text/html; charset=UTF-8

                    if (contenTypeString.StartsWith("text/html"))
                    {

                        ParseHTML(HTTPResponseMessage.Content);

                        if (_serviceDocKind == ServiceDocumentKind.AtomSrv)
                        {
                            // return
                        }
                        else if (_serviceDocKind == ServiceDocumentKind.RSD)
                        {
                            //
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
                        _serviceDocKind = ServiceDocumentKind.AtomSrv;
                        _serviceDocUrl = addr.AbsoluteUri;

                        // return
                    }
                    else if (contenTypeString.StartsWith("application/rsd+xml"))
                    {
                        //
                        ParseRSD(HTTPResponseMessage.Content);
                    }


                }



            }
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
                                    _serviceDocKind = ServiceDocumentKind.RSD;
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
                                            _serviceDocKind = ServiceDocumentKind.AtomSrv;
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

    }

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
