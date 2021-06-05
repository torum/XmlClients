using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlogWrite.ViewModels;
using BlogWrite.Models;
using System.Xml;
using System.Xml.Linq;

namespace BlogWrite.ViewModels
{
    /// <summary>
    /// Wrapper Class for storing ObservableCollection<Profile> in the settings. 
    /// </summary>
    public class ProfileSettings
    {
        public XmlDocument Profiles;

        public ProfileSettings()
        {
            Profiles = new XmlDocument();
        }
    }
}
