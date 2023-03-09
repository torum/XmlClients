using System.Xml;
using System.Xml.Linq;
using BlogDesk.Contracts.Services;
using BlogWrite.Core.Helpers;
using BlogDesk.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.ApplicationModel.Resources;
using Windows.System;

namespace BlogDesk.Views;

public sealed partial class ShellPage : Page
{

    public ShellViewModel ViewModel
    {
        get;
    }

    public ShellPage(ShellViewModel viewModel)
    {
        ViewModel = viewModel;

        try
        {
            InitializeComponent();
        }
        catch (XamlParseException parseException)
        {
            Debug.WriteLine($"Unhandled XamlParseException in ShellPage: {parseException.Message}");
            foreach (var key in parseException.Data.Keys)
            {
                Debug.WriteLine("{Key}:{Value}", key.ToString(), parseException.Data[key]?.ToString());
            }
            throw;
        }

        ViewModel.NavigationService.Frame = NavigationFrame;
        //ViewModel.NavigationViewService.Initialize(NavigationViewControl);

        AppTitleBarText.Text = "AppDisplayName".GetLocalized();

        App.MainWindow.ExtendsContentIntoTitleBar = true;

        App.MainWindow.SetTitleBar(AppTitleBar);

        App.MainWindow.Activated += MainWindow_Activated;
        App.MainWindow.Closed += MainWindow_Closed;

        Debug.WriteLine("App.MainWindow.Closed += MainWindow_Closed");
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Needed to be here. (don't put this in constructor.. messes up when theme changed.)
        TitleBarHelper.UpdateTitleBar(RequestedTheme, App.MainWindow);

        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));

        //ShellMenuBarSettingsButton.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(ShellMenuBarSettingsButton_PointerPressed), true);
        //ShellMenuBarSettingsButton.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(ShellMenuBarSettingsButton_PointerReleased), true);

        // give some time to let window draw itself.
        //await Task.Delay(100);

        // Needed to be here.
        //var navigationService = App.GetService<INavigationService>();
        //navigationService.NavigateTo(typeof(MainViewModel).FullName!, null);
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";

        AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];
        AppTitleBarIcon.Opacity = args.WindowActivationState == WindowActivationState.Deactivated ? 0.4 : 0.7;
        AppMenuBar.Opacity = args.WindowActivationState == WindowActivationState.Deactivated ? 0.4 : 0.7;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        //ShellMenuBarSettingsButton.RemoveHandler(UIElement.PointerPressedEvent, (PointerEventHandler)ShellMenuBarSettingsButton_PointerPressed);
        //ShellMenuBarSettingsButton.RemoveHandler(UIElement.PointerReleasedEvent, (PointerEventHandler)ShellMenuBarSettingsButton_PointerReleased);
    }

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        Debug.WriteLine("asdf");
        // Save err log.
        //(App.Current as App)?.SaveErrorLog();
    }

    private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
    {
        var keyboardAccelerator = new KeyboardAccelerator() { Key = key };

        if (modifiers.HasValue)
        {
            keyboardAccelerator.Modifiers = modifiers.Value;
        }

        keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;

        return keyboardAccelerator;
    }

    private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var navigationService = App.GetService<INavigationService>();

        var result = navigationService.GoBack();

        args.Handled = result;
    }

    private void ShellMenuBarSettingsButton_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        AnimatedIcon.SetState((UIElement)sender, "PointerOver");
    }

    private void ShellMenuBarSettingsButton_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        AnimatedIcon.SetState((UIElement)sender, "Pressed");
    }

    private void ShellMenuBarSettingsButton_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        AnimatedIcon.SetState((UIElement)sender, "Normal");
    }

    private void ShellMenuBarSettingsButton_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        AnimatedIcon.SetState((UIElement)sender, "Normal");
    }

    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        AppTitleBar.Margin = new Thickness()
        {
            Left = sender.CompactPaneLength * (sender.DisplayMode == NavigationViewDisplayMode.Minimal ? 2 : 1) ,
            Top = AppTitleBar.Margin.Top,
            Right = AppTitleBar.Margin.Right,
            Bottom = AppTitleBar.Margin.Bottom
        };
    }

    private void NavigationViewControl_Loaded(object sender, RoutedEventArgs e)
    {
        /* 
        var settings = (Microsoft.UI.Xaml.Controls.NavigationViewItem)NavigationViewControl.SettingsItem;
        if (settings != null)
            settings.Content = "Setting".GetLocalized();
        */
    }

    private void NavigationViewControl_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked == true)
        {
            // pass mainviewmodel for setting page.
            //NavigationFrame.Navigate(typeof(SettingsPage), MainVM, new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());
            NavigationFrame.Navigate(typeof(SettingsPage), null, args.RecommendedNavigationTransitionInfo);
        }
        else if (args.InvokedItemContainer != null && (args.InvokedItemContainer?.Tag != null))
        {
            /*
            if (_pages is null)
                return;

            var item = _pages.FirstOrDefault(p => p.Tag.Equals(args.InvokedItemContainer.Tag.ToString()));

            var _page = item.Page;

            if (_page is null)
                return;


            NavigationFrame.Navigate(_page, vm, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            //NavigationFrame.Navigate(_page, vm, args.RecommendedNavigationTransitionInfo);
            //NavigationFrame.Navigate(_page, vm, new SuppressNavigationTransitionInfo());
            */
        }
    }

    /*
    private void NavigationFrame_Navigated(object sender, NavigationEventArgs e)
    {
        if (NavigationFrame.SourcePageType == typeof(SettingsPage))
        {
            // SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag.
            //NavigationViewControl.SelectedItem = (NavigationViewItem)NavigationViewControl.SettingsItem;
            //NavigationViewControl.Header = "設定";
            return;
        }
        else if (NavigationFrame.SourcePageType != null)
        {
            //NavigationViewControl.Header = null;

            //var item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);

            //NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().First(n => n.Tag.Equals(item.Tag));

            //NavigationViewControl.Header = ((NavigationViewItem)NavigationViewControl.SelectedItem)?.Content?.ToString();

            // Do nothing.
        }
    }
    */

    private void NavigationFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
    
    }
}
