﻿using System.Xml.Linq;
using BlogDesk.Contracts.Services;
using BlogDesk.ViewModels;

using Microsoft.UI.Xaml;

namespace BlogDesk.Activation;

public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly INavigationService _navigationService;

    public DefaultActivationHandler(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        // None of the ActivationHandlers has handled the activation.
        return _navigationService.Frame?.Content == null;
    }

    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        _navigationService.NavigateTo(typeof(MainViewModel).FullName!, null);

        await Task.FromResult(true);
        //await Task.CompletedTask;
    }
}
