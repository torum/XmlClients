using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using BlogWrite.ViewModels;

namespace BlogWrite.Views
{
    public sealed partial class AddFeedPage : Page
    {
        public AddFeedViewModel ViewModel
        {
            get;
        }

        public AddFeedPage()
        {
            ViewModel = App.GetService<AddFeedViewModel>();
            this.InitializeComponent();
        }
    }
}
