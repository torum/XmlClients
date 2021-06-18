using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    { }

    public class SqliteDataAccessInsertResultWrapper: SqliteDataAccessResultWrapper
    {
        public int InsertedCount;
    }

    public class SqliteDataAccessSelectResultWrapper: SqliteDataAccessResultWrapper
    {
        public int UnreadCount;
    }

    public class HttpClientEntryItemCollectionResultWrapper
    {
        public ErrorObject Error = new ErrorObject();
        public bool IsError = false;
        public ObservableCollection<EntryItem> Entries;
    }
}
