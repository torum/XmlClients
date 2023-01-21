using System.Xml.Linq;
using AngleSharp.Dom;
using BlogWrite.Contracts.Services;
using BlogWrite.ViewModels;

using Microsoft.UI.Xaml;

namespace BlogWrite.Activation;

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
        _navigationService.NavigateTo(typeof(FeedsViewModel).FullName!, args.Arguments);

        await Task.CompletedTask;
    }
}
