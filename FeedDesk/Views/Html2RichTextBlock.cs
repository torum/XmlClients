using System.Net;
using BlogWrite.Core.Models;
using System.Xml.Linq;
using HtmlAgilityPack;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;

namespace FeedDesk.Views;

// Code adapted from https://blogs.msdn.microsoft.com/tess/2013/05/13/displaying-html-content-in-a-richtextblock/
// https://github.com/xleon/HTML2XAML/blob/master/XAMLHtml/XAMLHtml.cs

public class HtmlProperties : DependencyObject
{
    public static readonly DependencyProperty HtmlProperty = DependencyProperty.RegisterAttached(
             "Html",
            typeof(string),
            typeof(HtmlProperties),
            new PropertyMetadata(null, HtmlChanged));

    public static void SetHtml(DependencyObject obj, string value)
        => obj.SetValue(HtmlProperty, value);

    public static string GetHtml(DependencyObject obj)
        => (string)obj.GetValue(HtmlProperty);

    public static Func<Span> H1SpanFactory
    {
        get; set;
    }
    public static Func<Span> H2SpanFactory
    {
        get; set;
    }
    public static Func<Span> H3SpanFactory
    {
        get; set;
    }

    public static Action<object, TappedRoutedEventArgs> OnImageTapped
    {
        get; set;
    }

    public static double ImageMaxPixelWidth { get; set; } = 800.0;
    public static double ImageMaxPixelHeight { get; set; } = 600.0;

    private static RichTextBlock? _currentObject;

    private static void HtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var richText = d as RichTextBlock;
        if (richText == null) 
            return;

        _currentObject = richText;

        App.CurrentDispatcherQueue?.TryEnqueue(() =>
        {
            try
            {
                //Generate blocks
                if (e.NewValue is string xhtml)
                {
                    var blocks = GenerateBlocksForHtml(xhtml);
                    _currentObject = null;

                    //Add the blocks to the RichTextBlock
                    richText.Blocks.Clear();
                    foreach (var b in blocks)
                    {
                        richText.Blocks.Add(b);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("HtmlChanged: " + ex.ToString());
            }
        });


    }

    private static List<Block> GenerateBlocksForHtml(string xhtml)
    {
        var blocks = new List<Block>();

        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(xhtml);

            var block = GenerateParagraph(doc.DocumentNode);
            blocks.Add(block);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GenerateBlocksForHtml: " + ex.Message);
        }

        return blocks;
    }

    // TODO this method seams to be removing necessary spaces in #text nodes
    private static string CleanText(string input)
    {
        /*
        var clean = Windows.Data.Html.HtmlUtilities.ConvertToText(input);
        //clean = System.Net.WebUtility.HtmlEncode(clean);
        if (clean == "\0")
        {
            clean = "\n";
        }
        return clean;
        */
        return input;
    }

    private static Block GenerateBlockForTopNode(HtmlNode node)
        => GenerateParagraph(node);


    private static void AddChildren(Paragraph p, HtmlNode node)
    {
        var added = false;

        foreach (var child in node.ChildNodes)
        {
            var i = GenerateBlockForNode(child);
            if (i != null)
            {
                p.Inlines.Add(i);
                added = true;
            }
        }
        if (!added)
        {
            //p.Inlines.Add(new Run { Text = node.InnerText });
            if (node.InnerText.Trim() != "")
                p.Inlines.Add(new Run { Text = CleanText(node.InnerText) });
        }
    }

    private static void AddChildren(Span s, HtmlNode node)
    {
        var added = false;

        foreach (var child in node.ChildNodes)
        {
            var i = GenerateBlockForNode(child);
            if (i != null)
            {
                s.Inlines.Add(i);
                added = true;
            }
        }
        if (!added)
        {
            //s.Inlines.Add(new Run { Text = node.InnerText });
            if (node.InnerText.Trim() != "")
                s.Inlines.Add(new Run { Text = CleanText(node.InnerText) });
        }
    }

    private static Inline GenerateBlockForNode(HtmlNode node)
    {
        try
        {
            switch (node.Name.ToLower())
            {
                case "div":
                    return GenerateSpan(node);
                case "p":
                    return GenerateInnerParagraph(node);

                case "img":
                    return GenerateImage(node);
                case "a":
                    /*
                    return node.ChildNodes.Count >= 1 && (node.FirstChild.Name == "img")
                        ? GenerateImage(node.FirstChild)
                        : GenerateHyperLink(node);
                    */
                    return GenerateHyperLink(node);
                case "li":
                    return GenerateLi(node);
                case "b":
                case "strong":
                    return GenerateBold(node);
                case "i":
                case "em":
                    return GenerateItalic(node);
                case "u":
                    return GenerateUnderline(node);
                case "br":
                    return new LineBreak();
                case "span":
                    return GenerateSpan(node);
                //case "iframe":
                //case "Iframe":
                //return GenerateIFrame(node);
                case "#text":
                    if (!string.IsNullOrWhiteSpace(node.InnerText))
                        //return new Run { Text = CleanText(node.InnerText) }; // CleanText is removing white spaces in this case
                        return new Run { Text = node.InnerText };
                    break;
                case "h1":
                    return GenerateH1(node);
                case "h2":
                    return GenerateH2(node);
                case "h3":
                    return GenerateH3(node);
                case "ul":
                    return GenerateUl(node);

                default:
                    return GenerateSpanWNewLine(node);
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine("GenerateBlockForNode: " + e);
        }

        return null;
    }

    private static Inline GenerateLi(HtmlNode node)
    {
        var span = new Span();
        var inlineUiContainer = new InlineUIContainer();
        var ellipse = new Ellipse
        {
            Fill = _currentObject.Foreground ?? new SolidColorBrush(Colors.Black),
            Width = 6,
            Height = 6,
            Margin = new Thickness(-30, 0, 0, 1)
        };
        inlineUiContainer.Child = ellipse;
        span.Inlines.Add(inlineUiContainer);
        AddChildren(span, node);
        span.Inlines.Add(new LineBreak());
        return span;
    }

    private static Inline GenerateImage(HtmlNode node)
    {
        var span = new Span();
        try
        {
            var inlineUiContainer = new InlineUIContainer();
            var sourceUri = WebUtility.HtmlDecode(node.Attributes["src"].Value);
            var sourceWidth = WebUtility.HtmlDecode(node.Attributes["width"]?.Value);
            var sourceHeight = WebUtility.HtmlDecode(node.Attributes["height"]?.Value);

            var image = new Image
            {
                Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
            };

            if (sourceWidth != null || sourceHeight != null)
            {
                if (sourceWidth != null)
                {
                    image.MaxWidth = double.Parse(sourceWidth);
                    //image.Width = double.Parse(sourceWidth);
                }

                if (sourceHeight != null)
                {
                    image.MaxHeight = double.Parse(sourceHeight);
                    //image.Height = double.Parse(sourceHeight);
                }
            }
            else
            {
                image.ImageOpened += ImageOpened;
            }

            image.ImageFailed += ImageFailed;
            image.Tapped += ImageOnTapped;

            image.Source = new BitmapImage(new Uri(sourceUri, UriKind.Absolute));

            inlineUiContainer.Child = image;

            span.Inlines.Add(inlineUiContainer);
            span.Inlines.Add(new LineBreak());
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
        return span;
    }

    private static void ImageOnTapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
        => OnImageTapped?.Invoke(sender, tappedRoutedEventArgs);

    private static void ImageFailed(object sender, ExceptionRoutedEventArgs e)
        => Debug.WriteLine("image failed to load");

    private static void ImageOpened(object sender, RoutedEventArgs e)
    {
        var img = sender as Image;
        if (img == null) return;

        var bimg = img.Source as BitmapImage;
        if (bimg != null && (bimg.PixelWidth > ImageMaxPixelWidth || bimg.PixelHeight > ImageMaxPixelHeight))
        {
            img.Width = ImageMaxPixelWidth;
            img.Height = ImageMaxPixelHeight;

            if (bimg.PixelWidth > ImageMaxPixelWidth)
            {
                img.Width = ImageMaxPixelWidth;
                img.Height = (ImageMaxPixelWidth / bimg.PixelWidth) * bimg.PixelHeight;
            }
            if (img.Height > ImageMaxPixelHeight)
            {
                img.Height = ImageMaxPixelHeight;
                img.Width = (ImageMaxPixelHeight / img.Height) * img.Width;
            }
        }
        else
        {
            if (bimg == null) return;

            img.Height = bimg.PixelHeight;
            img.Width = bimg.PixelWidth;
        }
    }

    private static Inline? GenerateHyperLink(HtmlNode node)
    {
        if (node.Attributes == null)
        {
            Debug.WriteLine("GenerateHyperLink node.Attributes is null.");
            return null;
            //return new LineBreak();
        }
        if (node.Attributes["href"] == null)
        {
            Debug.WriteLine("GenerateHyperLink node.Attributes[\"href\"] is null.");
            return null;
            //return new LineBreak();
        }
        //

        try
        {
            var hyperlinkButton = new Hyperlink();
            hyperlinkButton.NavigateUri = new Uri(node.Attributes["href"].Value, UriKind.Absolute);
            hyperlinkButton.Inlines.Add(new Run { Text = node.InnerText });
            var span = new Span();
            span.Inlines.Add(hyperlinkButton);
            return span;
/*
            var span = new Span();
            var inlineUiContainer = new InlineUIContainer();
            var hyperlinkButton = new Hyperlink//new HyperlinkButton
            {
                NavigateUri = new Uri(node.Attributes["href"].Value, UriKind.Absolute),
                //Content = CleanText(node.InnerText)
            };
            */
            /*
            if (node.ParentNode != null && (node.ParentNode.Name == "li" || node.ParentNode.Name == "LI"))
                hyperlinkButton.Style = (Style)Application.Current.Resources["RTLinkLI"];
            else if ((node.NextSibling == null
                || string.IsNullOrWhiteSpace(node.NextSibling.InnerText)) && (node.PreviousSibling == null
                || string.IsNullOrWhiteSpace(node.PreviousSibling.InnerText)))
                hyperlinkButton.Style = (Style)Application.Current.Resources["RTLinkOnly"];
            else
                hyperlinkButton.Style = (Style)Application.Current.Resources["RTLink"];
            */
            /*
            inlineUiContainer.Child = hyperlinkButton;
            span.Inlines.Add(inlineUiContainer);
            return span;
            */
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GenerateHyperLink: " + ex.Message);
            return null;
            //return new LineBreak();
        }

    }

    /*
    private static Inline GenerateIFrame(HtmlNode node)
    {
        try
        {
            var span = new Span();
            span.Inlines.Add(new LineBreak());
            var inlineUiContainer = new InlineUIContainer();
            var webView = new WebView
            {
                Source = new Uri(node.Attributes["src"].Value, UriKind.Absolute),
                Width = int.Parse(node.Attributes["width"].Value),
                Height = int.Parse(node.Attributes["height"].Value)
            };
            inlineUiContainer.Child = webView;
            span.Inlines.Add(inlineUiContainer);
            span.Inlines.Add(new LineBreak());
            return span;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return null;
        }
    }

    private static Block GenerateTopIFrame(HtmlNode node)
    {
        try
        {
            var paragraph = new Paragraph();
            var inlineUiContainer = new InlineUIContainer();
            var webView = new WebView
            {
                Source = new Uri(node.Attributes["src"].Value, UriKind.Absolute),
                Width = int.Parse(node.Attributes["width"].Value),
                Height = int.Parse(node.Attributes["height"].Value)
            };
            inlineUiContainer.Child = webView;
            paragraph.Inlines.Add(inlineUiContainer);
            return paragraph;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return null;
        }
    }
    */

    private static Inline GenerateBold(HtmlNode node)
    {
        var bold = new Bold();
        AddChildren(bold, node);
        return bold;
    }

    private static Inline GenerateUnderline(HtmlNode node)
    {
        var underline = new Underline();
        AddChildren(underline, node);
        return underline;
    }

    private static Inline GenerateItalic(HtmlNode node)
    {
        var italic = new Italic();
        AddChildren(italic, node);
        return italic;
    }

    private static Block GenerateParagraph(HtmlNode node)
    {
        var paragraph = new Paragraph();
        AddChildren(paragraph, node);
        return paragraph;
    }

    private static Inline GenerateUl(HtmlNode node)
    {
        var span = new Span();
        span.Inlines.Add(new LineBreak());
        AddChildren(span, node);
        return span;
    }

    private static Inline GenerateInnerParagraph(HtmlNode node)
    {
        var span = new Span();
        span.Inlines.Add(new LineBreak());
        AddChildren(span, node);
        span.Inlines.Add(new LineBreak());
        return span;
    }

    private static Inline GenerateSpan(HtmlNode node)
    {
        var span = new Span();
        AddChildren(span, node);
        return span;
    }

    private static Inline GenerateSpanWNewLine(HtmlNode node)
    {
        var span = new Span();
        AddChildren(span, node);
        if (span.Inlines.Count > 0)
            span.Inlines.Add(new LineBreak());
        return span;
    }

    private static Span GenerateH3(HtmlNode node)
    {
        var span = H3SpanFactory?.Invoke() ?? new Span();
        span.Inlines.Add(new LineBreak());
        var bold = new Bold();
        var run = new Run { Text = CleanText(node.InnerText) };
        bold.Inlines.Add(run);
        span.Inlines.Add(bold);
        span.Inlines.Add(new LineBreak());
        return span;
    }

    private static Inline GenerateH2(HtmlNode node)
    {
        var span = H2SpanFactory?.Invoke() ?? new Span { FontSize = 24 };
        span.Inlines.Add(new LineBreak());
        var run = new Run { Text = CleanText(node.InnerText) };
        span.Inlines.Add(run);
        span.Inlines.Add(new LineBreak());
        return span;
    }

    private static Inline GenerateH1(HtmlNode node)
    {
        var span = H1SpanFactory?.Invoke() ?? new Span { FontSize = 30 };
        span.Inlines.Add(new LineBreak());
        var run = new Run { Text = CleanText(node.InnerText) };
        span.Inlines.Add(run);
        span.Inlines.Add(new LineBreak());
        return span;
    }
}
