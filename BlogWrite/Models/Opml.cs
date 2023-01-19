using System.Xml;

namespace BlogWrite.Models;

public class Opml
{
    // Import OPML
    public NodeFolder LoadOpml(XmlDocument xdoc)
    {
        var opmlTitle = "OPML imported feeds";

        // Dummy Root Folder
        var dummyFolder = new NodeFolder(opmlTitle)
        {
            IsSelected = true,
            IsExpanded = true,
            Parent = null // for now
        };

        if (xdoc is null)
            return dummyFolder;

        var opmlTitleNode = xdoc.SelectSingleNode("//opml/head/title");
        if (opmlTitleNode is not null)
        {
            dummyFolder.Name = opmlTitleNode.InnerText;
        }

        var bodyNode = xdoc.SelectSingleNode("//opml/body");

        if (bodyNode is null)
            return dummyFolder;

        if (bodyNode.ChildNodes.Count > 0)
        {
            ProcessOutlineChild(bodyNode.ChildNodes, dummyFolder);
        }

        return dummyFolder;
    }

    private void ProcessOutlineChild(XmlNodeList childs, NodeFolder parent)
    {
        foreach (XmlNode outline in childs)
        {
            if (!outline.LocalName.Equals("outline"))
                continue;

            var title = "empty";
            if (outline.Attributes["text"] is not null)
            {
                title = outline.Attributes["text"].InnerText;
            }

            var xmlUrl = "";
            if (outline.Attributes["xmlUrl"] is not null)
            {
                xmlUrl = outline.Attributes["xmlUrl"].Value;

                if (!string.IsNullOrEmpty(xmlUrl))
                {
                    try
                    {
                        Uri xu = new Uri(xmlUrl);

                        NodeFeed feed = new(title, xu);

                        if (outline.Attributes["htmlUrl"] is not null)
                        {
                            var htmlUrl = "";
                            htmlUrl = outline.Attributes["htmlUrl"].Value;

                            var hu = new Uri(htmlUrl);
                            feed.SiteUri = hu;
                        }

                        feed.Parent = parent;

                        parent.Children.Add(feed);

                        continue;
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            if (outline.ChildNodes.Count > 0)
            {
                NodeFolder folder = new NodeFolder(title);
                folder.IsSelected = true;
                folder.IsExpanded = true;

                folder.Parent = parent;

                parent.Children.Add(folder);

                ProcessOutlineChild(outline.ChildNodes, folder);
            }
        }
    }

    // Export OPML
    public XmlDocument WriteOpml(NodeTree serviceRootNode)
    {
        XmlDocument xdoc = new();
        XmlDeclaration xdec = xdoc.CreateXmlDeclaration("1.0", "UTF-8", null);
        xdoc.AppendChild(xdec);

        XmlElement eleRoot = xdoc.CreateElement(string.Empty, "opml", string.Empty);
        xdoc.AppendChild(eleRoot);

        XmlAttribute verAttr = xdoc.CreateAttribute("version");
        verAttr.Value = "1.0";
        eleRoot.SetAttributeNode(verAttr);

        XmlElement eleHead = xdoc.CreateElement(string.Empty, "head", string.Empty);
        XmlElement eleTitle = xdoc.CreateElement(string.Empty, "title", string.Empty);
        eleTitle.InnerText = "Exported from BlogWrite at " + DateTime.Now.ToString();
        eleHead.AppendChild(eleTitle);
        eleRoot.AppendChild(eleHead);

        XmlElement eleBody = xdoc.CreateElement(string.Empty, "body", string.Empty);
        eleRoot.AppendChild(eleBody);

        foreach (NodeTree nt in serviceRootNode.Children)
        {
            if ((nt is NodeFeed) || (nt is NodeFolder))
            {
                ProcessNodeChild(nt, xdoc, eleBody);
            }
        }

        return xdoc;
    }

    private void ProcessNodeChild(NodeTree nt, XmlDocument xdoc, XmlElement parent)
    {
        if (nt is NodeFeed feed)
        {
            XmlElement eleOutline = xdoc.CreateElement(string.Empty, "outline", string.Empty);
            parent.AppendChild(eleOutline);

            XmlAttribute typeAttr = xdoc.CreateAttribute("type");
            typeAttr.Value = "rss";
            eleOutline.SetAttributeNode(typeAttr);

            XmlAttribute textAttr = xdoc.CreateAttribute("text");
            textAttr.Value = nt.Name;
            eleOutline.SetAttributeNode(textAttr);

            XmlAttribute titleAttr = xdoc.CreateAttribute("title");
            titleAttr.Value = nt.Name;
            eleOutline.SetAttributeNode(titleAttr);

            XmlAttribute xmlUriAttr = xdoc.CreateAttribute("xmlUrl");
            xmlUriAttr.Value = feed.EndPoint.AbsoluteUri;
            eleOutline.SetAttributeNode(xmlUriAttr);

            if (feed.SiteUri is not null)
            {
                XmlAttribute htmlUriAttr = xdoc.CreateAttribute("htmlUrl");
                htmlUriAttr.Value = feed.SiteUri.AbsoluteUri;
                eleOutline.SetAttributeNode(htmlUriAttr);
            }
        }
        else if (nt is NodeFolder folder)
        {
            XmlElement eleOutline = xdoc.CreateElement(string.Empty, "outline", string.Empty);
            parent.AppendChild(eleOutline);

            XmlAttribute textAttr = xdoc.CreateAttribute("text");
            textAttr.Value = nt.Name;
            eleOutline.SetAttributeNode(textAttr);

            XmlAttribute titleAttr = xdoc.CreateAttribute("title");
            titleAttr.Value = nt.Name;
            eleOutline.SetAttributeNode(titleAttr);

            if (folder.Children.Count > 0)
            {
                foreach (NodeTree ntc in nt.Children)
                {
                    ProcessNodeChild(ntc, xdoc, eleOutline);
                }
            }
        }
    }
}
