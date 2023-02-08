## FeedDesk

Desktop feed reader.

## Implements following standards  

* [The Atom Syndication Format](https://tools.ietf.org/html/rfc4287) (Atom 1.0) and Atom 0.3
* [RDF Site Summary](https://www.w3.org/2001/09/rdfprimer/rss.html) (RSS 1.0) and [Really Simple Syndication](https://validator.w3.org/feed/docs/rss2.html) (RSS 2.0)


## Change log

* v1.0.0.8 (2023/2/3)
 Podcast support.
* v1.0.0.7 (2023/1/21)
 Some code refactoring.
* v1.0.0.5 (2023/1/20)
 Some updates.
* v1.0.0.4 (2023/1/18)
 Some UI update.
* v1.0.0.3 (2023/1/11)
 Finishing up feed updates.
* v1.0.0.2 (2023/1/8) 
 Updated WinUIEx and now uses local persistence file. Now, the MainWindow remembers its size and pos. (previous ver did not support unpackaged app)
* v1.0.0.1 (2023/1/8) 
 Very basic feed reader functionality. 
* v1.0.0.0 (2023/1/1) 
 Moved to WinUI3 from a WPF project. 

## Things that currentry working on

### Feed reader

- [ ] MainMenu shortcuts key binding.
- [ ] Finish Settings page.
- [ ] Theme setting save and restore.
- [ ] Internationalization

## Planning

### Feed reader
- [ ] Database file management view (menu>tools)
- [ ] 元記事の内容が更新された場合、それを反映させたい。
- [ ] エントリを選択したら、DBのisVisited(isRead)フラグを更新する。
- [ ] "Read lator" or saved or boolmark or whatever.
- [ ] Grouping display of entries. 
- [ ] multiple viewing style (3 panes, cards, etc)
- [ ] Auto reflesh.
- [ ] A progressbar or some kind to notify refleshing is on going or not.


### Sync 
- [ ] Sync with Firefox Pocket or Instapaper etc for "read lator".
- [ ] Sync with online feed readers such as Miniflux or creat one.


### Other

- [ ] Settings
- [ ] Themes



