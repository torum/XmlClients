using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Windows.System;

namespace BlogDesk.Models;

public class ThemeChangedMessage : ValueChangedMessage<string>
{
    public ThemeChangedMessage(string theme) : base(theme)
    {
    }
}
