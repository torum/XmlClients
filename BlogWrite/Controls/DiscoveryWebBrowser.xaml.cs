using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Reflection;
using System.Runtime.InteropServices;


namespace BlogWrite.Controls
{
    /// <summary>
    /// DiscoveryWebBrowser.xaml の相互作用ロジック
    /// </summary>
    public partial class DiscoveryWebBrowser : UserControl
    {
        public DiscoveryWebBrowser()
        {
            InitializeComponent();
            /*
            DWB.Navigated += new NavigatedEventHandler(WebBrowserNavigated);
            DWB.LoadCompleted += WebBrowserLoadCompleted;
            */
            //DWB.Navigate("http://torum.jp/");
        }
        /*
        private void WebBrowserNavigated(object sender, NavigationEventArgs e)
        {
            SetSilent(DWB, true); // make it silent
        }

        private void WebBrowserLoadCompleted(object sender, NavigationEventArgs e)
        {

            System.Diagnostics.Debug.WriteLine("WebBrowserLoadCompleted");

            var browser = sender as WebBrowser;

            if (browser == null || browser.Document == null)
                return;

            dynamic document = browser.Document;

            if (document.readyState != "complete")
                return;

            mshtml.HTMLDocument hdoc = document as mshtml.HTMLDocument;

        }


        // https://stackoverflow.com/questions/6138199/wpf-webbrowser-control-how-to-suppress-script-errors
        public static void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            // get an IWebBrowser2 from the document
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }

        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }
        */
    }
}
