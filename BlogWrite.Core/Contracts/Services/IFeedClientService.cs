using BlogWrite.Core.Models;
using BlogWrite.Core.Models.Clients;

namespace BlogWrite.Core.Contracts.Services;

public interface IFeedClientService
{
    BaseClient BaseClient
    {
        get;
    }

    Task<HttpClientEntryItemCollectionResultWrapper> GetEntries(Uri entriesUrl, string feedId);
}
