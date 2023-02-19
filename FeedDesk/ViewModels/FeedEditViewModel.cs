﻿using System.Windows.Input;
using BlogWrite.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FeedDesk.Contracts.Services;
using FeedDesk.Contracts.ViewModels;

namespace FeedDesk.ViewModels;

public class FeedEditViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService _navigationService;

    #region == Properties ==

    private NodeFeed? _feed;
    public NodeFeed? Feed
    {
        get => _feed;
        set => SetProperty(ref _feed, value);
    }

    private string? _name = "";
    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    #endregion

    #region == Command ==

    public ICommand GoBackCommand
    {
        get;
    }

    public ICommand UpdateFeedItemPropertyCommand
    {
        get;
    }


    #endregion

    public FeedEditViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;

        GoBackCommand = new RelayCommand(OnGoBack);
        UpdateFeedItemPropertyCommand = new RelayCommand(OnUpdateFeedItemProperty);

        Feed = null;
        Name = "";
    }

    public void OnNavigatedTo(object parameter)
    {
        Feed = null;
        Name = "";

        if (parameter is NodeTree)
        {
            if (parameter is NodeFeed feed)
            {
                Feed = feed;
                Name = feed.Name;
            }
        }
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


    private void OnUpdateFeedItemProperty()
    {
        if (!string.IsNullOrEmpty(Name))
        {
            if (Feed != null)
            {
                /* Not good when navigate go back.
                var hoge = new NodeTreePropertyChangedArgs();
                hoge.Name = Name;
                hoge.Node = Feed;
                _navigationService.NavigateTo(typeof(MainViewModel).FullName!, hoge);
                */

                var vm = App.GetService<MainViewModel>();
                vm.UpdateFeed(Feed, Name);
            }

            _navigationService.NavigateTo(typeof(MainViewModel).FullName!, null);
        }
    }
}
