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

namespace BlogWrite.Models.Clients
{
    class XmlRpcClient : BlogClient
    {
        public XmlRpcClient(string userName, string userPassword, Uri endpoint) : base(userName, userPassword, endpoint)
        {

        }

        public override async Task<NodeService> GetAccount(string accountName)
        {
            NodeService account = new NodeService(accountName, _userName, _userPassword, _endpoint, ApiTypes.atXMLRPC);

            NodeCollections blogs = await GetBlogs();

            foreach (var item in blogs.Children)
            {
                item.Parent = account;
                account.Children.Add(item);
            }

            account.Expanded = true;

            return account;
        }

        public override async Task<NodeCollections> GetBlogs()
        {
            NodeCollections blogs = new NodeCollections();

            return blogs;
        }

        public override async Task<List<EntryItem>> GetEntries(Uri entriesUrl)
        {
            List<EntryItem> list = new List<EntryItem>();

            return list;
        }

        public override async Task<EntryFull> GetFullEntry(Uri entryUri)
        {

            return null;
        }

        public override async Task<bool> UpdateEntry(EntryFull entry)
        {
            return true;

        }

        public override async Task<bool> PostEntry(EntryFull entry)
        {
            return true;

        }

        public override async Task<bool> DeleteEntry(Uri editUri)
        {
            return true;

        }
    }
}
