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
    public abstract class BaseClient
    {
        protected HTTPConnection _HTTPConn;

        public BaseClient()
        { 
            _HTTPConn = HTTPConnection.Instance;
        }

        public abstract Task<List<EntryItem>> GetEntries(Uri entriesUrl);

    }

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
}
