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

    public abstract class BlogClient : BaseClient
    {
        protected string _userName = "";
        protected string _userPassword = "";
        protected Uri _endpoint;

        public BlogClient(string userName, string userPassword, Uri endpoint)
        {
            _userName = userName;
            _userPassword = userPassword;
            _endpoint = endpoint;

        }

        public abstract Task<NodeService> GetAccount(string accountName);

        public abstract Task<NodeCollections> GetBlogs();

        //public abstract Task<List<EntryItem>> GetEntries(Uri entriesUrl);

        public abstract Task<EntryFull> GetFullEntry(Uri entryUri);

        public abstract Task<bool> UpdateEntry(EntryFull entry);

        public abstract Task<bool> PostEntry(EntryFull entry);

        public abstract Task<bool> DeleteEntry(Uri editUri);

    }

}
