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
    /// <summary>
    /// BlogClient 
    /// Base HTTP Blog client
    /// </summary>
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

        public abstract Task<NodeWorkspaces> GetBlogs();

        // This has been moved to BaseClient.
        //public abstract Task<List<EntryItem>> GetEntries(Uri entriesUrl);

        public abstract Task<EntryFull> GetFullEntry(Uri entryUri, string postid = "");

        public abstract Task<bool> UpdateEntry(EntryFull entry);

        public abstract Task<bool> PostEntry(EntryFull entry);

        public abstract Task<bool> DeleteEntry(Uri editUri);

        public string AsUTF8Xml(XmlDocument xdoc)
        {
            var sb = new StringBuilder();
            using (var stringWriter = new StringWriterWithEncoding(sb, Encoding.UTF8))
            using (var xmlTextWriter = XmlWriter.Create(stringWriter))
            {
                xdoc.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                return stringWriter.GetStringBuilder().ToString();
            }
        }

        public string AsUTF16Xml(XmlDocument xdoc)
        {
            using (var stringWriter = new System.IO.StringWriter())
            using (var xmlTextWriter = XmlWriter.Create(stringWriter))
            {
                xdoc.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                return stringWriter.GetStringBuilder().ToString();
            }
        }

    }

}
