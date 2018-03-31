using System;
using System.Windows.Input;
using System.Net.Http;
using BlogWrite.Common;

namespace BlogWrite.ViewModels
{
    public class ServiceDiscoveryViewModel : ViewModelBase
    {
        private HttpClient _httpClient;
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

        #endregion

        /// <summary>Constructor.</summary>
        public ServiceDiscoveryViewModel()
        {
            _httpClient = new HttpClient();

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

            IsBusy = true;
            try
            {
                var HTTPResponseMessage = await _httpClient.GetAsync(WebsiteOrEndpointUrl);

                if (HTTPResponseMessage.IsSuccessStatusCode)
                {
                    string s = await HTTPResponseMessage.Content.ReadAsStringAsync();

                    System.Diagnostics.Debug.WriteLine("GET returned: " + s);
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
