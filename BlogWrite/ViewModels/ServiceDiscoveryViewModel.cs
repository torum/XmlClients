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

namespace BlogWrite.ViewModels
{
    public class ServiceDiscoveryViewModel : ViewModelBase
    {
        private ServiceDiscovery _serviceDiscovery;
        private bool _isBusy;
        private string _websiteOrEndpointUrl;

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
                return "http://torum.jp/";//_websiteOrEndpointUrl;
            }
            set
            {
                if (_websiteOrEndpointUrl == value)
                    return;

                _websiteOrEndpointUrl = value;

                NotifyPropertyChanged(nameof(_websiteOrEndpointUrl));
            }
        }

        #endregion

        /// <summary>Constructor.</summary>
        public ServiceDiscoveryViewModel()
        {
            _serviceDiscovery = new ServiceDiscovery();

            CheckEndpointCommand = new RelayCommand(CheckEndpointCommand_Execute, CheckEndpointCommand_CanExecute);

        }

        #region == Events ==

        #endregion

        #region == Methods ==

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
                ServiceResult sr = await _serviceDiscovery.DiscoverService(uri);

                if (sr.Err != "")
                {
                    // ErrorInfo
                    return;
                }

                //sr.EndpointUri

                switch (sr.ServiceType)
                {
                    case ServiceDiscovery.ServiceTypes.AtomPub:
                        //
                        break;
                    case ServiceDiscovery.ServiceTypes.AtomPub_Hatena:
                        //
                        break;
                    case ServiceDiscovery.ServiceTypes.XmlRpc_WordPress:
                        //
                        break;
                    case ServiceDiscovery.ServiceTypes.XmlRpc_MovableType:
                        //
                        break;
                    case ServiceDiscovery.ServiceTypes.AtomApi:
                        //
                        break;
                    case ServiceDiscovery.ServiceTypes.AtomApi_GData:
                        //
                        break;
                    case ServiceDiscovery.ServiceTypes.Unknown:
                        //
                        break;

                }
            }
            finally
            {
                IsBusy = false;
            }





        }

        #endregion

    }
}
