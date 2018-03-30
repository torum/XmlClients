using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using mshtml;

namespace BlogWrite.Controls
{
    /// <summary>
    /// HTMLBrowserEdit.xaml
    /// </summary>
    public partial class HTMLBrowserEdit : UserControl
    {
        public HTMLBrowserEdit()
        {
            InitializeComponent();

            HTMLBrowser.LoadCompleted += HTMLBrowser_LoadCompleted;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

        }

        public bool DocumentIsReady
        {
            get
            {
                return HtmlDocument != null && HtmlDocument.readyState == "complete";
            }
        }

        private IHTMLDocument2 HtmlDocument
        {
            get
            {
                return HTMLBrowser != null ? (IHTMLDocument2)HTMLBrowser.Document : null;
            }
        }

        private void SetNewStyleSheet(string styleSheet)
        {
            if (DocumentIsReady)
            {
                var styleSheets = HtmlDocument.styleSheets.Cast<IHTMLStyleSheet>().ToList();

                if (styleSheets.Count == 2)
                {
                    var customStyleSheet = styleSheets[1];
                    customStyleSheet.cssText = styleSheet;
                }
            }
        }

        private static string WrapHtmlContent(string source, string styles = null)
        {
            return String.Format(
                @"<html>
                    <head>
                        <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />

                        <!-- Default style sheet -->
                        <style type='text/css'>
                            body {{ font: 10pt verdana; color: #505050; background: #fcfcfc; }}
                            table, td, th, tr {{ border: 1px solid black; border-collapse: collapse; }}
                        </style>
                        <!-- Custom style sheet -->
                        <style type='text/css'>{1}</style>
                    </head>
                    <body contenteditable>{0}</body>
                </html>",
                source, styles);
        }

        #region Event Handlers

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("OnLoaded.");

            if (!DocumentIsReady)
            {
                string HtmlSource="";
                string StyleSheet = "";

                HTMLBrowser.NavigateToString(WrapHtmlContent(HtmlSource, StyleSheet));

            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {

        }


        private void HTMLBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            string StyleSheet = "";
            SetNewStyleSheet(StyleSheet);

            IHTMLDocument2 doc = HTMLBrowser.Document as IHTMLDocument2;

            doc.designMode = "On";
        }


        #endregion
    }
}
