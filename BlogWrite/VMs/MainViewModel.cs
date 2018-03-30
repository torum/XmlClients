/// 
/// 
/// BlogWrite
/// https://github.com/torum/BlogWrite
/// 
/// TODO:
/// -- Priority 1 --
///  
///  
/// -- Priority 2 --
///  Better error messages for users.
///
/// Known issues:
/// 
/// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Configuration;
using System.Net;
using System.Security.Cryptography;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using BlogWrite.Common;
using BlogWrite.Models;
using System.Globalization;
using BlogWrite.Models.Clients;


namespace BlogWrite.VMs
{

    public class MainViewModel : ViewModelBase
    {
        private ServiceTreeBuilder _services = new ServiceTreeBuilder();
        private object _selectedNode = null;
        private object _selectedItem = null;
        //private BlogClient _bc;

        #region == Properties ==

        public ObservableCollection<NodeTree> Services
        {
            get { return _services.Children;}
            set
            {
                _services.Children = value;
                NotifyPropertyChanged(nameof(Services));
            }
        }

        public object SelectedNode
        {
            get { return _selectedNode; }
            set
            {
                if (_selectedNode == value)
                    return;

                _selectedNode = value;
                NotifyPropertyChanged(nameof(SelectedNode));

                if (_selectedNode is NodeEntry)
                {
                    if ((_selectedNode as NodeEntry).List.Count == 0)
                    {
                        Task.Run(() => GetEntries((_selectedNode as NodeEntry)));
                    }
                }

                // This changes the listview.
                NotifyPropertyChanged(nameof(Entries));
            }
        }

        public ObservableCollection<EntryItem> Entries
        {
            get
            {
                if (_selectedNode == null)
                    return null;

                if (_selectedNode is NodeEntry)
                {
                    return (_selectedNode as NodeEntry).List;
                }
                else
                {
                    return null;
                }
            }
        }

        public object SelectedItem
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
            }
        }

        public bool IsContentText
        {
            get
            {
                return true;
                /*
                if (_selectedItem == null)
                    return false;

                if (!(_selectedItem is EntryItem))
                    return false;

                if ((_selectedItem as EntryItem).EntryBody == null)
                    return false;

                if ((_selectedItem as EntryItem).EntryBody.ContentType == EntryFull.ContentTypes.text)
                {
                    System.Diagnostics.Debug.WriteLine("IsContentText");
                    return true;
                }

                return false;
                */
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
                    System.Diagnostics.Debug.WriteLine("IsContentHTML");
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
                    return null;

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
                            return null;
                        }
                    }
                    else
                    {
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
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion

        #region == Events ==

        public event EventHandler<BlogEntryEventArgs> OpenEditorView;
        public event EventHandler<BlogEntryEventArgs> OpenEditorNewView;

        #endregion

        /// <summary>Constructor.</summary>
        public MainViewModel()
        {
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

            OpenInBrowserCommand = new GenericRelayCommand<EntryItem>(
                param => OpenInBrowserCommand_Execute(param),
                param => OpenInBrowserCommand_CanExecute());

            ListviewEnterKeyCommand = new GenericRelayCommand<EntryItem>(
                param => ListviewEnterKeyCommand_Execute(param),
                param => ListviewEnterKeyCommand_CanExecute());

            OpenEditorAsNewCommand = new RelayCommand(OpenEditorAsNewCommand_Execute, OpenEditorAsNewCommand_CanExecute);
            RefreshEntriesCommand = new RelayCommand(RefreshEntriesCommand_Execute, RefreshEntriesCommand_CanExecute);

            WindowClosingCommand = new RelayCommand(WindowClosingCommand_Execute, WindowClosingCommand_CanExecute);
            ShowSettingsCommand = new RelayCommand(ShowSettingsCommand_Execute, ShowSettingsCommand_CanExecute);

            // Upgrade settings.
            Properties.Settings.Default.Upgrade();

            // Load settings.
            if (Properties.Settings.Default.Profiles != null)
            {
                // Must be the first time.
                _services.LoadXmlDoc(Properties.Settings.Default.Profiles.Profiles);
            }
            else
            {
                //TODO:
            }



            //Task.Run(() => Do());


        }

        #region == Methods ==

        /*
        public async void Do()
        {
            string accountName = "Account Name";
            string userName = "test";
            string userPassword = "pass";
            Uri endpoint = new Uri("http://127.0.0.1/atom");

            _bc = new BlogClient(userName, userPassword, endpoint);

            // Access endpoint and create Account Node class.
            NodeServies a = await _bc.GetAccount(accountName);
            if (a == null) return;

            // Add Account Node to internal (virtual) Treeview.
            Application.Current.Dispatcher.Invoke(() => Services.Add(a));

            // Add to settings.
            //Application.Current.Dispatcher.Invoke(() => BlogWrite.Properties.Settings.Default.Services.Profile.Add(a));
            //BlogWrite.Properties.Settings.Default.Profiles.Profiles.Add(a);
            Properties.Settings.Default.Profiles.Profiles = null;
            Properties.Settings.Default.Profiles.Profiles = _services.AsXmlDoc();

            // Save settings.
            Properties.Settings.Default.Save();
        }
        */

        public async void GetEntries(NodeTree selectedNode)
        {
            if (selectedNode == null)
                return;

            if (!(selectedNode is NodeEntry))
                return;

            var bc = (selectedNode as NodeEntry).Client;
            if (bc == null)
                return;

            // TODO: 
            // HTTP Head, if_modified_since or etag or something... then  UpdateEntries();

            List<EntryItem> entLi = await bc.GetEntries((selectedNode as NodeEntry).Uri);

            // Minimize the time to block UI thread.
            Application.Current.Dispatcher.Invoke(() =>
            {

                (selectedNode as NodeEntry).List.Clear();
                foreach (EntryItem ent in entLi)
                {
                    ent.NodeEntry = (selectedNode as NodeEntry);

                    (selectedNode as NodeEntry).List.Add(ent);
                }

            });

            // This updates listview.
            NotifyPropertyChanged(nameof(Entries));

        }

        public async Task<bool> GetEntry(EntryItem selectedEntry)
        {
            if (selectedEntry == null)
                return false;
            BlogClient bc = selectedEntry.Client;
            if (bc == null)
                return false;

            if (selectedEntry.EditUri == null)
                return false;

            // TODO: 
            // HTTP Head, if_modified_since or etag or something... then  UpdateEntry();

            EntryFull bfe = await bc.GetFullEntry(selectedEntry.EditUri);

            if (selectedEntry == null)
                return false;

            selectedEntry.EntryBody = bfe;

            return true;
        }

        public async Task<bool> DeleteEntry(EntryItem selectedEntry)
        {
            if (selectedEntry == null)
                return false;

            BlogClient bc = selectedEntry.Client;

            if (bc == null)
                return false;

            if (selectedEntry.EditUri == null)
                return false;

            bool b = await bc.DeleteEntry(selectedEntry.EditUri);

            return b;
        }

        private static string WrapHtmlContent(string source, string styles = null)
        {
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

            if (selectedNode is NodeEntry) { 
                this.GetEntries(selectedNode);
            }
        }

        public ICommand ListviewLeftDoubleClickCommand { get; }

        public bool ListviewLeftDoubleClickCommand_CanExecute()
        {
            return true;
        }

        public void ListviewLeftDoubleClickCommand_Execute(EntryItem selectedEntry)
        {
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
            if (OpenEditorCommand_CanExecute())
                OpenEditorCommand.Execute(selectedEntry);
        }

        public ICommand OpenEditorCommand { get; }

        public bool OpenEditorCommand_CanExecute() { return true; }

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

        public bool DeleteEntryCommand_CanExecute() { return true; }

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
                            try {
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

        public ICommand OpenEditorAsNewCommand { get; }

        public bool OpenEditorAsNewCommand_CanExecute()
        {
            if (SelectedNode == null) return false;
            return (SelectedNode is NodeEntry) ? true : false;
        }

        public void OpenEditorAsNewCommand_Execute()
        {
            if (SelectedNode == null)
                return;

            if (!(SelectedNode is NodeEntry))
                return;

            // TODO: Check "accept".

            // TODO: AtomEntry...
            EntryFull newEntry = new AtomEntry("", (SelectedNode as NodeEntry).Client);
            newEntry.PostUri = (SelectedNode as NodeEntry).Uri;

            BlogEntryEventArgs ag = new BlogEntryEventArgs
            {
                Entry = newEntry
                //
            };

            OpenEditorNewView?.Invoke(this, ag);
        }

        public ICommand RefreshEntriesCommand { get; }

        public bool RefreshEntriesCommand_CanExecute()
        {
            if (SelectedNode == null) return false;
            return (SelectedNode is NodeEntry) ? true : false;
        }

        public void RefreshEntriesCommand_Execute()
        {
            if (SelectedNode == null)
                return;

            if (!(SelectedNode is NodeEntry))
                return;

            this.GetEntries((SelectedNode as NodeEntry));
        }
        
        public ICommand OpenInBrowserCommand { get; }

        public bool OpenInBrowserCommand_CanExecute() { return true; }

        public void OpenInBrowserCommand_Execute(EntryItem selectedEntry)
        {
            //
        }

        public ICommand ShowSettingsCommand { get; }

        public bool ShowSettingsCommand_CanExecute()
        {
            return true;
        }

        public void ShowSettingsCommand_Execute()
        {
            // TODO:
            System.Diagnostics.Debug.WriteLine("ShowSettingsCommand_Execute: not implemented yet.");
        }

        public ICommand WindowClosingCommand { get; }

        public bool WindowClosingCommand_CanExecute()
        {
            return true;
        }

        public void WindowClosingCommand_Execute()
        {
            System.Diagnostics.Debug.WriteLine("WindowClosingCommand");


            BlogWrite.Properties.Settings.Default.Profiles.Profiles = null;
            BlogWrite.Properties.Settings.Default.Profiles.Profiles = _services.AsXmlDoc();

            // Save settings.
            BlogWrite.Properties.Settings.Default.Save();

        }


        #endregion

    }

    public class BlogEntryEventArgs : EventArgs
    {
        public EntryFull Entry;
        //public BlogClient BlogClient;
    }

    /// <summary>
    /// Wrapper Class for storing ObservableCollection<Profile> in the settings. 
    /// </summary>
    public class ProfileSettings
    {
        public XmlDocument Profiles;

        public ProfileSettings()
        {
            Profiles = new XmlDocument();
        }
    }
}
