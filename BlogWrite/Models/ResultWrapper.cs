using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Media.Imaging;

namespace BlogWrite.Models;

// ErrorInfo Class
public class ErrorObject
{
    public enum ErrTypes
    {
        DB, API, HTTP, XML, Other
    };

    // ErrTypes
    public ErrTypes ErrType { get; set; }

    // HTTP error code?
    public string? ErrCode { get; set; } 

    // eg Error title, or type of Exception, API error code translated via dictionaly.
    public string? ErrDescription { get; set; }

    // Raw exception error messages.
    public string? ErrText { get; set;}

    // eg method name, or PATH info for REST
    public string? ErrPlace { get; set; }

    // class name or site address 
    public string? ErrPlaceParent { get; set; }  

    //
    public DateTime ErrDatetime { get; set; }
}

// Result Wrapper Class
public abstract class ResultWrapper
{
    public ErrorObject Error = new();
    public bool IsError = false;
}

public class SqliteDataAccessResultWrapper: ResultWrapper
{
    public int AffectedCount;
}

public class SqliteDataAccessInsertResultWrapper: SqliteDataAccessResultWrapper
{
    public List<EntryItem> InsertedEntries = new();
}

public class SqliteDataAccessSelectResultWrapper: SqliteDataAccessResultWrapper
{
    public int UnreadCount;

    public List<EntryItem> SelectedEntries = new();
}

public class SqliteDataAccessSelectImageResultWrapper : SqliteDataAccessResultWrapper
{
    public BitmapImage Image;
}


public class HttpClientEntryItemCollectionResultWrapper : ResultWrapper
{
    public List<EntryItem> Entries = new();
}
