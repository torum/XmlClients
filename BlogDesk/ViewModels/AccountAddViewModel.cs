using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BlogDesk.Contracts.Services;
using BlogDesk.Contracts.ViewModels;
using BlogWrite.Core.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel;

namespace BlogDesk.ViewModels
{
    public class AccountAddViewModel : ObservableRecipient, INavigationAware
    {
        private readonly INavigationService _navigationService;

        public ICommand GoBackCommand
        {
            get;
        }

        public AccountAddViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            GoBackCommand = new RelayCommand(OnGoBack);
        }

        public void OnNavigatedTo(object parameter)
        {
        }

        public void OnNavigatedFrom()
        {
        }

        private void OnGoBack()
        {
            if (_navigationService.CanGoBack)
            {
                _navigationService.GoBack();
            }
        }
    }
}
