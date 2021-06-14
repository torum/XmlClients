using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlogWrite.ViewModels;
using BlogWrite.Models;
using BlogWrite.Common;

namespace BlogWrite.ViewModels
{
    // For ServiceDiscoveryViewModel.

    public abstract class ServiceDiscoveryLinkItem : ViewModelBase
    {
        private string _iconPath;
        public string IconPath
        {
            get
            {
                return _iconPath;
            }
            set
            {
                if (_iconPath == value)
                    return;

                _iconPath = value;

                NotifyPropertyChanged(nameof(IconPath));
            }
        }

        private string _typeText;
        public string TypeText
        {
            get
            {
                return _typeText;
            }
            set
            {
                if (_typeText == value)
                    return;

                _typeText = value;

                NotifyPropertyChanged(nameof(TypeText));
            }
        }

        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (_title == value)
                    return;

                _title = value;

                NotifyPropertyChanged(nameof(Title));
            }
        }

        private string _displayUrl;
        public string DisplayUrl
        {
            get
            {
                return _displayUrl;
            }
            set
            {
                if (_displayUrl == value)
                    return;

                _displayUrl = value;

                NotifyPropertyChanged(nameof(DisplayUrl));
            }
        }

    }

    public class FeedLinkItem : ServiceDiscoveryLinkItem
    {
        public FeedLink FeedLinkData { get; set; }

        public FeedLinkItem(FeedLink fd)
        {
            FeedLinkData = fd;
            DisplayUrl = fd.FeedUri.AbsoluteUri;
            Title = fd.Title;
        }
    }

    public class ServiceDocumentLinkItem : ServiceDiscoveryLinkItem
    {
        public SearviceDocumentLink SearviceDocumentLinkData { get; set; }

        public ServiceDocumentLinkItem(SearviceDocumentLink sd)
        {
            SearviceDocumentLinkData = sd;
        }
    }

}
