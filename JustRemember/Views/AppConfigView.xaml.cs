using JustRemember.Models;
using JustRemember.Services;
using JustRemember.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
			ext.InitializeDispatch();
		}
		
		public AppConfigViewModel config { get; } = new AppConfigViewModel();
		public ExtensionViewModel ext { get; } = new ExtensionViewModel();
		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			await MobileTitlebarService.Refresh(App.language.GetString("Config_general"));
			config.Initialie();
			base.OnNavigatedTo(e);
		}

		private async void changePage(Pivot sender, PivotItemEventArgs args)
		{
			if (sender.SelectedIndex == 0)
				NavigationService.Navigate<MainPage>();
			if (sender.SelectedIndex == 1)
				await MobileTitlebarService.Refresh(App.language.GetString("Config_general"));
			if (sender.SelectedIndex == 2)
				await MobileTitlebarService.Refresh(App.language.GetString("Config_session"));
			if (sender.SelectedIndex == 3)
				await MobileTitlebarService.Refresh(App.language.GetString("Config_stat"));
			if (sender.SelectedIndex == 4)
				await MobileTitlebarService.Refresh(App.language.GetString("Config_extension"));
			if (sender.SelectedIndex == 5)
				await MobileTitlebarService.Refresh(App.language.GetString("Config_about"));
		}

		private void timeChanged(object sender, TimePickerValueChangedEventArgs e)
		{
			config.timeLimit = e.NewTime;
		}

		private void TimePicker_Loaded(object sender, RoutedEventArgs e)
		{
			((TimePicker)sender).Time = config.timeLimit;
		}

		private void DeSelect(object sender, RoutedEventArgs e)
		{
			config.IselectedStat = -1;
		}

		private async void RemoveExt(object sender, RoutedEventArgs e)
		{
			string tag = (sender as Button).Tag.ToString();
			foreach (var item in ext.Extensions)
			{
				if (item.ID == tag)
				{
					var un = await ext.Notecatalog.RequestRemovePackageAsync(tag);
					if (un)
					{
						await PrenoteService.RequestRemovePrenoteExtension(item.Core.DisplayName);
					}
				}
			}
		}

		private async void OpenWebA(object sender, RoutedEventArgs e)
		{
			await Windows.System.Launcher.LaunchUriAsync(new Uri("https://www.twitter.com/ToonWK"));
		}

		private async void OpenWebB(object sender, RoutedEventArgs e)
		{
			await Windows.System.Launcher.LaunchUriAsync(new Uri("https://www.twitter.com/win10fan"));
			if (App.Config.showDebugging)
			{
				App.Config.showDebugging = false;
				return;
			}
			App.Config.showDebugging = true;
			App.Config.antiSpamChoice = false;
		}

		private async void OpenWebC(object sender, RoutedEventArgs e)
		{
			await Windows.System.Launcher.LaunchUriAsync(new Uri("https://www.facebook.com/nukun.fakfuy.1"));
		}

		private async void OpenWebD(object sender, RoutedEventArgs e)
		{
			await Windows.System.Launcher.LaunchUriAsync(new Uri("https://www.facebook.com/wipob.tita"));
		}

		private void ShowStatPage(object sender, SelectionChangedEventArgs e)
		{
			if ((sender as ListView).SelectedIndex > -1)
			{
				settingsContent.Visibility = Visibility.Visible;
				statView.Navigate(typeof(End), new List<object>() { config.selectedStat, true });
			}
			else
			{
				settingsContent.Visibility = Visibility.Visible;
				statView.Content = null;
			}
			config.OnPropertyChanged("stats");
		}

		private async void DeleteStat(object sender, RoutedEventArgs e)
		{
			int bfq = statList.SelectedIndex;
			statList.SelectedIndex = -1;
			StatModel.Delete(bfq);
			await Task.Delay(500);
			config.stats.Clear();
			config.stats = await StatModel.Get();
			config.OnPropertyChanged("stats");
		}

		private void QuitStat(object sender, RoutedEventArgs e)
		{
			statList.SelectedIndex = -1;
		}

		private async void MakeExt(object sender, RoutedEventArgs e)
		{
			await Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/ray1997/BundleMemosExample"));
		}

		private void AddExt(object sender, RoutedEventArgs e)
		{
			//await Windows.System.Launcher.LaunchUriAsync(new Uri("[uriToExtensionList]"));
		}
	}
}