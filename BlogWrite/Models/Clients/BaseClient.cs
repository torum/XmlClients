/// 
/// 
/// BlogWrite 
///  - C#/WPF port of the original "BlogWrite" developed with Delphi.
/// https://github.com/torum/BlogWrite

/**
 * 
 * (BaseClient)
 *   AtomFeedClient (BaseClient)
 * 
 *   (BlogClient (BaseClient))
 *     AtomPubClient : BlogClient : BaseClient
 *     XmlRpcMTClient : BlogClient : BaseClient
 *     XmlRpcWPClient : BlogClient : BaseClient
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BlogWrite.Models;

namespace BlogWrite.Models.Clients
{
    /// <summary>
    /// Base HTTP client
    /// </summary>
    public abstract class BaseClient
    {
        protected HTTPConnection _HTTPConn;

        public BaseClient()
        {
            //_HTTPConn = HTTPConnection.Instance;
            _HTTPConn = new HTTPConnection();
        }

        public abstract Task<List<EntryItem>> GetEntries(Uri entriesUrl);


        #region == Events ==

        public delegate void ClientDebugOutput(BaseClient sender, string data);

        public event ClientDebugOutput DebugOutput;

        #endregion

        protected async void ToDebugWindow(string data)
        {
            await Task.Run(() => { DebugOutput?.Invoke(this, data); });
        }

    }

    public class HTTPConnection
    {
        public HttpClient Client { get; }

        public HTTPConnection()
        {
            Client = new HttpClient();
        }
    }
    
    /*
    /// <summary>
    /// Holds HTTP connection. Singleton.
    /// https://qiita.com/laughter/items/e6be52db15d7326b46b9
    /// </summary>
    public class HTTPConnection
    {
        public HttpClient Client { get; }

        public static HTTPConnection Instance
        {
            get { return SingletonHolder._Instance; }
        }

        private static class SingletonHolder
        {
            static SingletonHolder() { }
            internal static readonly HTTPConnection _Instance = new HTTPConnection();
        }

        private HTTPConnection()
        {
            Client = new HttpClient();
        }

    }
    */

}
