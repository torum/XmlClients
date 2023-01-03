using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace BlogWrite.ViewModels
{

    /// <summary>
    /// IO Dialog Service
    /// </summary>
    #region == IO Dialog Service ==

    /// TODO: 
    /// https://stackoverflow.com/questions/28707039/trying-to-understand-using-a-service-to-open-a-dialog?noredirect=1&lq=1
    /*
    public interface IOpenDialogService
    {
        string[] GetOpenPictureFileDialog(string title, bool multi = true);
    }
    */

    public class OpenDialogService// : IOpenDialogService
    {
        /*
        public string GetOpenOpmlFileDialog(string title, bool multi = false)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = multi;
            openFileDialog.Filter = "OPML file (*.opml)|*.opml";
            // TODO: remember the last folder user accessed.
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog.Title = title;

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }
            return null;
        }

        public string GetSaveOpmlFileDialog(string title)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "OPML file (*.opml)|*.opml";
            // TODO: remember the last folder user accessed.
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            saveFileDialog.Title = title;

            if (saveFileDialog.ShowDialog() == true)
            {
                return saveFileDialog.FileName;
            }
            return null;
        }
        */
    }

    #endregion
}
