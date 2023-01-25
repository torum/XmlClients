using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlogWrite.Core.Models;
using Windows.Storage;
using Microsoft.UI.Xaml.Media.Imaging;

namespace BlogWrite.Core.Contracts.Services;

public interface IDataAccessService
{
    SqliteDataAccessResultWrapper InitializeDatabase(string dataBaseFilePath);

    SqliteDataAccessResultWrapper InsertFeed(string feedId, Uri feedUri, string feedName, string feedTitle, string feedDescription, DateTime updated, Uri htmlUri);

    SqliteDataAccessResultWrapper UpdateFeed(string feedId, Uri feedUri, string feedName, string feedTitle, string feedDescription, DateTime updated, Uri htmlUri);

    SqliteDataAccessResultWrapper DeleteFeed(string feedId);

    SqliteDataAccessSelectResultWrapper SelectEntriesByFeedId(string feedId, bool IsUnarchivedOnly = true);
    
    SqliteDataAccessSelectResultWrapper SelectEntriesByFeedIds(List<string> feedIds, bool IsUnarchivedOnly = true);


    //SqliteDataAccessSelectImageResultWrapper SelectImageByImageId(string imageId);

    SqliteDataAccessInsertResultWrapper InsertEntries(List<EntryItem> entries, string feedId, string feedName, string feedTitle, string feedDescription, DateTime updated, Uri htmlUri);


    //SqliteDataAccessInsertResultWrapper InsertImages(List<EntryItem> entries);


    //SqliteDataAccessResultWrapper UpdateEntriesAsRead(List<EntryItem> entries);


    SqliteDataAccessResultWrapper UpdateAllEntriesAsArchived(List<string> feedIds);


    //SqliteDataAccessResultWrapper UpdateEntryStatus(EntryItem entry);

    SqliteDataAccessResultWrapper DeleteEntriesByFeedIds(List<string> feedIds);

}



