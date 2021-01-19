/// 
/// 
/// BlogWrite 
///  - C#/WPF port of the original "BlogWrite" developed with Delphi.
/// https://github.com/torum/BlogWrite
/// 
/// EditorViewModel
/// 
/// TODO:
/// -- Priority 1 --
///  Let MainWindow know that post has been made and let it update preview.
///  
/// -- Priority 2 --
///  Better error messages for users.
///
/// Known issues:
///  Hatena does not let draft=no once it is published.
/// 

using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using BlogWrite.Common;
using BlogWrite.Models;
using BlogWrite.Models.Clients;

namespace BlogWrite.ViewModels
{
    /// <summary>
    /// Editor Window ViewModel 
    /// </summary>
    class EditorViewModel : ViewModelBase
    {

        private EntryFull _ent;
        private BlogClient _bc;
        private int _publishStatusIndex = 0;

        #region == Properties ==

        public string ID
        {
            get
            {
                if (_ent == null)
                    return null;

                return _ent.ID;
            }
        }

        public string PathIcon
        {
            get
            {
                if (_ent == null)
                    return null;
                return _ent.PathIcon;

            }
        }

        public string EntryTitle
        {
            get
            {
                if (_ent == null)
                    return "";

                return _ent.Title;
            }
            set
            {
                if (_ent == null)
                    return;

                if (_ent.Title == value)
                    return;

                _ent.Title = value;

                NotifyPropertyChanged(nameof(EntryTitle));
            }
        }

        public string EntryContent
        {
            get
            {
                if (_ent == null)
                    return "";

                return _ent.Content;
            }
            set
            {
                if (_ent == null)
                    return;

                if (_ent.Content == value)
                    return;

                _ent.Content = value;

                NotifyPropertyChanged(nameof(EntryContent));
            }
        }

        public bool IsPublishButtonVisible
        {
            get
            {
                return (_ent.Status == EntryFull.EntryStatus.esNew) ? true : false;
            }
        }

        public bool IsUpdateButtonVisible
        {
            get
            {
                bool n, d;
                n = (_ent.Status == EntryItem.EntryStatus.esNormal) ? true : false;
                d = (_ent.Status == EntryItem.EntryStatus.esDraft) ? true : false;

                return (n || d) ? true : false;
            }
        }

        public bool IsOpenInButtonVisible
        {
            get
            {
                return (_ent.Status == EntryItem.EntryStatus.esNormal) ? true : false;
            }
        }

        public ObservableCollection<string> PublishStatus { get; } = new ObservableCollection<String>();

        public int PublishStatusIndex
        {
            get
            {
                if (_ent == null)
                    return 0;

                return _publishStatusIndex;

            }
            set
            {
                if (_publishStatusIndex == value)
                    return;

                _publishStatusIndex = value;
                NotifyPropertyChanged(nameof(PublishStatusIndex));

                switch (_publishStatusIndex)
                {
                    case 0:
                        _ent.IsDraft = false;
                        break;
                    case 1:
                        _ent.IsDraft = true;
                        break;
                }

                // IsDirty.

            }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public EditorViewModel(EntryFull ent)
        {
            _ent = ent;
            _bc = ent.Client as BlogClient;

            PublishStatus.Add("Publish");
            PublishStatus.Add("Draft");

            switch (_ent.Status)
            {
                case EntryItem.EntryStatus.esNew:
                    PublishStatusIndex = 0;
                    break;
                case EntryItem.EntryStatus.esNormal:
                    PublishStatusIndex = 0;
                    break;
                case EntryItem.EntryStatus.esDraft:
                    PublishStatusIndex = 1;
                    break;
                default:
                    PublishStatusIndex = 0;
                    break;
            }
            // Or
            if (_ent.IsDraft)
            {
                PublishStatusIndex = 1;
            }
            else
            {
                PublishStatusIndex = 0;
            }

            OpenInBrowserCommand = new RelayCommand(OpenInBrowserCommand_Execute, OpenInBrowserCommand_CanExecute);
            UpdateEntryCommand = new RelayCommand(UpdateEntryCommand_Execute, UpdateEntryCommand_CanExecute);
            PostNewEntryCommand = new RelayCommand(PostNewEntryCommand_Execute, PostNewEntryCommand_CanExecute);

        }

        #region == Methods ==



        #endregion

        #region == ICommands ==

        public ICommand OpenInBrowserCommand { get; }

        public bool OpenInBrowserCommand_CanExecute()
        {
            return true;
        }

        public void OpenInBrowserCommand_Execute()
        {
            //
        }

        public ICommand UpdateEntryCommand { get; }

        public bool UpdateEntryCommand_CanExecute()
        {
            //TODO:
            return true;
        }

        public async void UpdateEntryCommand_Execute()
        {
            bool result = await _bc.UpdateEntry(_ent);

            if (result) {

                if (_ent.IsDraft)
                {
                    _ent.Status = EntryItem.EntryStatus.esDraft;
                }
                else
                {
                    _ent.Status = EntryItem.EntryStatus.esNormal;

                }

                switch (_ent.Status)
                {
                    case EntryItem.EntryStatus.esNew:
                        PublishStatusIndex = 0;
                        break;
                    case EntryItem.EntryStatus.esNormal:
                        PublishStatusIndex = 0;
                        break;
                    case EntryItem.EntryStatus.esDraft:
                        PublishStatusIndex = 1;
                        break;
                    default:
                        PublishStatusIndex = 0;
                        break;
                }

                NotifyPropertyChanged(nameof(IsOpenInButtonVisible));
                NotifyPropertyChanged(nameof(PathIcon));
            }

        }

        public ICommand PostNewEntryCommand { get; }

        public bool PostNewEntryCommand_CanExecute()
        {
            //TODO:
            return true;
        }

        public async void PostNewEntryCommand_Execute()
        {

            if (String.IsNullOrEmpty(_ent.Title))
            {
                // TODO use errorinfo.
                return;
            }


            bool result = await _bc.PostEntry(_ent);

            if (result)
            {

                if (_ent.IsDraft)
                {
                    _ent.Status = EntryItem.EntryStatus.esDraft;
                }
                else
                {
                    _ent.Status = EntryItem.EntryStatus.esNormal;

                }

                switch (_ent.Status)
                {
                    case EntryItem.EntryStatus.esNew:
                        PublishStatusIndex = 0;
                        break;
                    case EntryItem.EntryStatus.esNormal:
                        PublishStatusIndex = 0;
                        break;
                    case EntryItem.EntryStatus.esDraft:
                        PublishStatusIndex = 1;
                        break;
                    default:
                        PublishStatusIndex = 0;
                        break;
                }

                NotifyPropertyChanged(nameof(IsOpenInButtonVisible));
                NotifyPropertyChanged(nameof(PathIcon));
            }


        }


        #endregion

    }

}
