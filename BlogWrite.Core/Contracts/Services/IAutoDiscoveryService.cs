using BlogWrite.Core.Models;
using BlogWrite.Core.Services;

namespace BlogWrite.Core.Contracts.Services;

public delegate void AutoDiscoveryStatusUpdateEventHandler(AutoDiscoveryService sender, string data);

public interface IAutoDiscoveryService
{
    event AutoDiscoveryStatusUpdateEventHandler? StatusUpdate;

    Task<ServiceResultBase> DiscoverService(Uri addr, bool isFeed);

    Task<ServiceResultBase> DiscoverServiceWithAuth(Uri addr, string userName, string apiKey, AuthTypes authType);

}