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
			ext.InitializeDispatch();
		}
		
		public AppConfigViewModel config { get; } = new AppConfigViewModel();
		public ExtensionViewModel ext { get; } = new ExtensionViewModel();
		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			await MobileTitlebarService.Refresh(App.language.GetString("Config_general"));
			config.Initialie();
			ext.Initialize();
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

		private void RemoveExt(object sender, RoutedEventArgs e)
		{
			ext.RequestUninstallSelected();
		}
	}
}
