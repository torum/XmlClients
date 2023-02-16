# BlogWrite project
Work In Progress.  
This project was originally started as a C#/WPF port of a blog editing client called "BlogWrite" developed in Delphi language. Currently, there are two apps called "FeedDesk" and "BlogDesk" developed in C#/WinUI3 and both use the same core source code. 

## [FeedDesk](https://torum.github.io/BlogWrite/FeedDesk/)
A desktop feed reader. (work in progress)

### Download
from [Microsoft Store](https://www.microsoft.com/store/apps/9PGDGKFSV6L9).

### Features
#### Implements following formats:  
* Atom 0.3
* [The Atom Syndication Format](https://tools.ietf.org/html/rfc4287) (Atom 1.0)
* [RDF Site Summary](https://www.w3.org/2001/09/rdfprimer/rss.html) (RSS 1.0)
* [Really Simple Syndication](https://validator.w3.org/feed/docs/rss2.html) (RSS 2.0)

#### Supported extentions:
* media
* hatena
* dc
* iTunes

#### Other features:
* Feed Autodiscovery.
* OPML import, export.
* Display enclosed or embeded images.
* Podcasts audio and in-app playback.

### Screenshots:

![FeedDesk](https://github.com/torum/BlogWrite/blob/master/docs/images/FeedDesk-Screenshot-Dark.png?raw=true) 

![FeedDesk](https://github.com/torum/BlogWrite/blob/master/docs/images/FeedDesk-Screenshot-Light.png?raw=true) 

## [BlogDesk](https://torum.github.io/BlogWrite/BlogDesk/):
A desktop blogging client. (under development)

### Features
#### Implements following formats, protocols & APIs:  

* [The Atom Publishing Protocol](https://tools.ietf.org/html/rfc5023)
* [XML-RPC API](https://codex.wordpress.org/XML-RPC_Support)
([Blogger API](https://codex.wordpress.org/XML-RPC_Blogger_API),
[MetaWeblog API](https://codex.wordpress.org/XML-RPC_MetaWeblog_API),
[MovableType API](https://codex.wordpress.org/XML-RPC_MovableType_API), and
[WordPress API](https://codex.wordpress.org/XML-RPC_WordPress_API))


### Screenshots:

N/A


## Contributing
Feel free to open issues and send PRs. 

## Technologies & Frameworks
* [.NET 6](https://github.com/dotnet/runtime)  
* [WinUI3 (Windows App SDK)](https://github.com/microsoft/WindowsAppSDK) 
* [Community Toolkit](https://github.com/CommunityToolkit) 
* [WinUIEx](https://github.com/dotMorten/WinUIEx)
* [Json.NET](https://github.com/JamesNK/Newtonsoft.Json)
* [SQLite](https://github.com/sqlite/sqlite)

## Getting Started

### Requirements
* Windows 10.0.19041.0 or higher

### Building
1. Visual Studio 2022 with support for .NET Desktop App UI development (and optionally .NET Universal App development)
2. Clone this repository
3. Open solution in Visual Studio and run
