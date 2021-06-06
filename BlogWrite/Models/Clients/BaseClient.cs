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
    /// Plain wrapped HTTP client.
    /// </summary>
    /// 
    public class HTTPConnection
    {
        public HttpClient Client { get; }

        public HTTPConnection()
        {
            Client = new HttpClient();
        }
    }

    /// <summary>
    /// Base HTTP client.
    /// </summary>
    public abstract class BaseClient
    {
        // HTTP client
        protected HTTPConnection _HTTPConn;

        public abstract Task<List<EntryItem>> GetEntries(Uri entriesUrl);

        private string _clientErrorMessage;
        public string ClientErrorMessage
        {
            get
            {
                return _clientErrorMessage;
            }
            protected set
            {
                _clientErrorMessage = value;
            }
        }

        #region == Events ==

        public delegate void ClientDebugOutput(BaseClient sender, string data);

        public event ClientDebugOutput DebugOutput;

        #endregion

        public BaseClient()
        {
            //_HTTPConn = HTTPConnection.Instance;
            _HTTPConn = new HTTPConnection();
        }

        protected void ToDebugWindow(string data)
        {
            Task nowait = Task.Run(() => { DebugOutput?.Invoke(this, data); });
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
