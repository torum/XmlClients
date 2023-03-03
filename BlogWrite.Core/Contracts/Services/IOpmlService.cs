using System.Xml;
using BlogWrite.Core.Models;

namespace BlogWrite.Core.Contracts.Services;

public interface IOpmlService
{
    NodeFolder LoadOpml(XmlDocument xdoc);

    XmlDocument WriteOpml(NodeTree serviceRootNode);
}