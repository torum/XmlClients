/// 
/// 
/// BlogWrite 
///  - C#/WPF port of the original "BlogWrite" developed with Delphi.
/// https://github.com/torum/BlogWrite
/// 
/// 

using System;
using System.Windows.Input;
using System.Net.Http;
using BlogWrite.Common;
using BlogWrite.Models;
using System.Windows;

namespace BlogWrite.ViewModels
{
    public class ServiceDiscoveryViewModel : ViewModelBase
    {

        private ServiceDiscovery _serviceDiscovery;
        private bool _isBusy;
        private string _websiteOrEndpointUrl;
        private string _statusString;

        #region == Properties ==

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

        public bool IsButtonEnabled
        {
            get
            {
                return !IsBusy;
            }
        }

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

                _websiteOrEndpointUrl = value;

                NotifyPropertyChanged(nameof(_websiteOrEndpointUrl));
            }
        }

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

        #endregion

        /// <summary>Constructor.</summary>
        public ServiceDiscoveryViewModel()
        {
            _serviceDiscovery = new ServiceDiscovery();

            _serviceDiscovery.StatusUpdate += new ServiceDiscovery.ServiceDiscoveryStatusUpdate(OnStatusUpdate);

            CheckEndpointCommand = new RelayCommand(CheckEndpointCommand_Execute, CheckEndpointCommand_CanExecute);

        }

        #region == Events ==

        #endregion

        #region == Methods ==

        private void OnStatusUpdate(ServiceDiscovery sender, string data)
        {
            System.Diagnostics.Debug.WriteLine(data);

            StatusText = StatusText + Environment.NewLine + data;

        }

        #endregion

        #region == ICommands ==

        public ICommand CheckEndpointCommand { get; }

        public bool CheckEndpointCommand_CanExecute()
        {
            return String.IsNullOrEmpty(WebsiteOrEndpointUrl)? false : true;
        }

        public async void CheckEndpointCommand_Execute()
        {
            if (String.IsNullOrEmpty(WebsiteOrEndpointUrl))
                return;

            StatusText = "";

            Uri uri;
            try
            { 
                uri = new Uri(WebsiteOrEndpointUrl);
            }
            catch
            {
                // TODO make use of ErrorInfo
                return;
            }

            IsBusy = true;
            try
            {
                // 
                ServiceResultBase sr = await _serviceDiscovery.DiscoverService(uri);

                if (sr == null)
                    return;

                if (sr is ServiceResultErr)
                {
                    // TODO ErrorInfo
                    //(sr as ServiceResultErr).Err
                    return;
                }

                if (sr is ServiceResultAtomFeed)
                {
                    //(sr as ServiceResultAtomFeed).AtomFeedUrl
                }

                //ServiceResultAuthRequired

                //ServiceResultAtomPub
                //ServiceResultXmlRpc
                //ServiceResultAtomAPI

                //sr.EndpointUri

                /*
                switch (sr.Service)
                {
                    case ServiceTypes.AtomPub:
                        //
                        break;
                    case ServiceTypes.AtomPub_Hatena:
                        //
                        break;
                    case ServiceTypes.XmlRpc_WordPress:
                        //
                        break;
                    case ServiceTypes.XmlRpc_MovableType:
                        //
                        break;
                    case ServiceTypes.AtomApi:
                        //
                        break;
                    case ServiceTypes.AtomApi_GData:
                        //
                        break;
                    case ServiceTypes.Unknown:
                        //
                        break;

                }

                */
            }
            finally
            {
                IsBusy = false;
            }





        }

        #endregion

    }

}
