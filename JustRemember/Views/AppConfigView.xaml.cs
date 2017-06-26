using JustRemember.Models;
using JustRemember.Services;
using JustRemember.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace JustRemember.Views
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class AppConfigView : Page
	{
		public AppConfigView()
		{
			this.InitializeComponent();
			config.UpdateUI();
		}

		public AppConfigViewModel config { get; } = new AppConfigViewModel();
		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			await MobileTitlebarService.Refresh("General settings");
			config.Initialie();
			base.OnNavigatedTo(e);
		}

		protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
			await config.config.Save();
			base.OnNavigatingFrom(e);
		}

		private async void changePage(Pivot sender, PivotItemEventArgs args)
		{
			switch (sender.SelectedIndex)
			{
				case 0:
					await MobileTitlebarService.Refresh("General setting");
					return;
				case 1:
					await MobileTitlebarService.Refresh("Session setting");
					return;
				case 2:
					await MobileTitlebarService.Refresh("Stat list");
					return;
				case 3:
					await MobileTitlebarService.Refresh("About & Reset");
					return;
			}
			await config.config.Save();
			App.Config = config.config;
		}

		private async void timeChanged(object sender, TimePickerValueChangedEventArgs e)
		{
			config.timeLimit = e.NewTime;
			await config.config.Save();
		}

		private void TimePicker_Loaded(object sender, RoutedEventArgs e)
		{
			((TimePicker)sender).Time = config.timeLimit;
		}

		private void DeSelect(object sender, RoutedEventArgs e)
		{
			config.IselectedStat = -1;
		}
	}
}
