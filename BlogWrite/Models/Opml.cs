using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Xml;
using System.Diagnostics;

namespace BlogWrite.Models;

public class Opml
{
    // Import OPML
    public NodeFolder LoadOpml(XmlDocument xdoc)
    {
        string opmlTitle = "OPML imported feeds";

        // Dummy Root Folder
        NodeFolder dummyFolder = new NodeFolder(opmlTitle);
        dummyFolder.IsSelected = true;
        dummyFolder.IsExpanded = true;
        dummyFolder.Parent = null; // for now

        if (xdoc is null)
            return dummyFolder;

        XmlNode opmlTitleNode = xdoc.SelectSingleNode("//opml/head/title");
        if (opmlTitleNode is not null)
        {
            dummyFolder.Name = opmlTitleNode.InnerText;
        }

        XmlNode bodyNode = xdoc.SelectSingleNode("//opml/body");

        if (bodyNode is null)
            return dummyFolder;

        if (bodyNode.ChildNodes.Count > 0)
        {
            foreach (XmlNode outline in bodyNode.ChildNodes)
            {
                if (!outline.LocalName.Equals("outline"))
                    continue;

                string title = "empty";
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

                                Uri hu = new Uri(htmlUrl);
                                feed.SiteUri = hu;
                            }

                            feed.Parent = dummyFolder;

                            dummyFolder.Children.Add(feed);

                            //Debug.WriteLine(feed.Name + " feed added");

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

                    folder.Parent = dummyFolder;

                    dummyFolder.Children.Add(folder);

                    //Debug.WriteLine(folder.Name + " folder added");

                    ProcessOutlineChild(outline.ChildNodes, folder);
                }
            }
        }

        return dummyFolder;
    }

    private void ProcessOutlineChild(XmlNodeList childs, NodeFolder parent)
    {
        foreach (XmlNode outline in childs)
        {
            if (!outline.LocalName.Equals("outline"))
                continue;

            string title = "empty";
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

                            Uri hu = new Uri(htmlUrl);
                            feed.SiteUri = hu;
                        }

                        feed.Parent = parent;

                        parent.Children.Add(feed);

                        //Debug.WriteLine(feed.Name + " feed added");

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
                ProcessOutlineChild(outline.ChildNodes, parent);
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
        eleTitle.InnerText = "Exported at " + DateTime.Now.ToString();
        eleHead.AppendChild(eleTitle);
        eleRoot.AppendChild(eleHead);

        XmlElement eleBody = xdoc.CreateElement(string.Empty, "body", string.Empty);
        eleRoot.AppendChild(eleBody);

        foreach (NodeTree nt in serviceRootNode.Children)
        {
            if (nt is NodeFeed)
            {
                XmlElement eleOutline= xdoc.CreateElement(string.Empty, "outline", string.Empty);
                eleBody.AppendChild(eleOutline);

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
                xmlUriAttr.Value = (nt as NodeFeed).EndPoint.AbsoluteUri;
                eleOutline.SetAttributeNode(xmlUriAttr);

                if ((nt as NodeFeed).SiteUri is not null)
                {
                    XmlAttribute htmlUriAttr = xdoc.CreateAttribute("htmlUrl");
                    htmlUriAttr.Value = (nt as NodeFeed).SiteUri.AbsoluteUri;
                    eleOutline.SetAttributeNode(htmlUriAttr);
                }

                continue;
            }

            if (nt is NodeFolder)
            {
                XmlElement eleOutline = xdoc.CreateElement(string.Empty, "outline", string.Empty);
                eleBody.AppendChild(eleOutline);

                XmlAttribute textAttr = xdoc.CreateAttribute("text");
                textAttr.Value = nt.Name;
                eleOutline.SetAttributeNode(textAttr);

                XmlAttribute titleAttr = xdoc.CreateAttribute("title");
                titleAttr.Value = nt.Name;
                eleOutline.SetAttributeNode(titleAttr);

                ProcessNodeFolderChild((nt as NodeFolder), xdoc, eleOutline);
            }
        }

        return xdoc;
    }

    private void ProcessNodeFolderChild(NodeFolder nf, XmlDocument xdoc ,XmlElement parent)
    {
        foreach (NodeTree nt in nf.Children)
        {
            if (nt is NodeFeed)
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
                xmlUriAttr.Value = (nt as NodeFeed).EndPoint.AbsoluteUri;
                eleOutline.SetAttributeNode(xmlUriAttr);

                if ((nt as NodeFeed).SiteUri is not null)
                {
                    XmlAttribute htmlUriAttr = xdoc.CreateAttribute("htmlUrl");
                    htmlUriAttr.Value = (nt as NodeFeed).SiteUri.AbsoluteUri;
                    eleOutline.SetAttributeNode(htmlUriAttr);
                }
            }
        }
    }
}
