using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace JustRemember_UWP
{
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			InitializeComponent();
			QuitDialog = new MessageDialog("Are you sure", "Quit");
			QuitDialog.Commands.Add(new UICommand("Yes") { Invoked = delegate { Application.Current.Exit(); } });
			QuitDialog.Commands.Add(new UICommand("No") { Id = 1 });
			QuitDialog.CancelCommandIndex = 1;
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;
        }
        
        MessageDialog QuitDialog;

		private async void quit_btn_Click(object sender, RoutedEventArgs e)
		{
			await QuitDialog.ShowAsync();
		}

		private void setting_btn_Click(object sender, RoutedEventArgs e)
		{
			Frame.Navigate(typeof(Setting));
		}

		private void start_btn_Click(object sender, RoutedEventArgs e)
		{
			Frame.Navigate(typeof(Selector));
		}

		private void prenote_btn_Click(object sender, RoutedEventArgs e)
		{
			Frame.Navigate(typeof(Prenote));
		}
    }
}