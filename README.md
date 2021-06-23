# BlogWrite

### - Under developement -
  
A blog editing (web publishing) client with a full-featured feed reading functionality - C#/WPF port of "BlogWrite" originaly developed with Delphi.

Cards View
![BlogWrite_CardsView](https://github.com/torum/BlogWrite/blob/master/docs/images/BlogWrite_Cards.png?raw=true) 

Embedded Browser
![BlogWrite_Embedded_Browser](https://github.com/torum/BlogWrite/blob/master/docs/images/BlogWrite_Embedded.png?raw=true) 

Magazine View
![BlogWrite_MagazineView](https://github.com/torum/BlogWrite/blob/master/docs/images/BlogWrite_Magazine.png?raw=true) 

ThreePane View
![BlogWrite_ThreePane](https://github.com/torum/BlogWrite/blob/master/docs/images/BlogWrite_ThreePane.png?raw=true) 

Debug Window
![BlogWrite_DebugWindow](https://github.com/torum/BlogWrite/blob/master/docs/images/DebugWindow.png?raw=true) 



### Implements following Feed formats, REST protocols & API specifications:  

The Atom Syndication Format (Atom 1.0)  
https://tools.ietf.org/html/rfc4287

RSS 1.0 RDF Site Summary  
https://www.w3.org/2001/09/rdfprimer/rss.html

RSS 2.0 Really Simple Syndication  
https://validator.w3.org/feed/docs/rss2.html

Atom Publishing Protocol  
https://tools.ietf.org/html/rfc5023

XML-RPC API (MetaWeblog API, Blogger_API, Movable Type API, Wordpress API)  
https://codex.wordpress.org/XML-RPC_Support  
https://codex.wordpress.org/XML-RPC_WordPress_API  
https://codex.wordpress.org/XML-RPC_MovableType_API  
https://codex.wordpress.org/XML-RPC_MetaWeblog_API  
https://codex.wordpress.org/XML-RPC_Blogger_API  


### Road map

or progress so far...

#### Feed Reader

The Feed Reading functionality.

- [x] Feed Auto Discovery (Parse a HTML Web page and find RSS and Atom links)
- [x] Parse feeds (Atom 1.0, 0.3, RSS 2.0, 1.0)
- [x] Display and manage feeds and entries. 
- [x] Display contents in various views and webpages in a embeded browser. 
- [x] Manage entries read/unread status with sqlite database.
- [x] Auto update entries.
- [x] Import feed list from OPML and export feed list as OPML. 
- [x] Get optional "eye catch" image Uris.
- [ ] Download and display optional images. 

#### CRUD API and protocol implementations

The Editing functionality.

- [x] Service Auto Discovery (Atom Publishing protocol, ~~RSD, XML-RPC~~)
- [ ] Display and manage service or blog infomation (~~Atom Publishing protocol,~~ WordPress, Movable Type, etc)
- [ ] Display and manage list of entries for editing.

##### Editor

The WYSIWYG editor.

- [x] Basic editor window
- [ ] Post, Update, Delete
- [ ] WYSIWYG editing.
- [ ] ...
- [ ] ...
- [ ] ...
- [ ] ...
- [ ] ...
- [ ] ...

