using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlogWrite.Models;
using Windows.Storage;
using Microsoft.UI.Xaml.Media.Imaging;

namespace BlogWrite.Contracts.Services;

public interface IDataAccessService
{
    SqliteDataAccessResultWrapper InitializeDatabase(string dataBaseFilePath);

    SqliteDataAccessSelectResultWrapper SelectEntriesByFeedId(string feedId, bool IsUnarchivedOnly = true);
    
    SqliteDataAccessSelectResultWrapper SelectEntriesByFeedIds(List<string> feedIds, bool IsUnarchivedOnly = true);


    //SqliteDataAccessSelectImageResultWrapper SelectImageByImageId(string imageId);

    SqliteDataAccessInsertResultWrapper InsertEntries(List<EntryItem> entries);


    //SqliteDataAccessInsertResultWrapper InsertImages(List<EntryItem> entries);


    //SqliteDataAccessResultWrapper UpdateEntriesAsRead(List<EntryItem> entries);


    SqliteDataAccessResultWrapper UpdateAllEntriesAsRead(List<string> feedIds);


    //SqliteDataAccessResultWrapper UpdateEntryStatus(EntryItem entry);

    SqliteDataAccessResultWrapper DeleteEntriesByFeedIds(List<string> feedIds);

}



