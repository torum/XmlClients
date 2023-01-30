using System.Collections.ObjectModel;
using System.Windows.Input;
using FeedDesk.Contracts.Services;
using FeedDesk.Contracts.ViewModels;
using BlogWrite.Core.Models;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FeedDesk.ViewModels;

public class FeedAddViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService _navigationService;
    //private readonly ISampleDataService _sampleDataService;

    private readonly ServiceDiscovery _serviceDiscovery;

    #region == Properties ==

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            SetProperty(ref _isBusy, value);

            IsButtonEnabled = value;
        }
    }

    private bool _isShowError;
    public bool IsShowError
    {
        get => _isShowError;
        set => SetProperty(ref _isShowError, value);
    }

    private bool _isShowLog;
    public bool IsShowLog
    {
        get => _isShowLog;
        set => SetProperty(ref _isShowLog, value);
    }

    private bool _isButtonEnabled;
    public bool IsButtonEnabled
    {
        get => _isButtonEnabled;
        private set=> SetProperty(ref _isButtonEnabled, value);
    }

    private string _websiteOrEndpointUrl = "";
    public string WebsiteOrEndpointUrl
    {
        get => _websiteOrEndpointUrl;
        set => SetProperty(ref _websiteOrEndpointUrl, value.Trim());
    }

    private string _userIdAtomPub = "";
    public string UserIdAtomPub
    {
        get => _userIdAtomPub;
        set => SetProperty(ref _userIdAtomPub, value);
    }

    private string _apiKeyAtomPub = "";
    public string ApiKeyAtomPub
    {
        get => _apiKeyAtomPub;
        set => SetProperty(ref _apiKeyAtomPub, value);
    }

    private AuthTypes _authType = AuthTypes.Wsse;
    public AuthTypes AuthType
    {
        get => _authType;
        set => SetProperty(ref _authType, value);
    }

    private string _selectedItemType;
    public string SelectedItemType
    {
        get => _selectedItemType;
        set => SetProperty(ref _selectedItemType, value);
    }

    private string _selectedItemTitleLabel;
    public string SelectedItemTitleLabel
    {
        get => _selectedItemTitleLabel;
        set => SetProperty(ref _selectedItemTitleLabel, value);
    }

    private bool _isXmlRpc;
    public bool IsXmlRpc
    {
        get => _isXmlRpc;
        set => SetProperty(ref _isXmlRpc, value);
    }

    private string _userIdXmlRpc = "";
    public string UserIdXmlRpc
    {
        get => _userIdXmlRpc;
        set => SetProperty(ref _userIdXmlRpc, value);
    }

    private string _passwordXmlRpc = "";
    public string PasswordXmlRpc
    {
        get => _passwordXmlRpc;
        set => SetProperty(ref _passwordXmlRpc, value);
    }

    private string _statusString;
    public string StatusText
    {
        get => _statusString;
        set => SetProperty(ref _statusString, value);
    }

    private string _statusTitleString;
    public string StatusTitleText
    {
        get => _statusTitleString;
        set => SetProperty(ref _statusTitleString, value);
    }

    private string _statusLogString;
    public string StatusLogText
    {
        get => _statusLogString;
        set => SetProperty(ref _statusLogString, value);
    }

    private int _selectedTabIndex;
    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => SetProperty(ref _selectedTabIndex, value);
    }

    private ObservableCollection<LinkItem> _linkItems = new();
    public ObservableCollection<LinkItem> LinkItems
    {
        get => _linkItems;
        set => SetProperty(ref _linkItems, value);
    }

    private LinkItem _selectedLinkItem;
    public LinkItem SelectedLinkItem
    {
        get => _selectedLinkItem;
        set => SetProperty(ref _selectedLinkItem, value);
    }

    #endregion

    #region == Command ==

    public ICommand GoBackCommand
    {
        get;
    }

    public ICommand GoCommand
    {
        get;
    }

    public ICommand GoSelectedCommand
    {
        get;
    }

    public ICommand AddSelectedAndCloseCommand
    {
        get;
    }


    public ICommand GoToFirstTabCommand
    {
        get;
    }

    public ICommand GoToSecondTabCommand
    {
        get;
    }

    public ICommand GoToThirdTabCommand
    {
        get;
    }

    public ICommand GoToFourthTabCommand
    {
        get;
    }

    #endregion

    //public ObservableCollection<SampleOrder> Source { get; } = new ObservableCollection<SampleOrder>();

    public FeedAddViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        //_sampleDataService = sampleDataService;

        _serviceDiscovery = new ServiceDiscovery();
        _serviceDiscovery.StatusUpdate += new ServiceDiscovery.ServiceDiscoveryStatusUpdate(OnStatusUpdate);


        GoBackCommand = new RelayCommand(OnGoBack);
        GoCommand = new RelayCommand(OnGo);
        GoSelectedCommand = new RelayCommand(OnGoSelected);
        AddSelectedAndCloseCommand = new RelayCommand(OnAddSelectedAndClose);



        GoToFirstTabCommand = new RelayCommand(OnGoToFirstTab);
        GoToSecondTabCommand = new RelayCommand(OnGoToSecondTab);
        GoToThirdTabCommand = new RelayCommand(OnGoToThirdTab);
        GoToFourthTabCommand = new RelayCommand(OnGoToFourthTab);
    }

    public void OnNavigatedTo(object parameter)
    {

    }

    public void OnNavigatedFrom()
    {
    }

    private void OnGoBack()
    {
        if (_navigationService.CanGoBack)
        {
            _navigationService.GoBack();
        }
    }

    private void OnGoToFirstTab()
    {
        GoToFirstPage();
    }

    private void OnGoToSecondTab()
    {
        GoToSelectFeedOrServicePage();
    }

    private void OnGoToThirdTab()
    {
        if (SelectedLinkItem is FeedLinkItem)
        {
            GoToSelectFeedOrServicePage();
        }
        else if (SelectedLinkItem is ServiceDocumentLinkItem)
        {
            GoToAuthInputPage();
        }
    }

    private void OnGoToFourthTab()
    {
        GoToServiceFoundPage();
    }


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

    private void OnStatusUpdate(ServiceDiscovery sender, string data)
    {
        bool? uithread = App.CurrentDispatcherQueue?.HasThreadAccess;

        if (uithread != null)
        {
            if (uithread == true)
            {
                StatusLogText = StatusLogText + data + Environment.NewLine;
            }
            else
            {
                App.CurrentDispatcherQueue?.TryEnqueue(() =>
                {
                    StatusLogText = StatusLogText + data + Environment.NewLine;
                });
            }
        }
    }

    private async void OnGo()
    {
        StatusTitleText = "";
        StatusText = "";
        StatusLogText = "";
        LinkItems.Clear();
        SelectedLinkItem = null;


        if (string.IsNullOrEmpty(WebsiteOrEndpointUrl))
        {
            StatusTitleText = "Invalid URL format";
            StatusText = "Text input field is empty.";

            IsShowError = true;
            IsShowLog = false;

            GoToFirstPage();

            return;
        }


        Uri uri;
        try
        {
            uri = new Uri(WebsiteOrEndpointUrl);
        }
        catch
        {
            StatusTitleText = "Invalid URL format";
            StatusText = $"{WebsiteOrEndpointUrl} is not a valid URL.";//"Should be something like https://www.example.com/app/atom";

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

    private void OnGoSelected()
    {
        if (SelectedLinkItem == null)
        {
        
            return; 
        }

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


    private void OnAddSelectedAndClose()
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

            //RegisterFeed?.Invoke(this, arg);

            _navigationService.NavigateTo(typeof(FeedsViewModel).FullName!, arg);


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

                // TODO
                //RegisterXmlRpc?.Invoke(this, arg);
            }
            else if ((SelectedLinkItem as ServiceDocumentLinkItem).SearviceDocumentLinkData is AppLink)
            {
                AppLink sd = (SelectedLinkItem as ServiceDocumentLinkItem).SearviceDocumentLinkData as AppLink;

                if (!string.IsNullOrEmpty(SelectedItemTitleLabel))
                    sd.NodeService.Name = SelectedItemTitleLabel;

                RegisterAtomPubEventArgs arg = new();
                arg.NodeService = sd.NodeService;

                // TODO
                //RegisterAtomPub?.Invoke(this, arg);
            }
        }

    }

}
