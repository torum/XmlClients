# BlogWrite

## Under developement
  
A blog editing (web publishing) client with a feed reading functionality - C# port of "BlogWrite" originaly developed in Delphi.


## Implements following standards  

* [The Atom Syndication Format](https://tools.ietf.org/html/rfc4287) (Atom 1.0) and Atom 0.3
* [RDF Site Summary](https://www.w3.org/2001/09/rdfprimer/rss.html) (RSS 1.0) and [Really Simple Syndication](https://validator.w3.org/feed/docs/rss2.html) (RSS 2.0)
* [Atom Publishing Protocol](https://tools.ietf.org/html/rfc5023)
* [XML-RPC API](https://codex.wordpress.org/XML-RPC_Support)
([Blogger API](https://codex.wordpress.org/XML-RPC_Blogger_API),
[MetaWeblog API](https://codex.wordpress.org/XML-RPC_MetaWeblog_API),
[MovableType API](https://codex.wordpress.org/XML-RPC_MovableType_API), and
[WordPress API](https://codex.wordpress.org/XML-RPC_WordPress_API))


## Done
### Things that are implimented
- [x] Feed Auto Discovery. (gets & parses HTML pages and find RSS and Atom links)
- [x] Parse feeds. (Atom 1.0, 0.3, RSS 2.0, 1.0)
- [x] Manage feeds list.
- [x] Display feeds and entries. (treeview and listview)
- [x] Display contents as text.
- [x] Save entries in a  SQLite database.
- [x] Display contents in an embeded browser. 


## Change log

* v1.0.0.5 (2023/1/20)
 Some updates.
* v1.0.0.4 (2023/1/18)
 UI update
* v1.0.0.3 (2023/1/11)
 Finishing up feed updates.
* v1.0.0.2 (2023/1/8) 
 Updated WinUIEx and now uses local persistence file. Now, the MainWindow remembers its size and pos. (previous ver did not support unpackaged app)
* v1.0.0.1 (2023/1/8) 
 Very basic feed reader functionality. 
* v1.0.0.0 (2023/1/1) 
 Moved to WinUI3 from a WPF project. 

## TODO

### Things that currentry working on

#### Feed reader

- [ ] Folderでまとめて読む際にどのFeedからのか分かりやすいようにPublisherを表示したい。要DBテーブル。
- [ ] Refactor Add Feed page.
- [ ] GoogleNewsのfeedでentry id を確認したほうがよい。
- [ ] NewEntryCountをもっと効率よく（上下に伝播させるのではなく）。
- [ ] Manage entries read/unread status with SQLite database. Newステータス(SQLでロードと同時に既読にする)
- [ ] Dupecheckとかの結果や起動時データベース接続エラーを画面内で通知する。
- [ ] update 中にドラッグドロップできないようにする。
- [ ] Check "category" and "publisher" etc.
- [ ] 元記事の内容が更新された場合、それを反映させたい。
- [ ] DB管理（容量制限というか自動削除機能）
- [x] Add Folder page.
- [x] OPML import/export
- [x] Delete entries from database when removing feed and folder items.
- [x] Edit Feed and Folder property (currently limited to "Name"). 
- [x] Show "Inbox (unarchived)" and "All (include archived)" entries.


### Planning

#### Feed reader
- [ ] Grouping display of feed for Folder view. 
- [ ] Download and display "eye catching" images. 
- [ ] Auto update feed entries safely. 
- [ ] Auto update progressbar or some kind to notify it is on going or not.


#### API
- [ ] Service Auto Discovery (Atom Service Document, Really Simple Discovery)
- [ ] Display and manage service/blog infomation (Atom Publishing protocol, WordPress, Movable Type, etc)
- [ ] Display and manage list of entries for editing with SQLite database.


##### Editor
- [ ] Basic editor window
- [ ] Post, Update, Delete entries
- [ ] Manage categories and tags
- [ ] WYSIWYG editing
- [ ] Markdown support
- [ ] Manage images and files


##### Other
- [ ] Internationalization
- [ ] Settings
- [ ] Themes



