namespace BlogWrite.Core.Models;

// For opening editor window from MainViewModel.
public class BlogEntryEventArgs : EventArgs
{
    public EntryFull? Entry;
}

// For opening ServiceDiscovery window from MainViewModel.
public class ServiceDiscoveryEventArgs : EventArgs
{

}

// For registering feed from ServiceDiscovery AddViewModel.
public class RegisterFeedEventArgs : EventArgs
{
    public FeedLink? FeedLinkData
    {
        get; set;
    }
}

// For registering Service from ServiceDiscovery AddViewModel.
public class RegisterAtomPubEventArgs : EventArgs
{
    public NodeService? NodeService
    {
        get; set;
    }
}

public class RegisterXmlRpcEventArgs : EventArgs
{
    public string? UserIdXmlRpc
    {
        get; set;
    }
    public string? PasswordXmlRpc
    {
        get; set;
    }
    public RsdLink? RsdLink
    {
        get; set;
    }
}

public class NodeTreePropertyChangedArgs : EventArgs
{
    public string? Name
    {
        get; set;
    }

    public NodeTree? Node
    {
        get;set;
    }
}
