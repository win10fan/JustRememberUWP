using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace JustRemember_UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
			QuitDialog = new MessageDialog("Are you sure", "Quit");
			QuitDialog.Commands.Add(new UICommand("Yes") { Invoked = delegate { Application.Current.Exit(); } });
			QuitDialog.Commands.Add(new UICommand("No") { Id = 1 });
			QuitDialog.CancelCommandIndex = 1;
			this.InitializeComponent();
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
	}
}
