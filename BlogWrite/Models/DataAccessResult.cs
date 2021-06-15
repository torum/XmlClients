using System;
using System.Collections.Generic;
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
            DB, API, Other
        };

        public ErrTypes ErrType { get; set; } // eg "API, DB, other"
        public int ErrCode { get; set; } // HTTP ERROR CODE?
        public string ErrText { get; set; } // API error code translated via dictionaly.
        public string ErrPlace { get; set; } // eg RESTのPATH。
        public string ErrPlaceParent { get; set; } // ?
        public DateTime ErrDatetime { get; set; }
        public string ErrDescription { get; set; } // 自前の補足説明。
    }

    // Result Wrapper Class
    public class ResultWrapper
    {
        public ErrorObject Error;
        public bool IsError = false;
        public Object Data;

        public ResultWrapper()
        {
            Error = new ErrorObject();
        }
    }

}
