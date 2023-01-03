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
using Windows.Graphics.Imaging;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Reflection;
using Microsoft.UI.Xaml.Media.Imaging;

namespace BlogWrite.Models.Clients
{
    // Plain HTTP client wrapped as HTTP Connection.
    public class HTTPConnection
    {
        public HttpClient Client { get; }

        public HTTPConnection()
        {
            Client = new HttpClient();
            Client.DefaultRequestHeaders.UserAgent.TryParseAdd("BlogWrite" + "/" + Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }
    }

    // Base HTTP client.
    public abstract class BaseClient
    {
        protected HTTPConnection _HTTPConn;

        public abstract Task<HttpClientEntryItemCollectionResultWrapper> GetEntries(Uri entriesUrl, string feedId);

        public async Task<List<EntryItem>> GetImages(List<EntryItem> entryItems)
        {
            foreach (var entItem in entryItems)
            {
                if (entItem.IsImageDownloaded)
                    continue;

                if (entItem.ImageUri != null)
                {
                    //Debug.WriteLine("Gettting Image: " + entItem.ImageUri.AbsoluteUri);
                    /*
                    // 
                    Byte[] bytes = await this.GetImage(entItem.ImageUri);

                    if (bytes != Array.Empty<byte>())
                    {
                        var imageSource = (BitmapSource)new ImageSourceConverter().ConvertFrom(bytes);  
                        var width = 220d;
                        var scale = width / imageSource.PixelWidth;
                        //var height = 127d;
                        //var scale = height / imageSource.PixelHeight;
                        WriteableBitmap writable = new WriteableBitmap(new TransformedBitmap(imageSource, new ScaleTransform(scale, scale)));
                        writable.Freeze();

                        entItem.ImageByteArray = WritableBitmapImageToByteArray(writable);

                        if (Application.Current != null)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                entItem.Image = BitmapImageFromBytes(bytes);
                            });
                        }
                    }
                    */
                }
            }

            return entryItems;
        }

        private Byte[] WritableBitmapImageToByteArray(WriteableBitmap writableBitmapImage)
        {
            try
            {
                byte[] data = Array.Empty<byte>();
                /*
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)writableBitmapImage));
                using (MemoryStream ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    data = ms.ToArray();
                }
                */
                return data;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + " @BaseClient::WritableBitmapImageToByteArray");

                ToDebugWindow("<< " + e.InnerException.ToString() + " @BaseClient::WritableBitmapImageToByteArray: " + e.Message + Environment.NewLine);

                return Array.Empty<byte>();
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

        // Writes to Debug (raises event)
        protected void ToDebugWindow(string data)
        {
            Task nowait = Task.Run(() => { DebugOutput?.Invoke(this, data); });
        }

        private async Task<byte[]> GetImage(Uri imageUri)
        {
            byte[] res = Array.Empty<byte>();

            try
            {
                res = await _HTTPConn.Client.GetByteArrayAsync(imageUri);
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    Debug.WriteLine(e.InnerException.Message + " @BaseClient::GetImage:GetByteArrayAsync");

                    ToDebugWindow("<< " + e.InnerException.ToString() + " @BaseClient::GetImage:GetByteArrayAsync: " + e.InnerException.Message + Environment.NewLine + imageUri.AbsoluteUri + Environment.NewLine);
                }
                else
                {
                    Debug.WriteLine(e.Message + " @BaseClient::GetImage:GetByteArrayAsync");

                    ToDebugWindow("<< " + e.ToString() + " @BaseClient::GetImage:GetByteArrayAsync: " + e.Message + Environment.NewLine + imageUri.AbsoluteUri + Environment.NewLine);
                }
            }

            return res;
        }

        #region == Fill ErrorObject ==

        protected ErrorObject InvalidUriScheme(ErrorObject err, string scheme, string errPlaceParent)
        {
            err.ErrCode = "";
            err.ErrType = ErrorObject.ErrTypes.HTTP;
            err.ErrText = "Invalid URI scheme";
            err.ErrDescription = "URI scheme should be http or https : " + scheme;
            err.ErrPlace = "";
            err.ErrPlaceParent = errPlaceParent;
            err.ErrDatetime = DateTime.Now;
            return err;
        }

        protected ErrorObject InvalidContentType(ErrorObject err, string errText, string errPlace, string errPlaceParent)
        {
            err.ErrCode = "";
            err.ErrType = ErrorObject.ErrTypes.API;
            err.ErrText = "Invalid Content-Type returned";
            err.ErrDescription = errText;
            err.ErrPlace = errPlace;
            err.ErrPlaceParent = errPlaceParent;
            err.ErrDatetime = DateTime.Now;

            return err;
        }

        protected ErrorObject InvalidXml(ErrorObject err, string eMessage, string errPlaceParent)
        {
            err.ErrCode = "";
            err.ErrType = ErrorObject.ErrTypes.API;
            err.ErrText = "Invalid XML document returned";
            err.ErrDescription = eMessage;
            err.ErrPlace = "xdoc.Load()";
            err.ErrPlaceParent = errPlaceParent;
            err.ErrDatetime = DateTime.Now;

            return err;
        }

        protected ErrorObject FormatUndetermined(ErrorObject err, string errPlaceParent)
        {
            err.ErrCode = "";
            err.ErrType = ErrorObject.ErrTypes.API;
            err.ErrText = "Document parse failed";
            err.ErrDescription = "Unknown format";
            err.ErrPlace = "xdoc.DocumentElement.LocalName/NamespaceURI";
            err.ErrPlaceParent = errPlaceParent;
            err.ErrDatetime = DateTime.Now;

            return err;
        }

        protected ErrorObject NonSuccessStatusCode(ErrorObject err, string statusCode, string errPlace, string errPlaceParent)
        {
            err.ErrCode = "";
            err.ErrType = ErrorObject.ErrTypes.HTTP;
            err.ErrText = "HTTP request failed";
            err.ErrDescription = statusCode;
            err.ErrPlace = errPlace;
            err.ErrPlaceParent = errPlaceParent;
            err.ErrDatetime = DateTime.Now;

            return err;
        }

        protected ErrorObject HttpReqException(ErrorObject err, string eMessage, string errPlace, string errPlaceParent)
        {
            err.ErrCode = "";
            err.ErrType = ErrorObject.ErrTypes.HTTP;
            err.ErrText = "HTTP request error (HttpRequestException)";
            err.ErrDescription = eMessage;
            err.ErrPlace = errPlace;
            err.ErrPlaceParent = errPlaceParent;
            err.ErrDatetime = DateTime.Now;

            return err;
        }

        protected ErrorObject GenericException(ErrorObject err, string errCode, ErrorObject.ErrTypes errType, string errText, string errDescription, string errPlace, string errPlaceParent)
        {
            err.ErrCode = errCode;
            err.ErrType = errType;
            err.ErrText = errText;
            err.ErrDescription = errDescription;
            err.ErrPlace = errPlace;
            err.ErrPlaceParent = errPlaceParent;
            err.ErrDatetime = DateTime.Now;

            return err;
        }

        #endregion

        #region == Util methods ==

        // <summary>Strips style attributes from HTML string. </summary>
        protected async Task<string> StripStyleAttributes(string s)
        {
            try
            {
                var context = BrowsingContext.New(Configuration.Default);
                var document = await context.OpenAsync(req => req.Content(s));
                //var blueListItemsLinq = document.QuerySelectorAll("*")
                var ItemsLinq = document.All.Where(m => m.HasAttribute("style"));
                foreach (var item in ItemsLinq)
                {
                    item.RemoveAttribute("style");
                }

                // TODO: get "Body" element's chiled.
                return document.DocumentElement.InnerHtml;

            }
            catch(Exception e)
            {
                Debug.WriteLine("Exception@StripStyleAttributes: "+e.Message);

                return "";
            }
        }

        // <summary>Strips HTML tags from HTML string. </summary>
        protected async Task<string> StripHtmlTags(string s)
        {
            try
            {
                var context = BrowsingContext.New(Configuration.Default);
                var document = await context.OpenAsync(req => req.Content(s));

                return document.DocumentElement.TextContent;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception@StripHtmlTags: " + e.Message);

                return "";
            }
        }

        // <summary>GetImageUriFromHtml. </summary>
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

        // <summary>BitmapImageFromBytes. </summary>
        protected static BitmapImage BitmapImageFromBytes(Byte[] bytes)
        {
            try
            {
                using (var stream = new MemoryStream(bytes))
                {
                    BitmapImage bmimage = new BitmapImage();
                    /*
                    bmimage.BeginInit();
                    bmimage.CacheOption = BitmapCacheOption.OnLoad;
                    bmimage.StreamSource = stream;
                    bmimage.EndInit();
                    */
                    return bmimage;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + " @BaseClient::BitmapImageFromBytes");

                return null;
            }
        }

        // <summary> Truncates string with maxLength. </summary>
        protected static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + " ...";
        }

        #endregion

    }
}


