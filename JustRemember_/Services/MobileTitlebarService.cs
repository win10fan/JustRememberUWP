using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI;

namespace JustRemember_.Services
{
	public static class MobileTitlebarService
	{
		public static async void Refresh(string text, Color bg, Color fg)
		{
			if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
			{
				var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
				statusBar.BackgroundColor = bg;
				statusBar.ForegroundColor = fg;
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