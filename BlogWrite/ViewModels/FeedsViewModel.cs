using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Xml;
using BlogWrite.Contracts.Services;
using BlogWrite.Contracts.ViewModels;
using BlogWrite.Models;
using BlogWrite.Models.Clients;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json.Linq;
using Windows.System;

namespace BlogWrite.ViewModels;

public partial class FeedsViewModel : ObservableRecipient, INavigationAware
{

    #region == Service Treeview ==

    private readonly ServiceTreeBuilder _services = new();

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

    private NodeTree? _selectedTreeViewItem;// = new NodeService("", "", "", new Uri("http://127.0.0.1"), ApiTypes.atUnknown, ServiceTypes.Unknown);
    public NodeTree? SelectedTreeViewItem
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

            if (_selectedTreeViewItem != null)
            {
                IsToggleInboxAppButtonEnabled = true;
            }
            else
            {
                IsToggleInboxAppButtonEnabled = false;
            }

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

                //
                IsShowInboxEntries = nds.IsDisplayUnarchivedOnly;

                // NodeFeed is selected
                if (_selectedTreeViewItem is NodeFeed nfeed)
                {
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
            else if (_selectedTreeViewItem is NodeFolder folder)
            {
                //IsShowInFeedAndFolder = true;
                //IsShowInFeed = false;


                IsShowInboxEntries = folder.IsDisplayUnarchivedOnly;

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

    //[ObservableProperty]
    //[NotifyCanExecuteChangedFor(nameof(EntryArchiveAllCommand))]
    //private ObservableCollection<EntryItem> entries = new();

    private ObservableCollection<EntryItem> _entries = new();
    public ObservableCollection<EntryItem> Entries
    {
        get => _entries;
        set 
        {
            if (SetProperty(ref _entries, value))
            {
                EntryArchiveAllCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private EntryItem? _selectedListViewItem = null;
    public EntryItem? SelectedListViewItem
    {
        get => _selectedListViewItem;
        set
        {
            if (SetProperty(ref _selectedListViewItem, value))
            {
                EntryViewExternalCommand.NotifyCanExecuteChanged();
            }

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

            if ((_selectedListViewItem as EntryItem).AltHtmlUri != null)
            {
                IsAltLinkExists = true;
            }
            else
            {
                IsAltLinkExists = false;
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

    private bool _isAltLinkExists;
    public bool IsAltLinkExists
    {
        get => _isAltLinkExists;
        set
        {
            if (SetProperty(ref _isAltLinkExists, value))
            {
                IsNoAltLinkExists = !value;
            }
        }
    }

    private bool _isNoAltLinkExists;
    public bool IsNoAltLinkExists
    {
        get => _isNoAltLinkExists;
        set => SetProperty(ref _isNoAltLinkExists, value);
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

    private bool _isToggleInboxAppButtonEnabled;
    public bool IsToggleInboxAppButtonEnabled
    {
        get => _isToggleInboxAppButtonEnabled;
        set => SetProperty(ref _isToggleInboxAppButtonEnabled, value);
    }

    private string _toggleInboxAppButtonLabel = "Inbox";
    public string ToggleInboxAppButtonLabel
    {
        get => _toggleInboxAppButtonLabel;
        set => SetProperty(ref _toggleInboxAppButtonLabel, value);
    }

    private string _toggleInboxAppButtonIcon = "M19,15H15A3,3 0 0,1 12,18A3,3 0 0,1 9,15H5V5H19M19,3H5C3.89,3 3,3.9 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V5A2,2 0 0,0 19,3Z";
    public string ToggleInboxAppButtonIcon
    {
        get => _toggleInboxAppButtonIcon;
        set => SetProperty(ref _toggleInboxAppButtonIcon, value);
    }

    private bool _isShowInboxEntries = true;
    public bool IsShowInboxEntries
    {
        get => _isShowInboxEntries;
        set
        {
            if (SetProperty(ref _isShowInboxEntries, value))
            {
                IsShowAllEntries = !value;
                toggleInboxAppButtonLabel();
            }
        }
    }

    private bool _isShowAllEntries = false;
    public bool IsShowAllEntries
    {
        get => _isShowAllEntries;
        set 
        {
            if (SetProperty(ref _isShowAllEntries, value))
            {
                IsShowInboxEntries = !value;
                toggleInboxAppButtonLabel();
            }
        }
    }

    private void toggleInboxAppButtonLabel()
    {
        if (IsShowAllEntries)
        {
            ToggleInboxAppButtonLabel = "All";
            ToggleInboxAppButtonIcon = "M14.5 11C14.78 11 15 11.22 15 11.5V13H9V11.5C9 11.22 9.22 11 9.5 11H14.5M20 13.55V10H18V13.06C18.69 13.14 19.36 13.31 20 13.55M21 9H3V3H21V9M19 5H5V7H19V5M8.85 19H6V10H4V21H9.78C9.54 20.61 9.32 20.19 9.14 19.75L8.85 19M17 18C16.44 18 16 18.44 16 19S16.44 20 17 20 18 19.56 18 19 17.56 18 17 18M23 19C22.06 21.34 19.73 23 17 23S11.94 21.34 11 19C11.94 16.66 14.27 15 17 15S22.06 16.66 23 19M19.5 19C19.5 17.62 18.38 16.5 17 16.5S14.5 17.62 14.5 19 15.62 21.5 17 21.5 19.5 20.38 19.5 19Z";
        }
        if (IsShowInboxEntries)
        {
            ToggleInboxAppButtonLabel = "Inbox";
            ToggleInboxAppButtonIcon = "M19,15H15A3,3 0 0,1 12,18A3,3 0 0,1 9,15H5V5H19M19,3H5C3.89,3 3,3.9 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V5A2,2 0 0,0 19,3Z";
        }
    }

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

    private readonly INavigationService _navigationService;

    private readonly IFileDialogService _fileDialogService;

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

    // db
    private readonly DataAccess dataAccessModule = new();
    private readonly ReaderWriterLockSlim _readerWriterLock = new();

    // http
    private readonly FeedClient _feedClient = new();

    public FeedsViewModel(INavigationService navigationService, IFileDialogService fileDialogService)
    {
        // Init services.
        _navigationService = navigationService;
        _navigationService.Navigated += OnNavigated;

        _fileDialogService = fileDialogService;

        // Load searvice tree.
        // TODO: make it service.
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
        // TODO: make it service.
        try
        {
            var databaseFileFolerPath = App.AppDataFolder;
            System.IO.Directory.CreateDirectory(databaseFileFolerPath);
            var dataBaseFilePath = databaseFileFolerPath + System.IO.Path.DirectorySeparatorChar + "FeedEntries.db";

            var res = dataAccessModule.InitializeDatabase(dataBaseFilePath);
            if (res.IsError)
            {
                // TODO:
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

    #region == INavigationService ==

    private void OnNavigated(object sender, NavigationEventArgs e) => IsBackEnabled = _navigationService.CanGoBack;

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

    private SqliteDataAccessSelectResultWrapper SelectEntriesByFeedIdLock(string id, bool IsUnarchivedOnly)
    {
        //var res = new SqliteDataAccessSelectResultWrapper();

        var res = dataAccessModule.SelectEntriesByFeedId(id, IsUnarchivedOnly);
        /*
        try
        {
            _readerWriterLock.EnterReadLock();

            res = dataAccessModule.SelectEntriesByFeedId(id, IsUnarchivedOnly);
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }
        */
        return res;
    }

    private SqliteDataAccessSelectResultWrapper SelectEntriesByFeedIdsLock(List<string> list, bool IsUnarchivedOnly)
    {
        var res = new SqliteDataAccessSelectResultWrapper();

        try
        {
            _readerWriterLock.EnterReadLock();

            res = dataAccessModule.SelectEntriesByFeedIds(list, IsUnarchivedOnly);
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

    #region == Entries Refreshing ==

    // Loads node's all (including children) entries from database.
    private async Task<List<EntryItem>> LoadEntriesAsync(NodeTree nt)
    {
        if (nt == null)
            return new List<EntryItem>();

        // don't clear Entries here.

        if (nt is NodeFeed feed)
        {
            App.CurrentDispatcherQueue?.TryEnqueue(() =>
            {
                feed.IsBusy = true;
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

                    feed.Status = NodeFeed.DownloadStatus.error;

                    Debug.WriteLine(feed.ErrorDatabase.ErrText + ", " + feed.ErrorDatabase.ErrDescription + ", " + feed.ErrorDatabase.ErrPlace);

                    feed.IsBusy = false;
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
                    }

                    feed.IsBusy = false;
                });

                return res.SelectedEntries;
            }
        }
        else if (nt is NodeFolder folder)
        {
            List<string> tmpList = new();

            App.CurrentDispatcherQueue?.TryEnqueue(() =>
            {
                folder.IsBusy = false;
            });

            if (folder.Children.Count > 0)
            {
                tmpList = GetAllFeedIdsFromChildNodes(folder.Children);
            }

            if (tmpList.Count == 0)
            {
                App.CurrentDispatcherQueue?.TryEnqueue(() =>
                {
                    folder.IsBusy = false;
                });
                return new List<EntryItem>();
            }

            var res = await Task.FromResult(SelectEntriesByFeedIdsLock(tmpList, folder.IsDisplayUnarchivedOnly));

            if (res.IsError)
            {
                App.CurrentDispatcherQueue?.TryEnqueue(() =>
                {
                    // TODO: check this works...

                    // show error
                    ErrorObj = res.Error;

                    if (folder == _selectedTreeViewItem)
                    {
                        IsShowFeedError = true;
                    }

                    folder.IsBusy = false;
                });


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

                    folder.IsBusy = false;
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

    // update specific feed or feeds in a folder.
    private async void UpdateFeed()
    {
        if (_selectedTreeViewItem is null)
            return;

        if ((_selectedTreeViewItem is NodeFeed) || _selectedTreeViewItem is NodeFolder)
        {
            await GetEntriesAsync(_selectedTreeViewItem);
        }
    }

    // gets entories recursively and save to db.
    private async Task GetEntriesAsync(NodeTree nt)
    {
        if (nt == null)
            return;

        if (nt is NodeFeed feed)
        {
            // check some conditions.
            if ((feed.Api != ApiTypes.atFeed) || (feed.Client == null))
                return;

            // Update Node Downloading Status
            App.CurrentDispatcherQueue?.TryEnqueue(() =>
            {
                feed.Status = NodeFeed.DownloadStatus.downloading;

                // TODO: should I be doing this here? or after receiving the data...
                feed.LastUpdate = DateTime.Now;
            });

            Debug.WriteLine("Getting Entries from: " + feed.Name);

            // Get Entries from web.
            var resEntries = await feed.Client.GetEntries(feed.EndPoint, feed.Id);

            // Check Node exists. Could have been deleted.
            if (feed == null)
                return;

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

                    if (feed.Parent != null)
                        if (feed.Parent is NodeFolder parentFolder)
                            MinusAllParentEntryCount(parentFolder, feed.EntryNewCount);
                    feed.EntryNewCount = 0;

                    // Update Node Downloading Status
                    feed.Status = NodeFeed.DownloadStatus.error;
                });

                return;
            }
            // Result is success.
            else
            {
                App.CurrentDispatcherQueue?.TryEnqueue(() =>
                {
                    // Clear Node Error
                    feed.ErrorHttp = null;
                    if (feed == SelectedTreeViewItem)
                    {
                        // Hide any Error Message
                        ErrorObj = null;
                        IsShowFeedError = false;
                    }

                    feed.Status = NodeFeed.DownloadStatus.normal;

                    feed.LastUpdate = DateTime.Now;
                });

                if (resEntries.Entries.Count > 0)
                {
                    await SaveEntryListAsync(resEntries.Entries, feed);
                }
            }
        }
        else if (nt is NodeFolder folder)
        {
            //await Task.Run(() => UpdateAllFeedsRecursiveLoop(folder.Children));
            UpdateAllFeedsRecursiveLoop(folder);
        }
    }

    // update all feeds.
    private void UpdateAllFeeds()
    {
        //Task.Run(() => UpdateAllFeedsRecursiveLoop(_services));
        UpdateAllFeedsRecursiveLoop(_services);
    }

    // update all feeds recursive loop.
    private void UpdateAllFeedsRecursiveLoop(NodeTree nt)
    {
        if (nt.Children.Count > 0)
        {
            foreach (var c in nt.Children)
            {
                if ((c is NodeEntryCollection) || (c is NodeFeed))
                {
                    if (c is NodeFeed feed)
                    {
                        
                        Task.Run(async () =>
                        {
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

                                if (list.Count > 0)
                                {
                                    await SaveEntryListAsync(list, feed).ConfigureAwait(false);

                                    await Task.Delay(100);

                                    // 
                                    CheckParentSelectedAndLoadEntriesIfNotBusy(feed);
                                }
                            }
                        });
                    }
                }

                if (c.Children.Count > 0)
                {
                    //Task.Run(() => LoadAllEntriesRecursiveLoop(c.Children));
                    UpdateAllFeedsRecursiveLoop(c);

                }
            }
        }
    }

    // gets entries from web and return the list.
    private async Task<List<EntryItem>> GetEntryListAsync(NodeFeed feed)
    {
        var res = new List<EntryItem>();

        if (feed == null) return res;

        // check some conditions.
        if ((feed.Api != ApiTypes.atFeed) || (feed.Client == null)) return res;

        // Update Node Downloading Status
        App.CurrentDispatcherQueue?.TryEnqueue(() =>
        {
            feed.IsBusy = true;

            //Debug.WriteLine("Getting entries: " + feed.Name);

            feed.Status = NodeFeed.DownloadStatus.downloading;

            // TODO: should I be doing this here? or after receiving the data...
            feed.LastUpdate = DateTime.Now;
        });

        // Get Entries from web.
        var resEntries = await feed.Client.GetEntries(feed.EndPoint, feed.Id);

        // Check Node exists. Could have been deleted... but unlikely...
        if (feed == null)
        {
            //feed.IsBusy = false;
            return res;
        }

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

                if (feed.Parent != null)
                    if (feed.Parent is NodeFolder parentFolder)
                        MinusAllParentEntryCount(parentFolder, feed.EntryNewCount);
                feed.EntryNewCount = 0;

                // Update Node Downloading Status
                feed.Status = NodeFeed.DownloadStatus.error;

                feed.IsBusy = false;
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

                feed.IsBusy = false;
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

    // save them to database.
    private async Task<List<EntryItem>> SaveEntryListAsync(List<EntryItem> list, NodeFeed feed)
    {
        var res = new List<EntryItem>();

        if (list.Count == 0)
            return res;

        // Update Node Downloading Status
        App.CurrentDispatcherQueue?.TryEnqueue(() =>
        {
            feed.IsBusy = true;

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

                feed.IsBusy = false;
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

                    //feed.EntryNewCount += newItems.Count;
                    UpdateNewEntryCount(feed, newItems.Count);

                    if (feed.Status != NodeFeed.DownloadStatus.error)
                        feed.Status = NodeFeed.DownloadStatus.normal;


                    feed.IsBusy = false;
                });

                // If selected
                if (feed == SelectedTreeViewItem)
                {
                    await LoadEntriesAsync(feed);
                }
                else
                {
                    //CheckParentSelectedAndLoadEntriesIfNotBusy(feed);
                }
            }
            else
            {
                // Update Node Downloading Status
                App.CurrentDispatcherQueue?.TryEnqueue(() =>
                {
                    //feed.EntryCount = newItems.Count;

                    if (feed.Status != NodeFeed.DownloadStatus.error)
                        feed.Status = NodeFeed.DownloadStatus.normal;


                    feed.IsBusy = false;
                });
            }

            return resInsert.InsertedEntries;
        }
    }

    private void UpdateNewEntryCount(NodeFeed feed, int newCount)
    {
        feed.EntryNewCount = newCount;

        if (newCount > 0)
        {
            if (feed.Parent is NodeFolder folder)
            {
                UpdateParentNewEntryCount(folder, newCount);
            }
        }
        /*
        App.CurrentDispatcherQueue?.TryEnqueue(() =>
        {

        });
        */
    }

    private void UpdateParentNewEntryCount(NodeFolder folder, int newCount)
    {
        if (folder != null)
        {
            folder.EntryNewCount += newCount;

            if (folder.Parent is NodeFolder parentFolder)
            {
                UpdateParentNewEntryCount(parentFolder, newCount);
            }
        }
        /*
        App.CurrentDispatcherQueue?.TryEnqueue(() =>
        {

        });
        */
    }

    private async void CheckParentSelectedAndLoadEntriesIfNotBusy(NodeTree nt)
    {
        if (nt != null)
        {
            if (nt.Parent is NodeFolder parentFolder)
            {
                if (parentFolder == SelectedTreeViewItem)
                {
                    if (parentFolder.IsBusyChildrenCount <= 0)
                    {
                        await LoadEntriesAsync(parentFolder);
                    }
                }
                else
                {
                    CheckParentSelectedAndLoadEntriesIfNotBusy(parentFolder);
                }

            }
        }
    }

    #endregion

    #region == Entries Archiving ==

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
                        if (feed.Parent != null)
                        {
                            if (feed.Parent is NodeFolder parentFolder)
                                MinusAllParentEntryCount(parentFolder, feed.EntryNewCount);
                        }
                        feed.EntryNewCount = 0;

                        if (feed == _selectedTreeViewItem)
                        {
                            //DatabaseError = null;
                            //IsShowDatabaseErrorMessage = false;

                            // clear here.
                            Entries.Clear();
                            
                            // showing all entries. so realod.
                            if (!feed.IsDisplayUnarchivedOnly)
                            {
                                Task nowait = Task.Run(() => LoadEntriesAsync(_selectedTreeViewItem));
                            } 
                            //Task.Run(() => LoadEntries(_selectedNode));
                        }
                    }

                    //IsWorking = false;
                    EntryArchiveAllCommand.NotifyCanExecuteChanged();
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
                        {
                            Entries.Clear();

                            if (!folder.IsDisplayUnarchivedOnly)
                            {
                                Task nowait = Task.Run(() => LoadEntriesAsync(_selectedTreeViewItem));
                            }
                        }
                    });
                }
            }
            App.CurrentDispatcherQueue?.TryEnqueue(() =>
            {
                //IsWorking = false;

                EntryArchiveAllCommand.NotifyCanExecuteChanged();
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

    private void MinusAllParentEntryCount(NodeFolder folder, int minusCount)
    {
        if (folder is NodeFolder)
        {
            folder.EntryNewCount -= minusCount;

            if (folder.Parent != null)
            {
                if (folder.Parent is NodeFolder parentFolder)
                {
                    MinusAllParentEntryCount(parentFolder, minusCount);
                }
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

    #region == Feed Treeview commands ==

    [RelayCommand]
    private void FeedAdd() => _navigationService.NavigateTo(typeof(FeedAddViewModel).FullName!, null);

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

    [RelayCommand(CanExecute = nameof(CanNodeEdit))]
    private void NodeEdit()
    {
        if (SelectedTreeViewItem is null)
            return;

        if (SelectedTreeViewItem is NodeFeed)
        {
            _navigationService.NavigateTo(typeof(FeedEditViewModel).FullName!, SelectedTreeViewItem);
        }
        else if (SelectedTreeViewItem is NodeFolder)
        {
            _navigationService.NavigateTo(typeof(FolderEditViewModel).FullName!, SelectedTreeViewItem);
        }
    }

    private bool CanNodeEdit()
    {
        return SelectedTreeViewItem is not null;
    }

    [RelayCommand(CanExecute = nameof(CanFolderAdd))]
    private void FolderAdd()
    {
        NodeTree? targetNode = null;

        if (SelectedTreeViewItem is null) 
        {
            targetNode = _services;
        }
        else if (SelectedTreeViewItem is NodeFeed feed)
        {
            if (feed.Parent != null)
            {
                targetNode = feed.Parent;
            }
        }
        else if (SelectedTreeViewItem is NodeFolder folder)
        {
            if (folder != null)
            {
                targetNode = folder;
            }
        }

        if (targetNode is not null)
        {
            _navigationService.NavigateTo(typeof(FolderAddViewModel).FullName!, targetNode);
        }
    }

    private bool CanFolderAdd()
    {
        return true;
    }

    [RelayCommand(CanExecute = nameof(CanFeedRemove))]
    private void FeedRemove()
    {
        if (SelectedTreeViewItem is null)
            return;

        DeleteNodeTree(SelectedTreeViewItem);

        SaveServiceXml();
    }

    private async void DeleteNodeTree(NodeTree nt)
    {
        if (nt.IsBusy)
        {
            // TODO: let users know.
            Debug.WriteLine("DeleteNodeTree: IsBusy.");
            return;
        }

        if (!((nt is NodeFolder) || (nt is NodeFeed)))
        {
            return;
        }

        if (nt is NodeFeed feed)
        {
            App.CurrentDispatcherQueue?.TryEnqueue(() =>
            {
                // check status
                if (!((feed.Status == NodeFeed.DownloadStatus.normal) || (feed.Status == NodeFeed.DownloadStatus.error)))
                {
                    return;
                }

                feed.IsBusy = true;
            });

            List<string> ids = new()
                {
                    feed.Id
                };

            SqliteDataAccessResultWrapper resDelete = await DeleteEntriesByFeedIdsLock(ids);

            if (resDelete.IsError)
            {
                App.CurrentDispatcherQueue?.TryEnqueue(() =>
                {
                    feed.ErrorDatabase = resDelete.Error;

                    if (feed == _selectedTreeViewItem)
                    {
                        ErrorObj = feed.ErrorDatabase;
                        IsShowFeedError = true;
                    }

                    feed.IsBusy = false;

                    return;
                });

            }
            else
            {
                App.CurrentDispatcherQueue?.TryEnqueue(() =>
                {
                    if (feed.Parent != null)
                    {
                        feed.Parent.Children.Remove(feed);
                        feed.IsBusy = false; // remove self from parent IsBusyChildrenCount
                    }
                });
            }
        }
        else if (nt is NodeFolder folder)
        {
            if (folder.Children.Count > 0)
            {
                foreach (var ndc in folder.Children)
                {
                    DeleteNodeTree(ndc);
                }
            }

            // making sure folder is empty.
            if (folder.Children.Count == 0)
            {
                if (folder.Parent != null)
                {
                    folder.Parent.Children.Remove(nt);
                }
            }
        }
    }

    private bool CanFeedRemove()
    {
        return SelectedTreeViewItem is not null;
    }

    [RelayCommand(CanExecute = nameof(CanFeedRefreshAll))]
    private void FeedRefreshAll()
    {
        UpdateAllFeeds();
    }

    private bool CanFeedRefreshAll()
    {
        return Services.Count > 0;

    }

    [RelayCommand(CanExecute = nameof(CanFeedRefresh))]
    private void FeedRefresh()
    {
        UpdateFeed();
    }

    private bool CanFeedRefresh()
    {
        return SelectedTreeViewItem is not null;
    }

    #endregion

    #region == Feed OPML ex/import commands ==

    [RelayCommand(CanExecute = nameof(CanOpmlImport))]
    public async Task OpmlImportAsync()
    {
        var file = await _fileDialogService.GetOpenOpmlFileDialog();
        if (file is null)
            return;

        var filepath = file.Path;

        if (!File.Exists(filepath.Trim()))
            return;

        var doc = new XmlDocument();
        try
        {
            doc.Load(filepath.Trim());

            //MainError = null;
        }
        catch (Exception e)
        {
            /*
            MainError = new ErrorObject();
            MainError.ErrType = ErrorObject.ErrTypes.Other;
            MainError.ErrCode = "";
            MainError.ErrText = e.ToString();
            MainError.ErrDescription = e.Message;
            MainError.ErrDatetime = DateTime.Now;
            MainError.ErrPlace = "OpmlImportCommand_Execute.LoadXmlDoc";
            MainError.ErrPlaceParent = "MainViewModel()";

            IsShowMainErrorMessage = true;
            */
            Debug.WriteLine("OpmlImportAsync: " + e);
            return;
        }

        Opml opmlLoader = new();

        NodeFolder dummyFolder = opmlLoader.LoadOpml(doc);
        if (dummyFolder is not null)
        {
            /*
            if (dummyFolder.Children.Count > 0)
            {
                foreach (var nt in dummyFolder.Children)
                {
                    nt.Parent = _services;
                    Services.Add(nt);
                }
            }
            */
            Services.Insert(0, dummyFolder);
        }

        SaveServiceXml();
    }

    private bool CanOpmlImport()
    {
        return true;
    }

    [RelayCommand(CanExecute = nameof(CanOpmlExport))]
    public async void OpmlExport()
    {
        Opml opmlWriter = new();

        XmlDocument xdoc = opmlWriter.WriteOpml(_services);

        if (xdoc is null)
        {
            // TODO: show error?
            Debug.WriteLine("xdoc is null");
            return;
        }

        var file = await _fileDialogService.GetSaveOpmlFileDialog();

        if (file is null)
        {
            // canceled or something.
            return;
        }

        if (!string.IsNullOrEmpty(file.Path))
        {
            xdoc.Save(file.Path.Trim());
        }
    }

    private bool CanOpmlExport()
    {
        return true;
    }

    #endregion

    #region == Entry Listview commands  ==

    [RelayCommand(CanExecute = nameof(CanEntryArchiveAll))]
    private void EntryArchiveAll()
    {
        if (SelectedTreeViewItem is null)
            return;

        if (!((SelectedTreeViewItem is NodeFeed) || (SelectedTreeViewItem is NodeFolder)))
            return;

        Task.Run(() => ArchiveAllAsync(SelectedTreeViewItem));
    }

    private bool CanEntryArchiveAll()
    {
        if (SelectedTreeViewItem is null)
            return false;

        if (!((SelectedTreeViewItem is NodeFeed) || (SelectedTreeViewItem is NodeFolder)))
            return false;

        if (SelectedTreeViewItem.EntryNewCount <= 0)
            return false;

        if (_entries.Count <= 0)
            return false;

        return true;
    }

    [RelayCommand(CanExecute = nameof(CanEntryViewInternal))]
    private void EntryViewInternal()
    {
        if (SelectedListViewItem is not null)
            _navigationService.NavigateTo(typeof(EntryDetailsViewModel).FullName!, SelectedListViewItem);
    }
    private bool CanEntryViewInternal()
    {
        return SelectedListViewItem is not null;
    }

    [RelayCommand(CanExecute = nameof(CanEntryViewExternal))]
    private async void EntryViewExternal()
    {
        if (SelectedListViewItem is not null)
        {
            if (SelectedListViewItem.AltHtmlUri is not null)
            {
                await Windows.System.Launcher.LaunchUriAsync(SelectedListViewItem.AltHtmlUri);
            }
        }
    }

    private bool CanEntryViewExternal()
    {
        if (SelectedListViewItem is null)
            return false;

        if (SelectedListViewItem.AltHtmlUri is null)
            return false;

        return true;
    }

    [RelayCommand(CanExecute = nameof(CanToggleShowAllEntries))]
    private void ToggleShowAllEntries()
    {
        if (SelectedTreeViewItem is null)
            return;

        IsShowAllEntries = !IsShowAllEntries;
        
        if (SelectedTreeViewItem is NodeFeed feed)
        {
            feed.IsDisplayUnarchivedOnly = !IsShowAllEntries;
            Task nowait = Task.Run(() => LoadEntriesAsync(_selectedTreeViewItem));
        }
        else if (SelectedTreeViewItem is NodeFolder folder)
        {
            folder.IsDisplayUnarchivedOnly = !IsShowAllEntries;
            Task nowait = Task.Run(() => LoadEntriesAsync(_selectedTreeViewItem));
        }
    }

    private bool CanToggleShowAllEntries()
    {
        if (SelectedTreeViewItem is null)
            return false;

        return true;
    }

    [RelayCommand(CanExecute = nameof(CanToggleShowInboxEntries))]
    private void ToggleShowInboxEntries()
    {
        if (SelectedTreeViewItem is null)
            return;

        IsShowInboxEntries = !IsShowInboxEntries;

        if (SelectedTreeViewItem is NodeFeed feed)
        {
            feed.IsDisplayUnarchivedOnly = IsShowInboxEntries;
            Task nowait = Task.Run(() => LoadEntriesAsync(_selectedTreeViewItem));
        }
        else if (SelectedTreeViewItem is NodeFolder folder)
        {
            folder.IsDisplayUnarchivedOnly = IsShowInboxEntries;
            Task nowait = Task.Run(() => LoadEntriesAsync(_selectedTreeViewItem));
        }
    }

    private bool CanToggleShowInboxEntries()
    {
        if (SelectedTreeViewItem is null)
            return false;

        return true;
    }

    #endregion

    #region == Other command methods ==

    [RelayCommand]
    private void DetailsPaneShowHide()
    {
        IsEntryDetailPaneVisible = !IsEntryDetailPaneVisible;
    }

    #endregion
}
