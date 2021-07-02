# BlogWrite#

### - Under developement -
  
A blog editing (web publishing) client with a full-featured feed reading functionality - C#/WPF rewrite of "BlogWrite" originaly developed in Delphi.


### Implements following formats, protocols & APIs:  

* Atom 1.0 ([The Atom Syndication Format](https://tools.ietf.org/html/rfc4287)) and 0.3

* RSS 1.0 ([RDF Site Summary](https://www.w3.org/2001/09/rdfprimer/rss.html)) and 2.0 ([Really Simple Syndication](https://validator.w3.org/feed/docs/rss2.html))

* [Atom Publishing Protocol](https://tools.ietf.org/html/rfc5023)

* [XML-RPC API](https://codex.wordpress.org/XML-RPC_Support)
([Blogger API](https://codex.wordpress.org/XML-RPC_Blogger_API),
[MetaWeblog API](https://codex.wordpress.org/XML-RPC_MetaWeblog_API),
[MovableType API](https://codex.wordpress.org/XML-RPC_MovableType_API),
[WordPress API](https://codex.wordpress.org/XML-RPC_WordPress_API))

### Roadmap

or progress so far...

#### General

- [ ] Internationalization
- [ ] Settings
- [ ] Themes

#### Feed Reader

The Feed Reading functionality.

- [x] Feed Auto Discovery (Parse HTML Web pages and find RSS and Atom links)
- [x] Parse feeds (Atom 1.0, 0.3, RSS 2.0, 1.0)
- [x] Display and manage feeds and entries. 
- [x] Display contents in various views (Cards, Magazine, ThereePane)
- [x] View webpages in a embeded browser. 
- [x] Manage entries read/unread status with SQLite database.
- [x] Auto update feeds and entries.
- [x] Import feed list from OPML and export feed list as OPML. 
- [x] Download and display "eye catching" images. 
- [ ] Options (text configuration, etc)

*Additionaly, Search, Star/Pin/ReadItLator, Favicon, Wide view (three vertical columns) and more.

#### CRUD API and protocol implementations

The Editing functionality.

- [x] Service Auto Discovery (Atom Service Document, Really Simple Discovery)
- [x] Display and manage service/blog infomation (Atom Publishing protocol, WordPress, Movable Type, etc)
- [ ] Display and manage list of entries for editing with SQLite database.

##### Editor

The WYSIWYG editor.

- [x] Basic editor window
- [ ] Post, Update, Delete entries
- [ ] Manage categories and tags
- [ ] WYSIWYG editing
- [ ] Markdown support
- [ ] Manage images and files

### Screenshots  

Cards View
![BlogWrite_CardsView](https://github.com/torum/BlogWrite/blob/master/docs/images/BlogWrite_Cards.png?raw=true) 

Magazine View
![BlogWrite_MagazineView](https://github.com/torum/BlogWrite/blob/master/docs/images/BlogWrite_Magazine.png?raw=true) 

ThreePane View
![BlogWrite_ThreePane](https://github.com/torum/BlogWrite/blob/master/docs/images/BlogWrite_ThreePane.png?raw=true) 

Embedded Browser
![BlogWrite_Embedded_Browser](https://github.com/torum/BlogWrite/blob/master/docs/images/BlogWrite_Embedded.png?raw=true) 

Debug Window
![BlogWrite_DebugWindow](https://github.com/torum/BlogWrite/blob/master/docs/images/DebugWindow.png?raw=true) 


