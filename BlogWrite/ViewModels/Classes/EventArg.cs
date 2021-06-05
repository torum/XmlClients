using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlogWrite.ViewModels;
using BlogWrite.Models;

namespace BlogWrite.ViewModels
{

    /// <summary>
    /// BlogEntryEventArgs. 
    /// </summary>
    public class BlogEntryEventArgs : EventArgs
    {
        public EntryFull Entry;
    }

    public class ServiceDiscoveryEventArgs : EventArgs
    {

    }

}
