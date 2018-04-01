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
using System.Xml;
using System.Xml.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BlogWrite.Models;

namespace BlogWrite.Models.Clients
{

    public abstract class BlogClient
    {
        protected HTTPConnection _HTTPConn;
        protected string _userName = "";
        protected string _userPassword = "";
        protected Uri _endpoint;

        public BlogClient(string userName, string userPassword, Uri endpoint)
        {
            _userName = userName;
            _userPassword = userPassword;
            _endpoint = endpoint;

            _HTTPConn = HTTPConnection.Instance;

        }

        public abstract Task<NodeService> GetAccount(string accountName);

        public abstract Task<NodeCollections> GetBlogs();

        public abstract Task<List<EntryItem>> GetEntries(Uri entriesUrl);

        public abstract Task<EntryFull> GetFullEntry(Uri entryUri);

        public abstract Task<bool> UpdateEntry(EntryFull entry);

        public abstract Task<bool> PostEntry(EntryFull entry);

        public abstract Task<bool> DeleteEntry(Uri editUri);

    }

    /// <summary>
    /// Holds HTTP connection. Singleton.
    /// https://qiita.com/laughter/items/e6be52db15d7326b46b9
    /// </summary>
    public class HTTPConnection
    {
        private HttpClient _httpClient;

        public HttpClient Client
        {
            get
            {
                return _httpClient;
            }
        }

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
            _httpClient = new HttpClient();
        }


    }


}
