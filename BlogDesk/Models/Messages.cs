using CommunityToolkit.Mvvm.Messaging.Messages;

namespace BlogDesk.Models;

public class ThemeChangedMessage : ValueChangedMessage<string>
{
    public ThemeChangedMessage(string theme) : base(theme)
    {
    }
}
