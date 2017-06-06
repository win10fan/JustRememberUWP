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
		public static async Task Refresh(string text, object bg, object fg)
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
			else
			{
				var appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
				var titleBar = appView.TitleBar;
				if (text == "")
				{
					appView.Title = "";
				}
				else
				{
					appView.Title = $"{text}";
				}
				titleBar.BackgroundColor = ((SolidColorBrush)bg).Color;
				titleBar.ButtonBackgroundColor = ((SolidColorBrush)bg).Color;
				titleBar.ForegroundColor = ((SolidColorBrush)fg).Color;
			}
		}

		public static async Task Refresh(string text)
		{
			await Refresh(text, App.Current.Resources["ApplicationPageBackgroundThemeBrush"], App.Current.Resources["ApplicationForegroundThemeBrush"]);
		}

		public static async void Refresh()
		{
			await Refresh("", App.Current.Resources["ApplicationPageBackgroundThemeBrush"], App.Current.Resources["ApplicationForegroundThemeBrush"]);
		}
	}
}