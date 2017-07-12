using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Media;

namespace JustRemember.Services
{
	public static class MobileTitlebarService
	{
		public static bool isMobile
		{
			get
			{
				if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
				{
					return true;
				}
				return false;
			}
		}

		public static async Task Refresh(string text, object bg, object fg)
		{
			if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
			{
				var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
				if (bg != null)
				{
					statusBar.BackgroundColor = ((SolidColorBrush)bg).Color;
				}
				if (fg != null)
				{
					statusBar.ForegroundColor = ((SolidColorBrush)fg).Color;
				}
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
				if (bg != null)
				{
					titleBar.BackgroundColor = ((SolidColorBrush)bg).Color;
				}
				if (fg != null)
				{
					titleBar.ButtonBackgroundColor = ((SolidColorBrush)bg).Color;
				}
			}
		}

		public static async Task Refresh(string text)
		{
			await Refresh(text, null, null);
		}

		public static async void Refresh()
		{
			await Refresh("", null, null);
		}
	}
}