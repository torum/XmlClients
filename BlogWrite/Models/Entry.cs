using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogWrite.Models
{
    class Entry
    {
        public string ID { get; set; }
        public string Title { get; set; }

        // Constructor.
        public Entry()
        {
            ID = "";
            Title = "";
        }
    }

    class BlogEntry : Entry
    {
        private string _body = "";

        public string Body
        {
            get
            {
                return _body;
            }
            set
            {
                if (_body != value) {
                    _body = value;
                }
            }
        }
    }

}
