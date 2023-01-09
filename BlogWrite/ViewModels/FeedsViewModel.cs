using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using System.Xml;
using AngleSharp.Html.Dom;
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

public class FeedsViewModel : ObservableRecipient, INavigationAware
{

    #region == Service Treeview ==
    
    private readonly ServiceTreeBuilder _services = new ServiceTreeBuilder();

    public ObservableCollection<NodeTree> Services
    {
        get => _services.Children;
        set
        {
            _services.Children = value;
            OnPropertyChanged(nameof(Services));
        }
    }

    public void SaveServiceXml()
    {
        var xdoc = _services.AsXmlDoc();

        xdoc.Save(System.IO.Path.Combine(App.AppDataFolder, "Searvies.xml"));
    }

    private NodeTree _selectedTreeViewItem = new NodeService("", "", "", new Uri("http://127.0.0.1"), ApiTypes.atUnknown, ServiceTypes.Unknown);
    public NodeTree SelectedTreeViewItem
    {
        get => _selectedTreeViewItem;
        set
        {
            if (_selectedTreeViewItem == value)
                return;

            //_selectedTreeViewItem = value;
            SetProperty(ref _selectedTreeViewItem, value);

            // Clear Listview selected Item.
            SelectedListViewItem = null;

            // Clear error if shown.
            ErrorObj = null;
            IsShowFeedError = false;

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

            if (_selectedTreeViewItem is NodeService nds)
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

                if (nds.ErrorHttp != null)
                {
                    ErrorObj = nds.ErrorHttp;
                    IsShowFeedError = true;
                }
                else
                {
                    if (nds.ErrorDatabase != null)
                    {
                        ErrorObj = nds.ErrorDatabase;
                        IsShowFeedError = true;
                    }
                }

                


                    // NodeFeed is selected
                    if (_selectedTreeViewItem is NodeFeed nfeed)
                {
                    // Reset view...
                    nfeed.IsDisplayUnarchivedOnly = true;
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

                    if (nfeed.List.Count > 0)
                    {
                        Entries = new ObservableCollection<EntryItem>();
                        //Entries = nfeed.List;
                        //nfeed.EntryCount = 0;
                    }
                    else
                    {
                        Entries = new ObservableCollection<EntryItem>();
                        //GetEntriesAsync(_selectedTreeViewItem);
                        //Task.Run(() => GetEntriesAsync(_selectedTreeViewItem)).ConfigureAwait(false);
                        //Task nowait = Task.Run(() => GetEntriesAsync(_selectedTreeViewItem));
                    }

                    Task nowait = Task.Run(() => LoadEntriesAsync(_selectedTreeViewItem));
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

                Task nowait = Task.Run(() => LoadEntriesAsync(_selectedTreeViewItem));
            }

            //Entries.Clear();
            //Entries = new ObservableCollection<EntryItem>();

            //Task nowait = Task.Run(() => LoadEntriesAsync(_selectedTreeViewItem));
            //Task nowait = Task.Run(() => GetEntriesAsync(_selectedTreeViewItem));

        }
    }

    private TreeViewNode? _selectedTreeViewNode;
    public TreeViewNode? SelectedTreeViewNode
    {
        get => _selectedTreeViewNode;
        set => SetProperty(ref _selectedTreeViewNode, value);
    }
    
    private string _selectedServiceName = "";
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

    private EntryItem? _selectedListViewItem = null;
    public EntryItem? SelectedListViewItem
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

    private string? _selectedEntrySummary;
    public string? SelectedEntrySummary
    {
        get => _selectedEntrySummary;
        set => SetProperty(ref _selectedEntrySummary, value);
    }

    private string? _selectedEntryContentText;
    public string? SelectedEntryContentText
    {
        get => _selectedEntryContentText;
        set => SetProperty(ref _selectedEntryContentText, value);
    }

    private string? _selectedEntryContentHTML;
    public string? SelectedEntryContentHTML
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

    #region == Flags ==

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

    private bool _isShowFeedError = false;
    public bool IsShowFeedError
    {
        get => _isShowFeedError;
        set
        {
            SetProperty(ref _isShowFeedError, value);
            IsShowFeedErrorInverse = !value;
        }
    }

    private bool _isShowFeedErrorInverse = true;
    public bool IsShowFeedErrorInverse
    {
        get => _isShowFeedErrorInverse;
        set => SetProperty(ref _isShowFeedErrorInverse, value);
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

    #endregion

    #region == Error ==

    private ErrorObject? _errorObj;
    public ErrorObject? ErrorObj
    {
        get => _errorObj;
        set => SetProperty(ref _errorObj, value);
    }

    #endregion

    #region == Services ==

    public INavigationService NavigationService
    {
        get;
    }

    #endregion

    #region == Events ==

    public event EventHandler<string>? DebugOutput;

    public void OnDebugOutput(BaseClient sender, string data)
    {
        if (string.IsNullOrEmpty(data))
            return;

        if (IsDebugWindowEnabled)
        {
            var uithread = App.CurrentDispatcherQueue?.HasThreadAccess;

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


    public delegate void DebugClearEventHandler();
    public event DebugClearEventHandler? DebugClear;

    #endregion

    #region == Commands ==

    public ICommand FeedAddCommand
    {
        get;
    }

    public ICommand FeedRemoveCommand
    {
        get;
    }

    public ICommand FeedUpdateAllCommand
    {
        get;
    }

    public ICommand OpmlImportCommand
    {
        get;
    }
    public ICommand OpmlExportCommand
    {
        get;
    }

    public ICommand EntryArchiveAllCommand
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

    #endregion

    public FeedsViewModel(INavigationService navigationService)
    {
        // Init services.
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;

        // Init commands.
        FeedAddCommand = new RelayCommand(OnFeedAdd);
        FeedRemoveCommand = new RelayCommand(OnFeedRemove);
        FeedUpdateAllCommand = new RelayCommand(OnFeedUpdateAll);
        OpmlImportCommand = new RelayCommand(OnOpmlImportCommand);
        OpmlExportCommand = new RelayCommand(OnOpmlExportCommand);
        EntryArchiveAllCommand = new RelayCommand(OnEntryArchiveAll);
        EntryViewInternalCommand = new RelayCommand(OnEntryViewInternal);
        EntryViewExternalCommand = new RelayCommand(OnEntryViewExternal);
        DetailsPaneShowHideCommand = new RelayCommand(OnDetailsPaneShowHide);

        // Load searvice tree.
        if (File.Exists(App.AppDataFolder + System.IO.Path.DirectorySeparatorChar + "Searvies.xml"))
        {
            var doc = new System.Xml.XmlDocument();

            try
            {
                doc.Load(App.AppDataFolder + System.IO.Path.DirectorySeparatorChar + "Searvies.xml");

                _services.LoadXmlDoc(doc);
            }
            catch(Exception ex) 
            {
                Debug.WriteLine("Exception while loading service.xml:" + ex);
            }
        }

        // Init database.
        try
        {
            var databaseFileFolerPath = App.AppDataFolder;
            System.IO.Directory.CreateDirectory(databaseFileFolerPath);
            var dataBaseFilePath = databaseFileFolerPath + System.IO.Path.DirectorySeparatorChar + "FeedEntries.db";

            var res = dataAccessModule.InitializeDatabase(dataBaseFilePath);
            if (res.IsError)
            {
                //MainError = res.Error;
                //IsShowMainErrorMessage = true;

                Debug.WriteLine("SQLite DB init: " + res.Error.ErrText + ": " + res.Error.ErrDescription + " @" + res.Error.ErrPlace + "@" + res.Error.ErrPlaceParent);
            }

        }
        catch (System.Reflection.TargetInvocationException ex)
        {
            Debug.WriteLine("SQLite DB init: " + ex.ToString() + ": " + ex.Message);
        }
        catch (System.InvalidOperationException ex)
        {
            Debug.WriteLine("SQLite DB init: " + ex.ToString() + ": " + ex.Message);
        }
        catch (Exception e)
        {
            // TODO:
            /*
            MainError = new ErrorObject();
            MainError.ErrType = ErrorObject.ErrTypes.DB;
            MainError.ErrCode = "";
            MainError.ErrText = e.ToString();
            MainError.ErrDescription = e.Message;
            MainError.ErrDatetime = DateTime.Now;
            MainError.ErrPlace = "dataAccessModule.InitializeDatabase";
            MainError.ErrPlaceParent = "MainViewModel()";
            IsShowMainErrorMessage = true;
            */
            Debug.WriteLine("SQLite DB init: " + e.ToString() + ": " + e.Message);
        }




        IsDebugWindowEnabled = false;


        InitClients();


    }

    // db
    private readonly DataAccess dataAccessModule = new();
    private readonly ReaderWriterLockSlim _readerWriterLock = new();
    // http
    private readonly FeedClient _feedClient = new();

    #region == INavigationService contract ==

    private void OnNavigated(object sender, NavigationEventArgs e) => IsBackEnabled = NavigationService.CanGoBack;

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is RegisterFeedEventArgs args)
        {
            AddFeed(args.FeedLinkData);
        }
    }

    public void OnNavigatedFrom()
    {
    }

    #endregion

    #region == HTTP clinet ==

    private void InitClients()
    {
        InitClientsRecursiveLoop(_services.Children);
    }

    private void InitClientsRecursiveLoop(ObservableCollection<NodeTree> nt)
    {
        // subscribe to DebugOutput event.
        foreach (var c in nt)
        {
            if (c is NodeFeed nf)
            {
                nf.SetClient = _feedClient;
                nf.Client.DebugOutput += new BaseClient.ClientDebugOutput(OnDebugOutput);
            }

            if (c.Children.Count > 0)
                InitClientsRecursiveLoop(c.Children);
        }
    }

    #endregion

    #region == GetEntries ==

    // Not used. Only for testing purpus.
    // Gets entries from web and updates FeedNode when the FeedNode is selected.
    private async Task GetEntriesAsync(NodeTree nd)
    {
        if (nd == null)
            return;

        if (nd is NodeFeed fnd)
        {
            // check some conditions.
            if ((fnd.Api != ApiTypes.atFeed) || (fnd.Client == null))
                return;

            // Update Node Downloading Status
            App.CurrentDispatcherQueue?.TryEnqueue(() =>
            {
                fnd.Status = NodeFeed.DownloadStatus.downloading;

                // TODO: should I be doing this here? or after receiving the data...
                fnd.LastUpdate = DateTime.Now;
            });

            Debug.WriteLine("Getting Entries from: " + fnd.Name);

            // Get Entries from web.
            var resEntries = await fnd.Client.GetEntries(fnd.EndPoint, fnd.Id);

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
                        ErrorObj = fnd.ErrorHttp;
                        IsShowFeedError = true;
                    }

                    fnd.EntryNewCount = 0;

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
                        //ErrorObj = null;
                        //IsShowHttpClientError = false;
                    }

                    //fnd.Status = NodeFeed.DownloadStatus.saving;
                    fnd.Status = NodeFeed.DownloadStatus.normal;

                    fnd.LastUpdate = DateTime.Now;
                });

                if (resEntries.Entries.Count > 0)
                {

                    App.CurrentDispatcherQueue?.TryEnqueue(() =>
                    {
                        fnd.List = new ObservableCollection<EntryItem>(resEntries.Entries);

                        fnd.EntryNewCount = fnd.List.Count;

                        if (fnd == SelectedTreeViewItem)
                            Entries = fnd.List;
                    });
                }
            }
        }
    }
    
    #endregion

    #region == Load all entries from database ==

    // Loads node's all (including children) entries from database.
    private async Task<List<EntryItem>> LoadEntriesAsync(NodeTree nd, bool forceUnread = false)
    {
        if (nd == null)
            return new List<EntryItem>();

        // don't clear Entries here.

        if (nd is NodeFeed feed)
        {
            App.CurrentDispatcherQueue?.TryEnqueue(() =>
            {
                //Debug.WriteLine("LoadEntries: " + feed.Name);
                /*
                IsWorking = true;

                if (forceUnread)
                    fnd.IsDisplayUnarchivedOnly = true;
                */

                //feed.EntryCount = 0;
                /*
                if (feed.Status != NodeFeed.DownloadStatus.error)
                    feed.Status = NodeFeed.DownloadStatus.loading;
                */
            });

            var res = await Task.FromResult(SelectEntriesByFeedIdLock(feed.Id, feed.IsDisplayUnarchivedOnly));

            if (res.IsError)
            {
                App.CurrentDispatcherQueue?.TryEnqueue(() =>
                {
                    // set's error
                    feed.ErrorDatabase = res.Error;
                    
                    if (feed == _selectedTreeViewItem)
                    {
                        // show error
                        ErrorObj = feed.ErrorDatabase;
                        IsShowFeedError = true;
                    }

                    //IsWorking = false;
                    
                    feed.Status = NodeFeed.DownloadStatus.error;

                    Debug.WriteLine(feed.ErrorDatabase.ErrText + ", " + feed.ErrorDatabase.ErrDescription + ", " + feed.ErrorDatabase.ErrPlace);
                });

                return new List<EntryItem>();
            }
            else
            {
                var tmp = new List<EntryItem>();
                App.CurrentDispatcherQueue?.TryEnqueue(() =>
                {
                    //Debug.WriteLine("LoadEntries success: " + feed.Name);

                    // Clear error
                    feed.ErrorDatabase = null;

                    // Update the count
                    feed.EntryNewCount = res.UnreadCount;

                    if (feed.Status != NodeFeed.DownloadStatus.error)
                        feed.Status = NodeFeed.DownloadStatus.normal;

                    //feed.List = new ObservableCollection<EntryItem>(res.SelectedEntries);

                    // If this is selected Node.
                    if (feed == _selectedTreeViewItem)
                    {
                        // Hide error
                        //DatabaseError = null;
                        //IsShowDatabaseErrorMessage = false;

                        // Load entries.
                        //Entries = res.SelectedEntries;
                        // COPY!! 
                        Entries = new ObservableCollection<EntryItem>(res.SelectedEntries);

                        //if (Entries.Count > 0)
                        //    ResetListviewPosition?.Invoke(this, 0);

                    }

                    //IsWorking = false;
                });
                /*
                if (nd == SelectedNode)
                    if (res.SelectedEntries.Count > 0)
                        await LoadImagesAsync(nd, res.SelectedEntries);
                */
                //feed.List = 
                return res.SelectedEntries;
            }

        }
        else if (nd is NodeFolder folder)
        {
            List<string> tmpList = new();

            // TODO:
            App.CurrentDispatcherQueue?.TryEnqueue(() =>
            {
                folder.EntryNewCount = 0;
            });

            if (folder.Children.Count > 0)
            {
                tmpList = GetAllFeedIdsFromChildNodes(folder.Children);
            }

            if (tmpList.Count == 0)
                return new List<EntryItem>();

            var res = await Task.FromResult(SelectEntriesByFeedIdsLock(tmpList));

            if (res.IsError)
            {
                if (folder == _selectedTreeViewItem)
                {
                    // show error
                    ErrorObj = res.Error; ;
                    IsShowFeedError = true;
                }

                return new List<EntryItem>();
            }
            else
            {
                App.CurrentDispatcherQueue?.TryEnqueue(() =>
                {
                    if (folder == _selectedTreeViewItem)
                    {
                        // Load entries.  
                        //Entries = res.SelectedEntries;
                        // COPY!!
                        Entries = new ObservableCollection<EntryItem>(res.SelectedEntries);

                    }
                });

                return res.SelectedEntries;
            }


        }

        return new List<EntryItem>();
    }

    // gets all children's feed ids.
    private List<string> GetAllFeedIdsFromChildNodes(ObservableCollection<NodeTree> list)
    {
        var res = new List<string>();

        foreach (var nt in list)
        {
            if (nt is NodeFeed feed)
            {
                res.Add(feed.Id);
            }
            else if (nt is NodeFolder folder)
            {
                res.AddRange(GetAllFeedIdsFromChildNodes(folder.Children));
            }
        }

        return res;
    }

    #endregion

    #region == Update all entries ==

    private void UpdateAllEntries()
    {
        //Task.Run(() => LoadAllEntriesRecursiveLoop(_services.Children));
        UpdateAllEntriesRecursiveLoop(_services.Children);
    }

    private void UpdateAllEntriesRecursiveLoop(ObservableCollection<NodeTree> nt)
    {
        foreach (var c in nt)
        {
            if ((c is NodeEntryCollection) || (c is NodeFeed))
            {
                if (c is NodeFeed feed)
                {
                    Task.Run(async () =>
                    {
                        // No need to load entry here.
                        //await LoadEntriesAsync(feed).ConfigureAwait(false); ;
                        
                        await Task.Delay(100);

                        var now = DateTime.Now;
                        var last = feed.LastUpdate;

                        if ((last > now.AddMinutes(-5)) && (last <= now))
                        {
                            //Debug.WriteLine("Skippig " + feed.Name + ": " + last.ToString());
                        }
                        else
                        {
                            var list = await GetEntryListAsync(feed).ConfigureAwait(false);
                            
                            await Task.Delay(100);

                            if (list.Count > 0)
                                await SaveEntryListAsync(list, feed).ConfigureAwait(false); 
                        }
                    });
                }
            }

            if (c.Children.Count > 0)
            {
                //Task.Run(() => LoadAllEntriesRecursiveLoop(c.Children));
                UpdateAllEntriesRecursiveLoop(c.Children);
            }
        }
    }

    // Gets entries from web and return the list.
    private async Task<List<EntryItem>> GetEntryListAsync(NodeFeed feed)
    {
        var res = new List<EntryItem>();

        if (feed == null) return res;

        // check some conditions.
        if ((feed.Api != ApiTypes.atFeed) || (feed.Client == null)) return res;

        // Update Node Downloading Status
        App.CurrentDispatcherQueue?.TryEnqueue(() =>
        {
            //Debug.WriteLine("Getting entries: " + feed.Name);

            feed.Status = NodeFeed.DownloadStatus.downloading;

            // TODO: should I be doing this here? or after receiving the data...
            feed.LastUpdate = DateTime.Now;
        });

        // Get Entries from web.
        var resEntries = await feed.Client.GetEntries(feed.EndPoint, feed.Id);

        // Check Node exists. Could have been deleted.
        if (feed == null)
            return res;

        // Result is HTTP Error
        if (resEntries.IsError)
        {
            App.CurrentDispatcherQueue?.TryEnqueue(() =>
            {
                // Sets Node Error.
                feed.ErrorHttp = resEntries.Error;

                // If Node is selected, show the Error.
                if (feed == SelectedTreeViewItem)
                {
                    ErrorObj = feed.ErrorHttp;
                    IsShowFeedError = true;
                }

                //feed.EntryCount = 0;

                // Update Node Downloading Status
                feed.Status = NodeFeed.DownloadStatus.error;
            });

            return res;
        }
        else
        {
            // Result is success.

            App.CurrentDispatcherQueue?.TryEnqueue(() =>
            {
                //Debug.WriteLine("Getting entries success: " + feed.Name);

                // Clear Node Error
                feed.ErrorHttp = null;
                if (feed == SelectedTreeViewItem)
                {
                    // Hide any Error Message
                    //ErrorObj = null;
                    //IsShowHttpClientError = false;
                }

                //fnd.Status = NodeFeed.DownloadStatus.saving;
                feed.Status = NodeFeed.DownloadStatus.normal;

                feed.LastUpdate = DateTime.Now;
            });

            if (resEntries.Entries.Count > 0)
            {
                return new List<EntryItem>(resEntries.Entries);

                /*
                App.CurrentDispatcherQueue?.TryEnqueue(() =>
                {
                    feed.List = new ObservableCollection<EntryItem>(resEntries.Entries);

                    feed.EntryCount = feed.List.Count;

                    if (feed == SelectedTreeViewItem)
                        Entries = feed.List;
                });
                */
            }
            else
            {
                return res;
            }
        }
    }

    // Save them to database.
    private async Task<List<EntryItem>> SaveEntryListAsync(List<EntryItem> list, NodeFeed feed)
    {
        var res = new List<EntryItem>();

        if (list.Count == 0)
            return res;

        // Update Node Downloading Status
        App.CurrentDispatcherQueue?.TryEnqueue(() =>
        {
            //Debug.WriteLine("Saving entries: " + feed.Name);
            feed.Status = NodeFeed.DownloadStatus.saving;
        });

        var resInsert = await Task.FromResult(InsertEntriesLock(list));

        // Result is DB Error
        if (resInsert.IsError)
        {
            App.CurrentDispatcherQueue?.TryEnqueue(() =>
            {
                // Sets Node Error.
                feed.ErrorDatabase = resInsert.Error;
                
                // If Node is selected, show the Error.
                if (feed == _selectedTreeViewItem)
                {
                    ErrorObj = feed.ErrorDatabase;
                    IsShowFeedError = true;
                }
                
                feed.Status = NodeFeed.DownloadStatus.error;

                Debug.WriteLine(feed.ErrorDatabase.ErrText + ", " + feed.ErrorDatabase.ErrDescription + ", " + feed.ErrorDatabase.ErrPlace);
                //TODO
                // IsBusy = false;
                //return res;
            });
            return res;
        }
        else
        {
            if (resInsert.InsertedEntries.Count > 0)
            {
                var newItems = resInsert.InsertedEntries;
                /*
                newItems.Reverse();

                foreach (var hoge in newItems)
                {
                    feed.List.Insert(0, hoge);
                }
                */
                // Update Node Downloading Status
                App.CurrentDispatcherQueue?.TryEnqueue(() =>
                {
                    //Debug.WriteLine("Saving entries success: " + feed.Name);

                    
                    // TODO: if node is selected...

                    feed.EntryNewCount += newItems.Count;

                    if (feed.Status != NodeFeed.DownloadStatus.error)
                        feed.Status = NodeFeed.DownloadStatus.normal;
                });
            }
            else
            {
                // Update Node Downloading Status
                App.CurrentDispatcherQueue?.TryEnqueue(() =>
                {
                    //feed.EntryCount = newItems.Count;

                    if (feed.Status != NodeFeed.DownloadStatus.error)
                        feed.Status = NodeFeed.DownloadStatus.normal;
                });
            }
            return resInsert.InsertedEntries;
        }
    }

    #endregion

    #region == Database access method ==

    private SqliteDataAccessInsertResultWrapper InsertEntriesLock(List<EntryItem> list)
    {
        //var resInsert = new SqliteDataAccessInsertResultWrapper();

        // Insert result to Sqlite database.
        var resInsert = dataAccessModule.InsertEntries(list);
        /*
        var isbreaked = false;

        try
        {
            _readerWriterLock.EnterWriteLock();
            if (_readerWriterLock.WaitingReadCount > 0)
            {
                isbreaked = true;
            }
            else
            {
                // Insert result to Sqlite database.
                resInsert = dataAccessModule.InsertEntries(list);

            }
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }
        if (isbreaked)
        {
            Thread.Sleep(10);
            //await Task.Delay(100);

            return InsertEntriesLock(list);
        }
        */
        return resInsert;
    }

    private SqliteDataAccessSelectResultWrapper SelectEntriesByFeedIdLock(string id, bool bln)
    {
        //var res = new SqliteDataAccessSelectResultWrapper();

        var res = dataAccessModule.SelectEntriesByFeedId(id, bln);
        /*
        try
        {
            _readerWriterLock.EnterReadLock();

            res = dataAccessModule.SelectEntriesByFeedId(id, bln);
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }
        */
        return res;
    }

    private SqliteDataAccessSelectResultWrapper SelectEntriesByFeedIdsLock(List<string> list)
    {
        var res = new SqliteDataAccessSelectResultWrapper();

        try
        {
            _readerWriterLock.EnterReadLock();

            res = dataAccessModule.SelectEntriesByFeedIds(list);
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }

        return res;
    }

    private SqliteDataAccessResultWrapper UpdateAllEntriesAsReadLock(List<string> list)
    {
        var res = new SqliteDataAccessResultWrapper();

        var isbreaked = false;

        try
        {
            _readerWriterLock.EnterWriteLock();
            if (_readerWriterLock.WaitingReadCount > 0)
            {
                isbreaked = true;
            }
            else
            {
                res = dataAccessModule.UpdateAllEntriesAsRead(list);

            }
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }
        if (isbreaked)
        {
            Thread.Sleep(10);

            return UpdateAllEntriesAsReadLock(list);
        }

        return res;
    }

    private SqliteDataAccessResultWrapper UpdateEntriesAsReadLock(List<EntryItem> list)
    {
        var res = new SqliteDataAccessResultWrapper();

        var isbreaked = false;

        try
        {
            _readerWriterLock.EnterWriteLock();
            if (_readerWriterLock.WaitingReadCount > 0)
            {
                isbreaked = true;
            }
            else
            {
                res = dataAccessModule.UpdateEntriesAsRead(list);

            }
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }
        if (isbreaked)
        {
            Thread.Sleep(10);

            return UpdateEntriesAsReadLock(list);
        }

        return res;
    }

    private SqliteDataAccessResultWrapper UpdateEntryStatusLock(EntryItem entry)
    {
        var res = new SqliteDataAccessResultWrapper();

        var isbreaked = false;

        try
        {
            _readerWriterLock.EnterWriteLock();
            if (_readerWriterLock.WaitingReadCount > 0)
            {
                isbreaked = true;
            }
            else
            {
                res = dataAccessModule.UpdateEntryStatus(entry);

            }
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }
        if (isbreaked)
        {
            Thread.Sleep(10);

            return UpdateEntryStatusLock(entry);
        }

        return res;
    }

    private async Task<SqliteDataAccessResultWrapper> DeleteEntriesByFeedIdsLock(List<string> list)
    {
        var res = new SqliteDataAccessResultWrapper();

        var isbreaked = false;

        try
        {
            _readerWriterLock.EnterWriteLock();
            if (_readerWriterLock.WaitingReadCount > 0)
            {
                isbreaked = true;
            }
            else
            {
                res = dataAccessModule.DeleteEntriesByFeedIds(list);

            }
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }
        if (isbreaked)
        {
            Thread.Sleep(10);

            return await DeleteEntriesByFeedIdsLock(list);
        }

        return res;
    }

    #endregion

    #region == TODO ==

    private async Task ArchiveAllAsync(NodeTree nd)
    {
        if (nd == null)
            return;

        if (nd is NodeFeed feed)
        {
            App.CurrentDispatcherQueue?.TryEnqueue(() =>
            {
                //IsWorking = true;

                // TODO: not really saving
                //feed.Status = NodeFeed.DownloadStatus.saving;
            });

            List<string> list = new()
            {
                feed.Id
            };

            var res = await Task.FromResult(UpdateAllEntriesAsReadLock(list));

            if (res.IsError)
            {
                App.CurrentDispatcherQueue?.TryEnqueue(() =>
                {
                    feed.ErrorDatabase = res.Error;

                    if (feed == _selectedTreeViewItem)
                    {
                        //DatabaseError = (nd as NodeFeed).ErrorDatabase;
                        //IsShowDatabaseErrorMessage = true;
                    }

                    feed.Status = NodeFeed.DownloadStatus.error;

                    //IsWorking = false;
                });

                return;
            }
            else
            {
                App.CurrentDispatcherQueue?.TryEnqueue(() =>
                {
                    // Clear error
                    feed.ErrorDatabase = null;
                    /*
                    if (feed.Status != NodeFeed.DownloadStatus.error)
                        feed.Status = NodeFeed.DownloadStatus.normal;
                    */
                    if (res.AffectedCount > 0)
                    {
                        /*
                        // minus the parent folder's unread count.
                        if (nd.Parent is NodeFolder)
                        {
                            (nd.Parent as NodeFolder).EntryCount = (nd.Parent as NodeFolder).EntryCount - (nd as NodeFeed).EntryCount;
                        }
                        */
                        // reset unread count.
                        feed.EntryNewCount = 0;

                        if (feed == _selectedTreeViewItem)
                        {
                            //DatabaseError = null;
                            //IsShowDatabaseErrorMessage = false;

                            // clear here.
                            Entries.Clear();

                            // 
                            //Task.Run(() => LoadEntries(_selectedNode));
                        }
                    }

                    //IsWorking = false;
                });
                /*
                if (nd == SelectedNode)
                    await LoadEntriesAsync(_selectedNode);
                */
            }
        }
        else if (nd is NodeFolder folder)
        {
            if (folder.Children.Count > 0)
            {
                List<string> tmpList = new();
                /*
                foreach (NodeTree hoge in folder.Children)
                {
                    if (hoge is NodeFeed childfeed)
                    {
                        list.Add(childfeed.Id);
                    }
                }*/

                if (folder.Children.Count > 0)
                {
                    tmpList = GetAllFeedIdsFromChildNodes(folder.Children);
                }

                var res = UpdateAllEntriesAsReadLock(tmpList);

                if (res.AffectedCount > 0)
                {
                    App.CurrentDispatcherQueue?.TryEnqueue(() =>
                    {
                        foreach (var hoge in folder.Children)
                        {
                            if (hoge is NodeFeed)
                            {
                                folder.EntryNewCount = 0;
                            }
                        }

                        folder.EntryNewCount = 0;
                        ResetAllEntryCountAtChildNodes(folder.Children);

                        if (folder == _selectedTreeViewItem)
                            Entries.Clear();
                    });
                }
            }
            App.CurrentDispatcherQueue?.TryEnqueue(() =>
            {
                //IsWorking = false;
            });
        }
    }

    private void ResetAllEntryCountAtChildNodes(ObservableCollection<NodeTree> list)
    {
        foreach (var nt in list)
        {
            if (nt is NodeFeed feed)
            {
                feed.EntryNewCount = 0;
            }
            else if (nt is NodeFolder folder)
            {
                folder.EntryNewCount = 0;

                if (folder.Children.Count > 0)
                    ResetAllEntryCountAtChildNodes(folder.Children);
            }
        }
    }


    private void ArchiveThis(NodeTree nd, FeedEntryItem entry)
    {
        /*
        if (nd == null)
            return;

        if ((nd is not NodeFeed) && (nd is not NodeFolder))
            return;

        if (Application.Current == null) { return; }
        Application.Current.Dispatcher.Invoke(() =>
        {
            IsBusy = true;
        });

        List<EntryItem> list = new();

        list.Add(entry);

        SqliteDataAccessResultWrapper res = UpdateEntriesAsReadLock(list);

        if (res.IsError)
        {
            if (Application.Current == null) { return; }
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (nd is NodeFeed)
                    (nd as NodeFeed).ErrorDatabase = res.Error;

                if ((nd == SelectedNode) && (nd is NodeService))
                {
                    DatabaseError = (nd as NodeService).ErrorDatabase;
                    IsShowDatabaseErrorMessage = true;
                }

                IsBusy = false;
            });

            return;
        }
        else
        {
            if (Application.Current == null) { return; }
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Clear error
                if (nd is NodeFeed)
                    (nd as NodeFeed).ErrorDatabase = null;

                if (res.AffectedCount > 0)
                {
                    // remove entry from list
                    if (nd is NodeFeed)
                    {
                        if (nd.Parent is NodeFolder)
                        {
                            (nd.Parent as NodeFolder).EntryCount--;
                        }
                    }
                    if (nd is NodeFolder)
                    {
                        foreach (var cnd in (nd as NodeFolder).Children)
                        {
                            if (cnd is NodeFeed)
                            {
                                if ((cnd as NodeFeed).Id == entry.ServiceId)
                                {
                                    (cnd as NodeFeed).EntryCount--;
                                }
                            }
                        }

                    }

                    // minus the count.
                    nd.EntryCount--;

                    // remove
                    if (nd == SelectedNode)
                        Entries.Remove(entry);
                }

                IsBusy = false;
            });
        }
        */
    }

    private void UpdateEntryStatus(NodeTree nd, FeedEntryItem entry)
    {
        /*
        if (nd == null)
            return;

        if ((nd is not NodeFeed) && (nd is not NodeFolder))
            return;

        if (Application.Current == null) { return; }
        Application.Current.Dispatcher.Invoke(() =>
        {
            IsBusy = true;
        });

        SqliteDataAccessResultWrapper res = UpdateEntryStatusLock(entry);

        if (res.IsError)
        {
            if (Application.Current == null) { return; }
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (nd is NodeFeed)
                    (nd as NodeFeed).ErrorDatabase = res.Error;

                if ((nd == SelectedNode) && (nd is NodeFeed))
                {
                    DatabaseError = (nd as NodeFeed).ErrorDatabase;
                    IsShowDatabaseErrorMessage = true;
                }

                IsBusy = false;
            });

            return;
        }
        else
        {
            if (Application.Current == null) { return; }
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Clear error
                if (nd is NodeFeed)
                    (nd as NodeFeed).ErrorDatabase = null;

                if (nd == SelectedNode)
                {
                    DatabaseError = null;
                    IsShowDatabaseErrorMessage = false;
                }

                IsBusy = false;
            });
        }

        */
    }


    #endregion

    #region == Feed add command methods ==

    private void OnFeedAdd() => NavigationService.NavigateTo(typeof(FeedAddViewModel).FullName!, null);

    public void AddFeed(FeedLink feedlink)
    {
        if (feedlink == null) return;

        if (FeedDupeCheck(feedlink.FeedUri.AbsoluteUri))
        {
            Debug.WriteLine("FeedDupeCheck:" + feedlink.FeedUri.AbsoluteUri);


            // TODO: alart user
            return;
        }

        NodeFeed a = new(feedlink.Title, feedlink.FeedUri);
        a.IsSelected = true;

        a.SiteTitle = feedlink.SiteTitle;
        a.SiteUri = feedlink.SiteUri;

        a.SetClient = _feedClient;
        a.Client.DebugOutput += new BaseClient.ClientDebugOutput(OnDebugOutput);
        /*

        */

        if (SelectedTreeViewItem is NodeFolder)
        {
            a.Parent = SelectedTreeViewItem;
            SelectedTreeViewItem.Children.Add(a);
            SelectedTreeViewItem.IsExpanded = true;
        }
        else
        {
            a.Parent = _services;
            Services.Insert(0,a);//.Add(a);
        }


        SaveServiceXml();

        //Task.Run(() => GetEntriesAsync(a));
    }

    private bool FeedDupeCheck(string feedUri)
    {
        return FeedDupeCheckRecursiveLoop(Services, feedUri);
    }

    private bool FeedDupeCheckRecursiveLoop(ObservableCollection<NodeTree> nt, string feedUri)
    {
        foreach (var c in nt)
        {
            if (c is NodeFeed nf)
            {
                if (nf.EndPoint.AbsoluteUri.Equals(feedUri))
                {
                    return true;
                }
            }

            if (c.Children.Count > 0)
                if (FeedDupeCheckRecursiveLoop(c.Children, feedUri))
                    return true;
        }

        return false;
    }

    #endregion

    #region == Feed OPML ex/import command methods ==

    public void OnOpmlImportCommand()
    {
        /*
        var filepath = _openDialogService.GetOpenOpmlFileDialog("Import OPML");
        if (!string.IsNullOrEmpty(filepath.Trim()))
        {
            if (File.Exists(filepath.Trim()))
            {
                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.Load(filepath.Trim());

                    MainError = null;
                }
                catch (Exception e)
                {
                    MainError = new ErrorObject();
                    MainError.ErrType = ErrorObject.ErrTypes.Other;
                    MainError.ErrCode = "";
                    MainError.ErrText = e.ToString();
                    MainError.ErrDescription = e.Message;
                    MainError.ErrDatetime = DateTime.Now;
                    MainError.ErrPlace = "OpmlImportCommand_Execute.LoadXmlDoc";
                    MainError.ErrPlaceParent = "MainViewModel()";

                    IsShowMainErrorMessage = true;

                    return;
                }

                Opml opmlLoader = new();

                NodeFolder dummyFolder = opmlLoader.LoadOpml(doc);
                if (dummyFolder is not null)
                {
                    if (dummyFolder.Children.Count > 0)
                    {
                        foreach (var feed in dummyFolder.Children)
                        {
                            feed.Parent = _services;
                            Services.Add(feed);
                        }
                    }
                }

                SaveServiceXml();

                StartUpdate();
            }
        }
        */
    }

    public void OnOpmlExportCommand()
    {
        /*
        Opml opmlWriter = new();

        XmlDocument xdoc = opmlWriter.WriteOpml(_services);
        if (xdoc is not null)
        {
            var filepath = _openDialogService.GetSaveOpmlFileDialog("Export OPML");

            if (!string.IsNullOrEmpty(filepath))
            {
                xdoc.Save(filepath.Trim());
            }
        }
        */
    }

    #endregion

    #region == Feed other command methods ==

    private void OnFeedRemove()
    {
        if (SelectedTreeViewItem != null)
        {
            if (SelectedTreeViewItem.Parent != null)
            {
                SelectedTreeViewItem.Parent.Children.Remove(SelectedTreeViewItem);
            }
            else
            {
                Services.Remove(SelectedTreeViewItem);
            }

            SaveServiceXml();
        }
    }

    private void OnFeedUpdateAll()
    {
        UpdateAllEntries();
    }

    #endregion

    #region == Entry command methods  ==

    private void OnEntryArchiveAll()
    {
        if (SelectedTreeViewItem == null)
            return;

        if (!((SelectedTreeViewItem is NodeFeed) || (SelectedTreeViewItem is NodeFolder)))
            return;

        Task.Run(() => ArchiveAllAsync(SelectedTreeViewItem));
    }

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
            }
        }
    }

    #endregion

    #region == Other command methods ==

    private void OnDetailsPaneShowHide()
    {
        IsEntryDetailPaneVisible = !IsEntryDetailPaneVisible;
    }

    #endregion
}
