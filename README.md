# BlogWrite

### Under developement
  
A full featured feed reader and a WYSIWYG blog editing (web publishing) client.  - a C#/WPF port of "BlogWrite" originaly developed with Delphi.

![BlogWrite4](https://github.com/torum/BlogWrite/blob/master/docs/images/BlogWrite4.png?raw=true) 

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

The Feed Reader functionality.

- [x] ~~Feed Auto Discovery (Parse a HTML Web page and find RSS and Atom links)~~
- [x] ~~Download feeds and parse and display in various view types.~~ 
- [x] ~~Display contents in a embeded browser.~~ 
- [x] ~~Save entries in a sqlite database.~~ 
- [x] ~~Manage entries read/unread status.~~ 
- [x] ~~Import and export feeds list as OPML.~~ 
- [ ] ~~Download optional image~~ and display. 

#### Editor

WYSIWYG editor.

- [x] ~~Basic editor window~~
- [ ] Post, Update, Delete
- [ ] WYSIWYG editing.
- [ ] ...


#### API, protocol implementation

The editing functionality.

- [x] ~~Service Auto Discovery (Parse a xml document)~~
- [x] ~~Download and parse and display service or blog infomation~~
- [ ] ...


