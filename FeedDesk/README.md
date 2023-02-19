
## [FeedDesk](https://torum.github.io/BlogWrite/FeedDesk/)
A desktop feed reader. (work in progress)

### Implements following formats:  

* Atom 0.3
* [The Atom Syndication Format](https://tools.ietf.org/html/rfc4287) (Atom 1.0)
* [RDF Site Summary](https://www.w3.org/2001/09/rdfprimer/rss.html) (RSS 1.0)
* [Really Simple Syndication](https://validator.w3.org/feed/docs/rss2.html) (RSS 2.0)

### Other feature
* Feed Autodiscovery.
* OPML import, export.
* Display enclosed or embeded images.
* In-app playback of Podcast audio.

## Change log
* v1.0.5 (2023/2/19)
 Store App release. 
 Removed Arm version from packages since System.Data.SQLite does not support Arm.
* v1.0.4 (2023/2/18)
 Store App release.
* v1.0.3.123 (2023/2/17)
 Added NewVisited status. Added Atom:Media:Thumbnail@url. Improved readability of the entry listview.
* v1.0.3.1 (2023/2/16)
 Fixed DateTime issue regarding RDF dc:date.
* v1.0.3 (2023/2/16)
 Store App release. i18n. Added Japanese translation.
* v1.0.2 (2023/2/15)
 Store App release. Fixed some issue in AddFeed Page.
* v1.0.1 (2023/2/14)
 Store App release.
* v1.0.0.11 (2023/2/15)
 Convert relative path to abusolute uri.
 Add int value check after drag and drop.
 Update AddFeed view.
* v1.0.0.10 (2023/2/10)
 Initial release. "minimum viable product (mvp)"
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
 Updated WinUIEx and now uses local persistence file.
* v1.0.0.1 (2023/1/8) 
 Very basic feed reader functionality. 
* v1.0.0.0 (2023/1/1) 
 Switched to WinUI3 from a WPF project. 

## Things that currentry working on
- [ ] Database file management page (menu>tools), clean(delete all data) and optimize(vacume).
- [ ] Search.
- [ ] HTML content rendering without webview2

## Planning

### Core
- [ ] "Read lator" or saved or boolmark or star or favorites or whatever.
- [ ] Update entry data when entry is updated.
- [ ] Basic Auth.
- [ ] Auto reflesh.

### UI
- [ ] FavIcon. 
- [ ] Grouping display of entries. 
- [ ] multiple viewing style (3 panes, cards, etc)

### Sync 
- [ ] Sync with Firefox Pocket or Instapaper etc for "read lator".
- [ ] Sync with online feed readers such as Miniflux or creat one.

### Other
- [ ] More options.
- [ ] Refactoring...

## Known issues
* Two fingered touchpad gestures for scrolling. (WinUI3 issue)



