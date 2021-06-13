using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BlogWrite.Models;
using AngleSharp;
using BlogWrite.Common;
using System.Windows.Media.Imaging;
using System.IO;

namespace BlogWrite.Models.Clients
{
    /// <summary>
    /// Plain HTTP client wrapped.
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

        // TODO: create result class which inclues error info
        public abstract Task<List<EntryItem>> GetEntries(Uri entriesUrl);

        //
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

        public async Task<byte[]> GetImage(Uri imageUri)
        {
            byte[] res = Array.Empty<byte>();

            res = await _HTTPConn.Client.GetByteArrayAsync(imageUri);

            return res;
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

        /// <summary>
        /// Writes to Debug (raises event)
        /// </summary>
        protected void ToDebugWindow(string data)
        {
            Task nowait = Task.Run(() => { DebugOutput?.Invoke(this, data); });
        }

        /// <summary>
        /// Strips style attributes from HTML string.
        /// </summary>
        protected async Task<string> StripStyleAttributes(string s)
        {
            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(s));
            //var blueListItemsLinq = document.QuerySelectorAll("*")
            var ItemsLinq = document.All.Where(m => m.HasAttribute("style"));
            foreach (var item in ItemsLinq)
            {
                item.RemoveAttribute("style");
            }

            //return document.DocumentElement.TextContent;
            return document.DocumentElement.InnerHtml;

        }

        /// <summary>
        /// Strips HTML tags from HTML string.
        /// </summary>
        protected async Task<string> StripHtmlTags(string s)
        {
            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(s));

            return document.DocumentElement.TextContent;
        }

        /// <summary>
        /// Truncates string with maxLength.
        /// </summary>
        protected static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + " ...";
        }

        protected async Task<Uri> GetImageUriFromHtml(string s)
        {
            Uri imageUri = null;

            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(s));
            var sl = document.DocumentElement.QuerySelector("img");
            if (sl != null)
            {
                string imgSrc = sl.GetAttribute("src");
                if (!string.IsNullOrEmpty(imgSrc))
                {
                    try
                    {
                        //Debug.WriteLine("imgSrc: " + imgSrc);
                        imageUri = new Uri(imgSrc);
                    }
                    catch { }
                }
            }

            return imageUri;
        }

        protected static BitmapImage BitmapImageFromBytes(Byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                BitmapImage bmimage = new BitmapImage();
                bmimage.BeginInit();
                bmimage.CacheOption = BitmapCacheOption.OnLoad;

                bmimage.DecodePixelWidth = 220;

                bmimage.StreamSource = stream;
                bmimage.EndInit();
                return bmimage;
            }
        }

        /*
        public static Byte[] BitmapImageToByteArray(BitmapImage bitmapImage)
        {
            byte[] data;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                data = ms.ToArray();
            }

            return data;
        }
        */
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


