using System;
using System.Windows.Input;
using System.Net.Http;
using BlogWrite.Common;

namespace BlogWrite.ViewModels
{
    public class ServiceDiscoveryVewModel : ViewModelBase
    {
        private HttpClient _httpClient;
        private bool _isBusy;
        private string _websiteOrEndpoint;

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

        public string WebsiteOrEndpoint
        {
            get
            {
                return _websiteOrEndpoint;
            }
            set
            {
                if (_websiteOrEndpoint == value)
                    return;

                _websiteOrEndpoint = value;

                NotifyPropertyChanged(nameof(WebsiteOrEndpoint));
            }
        }



        public ServiceDiscoveryVewModel()
        {
            _httpClient = new HttpClient();

            CheckEndpointCommand = new RelayCommand(CheckEndpointCommand_Execute, CheckEndpointCommand_CanExecute);

        }



        public ICommand CheckEndpointCommand { get; }

        public bool CheckEndpointCommand_CanExecute()
        {
            return String.IsNullOrEmpty(WebsiteOrEndpoint)? false : true;
        }

        public async void CheckEndpointCommand_Execute()
        {
            if (String.IsNullOrEmpty(WebsiteOrEndpoint))
                return;

            IsBusy = true;
            try
            {
                var HTTPResponseMessage = await _httpClient.GetAsync(WebsiteOrEndpoint);

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

    }
}
