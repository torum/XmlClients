using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Media.Imaging;

namespace BlogWrite.Models
{
    // ErrorInfo Class
    public class ErrorObject
    {
        public enum ErrTypes
        {
            DB, API, HTTP, Other
        };

        public ErrTypes ErrType { get; set; } 
        public string ErrCode { get; set; } // HTTP error code?
        public string ErrText { get; set; } // eg Error title, or type of Exception, API error code translated via dictionaly.
        public string ErrPlace { get; set; } // eg method name, or PATH info for REST
        public string ErrPlaceParent { get; set; } // class name or site address  
        public DateTime ErrDatetime { get; set; }
        public string ErrDescription { get; set; } // error message.
    }

    // Result Wrapper Class
    public abstract class ResultWrapper
    {
        public ErrorObject Error = new ErrorObject();
        public bool IsError = false;
    }

    public class SqliteDataAccessResultWrapper: ResultWrapper
    {
        public int AffectedCount;
    }

    public class SqliteDataAccessInsertResultWrapper: SqliteDataAccessResultWrapper
    {
        public ObservableCollection<EntryItem> InsertedEntries = new();
    }

    public class SqliteDataAccessSelectResultWrapper: SqliteDataAccessResultWrapper
    {
        public int UnreadCount;

        public ObservableCollection<EntryItem> SelectedEntries = new();
    }

    public class SqliteDataAccessSelectImageResultWrapper : SqliteDataAccessResultWrapper
    {
        public BitmapImage Image;
    }


    public class HttpClientEntryItemCollectionResultWrapper : ResultWrapper
    {
        public List<EntryItem> Entries = new();
    }
}
