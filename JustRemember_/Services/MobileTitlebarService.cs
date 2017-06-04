using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace JustRemember.Services
{
	public static class MobileTitlebarService
	{
		public static async void Refresh(string text, object bg, object fg)
		{
			if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
			{
				var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
				statusBar.BackgroundColor = ((SolidColorBrush)bg).Color;
				statusBar.ForegroundColor = ((SolidColorBrush)fg).Color;
				if (text == "")
				{
					text = Package.Current.DisplayName;
				}
				//Thanks @RipleyWorp for the help here
				statusBar.ProgressIndicator.ProgressValue = 0;
				statusBar.ProgressIndicator.Text = text;
				await statusBar.ProgressIndicator.ShowAsync();
				//To here
			}
		}

		public static async void Refresh(string text)
		{
			if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
			{
				var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
				if (text == "")
				{
					text = Package.Current.DisplayName;
				}
				//Thanks @RipleyWorp for the help here
				statusBar.ProgressIndicator.ProgressValue = 0;
				statusBar.ProgressIndicator.Text = text;
				await statusBar.ProgressIndicator.ShowAsync();
				//To here
			}
		}

		public static async void Refresh()
		{
			if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
			{
				var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
				//Thanks @RipleyWorp for the help here
				statusBar.ProgressIndicator.ProgressValue = 0;
				statusBar.ProgressIndicator.Text = Package.Current.DisplayName;
				await statusBar.ProgressIndicator.ShowAsync();
				//To here
			}
		}
	}
}