using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Media;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Input;
using System.IO;
using System.ComponentModel;
using BlogWrite.Common;
using BlogWrite.Models;
using BlogWrite.Models.Clients;

namespace BlogWrite.ViewModels
{
    /// TODO: 
    /// 
    /// SearviceDiscoveryをMSHTML使わずにパースして動くようにする。
    /// 

    /// 更新履歴：
    /// v0.0.0.3 とりあえず、HTML取得、解析、RSS/AtomのFeed検出、登録、表示までの流れは出来た。
    /// v0.0.0.2 色々。
    /// v0.0.0.1 3年前の作りかけの状態を少なくとも最新の環境にあわせてアップデート。


    public class MainViewModel : ViewModelBase
    {
        // Application name
        const string _appName = "BlogWrite";

        // Application version
        const string _appVer = "0.0.0.3";
        public string AppVer
        {
            get
            {
                return _appVer;
            }
        }

        // Application config file folder
        const string _appDeveloper = "torum";

        // Application Window Title
        public string AppTitle
        {
            get
            {
                return _appName + " " + _appVer;
            }
        }

        private string _envDataFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private string _appDataFolder;
        private string _appConfigFilePath;

        #region == Properties ==

        #region == Treeview, Node, Menu, etc ==

        private ServiceTreeBuilder _services = new ServiceTreeBuilder();
        public ObservableCollection<NodeTree> Services
        {
            get { return _services.Children; }
            set
            {
                _services.Children = value;
                NotifyPropertyChanged(nameof(Services));
            }
        }

        private NodeTree _selectedNode = new NodeService("", "", "",new Uri("http://127.0.0.1"), ApiTypes.atUnknown, ServiceTypes.Unknown);
        public NodeTree SelectedNode
        {
            get { return _selectedNode; }
            set
            {
                if (_selectedNode == value)
                    return;

                _selectedNode = value;

                NotifyPropertyChanged(nameof(SelectedNode));

                ClientErrorMessage = "";
                IsShowClientErrorMessage = false;

                if (_selectedNode == null)
                    return;

                if (_selectedNode is NodeEntryCollection)
                {
                    if ((_selectedNode as NodeEntryCollection).List.Count == 0)
                    {
                        Task.Run(() => GetEntries((_selectedNode as NodeEntryCollection)));
                    }
                }
                else if (_selectedNode is NodeFeed)
                {
                    if ((_selectedNode as NodeFeed).List.Count == 0)
                    {
                        Task.Run(() => GetEntries((_selectedNode as NodeFeed)));
                    }
                }

                // This changes the listview.
                NotifyPropertyChanged(nameof(Entries));

                // todo: check loaded.
                // Clear preview browser.
                if (Application.Current == null) { return; }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    WriteHtmlToContentPreviewBrowser?.Invoke(this, WrapHtmlContent(""));
                });
            }
        }

        public ObservableCollection<EntryItem> Entries
        {
            get
            {
                if (_selectedNode == null)
                    return null;

                if (_selectedNode is NodeEntryCollection)
                {
                    return (_selectedNode as NodeEntryCollection).List;
                }
                else if (_selectedNode is NodeFeed)
                {
                    return (_selectedNode as NodeFeed).List;
                }
                /*
                else if (_selectedNode is NodeRssFeed)
                {
                    return (_selectedNode as NodeRssFeed).List;
                }
                */
                else
                {
                    return null;
                }
            }
        }

        private EntryItem _selectedItem = null;
        public EntryItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem == value)
                    return;

                _selectedItem = value;
                NotifyPropertyChanged(nameof(SelectedItem));

                // This changes the contents.
                NotifyPropertyChanged(nameof(Entry));
                NotifyPropertyChanged(nameof(EntryHTML));
                NotifyPropertyChanged(nameof(IsContentText));
                NotifyPropertyChanged(nameof(IsContentHTML));

                if (_selectedItem == null)
                    return;

                if (IsContentHTML)
                {
                    if (Application.Current == null) { return; }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        WriteHtmlToContentPreviewBrowser?.Invoke(this, EntryHTML);
                    });
                }


            }
        }

        public bool IsContentText
        {
            get
            {
                if (_selectedItem == null)
                    return false;

                if (!(_selectedItem is EntryItem))
                    return false;

                if ((_selectedItem as EntryItem).EntryBody == null)
                    return false;

                if ((_selectedItem as EntryItem).EntryBody.ContentType == EntryFull.ContentTypes.text)
                {
                    // Debug.WriteLine("IsContentText");
                    return true;
                }

                return false;
            }
        }

        public bool IsContentHTML
        {
            get
            {
                if (_selectedItem == null)
                    return false;

                if (!(_selectedItem is EntryItem))
                    return false;

                if ((_selectedItem as EntryItem).EntryBody == null)
                    return false;

                if ((_selectedItem as EntryItem).EntryBody.ContentType == EntryFull.ContentTypes.textHtml)
                {
                    //Debug.WriteLine("IsContentHTML");
                    return true;
                }

                return false;
            }
        }

        public string Entry
        {
            get
            {
                if (_selectedItem == null)
                    return null;

                if (_selectedItem is EntryItem)
                {
                    if ((_selectedItem as EntryItem).EntryBody != null)
                    {
                        return (_selectedItem as EntryItem).EntryBody.Content;
                        /*
                        if ((_selectedItem as EntryItem).EntryBody.ContentType == EntryFull.ContentTypes.textHtml)
                        {
                            return null;
                        }
                        else
                        {
                            return (_selectedItem as EntryItem).EntryBody.Content;
                        }
                        */
                    }
                    else
                    {
                        // For testing only.
                        /*
                        Task.Run(async () => {
                            bool b = await this.GetEntry(_selectedItem as EntryItem);
                            if (b)
                            {
                                NotifyPropertyChanged(nameof(Entry));
                                NotifyPropertyChanged(nameof(EntryHTML));
                                NotifyPropertyChanged(nameof(IsContentText));
                                NotifyPropertyChanged(nameof(IsContentHTML));
                            }
                        });
                        */
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public string EntryHTML
        {
            get
            {
                if (_selectedItem == null)
                    return WrapHtmlContent("");

                if (_selectedItem is EntryItem)
                {
                    if ((_selectedItem as EntryItem).EntryBody != null)
                    {
                        if ((_selectedItem as EntryItem).EntryBody.ContentType == EntryFull.ContentTypes.textHtml)
                        {

                            //System.Diagnostics.Debug.WriteLine(WrapHtmlContent((_selectedItem as EntryItem).EntryBody.Content));

                            return WrapHtmlContent((_selectedItem as EntryItem).EntryBody.Content);
                        }
                        else
                        {
                            return WrapHtmlContent("");
                        }
                    }
                    else
                    {
                        /*
                        Task.Run(async () => {
                            bool b = await GetEntry(_selectedItem as EntryItem);
                            if (b)
                            {
                                NotifyPropertyChanged(nameof(Entry));
                                NotifyPropertyChanged(nameof(EntryHTML));
                                NotifyPropertyChanged(nameof(IsContentText));
                                NotifyPropertyChanged(nameof(IsContentHTML));
                            }
                        });
                        */
                        return WrapHtmlContent("");
                    }
                }
                else
                {
                    return WrapHtmlContent("");
                }
            }
        }

        private static string WrapHtmlContent(string source, string styles = null)
        {
            if (styles == null)
            {
                styles = @"
::-webkit-scrollbar { width: 18px; height: 3px;}
::-webkit-scrollbar-button {  background-color: #666; }
::-webkit-scrollbar-track {  background-color: #646464; box-shadow: 0 0 4px #aaa inset;}
::-webkit-scrollbar-track-piece { background-color: #212121;}
::-webkit-scrollbar-thumb { height: 50px; background-color: #666;}
::-webkit-scrollbar-corner { background-color: #646464;}}
::-webkit-resizer { background-color: #666;}

body {
	
	line-height: 1.75em;
	font-size: 12px;
	background-color: #222;
	color: #aaa;
}

p {
	font-size: 12px;
}

h1 {
	font-size: 30px;
	line-height: 34px;
}

h2 {
	font-size: 20px;
	line-height: 25px;
}

h3 {
	font-size: 16px;
	line-height: 27px;
	padding-top: 15px;
	padding-bottom: 15px;
	border-bottom: 1px solid #D8D8D8;
	border-top: 1px solid #D8D8D8;
}

hr {
	height: 1px;
	background-color: #d8d8d8;
	border: none;
	width: 100%;
	margin: 0px;
}

a[href] {
	color: #1e8ad6;
}

a[href]:hover {
	color: #3ba0e6;
}

img {
    width: 160;
    height: auto;
    float: left;
    margin: 6px 12px 12px 6px;
}

li {
	line-height: 1.5em;
}
                ";
            }

            return String.Format(
                @"<html>
                    <head>
                        <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />

                        <!-- saved from url=(0014)about:internet -->

                        <style type='text/css'>
                            body {{ font: 10pt verdana; color: #101010; background: #cccccc; }}
                            table, td, th, tr {{ border: 1px solid black; border-collapse: collapse; }}
                        </style>

                        <!-- Custom style sheet -->
                        <style type='text/css'>{1}</style>
                    </head>
                    <body>{0}</body>
                </html>",
                source, styles);
        }

        #endregion

        #region == etc ==

        private bool _isFullyLoaded;
        public bool IsFullyLoaded
        {
            get
            {
                return _isFullyLoaded;
            }
            set
            {
                if (_isFullyLoaded == value)
                    return;

                _isFullyLoaded = value;
                this.NotifyPropertyChanged("IsFullyLoaded");
            }
        }
        
        private bool _isBusy;
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                if (_isBusy == value)
                    return;

                _isBusy = value;
                NotifyPropertyChanged("IsBusy");

                if (Application.Current == null) { return; }
                Application.Current.Dispatcher.Invoke(() => CommandManager.InvalidateRequerySuggested());
            }
        }

        private bool _isWorking;
        public bool IsWorking
        {
            get
            {
                return _isWorking;
            }
            set
            {
                if (_isWorking == value)
                    return;

                _isWorking = value;
                NotifyPropertyChanged("IsWorking");

                if (Application.Current == null) { return; }
                Application.Current.Dispatcher.Invoke(() => CommandManager.InvalidateRequerySuggested());
            }
        }

        private bool _isShowDebugWindow;
        public bool IsShowDebugWindow

        {
            get { return _isShowDebugWindow; }
            set
            {
                if (_isShowDebugWindow == value)
                    return;

                _isShowDebugWindow = value;

                NotifyPropertyChanged("IsShowDebugWindow");

                if (_isShowDebugWindow)
                {
                    if (Application.Current == null) { return; }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DebugWindowShowHide?.Invoke();
                    });
                }
            }
        }
        
        private string _statusBarMessage;
        public string StatusBarMessage
        {
            get { return _statusBarMessage; }
            set
            {
                if (_statusBarMessage == value)
                    return;

                _statusBarMessage = value;

                NotifyPropertyChanged(nameof(StatusBarMessage));
            }
        }

        private bool _isShowClientErrorMessage;
        public bool IsShowClientErrorMessage
        {
            get { return _isShowClientErrorMessage; }
            set
            {
                if (_isShowClientErrorMessage == value)
                    return;

                _isShowClientErrorMessage = value;

                NotifyPropertyChanged(nameof(IsShowClientErrorMessage));
            }
        }

        private string _clientErrorMessage;
        public string ClientErrorMessage
        {
            get { return _clientErrorMessage; }
            set
            {
                if (_clientErrorMessage == value)
                    return;

                _clientErrorMessage = value;

                NotifyPropertyChanged(nameof(ClientErrorMessage));
            }
        }

        #endregion

        #endregion

        #region == Events ==

        public event EventHandler<ServiceDiscoveryEventArgs> OpenServiceDiscoveryView;
        public event EventHandler<BlogEntryEventArgs> OpenEditorView;
        public event EventHandler<BlogEntryEventArgs> OpenEditorNewView;

        // DebugWindow
        public delegate void DebugWindowShowHideEventHandler();
        public event DebugWindowShowHideEventHandler DebugWindowShowHide;

        public event EventHandler<string> DebugOutput;

        public delegate void DebugClearEventHandler();
        public event DebugClearEventHandler DebugClear;

        public event EventHandler<string> WriteHtmlToContentPreviewBrowser;

        #endregion

        public MainViewModel()
        {
            // データ保存フォルダの取得
            _appDataFolder = _envDataFolder + System.IO.Path.DirectorySeparatorChar + _appDeveloper + System.IO.Path.DirectorySeparatorChar + _appName;
            // 設定ファイルのパス
            _appConfigFilePath = _appDataFolder + System.IO.Path.DirectorySeparatorChar + _appName + ".config";
            // 存在していなかったら作成
            System.IO.Directory.CreateDirectory(_appDataFolder);

            #region == Commands ==

            TreeviewLeftDoubleClickCommand = new GenericRelayCommand<NodeTree>(
                param => TreeviewLeftDoubleClickCommand_Execute(param),
                param => TreeviewLeftDoubleClickCommand_CanExecute());

            ListviewLeftDoubleClickCommand = new GenericRelayCommand<EntryItem>(
                param => ListviewLeftDoubleClickCommand_Execute(param),
                param => ListviewLeftDoubleClickCommand_CanExecute());

            OpenEditorCommand = new GenericRelayCommand<EntryItem>(
                param => OpenEditorCommand_Execute(param),
                param => OpenEditorCommand_CanExecute());

            DeleteEntryCommand = new GenericRelayCommand<EntryItem>(
                param => DeleteEntryCommand_Execute(param),
                param => DeleteEntryCommand_CanExecute());

            GetEntryCommand = new GenericRelayCommand<EntryItem>(
                param => GetEntryCommand_Execute(param),
                param => GetEntryCommand_CanExecute());

            OpenInBrowserCommand = new GenericRelayCommand<EntryItem>(
                param => OpenInBrowserCommand_Execute(param),
                param => OpenInBrowserCommand_CanExecute());

            ListviewEnterKeyCommand = new GenericRelayCommand<EntryItem>(
                param => ListviewEnterKeyCommand_Execute(param),
                param => ListviewEnterKeyCommand_CanExecute());

            ServiceAddCommand = new RelayCommand(ServiceAddCommand_Execute, ServiceAddCommand_CanExecute);
            OpenEditorAsNewCommand = new RelayCommand(OpenEditorAsNewCommand_Execute, OpenEditorAsNewCommand_CanExecute);
            RefreshEntriesCommand = new RelayCommand(RefreshEntriesCommand_Execute, RefreshEntriesCommand_CanExecute);
            ShowSettingsCommand = new RelayCommand(ShowSettingsCommand_Execute, ShowSettingsCommand_CanExecute);
            ShowDebugWindowCommand = new RelayCommand(ShowDebugWindowCommand_Execute, ShowDebugWindowCommand_CanExecute);
            ClearDebugTextCommand = new RelayCommand(ClearDebugTextCommand_Execute, ClearDebugTextCommand_CanExecute);

            #endregion

            // loads searvice tree
            if (File.Exists(_appDataFolder + System.IO.Path.DirectorySeparatorChar + "Searvies.xml"))
            {
                XmlDocument doc = new XmlDocument();
                
                doc.Load(_appDataFolder + System.IO.Path.DirectorySeparatorChar + "Searvies.xml");

                _services.LoadXmlDoc(doc);

                // subscribe to DebugOutput event.
                foreach (NodeService c in _services.Children)
                {
                    if (c.Client != null)
                    {
                        c.Client.DebugOutput += new BaseClient.ClientDebugOutput(OnDebugOutput);
                    }
                }
            }
        }

        public void OnDebugOutput(BaseClient sender, string data)
        {
            if (IsShowDebugWindow)
            {
                if (Application.Current == null) { return; }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DebugOutput?.Invoke(this, Environment.NewLine + data);
                });
            }
        }

        #region == Startup and Shutdown ==

        // 起動時の処理
        public void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            #region == アプリ設定のロード  ==

            try
            {
                // アプリ設定情報の読み込み
                if (File.Exists(_appConfigFilePath))
                {
                    XDocument xdoc = XDocument.Load(_appConfigFilePath);

                    #region == ウィンドウ関連 ==

                    if (sender is Window)
                    {
                        // Main Window element
                        var mainWindow = xdoc.Root.Element("MainWindow");
                        if (mainWindow != null)
                        {
                            var hoge = mainWindow.Attribute("top");
                            if (hoge != null)
                            {
                                (sender as Window).Top = double.Parse(hoge.Value);
                            }

                            hoge = mainWindow.Attribute("left");
                            if (hoge != null)
                            {
                                (sender as Window).Left = double.Parse(hoge.Value);
                            }

                            hoge = mainWindow.Attribute("height");
                            if (hoge != null)
                            {
                                (sender as Window).Height = double.Parse(hoge.Value);
                            }

                            hoge = mainWindow.Attribute("width");
                            if (hoge != null)
                            {
                                (sender as Window).Width = double.Parse(hoge.Value);
                            }

                            hoge = mainWindow.Attribute("state");
                            if (hoge != null)
                            {
                                if (hoge.Value == "Maximized")
                                {
                                    (sender as Window).WindowState = WindowState.Maximized;
                                }
                                else if (hoge.Value == "Normal")
                                {
                                    (sender as Window).WindowState = WindowState.Normal;
                                }
                                else if (hoge.Value == "Minimized")
                                {
                                    (sender as Window).WindowState = WindowState.Normal;
                                }
                            }

                        }

                    }

                    #endregion

                }

                IsFullyLoaded = true;
            }
            catch (System.IO.FileNotFoundException)
            {
                Debug.WriteLine("FileNotFoundException while loading config: " + _appConfigFilePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: " + ex + " while opening : " + _appConfigFilePath);
            }

            #endregion

            IsShowDebugWindow = true;
        }

        // 終了時の処理
        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            if (!IsFullyLoaded)
                return;

            #region == アプリ設定の保存 ==

            // 設定ファイル用のXMLオブジェクト
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.InsertBefore(xmlDeclaration, doc.DocumentElement);

            // Root Document Element
            XmlElement root = doc.CreateElement(string.Empty, "App", string.Empty);
            doc.AppendChild(root);

            XmlAttribute attrs = doc.CreateAttribute("Version");
            attrs.Value = _appVer;
            root.SetAttributeNode(attrs);

            #region == ウィンドウ関連 ==

            if (sender is Window)
            {
                // Main Window element
                XmlElement mainWindow = doc.CreateElement(string.Empty, "MainWindow", string.Empty);

                // Main Window attributes
                attrs = doc.CreateAttribute("height");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Height.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Height.ToString();
                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("width");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Width.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Width.ToString();

                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("top");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Top.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Top.ToString();
                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("left");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Left.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Left.ToString();
                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("state");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = "Maximized";
                }
                else if ((sender as Window).WindowState == WindowState.Normal)
                {
                    attrs.Value = "Normal";

                }
                else if ((sender as Window).WindowState == WindowState.Minimized)
                {
                    attrs.Value = "Minimized";
                }
                mainWindow.SetAttributeNode(attrs);


                // set Main Window element to root.
                root.AppendChild(mainWindow);

            }

            #endregion

            try
            {
                // 設定ファイルの保存
                doc.Save(_appConfigFilePath);
            }
            //catch (System.IO.FileNotFoundException) { }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: " + ex + " while saving : " + _appConfigFilePath);
            }

            #endregion

            #region == Services ==

            XmlDocument xdoc = _services.AsXmlDoc();
            xdoc.Save(_appDataFolder + System.IO.Path.DirectorySeparatorChar + "Searvies.xml");

            #endregion
        }

        #endregion

        #region == Methods ==

        public void AddFeed(FeedLink fl)
        {
            foreach (var serv in Services)
            {
                if (serv is NodeFeed)
                {
                    if ((serv as NodeFeed).EndPoint.AbsoluteUri == fl.FeedUri.AbsoluteUri)
                        return;
                }
            }
            
            if (fl.FeedKind == FeedLink.FeedKinds.Atom)
            {
                NodeAtomFeed a = new(fl.Title, fl.FeedUri);
                a.Api = ApiTypes.atAtomFeed;
                a.ServiceType = ServiceTypes.Feed;

                a.Client.DebugOutput += new BaseClient.ClientDebugOutput(OnDebugOutput);

                // Add Account Node to internal (virtual) Treeview.
                Application.Current.Dispatcher.Invoke(() => Services.Add(a));
            }
            else if (fl.FeedKind == FeedLink.FeedKinds.Rss)
            {
                NodeRssFeed a = new(fl.Title, fl.FeedUri);
                a.Api = ApiTypes.atRssFeed;
                a.ServiceType = ServiceTypes.Feed;

                a.Client.DebugOutput += new BaseClient.ClientDebugOutput(OnDebugOutput);

                // Add Account Node to internal (virtual) Treeview.
                Application.Current.Dispatcher.Invoke(() => Services.Add(a));
            }

        }

        private async void GetEntries(NodeTree selectedNode)
        {
            if (selectedNode == null)
                return;

            if (selectedNode is NodeFeed)
            {
                if (((selectedNode as NodeFeed).Api != ApiTypes.atRssFeed) && (selectedNode as NodeFeed).Api != ApiTypes.atAtomFeed)
                    return;

                var fc = (selectedNode as NodeFeed).Client;
                if (fc == null)
                    return;

                List<EntryItem> entLi = await fc.GetEntries((selectedNode as NodeFeed).EndPoint);

                if (string.IsNullOrEmpty(fc.ClientErrorMessage))
                {
                    ClientErrorMessage = "";
                    IsShowClientErrorMessage = false;
                }
                else
                {
                    ClientErrorMessage = fc.ClientErrorMessage;
                    IsShowClientErrorMessage = true;
                }

                // Minimize the time to block UI thread.
                Application.Current.Dispatcher.Invoke(() =>
                {
                    (selectedNode as NodeFeed).List.Clear();

                    foreach (EntryItem ent in entLi)
                    {
                        //ent.NodeEntry = (selectedNode as NodeEntry);

                        (selectedNode as NodeFeed).List.Add(ent);
                    }
                });
            }
            else if (selectedNode is NodeEntryCollection)
            {
                var bc = (selectedNode as NodeEntryCollection).Client;
                if (bc == null)
                    return;

                List<EntryItem> entLi = await bc.GetEntries((selectedNode as NodeEntryCollection).Uri);
                if (entLi == null)
                    return;

                if (string.IsNullOrEmpty(bc.ClientErrorMessage))
                {
                    ClientErrorMessage = "";
                    IsShowClientErrorMessage = false;
                }
                else
                {
                    ClientErrorMessage = bc.ClientErrorMessage;
                    IsShowClientErrorMessage = true;
                }

                // Minimize the time to block UI thread.
                Application.Current.Dispatcher.Invoke(() =>
                {
                    (selectedNode as NodeEntryCollection).List.Clear();

                    foreach (EntryItem ent in entLi)
                    {
                        ent.NodeEntry = (selectedNode as NodeEntryCollection);

                        (selectedNode as NodeEntryCollection).List.Add(ent);
                    }
                });
            }

            // TODO: dupe. (This updates listview)
            NotifyPropertyChanged(nameof(Entries));

        }

        private async Task<bool> GetEntry(EntryItem selectedEntry)
        {
            if (selectedEntry == null)
                return false;

            if (!(_selectedNode is NodeEntryCollection))
                return false;

            BlogClient bc = selectedEntry.Client as BlogClient;
            if (bc == null)
                return false;

            if (selectedEntry.EditUri == null)
                return false;

            EntryFull bfe = await bc.GetFullEntry(selectedEntry.EditUri, selectedEntry.EntryID);

            if (selectedEntry == null)
                return false;

            selectedEntry.EntryBody = bfe;

            return true;
        }

        private async Task<bool> DeleteEntry(EntryItem selectedEntry)
        {
            if (selectedEntry == null)
                return false;

            BlogClient bc = selectedEntry.Client as BlogClient;

            if (bc == null)
                return false;

            if (selectedEntry.EditUri == null)
                return false;

            bool b = await bc.DeleteEntry(selectedEntry.EditUri);

            return b;
        }

        #endregion

        #region == ICommands ==

        public ICommand TreeviewLeftDoubleClickCommand { get; }

        public bool TreeviewLeftDoubleClickCommand_CanExecute()
        {
            return true;
        }

        public void TreeviewLeftDoubleClickCommand_Execute(NodeTree selectedNode)
        {
            if (selectedNode == null)
                return;

            selectedNode.Expanded = selectedNode.Expanded ? false : true;

        }

        public ICommand ListviewLeftDoubleClickCommand { get; }

        public bool ListviewLeftDoubleClickCommand_CanExecute()
        {
            return true;
        }

        public void ListviewLeftDoubleClickCommand_Execute(EntryItem selectedEntry)
        {
            if (SelectedNode == null)
                return;
            if (selectedEntry == null)
                return;

            if (SelectedNode is NodeFeed)
            {
                if (OpenInBrowserCommand_CanExecute())
                    OpenInBrowserCommand_Execute(selectedEntry);
            }
            else if (SelectedNode is NodeEntryCollection)
                if (OpenEditorCommand_CanExecute())
                    OpenEditorCommand.Execute(selectedEntry);
        }

        public ICommand ListviewEnterKeyCommand { get; }

        public bool ListviewEnterKeyCommand_CanExecute()
        {
            return true;
        }

        public void ListviewEnterKeyCommand_Execute(EntryItem selectedEntry)
        {
            if (SelectedNode == null)
                return;
            if (selectedEntry == null)
                return;

            if (SelectedNode is NodeFeed)
            {
                if (OpenInBrowserCommand_CanExecute())
                    OpenInBrowserCommand_Execute(selectedEntry);
            }
            else if (SelectedNode is NodeEntryCollection)
                if (OpenEditorCommand_CanExecute())
                    OpenEditorCommand.Execute(selectedEntry);
        }

        public ICommand OpenEditorCommand { get; }

        public bool OpenEditorCommand_CanExecute()
        {
            if (SelectedNode == null) return false;
            return (SelectedNode is NodeEntryCollection) ? true : false;
        }

        public void OpenEditorCommand_Execute(EntryItem selectedEntry)
        {
            if (selectedEntry == null)
                return;

            if (selectedEntry is EntryItem)
            {
                if (selectedEntry.EntryBody == null)
                    return;

                if (selectedEntry.Client == null)
                    return;

                BlogEntryEventArgs ag = new BlogEntryEventArgs
                {
                    Entry = selectedEntry.EntryBody
                    //
                };

                OpenEditorView?.Invoke(this, ag);
            }


        }

        public ICommand DeleteEntryCommand { get; }

        public bool DeleteEntryCommand_CanExecute()
        {
            if (SelectedNode == null) return false;
            return (SelectedNode is NodeEntryCollection) ? true : false;
        }

        public void DeleteEntryCommand_Execute(EntryItem selectedEntry)
        {
            if (selectedEntry == null)
                return;

            if (selectedEntry is EntryItem)
            {
                if (selectedEntry.Client == null)
                    return;

                if (selectedEntry is EntryItem)
                {
                    Task.Run(async () => {
                        bool b = await this.DeleteEntry(selectedEntry); ;
                        if (b)
                        {
                            if (selectedEntry.NodeEntry == null)
                                return;

                            // remove item from the list.
                            try
                            {
                                Application.Current.Dispatcher.Invoke(() => selectedEntry.NodeEntry.List.Remove(selectedEntry));
                            }
                            catch (Exception e)
                            {
                                System.Diagnostics.Debug.WriteLine("Error @NodeEntry.List.Remove" + e.Message);
                            }

                            NotifyPropertyChanged(nameof(Entries));
                        }
                    });
                }
            }

        }

        public ICommand GetEntryCommand { get; }

        public bool GetEntryCommand_CanExecute()
        {
            if (SelectedNode == null) return false;
            return (SelectedNode is NodeEntryCollection) ? true : false;
        }

        public void GetEntryCommand_Execute(EntryItem selectedEntry)
        {
            if (selectedEntry == null)
                return;

            if (selectedEntry is EntryItem)
            {
                if (selectedEntry.Client == null)
                    return;

                if (selectedEntry is EntryItem)
                {
                    Task.Run(async () => {
                        bool b = await this.GetEntry(selectedEntry); ;
                        if (b)
                        {
                            if (selectedEntry.NodeEntry == null)
                                return;

                            if (selectedEntry == SelectedItem)
                            {
                                NotifyPropertyChanged(nameof(SelectedItem));

                                //System.Diagnostics.Debug.WriteLine("GetEntryCommand_Execute.");
                            }

                        }
                    });
                }
            }

        }

        public ICommand OpenEditorAsNewCommand { get; }

        public bool OpenEditorAsNewCommand_CanExecute()
        {
            if (SelectedNode == null) return false;
            return (SelectedNode is NodeEntryCollection) ? true : false;
        }

        public void OpenEditorAsNewCommand_Execute()
        {
            if (SelectedNode == null)
                return;

            if (!(SelectedNode is NodeEntryCollection))
                return;

            // TODO: Check "accept".

            EntryFull newEntry = null;

            if (SelectedNode is NodeAtomPubEntryCollection)
            {
                newEntry = new AtomEntry("", (SelectedNode as NodeEntryCollection).Client);
            }
            else if (SelectedNode is NodeXmlRpcMTEntryCollection)
            {
                newEntry = new MTEntry("", (SelectedNode as NodeEntryCollection).Client);
            }
            else if (SelectedNode is NodeXmlRpcWPEntryCollection)
            {
                newEntry = new WPEntry("", (SelectedNode as NodeEntryCollection).Client);
            }

            if (newEntry == null)
                return;

            newEntry.PostUri = (SelectedNode as NodeEntryCollection).Uri;

            BlogEntryEventArgs ag = new BlogEntryEventArgs
            {
                Entry = newEntry
            };

            OpenEditorNewView?.Invoke(this, ag);
        }

        public ICommand RefreshEntriesCommand { get; }

        public bool RefreshEntriesCommand_CanExecute()
        {
            if (SelectedNode == null) return false;
            if ((SelectedNode is NodeEntryCollection) || (SelectedNode is NodeFeed) )
                return true;
            return false;
        }

        public void RefreshEntriesCommand_Execute()
        {
            if (SelectedNode == null)
                return;

            if ((SelectedNode is NodeEntryCollection) || (SelectedNode is NodeFeed))
            {
                this.GetEntries(SelectedNode as NodeTree);
            }

        }

        public ICommand OpenInBrowserCommand { get; }

        public bool OpenInBrowserCommand_CanExecute()
        {
            if (SelectedItem == null) return false;
            if (SelectedItem.AltHTMLUri == null) return false;
            return true;
        }

        public void OpenInBrowserCommand_Execute(EntryItem selectedEntry)
        {
            if (selectedEntry.AltHTMLUri != null)
            {
                //System.Diagnostics.Process.Start(selectedEntry.AltHTMLUri.AbsoluteUri);
                ProcessStartInfo psi = new ProcessStartInfo(selectedEntry.AltHTMLUri.AbsoluteUri);
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
        }

        public ICommand ShowSettingsCommand { get; }

        public bool ShowSettingsCommand_CanExecute()
        {
            return true;
        }

        public void ShowSettingsCommand_Execute()
        {
            // TODO:
            //System.Diagnostics.Debug.WriteLine("ShowSettingsCommand_Execute: not implemented yet.");

        }

        public ICommand ShowDebugWindowCommand { get; }
        public bool ShowDebugWindowCommand_CanExecute()
        {
            return true;
        }
        public void ShowDebugWindowCommand_Execute()
        {
            if (Application.Current == null) { return; }
            Application.Current.Dispatcher.Invoke(() =>
            {
                DebugWindowShowHide?.Invoke();
            });
        }

        public ICommand ClearDebugTextCommand { get; }
        public bool ClearDebugTextCommand_CanExecute()
        {
            return true;
        }
        public void ClearDebugTextCommand_Execute()
        {
            if (Application.Current == null) { return; }
            Application.Current.Dispatcher.Invoke(() =>
            {
                DebugClear?.Invoke();
            });
        }

        public ICommand ServiceAddCommand { get; }

        public bool ServiceAddCommand_CanExecute()
        {
            return true;
        }

        public void ServiceAddCommand_Execute()
        {
            // TODO: Ask to close all editor windows before launching.

            ServiceDiscoveryEventArgs ag = new ServiceDiscoveryEventArgs();

            OpenServiceDiscoveryView?.Invoke(this, ag);
        }


        #endregion

    }


}
