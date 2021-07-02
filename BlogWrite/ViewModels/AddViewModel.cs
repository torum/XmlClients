using System;
using System.Windows.Input;
using System.Net.Http;
using BlogWrite.Common;
using BlogWrite.Models;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Security.Cryptography;

namespace BlogWrite.ViewModels
{
    public class AddViewModel : ViewModelBase
    {
        private ServiceDiscovery _serviceDiscovery;

        #region == Properties ==

        private bool _isBusy;
        public bool IsBusy {
            get
            {
                return _isBusy;
            }
            set
            {
                if (_isBusy == value)
                    return;

                _isBusy = value;

                NotifyPropertyChanged(nameof(IsBusy));
                NotifyPropertyChanged(nameof(IsButtonEnabled));
            }
        }

        private bool _isShowError;
        public bool IsShowError
        {
            get
            {
                return _isShowError;
            }
            set
            {
                if (_isShowError == value)
                    return;

                _isShowError = value;

                NotifyPropertyChanged(nameof(IsShowError));
            }
        }

        private bool _isShowLog;
        public bool IsShowLog
        {
            get
            {
                return _isShowLog;
            }
            set
            {
                if (_isShowLog == value)
                    return;

                _isShowLog = value;

                NotifyPropertyChanged(nameof(IsShowLog));
            }
        }

        public bool IsButtonEnabled
        {
            get
            {
                return !IsBusy;
            }
        }

        private string _dialogTitle;
        public string DialogTitle
        {
            get
            {
                return _dialogTitle;
            }
            set
            {
                if (_dialogTitle == value)
                    return;

                _dialogTitle = value;

                NotifyPropertyChanged(nameof(DialogTitle));
            }
        }

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get
            {
                return _selectedTabIndex;
            }
            set
            {
                if (_selectedTabIndex == value)
                    return;

                _selectedTabIndex = value;

                NotifyPropertyChanged(nameof(SelectedTabIndex));
            }
        }

        private string _websiteOrEndpointUrl = "";
        public string WebsiteOrEndpointUrl
        {
            get
            {
                return _websiteOrEndpointUrl;
            }
            set
            {
                if (_websiteOrEndpointUrl == value)
                    return;

                _websiteOrEndpointUrl = value.Trim();

                NotifyPropertyChanged(nameof(WebsiteOrEndpointUrl));
            }
        }

        private string _userIdAtomPub = "";
        public string UserIdAtomPub
        {
            get
            {
                return _userIdAtomPub;
            }
            set
            {
                if (_userIdAtomPub == value)
                    return;

                _userIdAtomPub = value;

                NotifyPropertyChanged(nameof(UserIdAtomPub));
            }
        }

        private string _apiKeyAtomPub = "";
        public string ApiKeyAtomPub
        {
            get
            {
                return _apiKeyAtomPub;
            }
            set
            {
                if (_apiKeyAtomPub == value)
                    return;

                _apiKeyAtomPub = value;

                NotifyPropertyChanged(nameof(ApiKeyAtomPub));
            }
        }

        private AuthTypes _authType = AuthTypes.Wsse;
        public AuthTypes AuthType
        {
            get
            {
                return _authType;
            }
            set
            {
                if (_authType == value) return;

                _authType = value;
                this.NotifyPropertyChanged("AuthType");
            }
        }

        private string _selectedItemType;
        public string SelectedItemType
        {
            get
            {
                return _selectedItemType;
            }
            private set
            {
                if (_selectedItemType == value)
                    return;

                _selectedItemType = value;

                NotifyPropertyChanged(nameof(SelectedItemType));
            }
        }

        private string _selectedItemTitleLabel;
        public string SelectedItemTitleLabel
        {
            get
            {
                return _selectedItemTitleLabel;
            }
            set
            {
                if (_selectedItemTitleLabel == value)
                    return;

                _selectedItemTitleLabel = value;

                NotifyPropertyChanged(nameof(SelectedItemTitleLabel));
            }
        }

        private bool _isXmlRpc;
        public bool IsXmlRpc
        {
            get
            {
                return _isXmlRpc;
            }
            set
            {
                if (_isXmlRpc == value)
                    return;

                _isXmlRpc = value;

                NotifyPropertyChanged(nameof(IsXmlRpc));
            }
        }

        private string _UserIdXmlRpc = "";
        public string UserIdXmlRpc
        {
            get
            {
                return _UserIdXmlRpc;
            }
            set
            {
                if (_UserIdXmlRpc == value)
                    return;

                _UserIdXmlRpc = value;

                NotifyPropertyChanged(nameof(_UserIdXmlRpc));
            }
        }

        private string _passwordXmlRpc = "";
        public string PasswordXmlRpc
        {
            get
            {
                return _passwordXmlRpc;
            }
            set
            {
                if (_passwordXmlRpc == value)
                    return;

                _passwordXmlRpc = value;

                NotifyPropertyChanged(nameof(PasswordXmlRpc));
            }
        }

        private string _statusString;
        public string StatusText
        {
            get
            {
                return _statusString;
            }
            private set
            {
                if (_statusString == value)
                    return;

                _statusString = value;

                NotifyPropertyChanged(nameof(StatusText));
            }
        }

        private string _statusTitleString;
        public string StatusTitleText
        {
            get
            {
                return _statusTitleString;
            }
            private set
            {
                if (_statusTitleString == value)
                    return;

                _statusTitleString = value;

                NotifyPropertyChanged(nameof(StatusTitleText));
            }
        }

        private string _statusLogString;
        public string StatusLogText
        {
            get
            {
                return _statusLogString;
            }
            private set
            {
                if (_statusLogString == value)
                    return;

                _statusLogString = value;

                NotifyPropertyChanged(nameof(StatusLogText));
            }
        }

        private ObservableCollection<ServiceDiscoveryLinkItem> _linkItems = new();
        public ObservableCollection<ServiceDiscoveryLinkItem> LinkItems
        {
            get
            {
                return _linkItems;
            }
            set
            {
                if (_linkItems == value)
                    return;

                _linkItems = value;

                NotifyPropertyChanged(nameof(LinkItems));
            }
        }

        private ServiceDiscoveryLinkItem _selectedLinkItem;
        public ServiceDiscoveryLinkItem SelectedLinkItem
        {
            get
            {
                return _selectedLinkItem;
            }
            set
            {
                if (_selectedLinkItem == value)
                    return;

                _selectedLinkItem = value;

                NotifyPropertyChanged(nameof(SelectedLinkItem));
            }
        }

        #endregion

        #region == Events ==

        public event EventHandler<RegisterFeedEventArgs> RegisterFeed;

        public event EventHandler<RegisterAtomPubEventArgs> RegisterAtomPub;

        public event EventHandler<RegisterXmlRpcEventArgs> RegisterXmlRpc;

        public Action CloseAction { get; set; }

        #endregion

        public AddViewModel()
        {
            _serviceDiscovery = new ServiceDiscovery();

            #region == Command init ==

            CheckEndpointCommand = new RelayCommand(CheckEndpointCommand_Execute, CheckEndpointCommand_CanExecute);

            CheckEndpointWithAuthCommand = new RelayCommand(CheckEndpointWithAuthCommand_Execute, CheckEndpointWithAuthCommand_CanExecute);

            GoBackTo1Command = new RelayCommand(GoBackTo1Command_Execute, GoBackTo1Command_CanExecute);

            AddSelectedAndCloseCommand = new RelayCommand(AddSelectedAndCloseCommand_Execute, AddSelectedAndCloseCommand_CanExecute);

            AddSelectedNextCommand = new RelayCommand(AddSelectedNextCommand_Execute, AddSelectedNextCommand_CanExecute);
            
            #endregion

            #region == Event subscription ==

            _serviceDiscovery.StatusUpdate += new ServiceDiscovery.ServiceDiscoveryStatusUpdate(OnStatusUpdate);

            #endregion

            DialogTitle = "Add";

            GoToFirstPage();
        }

        #region == Events ==

        private void OnStatusUpdate(ServiceDiscovery sender, string data)
        {
            StatusLogText = StatusLogText + data + Environment.NewLine;
        }

        #endregion

        #region == Methods ==

        private void GoToFirstPage()
        {
            SelectedTabIndex = 0;
        }

        private void GoToSelectFeedOrServicePage()
        {
            SelectedTabIndex = 1;
        }

        private void GoToAuthInputPage()
        {
            SelectedTabIndex = 2;
        }
        private void GoToServiceFoundPage()
        {
            SelectedTabIndex = 3;
        }
        
        #endregion

        #region == ICommands ==

        public ICommand CheckEndpointCommand { get; }

        public bool CheckEndpointCommand_CanExecute()
        {
            return !String.IsNullOrEmpty(WebsiteOrEndpointUrl);
        }

        public async void CheckEndpointCommand_Execute()
        {
            if (String.IsNullOrEmpty(WebsiteOrEndpointUrl))
                return;

            StatusTitleText = "";
            StatusText = "";
            StatusLogText = "";
            LinkItems.Clear();
            SelectedLinkItem = null;

            Uri uri;
            try
            { 
                uri = new Uri(WebsiteOrEndpointUrl);
            }
            catch
            {
                StatusTitleText = "Invalid URL format";
                StatusText = "Should be something like http://www.test.com/test/atom";

                IsShowError = true;
                IsShowLog = false;

                GoToFirstPage();

                return;
            }

            if (!(uri.Scheme.Equals("http") || uri.Scheme.Equals("https")))
            {
                StatusTitleText = "Invalid URI scheme";
                StatusText = "Should be http or https: " + uri.Scheme;

                IsShowError = true;
                IsShowLog = false;

                GoToFirstPage();

                return;
            }

            IsBusy = true;

            try
            {
                ServiceResultBase sr = await _serviceDiscovery.DiscoverService(uri);

                if (sr == null)
                {
                    IsBusy = false;
                    return;
                }

                if (sr is ServiceResultErr)
                {
                    StatusTitleText = (sr as ServiceResultErr).ErrTitle;
                    StatusText = (sr as ServiceResultErr).ErrDescription;

                    IsShowError = true;
                    IsShowLog = true;

                    IsBusy = false;
                    return;
                }

                // Aut hRequired returned. Probably API endpoint.
                if (sr is ServiceResultAuthRequired)
                {
                    IsShowError = false;
                    IsShowLog = false;

                    // Auth input page.
                    GoToAuthInputPage();

                    IsBusy = false;
                    return;
                }

                if (sr is ServiceResultHtmlPage)
                {
                    if (((sr as ServiceResultHtmlPage).Feeds.Count > 0) || ((sr as ServiceResultHtmlPage).Services.Count > 0))
                    {
                        // Feeds
                        if ((sr as ServiceResultHtmlPage).Feeds.Count > 0)
                        {
                            foreach (var f in (sr as ServiceResultHtmlPage).Feeds)
                            {
                                FeedLinkItem li = new(f);

                                LinkItems.Add(li);
                            }
                        }

                        // Services
                        if ((sr as ServiceResultHtmlPage).Services.Count > 0)
                        {
                            foreach (var s in (sr as ServiceResultHtmlPage).Services)
                            {
                                if (s is RsdLink)
                                {
                                    if ((s as RsdLink).Apis.Count > 0)
                                    {
                                        ServiceDocumentLinkItem li = new(s);

                                        if (li.IsSupported)
                                            LinkItems.Add(li);
                                    }
                                }
                                else
                                {
                                    // AtomApi?
                                    //....
                                }
                            }
                        }

                        IsShowError = false;
                        IsShowLog = false;

                        GoToSelectFeedOrServicePage();

                        IsBusy = false;
                    }
                    else
                    {
                        StatusTitleText = "Found 0 item";
                        StatusText = "Could not find any feeds or services.";

                        IsShowError = true;
                        IsShowLog = true;

                        IsBusy = false;
                        return;
                    }
                }
                else if (sr is ServiceResultFeed)
                {
                    FeedLink feed = (sr as ServiceResultFeed).FeedlinkInfo;

                    FeedLinkItem li = new(feed);

                    LinkItems.Add(li);

                    IsShowError = false;
                    IsShowLog = false;

                    GoToSelectFeedOrServicePage();

                    IsBusy = false;
                }
                else if (sr is ServiceResultRsd)
                {
                    if ((sr as ServiceResultRsd).Rsd is RsdLink)
                    {
                        RsdLink hoge = (sr as ServiceResultRsd).Rsd;

                        if (hoge.Apis.Count > 0)
                        {
                            ServiceDocumentLinkItem li = new(hoge);

                            if (li.IsSupported)
                            {
                                LinkItems.Add(li);

                                GoToSelectFeedOrServicePage();
                            }
                            else
                            {
                                StatusTitleText = "Found 0 item";
                                StatusText = "RSD found but no supported service found.";

                                IsShowError = true;
                                IsShowLog = true;

                                IsBusy = false;
                                return;
                            }
                        }
                        else
                        {
                            StatusTitleText = "Found 0 item";
                            StatusText = "RSD found but no supported api found.";

                            IsShowError = true;
                            IsShowLog = true;

                            IsBusy = false;
                            return;
                        }
                    }
                    else
                    {
                        // AtomApi?
                        //.
                    }
                }
            }
            finally
            {
                IsBusy = false;
            }

            IsBusy = false;
        }

        public ICommand CheckEndpointWithAuthCommand { get; }

        public bool CheckEndpointWithAuthCommand_CanExecute()
        {
            if (string.IsNullOrEmpty(UserIdAtomPub))
                return false;

            if (string.IsNullOrEmpty(ApiKeyAtomPub))
                return false;

            return true;
        }

        // TODO
        public async void CheckEndpointWithAuthCommand_Execute()
        {
            Uri uri;
            try
            {
                uri = new Uri(WebsiteOrEndpointUrl);
            }
            catch
            {
                return;
            }

            IsBusy = true;

            try
            {
                ServiceResultBase sr = await _serviceDiscovery.DiscoverServiceWithAuth(uri, UserIdAtomPub, ApiKeyAtomPub, AuthType);

                if (sr == null)
                    return;

                if (sr is ServiceResultErr)
                {
                    StatusTitleText = (sr as ServiceResultErr).ErrTitle;
                    StatusText = (sr as ServiceResultErr).ErrDescription;

                    IsShowError = true;
                    IsShowLog = true;

                    GoToFirstPage();

                    return;
                }

                // AuthRequired returned. Probably wrong auth info.
                if (sr is ServiceResultAuthRequired)
                {
                    StatusTitleText = "Auth Required";
                    StatusText = "Wrong auth information?";

                    IsShowError = true;
                    IsShowLog = true;

                    SelectedTabIndex = 0;

                    return;
                }

                if (sr is ServiceResultAtomPub)
                {
                    // TODO: Add Service button page.

                    AppLink al = new AppLink();
                    al.NodeService = (sr as ServiceResultAtomPub).AtomService;

                    ServiceDocumentLinkItem sdli = new(al);

                    SelectedLinkItem = sdli;

                    SelectedItemTitleLabel = sdli.Title;

                    SelectedItemType = sdli.TypeText;

                    IsXmlRpc = false;

                    GoToServiceFoundPage();

                    /*
                    RegisterAtomPubEventArgs arg = new();
                    arg.NodeService = (sr as ServiceResultAtomPub).AtomService;

                    RegisterAtomPub?.Invoke(this, arg);

                    if (CloseAction != null)
                        CloseAction();
                    */
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ICommand GoBackTo1Command { get; }

        public bool GoBackTo1Command_CanExecute()
        {
            return true;
        }

        public void GoBackTo1Command_Execute()
        {
            LinkItems.Clear();
            SelectedLinkItem = null;

            IsShowError = false;
            IsShowLog = false;

            GoToFirstPage();
        }

        public ICommand AddSelectedNextCommand { get; }

        public bool AddSelectedNextCommand_CanExecute()
        {
            if (SelectedLinkItem == null)
                return false;
            else
                return true;
        }

        public void AddSelectedNextCommand_Execute()
        {
            if (SelectedLinkItem == null)
                return;

            if (SelectedLinkItem is FeedLinkItem)
            {
                SelectedItemTitleLabel = (SelectedLinkItem as FeedLinkItem).Title;

                SelectedItemType = (SelectedLinkItem as FeedLinkItem).TypeText;

                IsXmlRpc = false;
            }
            else if (SelectedLinkItem is ServiceDocumentLinkItem)
            {
                SelectedItemTitleLabel = (SelectedLinkItem as ServiceDocumentLinkItem).Title;

                SelectedItemType = (SelectedLinkItem as ServiceDocumentLinkItem).TypeText;

                if ((SelectedLinkItem as ServiceDocumentLinkItem).SearviceDocumentLinkData is RsdLink)
                {
                    IsXmlRpc = true;
                }
                else if ((SelectedLinkItem as ServiceDocumentLinkItem).SearviceDocumentLinkData is AppLink)
                {
                    IsXmlRpc = false;
                }
            }

            GoToServiceFoundPage();
        }

        public ICommand AddSelectedAndCloseCommand { get; }

        public bool AddSelectedAndCloseCommand_CanExecute()
        {
            if (SelectedLinkItem == null)
                return false;

            if (IsXmlRpc)
            {
                if (string.IsNullOrEmpty(UserIdXmlRpc))
                    return false;

                if (string.IsNullOrEmpty(PasswordXmlRpc))
                    return false;
            }

            return true;
        }

        public void AddSelectedAndCloseCommand_Execute()
        {
            if (SelectedLinkItem == null)
                return;

            if (IsXmlRpc)
            {
                if (string.IsNullOrEmpty(UserIdXmlRpc))
                    return;

                if (string.IsNullOrEmpty(PasswordXmlRpc))
                    return;
            }

            if (SelectedLinkItem is FeedLinkItem)
            {
                if (!string.IsNullOrEmpty(SelectedItemTitleLabel))
                    (SelectedLinkItem as FeedLinkItem).FeedLinkData.Title = SelectedItemTitleLabel;

                RegisterFeedEventArgs arg = new();
                arg.FeedLinkData = (SelectedLinkItem as FeedLinkItem).FeedLinkData;

                RegisterFeed?.Invoke(this, arg);
            }
            else if (SelectedLinkItem is ServiceDocumentLinkItem)
            {
                if ((SelectedLinkItem as ServiceDocumentLinkItem).SearviceDocumentLinkData is RsdLink)
                {
                    RsdLink sd = (SelectedLinkItem as ServiceDocumentLinkItem).SearviceDocumentLinkData as RsdLink;

                    if (!string.IsNullOrEmpty(SelectedItemTitleLabel))
                        sd.Title = SelectedItemTitleLabel;

                    RegisterXmlRpcEventArgs arg = new();
                    arg.RsdLink = sd;
                    arg.UserIdXmlRpc = UserIdXmlRpc;
                    arg.PasswordXmlRpc = PasswordXmlRpc;

                    // TODO: check XML-RPC call?

                    RegisterXmlRpc?.Invoke(this, arg);
                }
                else if ((SelectedLinkItem as ServiceDocumentLinkItem).SearviceDocumentLinkData is AppLink)
                {
                    AppLink sd = (SelectedLinkItem as ServiceDocumentLinkItem).SearviceDocumentLinkData as AppLink;

                    if (!string.IsNullOrEmpty(SelectedItemTitleLabel))
                        sd.NodeService.Name = SelectedItemTitleLabel;

                    RegisterAtomPubEventArgs arg = new();
                    arg.NodeService = sd.NodeService;

                    RegisterAtomPub?.Invoke(this, arg);
                }
            }

            if (CloseAction != null)
                CloseAction();
        }

        #endregion
    }
}
