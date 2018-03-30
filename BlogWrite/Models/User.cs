using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogWrite.Models
{
    /// <summary>
    /// abstract base class for a User.
    /// 
    /// </summary>
    public class User
    {
        public string ID { get; set; }
        public string Name { get; set; }

        // Constructor.
        public User()
        {
            ID = "";
            Name = "";
        }
    }
}
