using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Configuration;
using System.Net;
using System.Security.Cryptography;
using System.Windows.Controls;

namespace BlogWrite.Common
{

    /// <summary>
    /// A base class for bindable ViewModels.
    /// Implements INotifyPropertyChanged and IDataErrorInfo.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged, IDataErrorInfo
    {
        #region == INotifyPropertyChanged ==

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string info)
        {
            //this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

            Application.Current.Dispatcher.Invoke(() =>
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
            });
        }

        #endregion

        #region == IDataErrorInfo ==

        private Dictionary<string, string> _ErrorMessages = new Dictionary<string, string>();

        string IDataErrorInfo.Error
        {
            get { return (_ErrorMessages.Count > 0) ? "Has Error" : null; }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (_ErrorMessages.ContainsKey(columnName))
                    return _ErrorMessages[columnName];
                else
                    return "";
            }
        }

        protected void SetError(string propertyName, string errorMessage)
        {
            _ErrorMessages[propertyName] = errorMessage;
        }

        protected void ClearErrror(string propertyName)
        {
            if (_ErrorMessages.ContainsKey(propertyName))
                //_ErrorMessages.Remove(propertyName);
                _ErrorMessages[propertyName] = "";
        }


        #endregion

    }
}
