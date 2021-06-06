using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlogWrite.ViewModels;
using BlogWrite.Models;

namespace BlogWrite.ViewModels
{
    // For opening editor window from MainViewModel.
    public class BlogEntryEventArgs : EventArgs
    {
        public EntryFull Entry;
    }

    // For opening ServiceDiscovery window from MainViewModel.
    public class ServiceDiscoveryEventArgs : EventArgs
    {

    }

    // For registering feed from ServiceDiscoveryViewModel.
    public class RegisterFeedEventArgs : EventArgs
    {
        public FeedLink FeedLinkData { get; set; }
    }
}
