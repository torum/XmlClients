using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlogWrite.Core.Models;
using BlogWrite.Core.Models.Clients;
using Windows.Media.Protection.PlayReady;
using Windows.Storage;

namespace BlogWrite.Core.Contracts.Services;
public interface IFeedClientService
{
    BaseClient BaseClient
    {
        get;
    }

    Task<HttpClientEntryItemCollectionResultWrapper> GetEntries(Uri entriesUrl, string feedId);
}
