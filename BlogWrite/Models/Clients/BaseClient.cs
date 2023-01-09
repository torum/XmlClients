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

namespace BlogWrite.Models.Clients;

public abstract class BaseClient : IDisposable
{
    private static readonly object _locker = new object();
    private static volatile HttpClient? _client;

    protected static HttpClient? Client
    {
        get
        {
            if (_client == null)
            {
                lock (_locker)
                {
                    if (_client == null)
                    {
                        _client = new HttpClient();
                    }
                }
            }

            return _client;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_client != null)
            {
                _client.Dispose();
            }

            _client = null;
        }
    }

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
            var data = Array.Empty<byte>();
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
            if (e != null)
            {
                Debug.WriteLine(e.Message + " @BaseClient::WritableBitmapImageToByteArray");

                ToDebugWindow("<< " + e.InnerException.ToString() + " @BaseClient::WritableBitmapImageToByteArray: " + e.Message + Environment.NewLine);

            }
            return Array.Empty<byte>();
        }
    }

    #region == Events ==

    public delegate void ClientDebugOutput(BaseClient sender, string data);

    public event ClientDebugOutput? DebugOutput;

    #endregion

    // Writes to Debug (raises event)
    protected void ToDebugWindow(string data)
    {
        var nowait = Task.Run(() => { DebugOutput?.Invoke(this, data); });
    }

    private async Task<byte[]> GetImage(Uri imageUri)
    {
        var res = Array.Empty<byte>();

        try
        {
            if (_client != null)
            {
                res = await _client.GetByteArrayAsync(imageUri);
            }
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
        err.ErrText = "URI scheme should be http or https : " + scheme;
        err.ErrDescription = "Invalid URI scheme";
        err.ErrPlace = "";
        err.ErrPlaceParent = errPlaceParent;
        err.ErrDatetime = DateTime.Now;
        return err;
    }

    protected ErrorObject InvalidContentType(ErrorObject err, string errText, string errPlace, string errPlaceParent)
    {
        err.ErrCode = "";
        err.ErrType = ErrorObject.ErrTypes.HTTP;
        err.ErrText = errText;
        err.ErrDescription = "Invalid Content-Type returned";
        err.ErrPlace = errPlace;
        err.ErrPlaceParent = errPlaceParent;
        err.ErrDatetime = DateTime.Now;

        return err;
    }

    protected ErrorObject InvalidXml(ErrorObject err, string eMessage, string errPlaceParent)
    {
        err.ErrCode = "";
        err.ErrType = ErrorObject.ErrTypes.XML;
        err.ErrText = eMessage;
        err.ErrDescription = "Invalid XML document returned";
        err.ErrPlace = "xdoc.Load()";
        err.ErrPlaceParent = errPlaceParent;
        err.ErrDatetime = DateTime.Now;

        return err;
    }

    protected ErrorObject FormatUndetermined(ErrorObject err, string errPlaceParent)
    {
        err.ErrCode = "";
        err.ErrType = ErrorObject.ErrTypes.API;
        err.ErrText = "Unknown format";
        err.ErrDescription = "Document parse failed";
        err.ErrPlace = "xdoc.DocumentElement.LocalName/NamespaceURI";
        err.ErrPlaceParent = errPlaceParent;
        err.ErrDatetime = DateTime.Now;

        return err;
    }

    protected ErrorObject NonSuccessStatusCode(ErrorObject err, string statusCode, string errPlace, string errPlaceParent)
    {
        err.ErrCode = "";
        err.ErrType = ErrorObject.ErrTypes.HTTP;
        err.ErrText = statusCode;
        err.ErrDescription = "HTTP request failed";
        err.ErrPlace = errPlace;
        err.ErrPlaceParent = errPlaceParent;
        err.ErrDatetime = DateTime.Now;

        return err;
    }

    protected ErrorObject HttpReqException(ErrorObject err, string eMessage, string errPlace, string errPlaceParent)
    {
        err.ErrCode = "";
        err.ErrType = ErrorObject.ErrTypes.HTTP;
        err.ErrText = eMessage;
        err.ErrDescription = "HTTP request error (HttpRequestException)";
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
    protected static async Task<string> StripStyleAttributes(string s)
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
        catch (Exception e)
        {
            Debug.WriteLine("Exception@StripStyleAttributes: " + e.Message);

            return "";
        }
    }

    // <summary>Strips HTML tags from HTML string. </summary>
    protected static async Task<string> StripHtmlTags(string s)
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
    protected static async Task<Uri> GetImageUriFromHtml(string s)
    {
        Uri imageUri = null;
        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(s));
        var sl = document.DocumentElement.QuerySelector("img");
        if (sl != null)
        {
            var imgSrc = sl.GetAttribute("src");
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
                var bmimage = new BitmapImage();
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


