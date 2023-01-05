using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Xml;
using BlogWrite.Contracts.Services;
using BlogWrite.Contracts.ViewModels;
using BlogWrite.Models;
using BlogWrite.Models.Clients;
using BlogWrite.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;

namespace BlogWrite.ViewModels;

public class MainViewModel : ObservableRecipient, INavigationAware
{
    public ICommand FeedAddCommand
    {
        get;
    }

    public ICommand EntryViewInternalCommand
    {
        get;
    }

    public ICommand EntryViewExternalCommand
    {
        get;
    }

    public ICommand DetailsPaneShowHideCommand
    {
        get;
    }

    #region == Service Treeview ==

    private ServiceTreeBuilder _services = new ServiceTreeBuilder();
    public ObservableCollection<NodeTree> Services
    {
        get
        {
            return _services.Children;
        }
        set
        {
            _services.Children = value;
            //NotifyPropertyChanged(nameof(Services));
        }
    }

    private NodeTree _selectedTreeViewItem = new NodeService("", "", "", new Uri("http://127.0.0.1"), ApiTypes.atUnknown, ServiceTypes.Unknown);
    public NodeTree SelectedTreeViewItem
    {
        get
        {
            return _selectedTreeViewItem;
        }
        set
        {
            if (_selectedTreeViewItem == value)
                return;

            //_selectedTreeViewItem = value;
            SetProperty(ref _selectedTreeViewItem, value);

            // Clear Listview selected Item.
            SelectedListViewItem = null;

            // Clear HTTP error if shown.
            HttpError = null;
            IsShowHttpClientError = false;

            // Clear DB error if shown.
            //DatabaseError = null;
            //IsShowDatabaseErrorMessage = false;

            if (_selectedTreeViewItem == null)
                return;

            // Update Title bar info
            SelectedServiceName = _selectedTreeViewItem.Name;

            // Reset visibility flags for buttons etc
            //IsShowInFeedAndFolder = false;
            //IsShowInFeed = false;

            /*
            if (_selectedNode.ViewType == ViewTypes.vtCards)
                SelectedViewTabIndex = 0;
            else if (_selectedNode.ViewType == ViewTypes.vtMagazine)
                SelectedViewTabIndex = 1;
            else if (_selectedNode.ViewType == ViewTypes.vtThreePanes)
                SelectedViewTabIndex = 2;
            */

            if (_selectedTreeViewItem is NodeService)
            {
                /*
                // Show HTTP Error if assigned.
                if ((_selectedTreeViewItem as NodeService).ErrorHttp != null)
                {
                    HttpError = (_selected_selectedTreeViewItemNode as NodeService).ErrorHttp;
                    IsShowHttpClientErrorMessage = true;
                }

                // Show DB Error if assigned.
                if ((_selectedTreeViewItem as NodeService).ErrorDatabase != null)
                {
                    DatabaseError = (_selectedTreeViewItem as NodeService).ErrorDatabase;
                    IsShowDatabaseErrorMessage = true;
                }
                */
                // NodeFeed is selected
                if (_selectedTreeViewItem is NodeFeed)
                {
                    // Reset view...
                    (_selectedTreeViewItem as NodeFeed).IsDisplayUnarchivedOnly = true;
                    /*
                    if ((SelectedTreeViewItem as NodeFeed).IsDisplayUnarchivedOnly)
                        _selectedComboBoxItemIndex = 0;
                    else
                        _selectedComboBoxItemIndex = 1;

                    // "Silent" update
                    NotifyPropertyChanged(nameof(SelectedComboBoxItemIndex));
                    */
                    //IsShowInFeedAndFolder = true;
                    //IsShowInFeed = true;

                    if ((_selectedTreeViewItem as NodeFeed).List.Count > 0)
                    {
                        Entries = (_selectedTreeViewItem as NodeFeed).List;
                    }
                    else
                    {
                        Entries = new ObservableCollection<EntryItem>();
                        //GetEntriesAsync(_selectedTreeViewItem);
                        //Task.Run(() => GetEntriesAsync(_selectedTreeViewItem)).ConfigureAwait(false);
                        Task nowait = Task.Run(() => GetEntriesAsync(_selectedTreeViewItem));
                    }

                }
                else
                {
                    // TODO: 
                    Entries = new ObservableCollection<EntryItem>();
                }
            }
            else if (_selectedTreeViewItem is NodeFolder)
            {
                //IsShowInFeedAndFolder = true;
                //IsShowInFeed = false;

                Entries = new ObservableCollection<EntryItem>();
            }

            //Entries.Clear();
            //Entries = new ObservableCollection<EntryItem>();

            //Task nowait = Task.Run(() => LoadEntriesAsync(_selectedTreeViewItem));
            //Task nowait = Task.Run(() => GetEntriesAsync(_selectedTreeViewItem));

        }
    }

    private TreeViewNode _selectedTreeViewNode;
    public TreeViewNode SelectedTreeViewNode
    {
        get => _selectedTreeViewNode;
        set => SetProperty(ref _selectedTreeViewNode, value);
    }

    private string _selectedServiceName;
    public string SelectedServiceName
    {
        get => _selectedServiceName;
        set => SetProperty(ref _selectedServiceName, value);
    }

    #endregion

    #region == Entry ListViews ==

    private ObservableCollection<EntryItem> _entries = new ObservableCollection<EntryItem>();
    public ObservableCollection<EntryItem> Entries
    {

        get => _entries;
        set => SetProperty(ref _entries, value);
    }

    private EntryItem _selectedListViewItem = null;
    public EntryItem SelectedListViewItem
    {
        get => _selectedListViewItem;
        set
        {
            SetProperty(ref _selectedListViewItem, value);

            if (_selectedListViewItem == null)
            {
                //WriteHtmlToContentPreviewBrowser?.Invoke(this, "");

                //NotifyPropertyChanged(nameof(EntryContentText));

                IsEntryDetailVisible = false;

                return;
            }
            else
            {
                IsEntryDetailVisible = true;
            }

            SelectedEntrySummary = _selectedListViewItem.Summary;

            if ((_selectedListViewItem as EntryItem).ContentType == EntryItem.ContentTypes.text)
            {
                IsContentText = true;
                SelectedEntryContentText = (_selectedListViewItem as EntryItem).Content;
            }
            else
            {
                IsContentText = false;
                SelectedEntryContentText = "";
            }

            if ((_selectedListViewItem as EntryItem).ContentType == EntryItem.ContentTypes.textHtml)
            {
                IsContentHTML = true;
                SelectedEntryContentHTML = (_selectedListViewItem as EntryItem).Content;
            }
            else
            {
                IsContentHTML = false;
                SelectedEntryContentHTML = "";
            }

            //NavigationService.SetListDataItemForNextConnectedAnimation(_selectedListViewItem);
            //NavigationService.NavigateTo(typeof(EntryDetailsViewModel).FullName!, _selectedListViewItem);
        }
    }

    private string _selectedEntrySummary;
    public string SelectedEntrySummary
    {
        get => _selectedEntrySummary;
        set => SetProperty(ref _selectedEntrySummary, value);
    }

    private string _selectedEntryContentText;
    public string SelectedEntryContentText
    {
        get => _selectedEntryContentText;
        set => SetProperty(ref _selectedEntryContentText, value);
    }

    private string _selectedEntryContentHTML;
    public string SelectedEntryContentHTML
    {
        get => _selectedEntryContentHTML;
        set => SetProperty(ref _selectedEntryContentHTML, value);
    }

    private bool _isContentText;
    public bool IsContentText
    {
        get => _isContentText;
        set => SetProperty(ref _isContentText, value);
    }

    private bool _isContentHTML;
    public bool IsContentHTML
    {
        get => _isContentHTML;
        set => SetProperty(ref _isContentHTML, value);
    }

    /*
    private bool _isContentBrowserVisible;
    public bool IsContentBrowserVisible
    {
        get
        {
            return _isContentBrowserVisible;
        }
        set
        {
            if (_isContentBrowserVisible == value)
                return;

            _isContentBrowserVisible = value;
            NotifyPropertyChanged(nameof(IsContentBrowserVisible));
        }
    }
    */
    /*
    private int _selectedComboBoxItemIndex;
    public int SelectedComboBoxItemIndex
    {
        get
        {
            return _selectedComboBoxItemIndex;
        }
        set
        {
            if (_selectedComboBoxItemIndex == value)
                return;

            _selectedComboBoxItemIndex = value;
            NotifyPropertyChanged(nameof(SelectedComboBoxItemIndex));

            if (SelectedNode == null)
                return;

            if (SelectedNode is NodeFeed)
            {
                if (_selectedComboBoxItemIndex == 0)
                    (SelectedNode as NodeFeed).IsDisplayUnarchivedOnly = true;
                else
                    (SelectedNode as NodeFeed).IsDisplayUnarchivedOnly = false;

                Task nowait = Task.Run(() => LoadEntriesAsync(SelectedNode as NodeFeed));
                //LoadEntries(SelectedNode as NodeFeed);
            }
        }
    }
    */
    #endregion

    private bool _isBackEnabled;
    public bool IsBackEnabled
    {
        get => _isBackEnabled;
        set => SetProperty(ref _isBackEnabled, value);
    }

    private bool _isDebugWindowEnabled;
    public bool IsDebugWindowEnabled
    {
        get => _isDebugWindowEnabled;
        set => SetProperty(ref _isDebugWindowEnabled, value);
    }

    private bool _isShowHttpClientError = false;
    public bool IsShowHttpClientError
    {
        get => _isShowHttpClientError;
        set
        {
            SetProperty(ref _isShowHttpClientError, value);
            IsShowHttpClientErrorInverse = !value;
        }
    }

    private bool _isShowHttpClientErrorInverse = true;
    public bool IsShowHttpClientErrorInverse
    {
        get => _isShowHttpClientErrorInverse;
        set => SetProperty(ref _isShowHttpClientErrorInverse, value);
    }

    private ErrorObject _httpError;
    public ErrorObject HttpError
    {
        get => _httpError;
        set => SetProperty(ref _httpError, value);
    }

    private bool _isEntryDetaileVisible = false;
    public bool IsEntryDetailVisible
    {
        get => _isEntryDetaileVisible;
        set => SetProperty(ref _isEntryDetaileVisible, value);
    }

    private bool _isEntryDetailPaneVisible = true;
    public bool IsEntryDetailPaneVisible
    {
        get => _isEntryDetailPaneVisible;
        set => SetProperty(ref _isEntryDetailPaneVisible, value);
    }

    //

    public INavigationService NavigationService
    {
        get;
    }

    public event EventHandler<string> DebugOutput;

    public delegate void DebugClearEventHandler();
    public event DebugClearEventHandler DebugClear;

    public MainViewModel(INavigationService navigationService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;

        FeedAddCommand = new RelayCommand(OnFeedAdd);
        EntryViewInternalCommand = new RelayCommand(OnEntryViewInternal);
        EntryViewExternalCommand = new RelayCommand(OnEntryViewExternal);
        DetailsPaneShowHideCommand = new RelayCommand(OnDetailsPaneShowHide);

        // Load searvice tree
        if (File.Exists(App.AppDataFolder + System.IO.Path.DirectorySeparatorChar + "Searvies.xml"))
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();

            // TODO: try catch?

            doc.Load(App.AppDataFolder + System.IO.Path.DirectorySeparatorChar + "Searvies.xml");

            _services.LoadXmlDoc(doc);

            InitClients();

#if DEBUG
            IsDebugWindowEnabled = true;
#endif
        }
    }

    public void OnDebugOutput(BaseClient sender, string data)
    {
        if (string.IsNullOrEmpty(data))
            return;

        if (IsDebugWindowEnabled)
        {
            bool? uithread = App.CurrentDispatcherQueue?.HasThreadAccess;

            if (uithread != null)
            {
                if (uithread == true)
                {
                    //Debug.WriteLine(data);
                    DebugOutput?.Invoke(this, Environment.NewLine + data + Environment.NewLine + Environment.NewLine);
                }
                else
                {
                    App.CurrentDispatcherQueue?.TryEnqueue(() =>
                    {
                        //Debug.WriteLine(data);
                        DebugOutput?.Invoke(this, Environment.NewLine + data + Environment.NewLine + Environment.NewLine);
                    });
                }
            }
        }

        //IsDebugTextHasText = true;
    }

    private void InitClients()
    {
        InitClientsRecursiveLoop(_services.Children);
    }

    private void InitClientsRecursiveLoop(ObservableCollection<NodeTree> nt)
    {
        // subscribe to DebugOutput event.
        foreach (NodeTree c in nt)
        {
            if ((c is NodeService) || (c is NodeFeed))
            {
                if ((c as NodeService).Client != null)
                {
                    (c as NodeService).Client.DebugOutput += new BaseClient.ClientDebugOutput(OnDebugOutput);
                }
            }

            if (c.Children.Count > 0)
                InitClientsRecursiveLoop(c.Children);
        }
    }

    private void OnFeedAdd() => NavigationService.NavigateTo(typeof(AddFeedViewModel).FullName!, null);

    private void OnEntryViewInternal()
    {
        if (SelectedListViewItem != null)
            NavigationService.NavigateTo(typeof(EntryDetailsViewModel).FullName!, SelectedListViewItem);
    }

    private async void OnEntryViewExternal()
    {
        if (SelectedListViewItem != null)
        {
            if (SelectedListViewItem.AltHtmlUri != null)
            {
                await Windows.System.Launcher.LaunchUriAsync(SelectedListViewItem.AltHtmlUri);
                //OpenCurrentUrlInExternalBrowser?.Invoke(this, SelectedListViewItem.AltHtmlUri);
                //NavigateUrlToContentPreviewBrowser?.Invoke(this, selectedEntry.AltHtmlUri);
            }
        }
    }

    //
    private void OnDetailsPaneShowHide()
    {
        IsEntryDetailPaneVisible = !IsEntryDetailPaneVisible;
    }

    private void OnNavigated(object sender, NavigationEventArgs e) => IsBackEnabled = NavigationService.CanGoBack;

    public async void OnNavigatedTo(object parameter)
    {
    }

    public void OnNavigatedFrom()
    {
    }

    private async Task GetEntriesAsync(NodeTree nd)
    {
        if (nd == null)
            return;

        if (nd is NodeFeed)
        {
            NodeFeed fnd = nd as NodeFeed;

            var fc = fnd.Client;

            // check some conditions.
            if ((fnd.Api != ApiTypes.atFeed) || (fc == null))
                return;

            // Update Node Downloading Status
            App.CurrentDispatcherQueue?.TryEnqueue(() =>
            {
                fnd.Status = NodeFeed.DownloadStatus.downloading;

                fnd.LastUpdate = DateTime.Now;
            });

            // Get Entries from web.
            HttpClientEntryItemCollectionResultWrapper resEntries = await fc.GetEntries(fnd.EndPoint, fnd.Id);

            // Check Node exists. Could have been deleted.
            if (fnd == null)
                return;

            // Result is HTTP Error
            if (resEntries.IsError)
            {
                App.CurrentDispatcherQueue?.TryEnqueue(() =>
                {
                    // Sets Node Error.
                    fnd.ErrorHttp = resEntries.Error;

                    // If Node is selected, show the Error.
                    if (fnd == SelectedTreeViewItem)
                    {
                        HttpError = fnd.ErrorHttp;
                        IsShowHttpClientError = true;
                    }

                    // Update Node Downloading Status
                    fnd.Status = NodeFeed.DownloadStatus.error;
                });

                return;
            }
            // Result is success.
            else
            {
                App.CurrentDispatcherQueue?.TryEnqueue(() =>
                {
                    // Clear Node Error
                    fnd.ErrorHttp = null;
                    if (fnd == SelectedTreeViewItem)
                    {
                        // Hide any Error Message
                        HttpError = null;
                        IsShowHttpClientError = false;
                    }

                    //fnd.Status = NodeFeed.DownloadStatus.saving;
                    fnd.Status = NodeFeed.DownloadStatus.normal;
                });

                if (resEntries.Entries.Count > 0){

                    App.CurrentDispatcherQueue?.TryEnqueue(() =>
                    {
                        fnd.List = new ObservableCollection<EntryItem>(resEntries.Entries);

                        if (fnd == SelectedTreeViewItem)
                            Entries = fnd.List;
                    });
                }
                /*
                // 
                SqliteDataAccessInsertResultWrapper resInsert = InsertEntriesLock(resEntries.Entries);

                if (resEntries.Entries.Count > 0)
                {
                    // Get Images
                    //await GetImagesAsync(fnd, resInsert.InsertedEntries);

                    // Result is DB Error
                    if (resInsert.IsError)
                    {
                        if (Application.Current == null) { return; }
                        Application.Current.Dispatcher.Invoke(() =>
                        {

                            // Sets Node Error.
                            fnd.ErrorDatabase = resInsert.Error;

                            // If Node is selected, show the Error.
                            if (fnd == SelectedNode)
                            {
                                DatabaseError = fnd.ErrorDatabase;
                                IsShowDatabaseErrorMessage = true;
                            }

                            fnd.Status = NodeFeed.DownloadStatus.error;

                            // IsBusy = false;
                            return;
                        });
                    }
                    else
                    {
                        if (Application.Current == null) { return; }
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            // Clear error.
                            fnd.ErrorDatabase = null;

                            // If Node is selected hide any Error message if shown.
                            if (nd == SelectedNode)
                            {
                                DatabaseError = null;
                                IsShowDatabaseErrorMessage = false;
                            }

                            // Update Node Unread count.
                            fnd.EntryCount += resInsert.InsertedEntries.Count;

                            // If parent is a folder 
                            if (nd.Parent is NodeFolder)
                            {
                                // Update parent folder's unread count.
                                (nd.Parent as NodeFolder).EntryCount = (nd.Parent as NodeFolder).EntryCount + resInsert.InsertedEntries.Count;
                            }
                        });

                        // If Node is selected Load Entries.
                        if (nd == SelectedNode)
                        {
                            if (resInsert.InsertedEntries.Count > 0)
                            {
                                await LoadEntriesAsync(nd);

                                return;
                            }
                        }
                        else if ((nd.Parent == SelectedNode) && (nd.Parent is NodeFolder))
                        {
                            if (Application.Current == null) { return; }
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                foreach (var asdf in resInsert.InsertedEntries)
                                {
                                    Entries.Insert(0, asdf);
                                }

                                // sort
                                Entries = new ObservableCollection<EntryItem>(Entries.OrderByDescending(n => n.Published));
                            });
                        }
                    }
                }
                else
                {
                    //Debug.WriteLine("0 entries. ");
                }
                */

                App.CurrentDispatcherQueue?.TryEnqueue(() =>
                {
                    // Update Node Downloading Status
                    //fnd.Status = NodeFeed.DownloadStatus.normal;
                });
            }
        }
        else if (nd is NodeEntryCollection)
        {
            if ((nd as NodeEntryCollection).Parent is NodeService)
            {
                NodeService ns = (nd as NodeEntryCollection).Parent as NodeService;

                var bc = (nd as NodeEntryCollection).Client;
                if (bc == null)
                    return;

                // TODO:
                /*
                HttpClientEntryItemCollectionResultWrapper resEntries = await bc.GetEntries((nd as NodeEntryCollection).Uri, ns.Id);

                App.CurrentDispatcherQueue?.TryEnqueue(() =>
                {
                    foreach (var asdf in resEntries.Entries)
                    {
                        Entries.Insert(0, asdf);
                    }

                    // sort
                    Entries = new ObservableCollection<EntryItem>(Entries.OrderByDescending(n => n.Published));
                });
                */
            }
        }
    }

    private async Task LoadEntriesAsync(NodeTree nd, bool forceUnread = false)
    {   
        /*
        if (nd == null)
            return;

        // don't clear Entries here.

        if (nd is NodeFeed)
        {
            NodeFeed fnd = nd as NodeFeed;

            if (Application.Current == null) { return; }
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsWorking = true;

                if (forceUnread)
                    fnd.IsDisplayUnarchivedOnly = true;

                fnd.Status = NodeFeed.DownloadStatus.loading;
            });

            SqliteDataAccessSelectResultWrapper res = SelectEntriesByFeedIdLock(fnd.Id, fnd.IsDisplayUnarchivedOnly);

            if (res.IsError)
            {
                if (Application.Current == null) { return; }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // set's error
                    fnd.ErrorDatabase = res.Error;

                    if (nd == SelectedNode)
                    {
                        // show error
                        DatabaseError = fnd.ErrorDatabase;
                        IsShowDatabaseErrorMessage = true;
                    }

                    IsWorking = false;

                    fnd.Status = NodeFeed.DownloadStatus.error;

                    return;
                });

            }
            else
            {
                if (Application.Current == null) { return; }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Clear error
                    fnd.ErrorDatabase = null;

                    // Update the count
                    fnd.EntryCount = res.UnreadCount;

                    // If this is selected Node.
                    if (nd == SelectedNode)
                    {
                        // Hide error
                        DatabaseError = null;
                        IsShowDatabaseErrorMessage = false;

                        // Load entries.
                        //Entries = res.SelectedEntries;
                        // COPY!! 
                        Entries = new ObservableCollection<EntryItem>(res.SelectedEntries);

                        if (Entries.Count > 0)
                            ResetListviewPosition?.Invoke(this, 0);
                    }

                    fnd.Status = NodeFeed.DownloadStatus.normal;

                    IsWorking = false;
                });

                if (nd == SelectedNode)
                    if (res.SelectedEntries.Count > 0)
                        await LoadImagesAsync(nd, res.SelectedEntries);
            }

        }
        else if (nd is NodeFolder)
        {
            NodeFolder ndf = nd as NodeFolder;

            if (ndf.Children.Count > 0)
            {
                IsWorking = true;

                List<string> tmpList = new();

                if (Application.Current == null) { return; }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (NodeTree nt in ndf.Children)
                    {
                        if (nt is NodeFeed)
                        {
                            tmpList.Add((nt as NodeFeed).Id);
                        }
                    }

                    // TODO:
                    //ndf.Status = NodeFeed.DownloadStatus.normal;
                });

                SqliteDataAccessSelectResultWrapper res = SelectEntriesByFeedIdsLock(tmpList);

                if (Application.Current == null) { return; }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ndf.EntryCount = res.SelectedEntries.Count;

                    if (nd == SelectedNode)
                    {
                        // Load entries.  
                        //Entries = res.SelectedEntries;
                        // COPY!!
                        Entries = new ObservableCollection<EntryItem>(res.SelectedEntries);

                        if (Entries.Count > 0)
                            ResetListviewPosition?.Invoke(this, 0);
                    }

                    // TODO:
                    //ndf.Status = NodeFeed.DownloadStatus.normal;

                    IsWorking = false;
                });

                if (nd == SelectedNode)
                    if (res.SelectedEntries.Count > 0)
                        await LoadImagesAsync(nd, res.SelectedEntries);
            }

        }
        else if (nd is NodeEntryCollection)
        {
            if (Application.Current == null) { return; }
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsBusy = true;

                // This changes the listview.
                NotifyPropertyChanged(nameof(Entries));

                IsBusy = false;
            });
        }

        */
    }

}
