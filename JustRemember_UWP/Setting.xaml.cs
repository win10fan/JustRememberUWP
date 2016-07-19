using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace JustRemember_UWP
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class Setting : Page
	{
		public Setting()
		{
			/*QuitDialog = new MessageDialog("Are you sure", "Quit");
			QuitDialog.Commands.Add(new UICommand("Yes") { Invoked = delegate { Application.Current.Exit(); } });
			QuitDialog.Commands.Add(new UICommand("No") { Id = 1 });
			QuitDialog.CancelCommandIndex = 1;*/
			resetStat = new MessageDialog("This will clear all stat\r\nAre you sure?", "Reset stat");
			resetStat.Commands.Add(new UICommand("Yes") { Invoked = delegate { Utilities.currentSettings.stat.Clear(); Settings.Save(); } });
			resetStat.Commands.Add(new UICommand("No") { Id = 1 });
			resetStat.CancelCommandIndex = 1;
			//
			resetApp = new MessageDialog("This will reset app settings to default.\nAnd quit application. Are you sure?", "Reset app");
			resetApp.Commands.Add(new UICommand("Yes") { Invoked = delegate { Utilities.currentSettings = new Settings(); Settings.Save(); Application.Current.Exit(); } });
			resetApp.Commands.Add(new UICommand("No") { Id = 1 });
			resetApp.CancelCommandIndex = 1;
			//
			restartToApply = new MessageDialog("Please restart app to apply change..");
			restartToApply.Commands.Add(new UICommand("OK. Let's restart") { Invoked = delegate { Application.Current.Exit(); } });
			restartToApply.Commands.Add(new UICommand("OK, I'll restart later") { Id = 0 });
			restartToApply.CancelCommandIndex = 0;
			this.InitializeComponent();
		}
		public MessageDialog resetStat;
		public MessageDialog resetApp;
		public MessageDialog restartToApply;

		private void button_Click(object sender, RoutedEventArgs e)
		{
			Frame.Navigate(typeof(MainPage));
		}

		private void slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
		{
			Utilities.currentSettings.displayTextSize = (int)slider.Value;
			Settings.Save();
		}

		private void slider1_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
		{
			Utilities.currentSettings.totalChoice = (int)slider1.Value;
			Settings.Save();
		}

		private void toggleSwitch_Toggled(object sender, RoutedEventArgs e)
		{
			Utilities.currentSettings.isLimitTime = toggleSwitch.IsOn;
			Settings.Save();
			slider2.IsEnabled = Utilities.currentSettings.isLimitTime;
			slider3.IsEnabled = Utilities.currentSettings.isLimitTime;
		}

		private void slider_Loaded(object sender, RoutedEventArgs e)
		{
			slider.Value = Utilities.currentSettings.displayTextSize;
		}

		private void slider1_Loaded(object sender, RoutedEventArgs e)
		{
			slider1.Value = Utilities.currentSettings.totalChoice;
		}

		private void toggleSwitch_Loaded(object sender, RoutedEventArgs e)
		{
			toggleSwitch.IsOn = Utilities.currentSettings.isLimitTime;
		}

		private void slider2_Loaded(object sender, RoutedEventArgs e)
		{
			slider2.IsEnabled = Utilities.currentSettings.isLimitTime;
			slider2.Value = TimeSpan.FromSeconds(Utilities.currentSettings.limitTime).Minutes;
		}

		private void slider3_Loaded(object sender, RoutedEventArgs e)
		{
			slider3.IsEnabled = Utilities.currentSettings.isLimitTime;
			slider3.Value = TimeSpan.FromSeconds(Utilities.currentSettings.limitTime).Seconds;
		}

		private void toggleSwitch2_Loaded(object sender, RoutedEventArgs e)
		{
			toggleSwitch2.IsEnabled = Utilities.currentSettings.defaultMode == challageMode.Easy;
			toggleSwitch2.IsOn = Utilities.currentSettings.showWrongContent;
		}

		private void slider2_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
		{
			Utilities.currentSettings.limitTime = Convert.ToSingle((slider2.Value * 60) + slider3.Value);
			Settings.Save();
			slider3.Minimum = slider2.Value == 0 ? 30 : 0;
			slider3.Maximum = slider2.Value == 30 ? 0 : 60;
		}

		private void slider3_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
		{
			Utilities.currentSettings.limitTime = Convert.ToSingle((slider2.Value * 60) + slider3.Value);
			Settings.Save();
		}

		private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Utilities.currentSettings.defaultMode = (challageMode)comboBox1.SelectedIndex;
			Settings.Save();
			if (toggleSwitch2 != null)
			{
				toggleSwitch2.IsEnabled = Utilities.currentSettings.defaultMode == challageMode.Easy;
			}
		}

		private void comboBox1_Loaded(object sender, RoutedEventArgs e)
		{
			comboBox1.SelectedIndex = (int)Utilities.currentSettings.defaultMode;
			Settings.Save();
		}
		
		private void toggleSwitch2_Toggled(object sender, RoutedEventArgs e)
		{
			Utilities.currentSettings.showWrongContent = toggleSwitch2.IsOn;
			Settings.Save();
		}

		private async void button1_Click(object sender, RoutedEventArgs e)
		{
			await resetStat.ShowAsync();
		}

		private async void button2_Click(object sender, RoutedEventArgs e)
		{
			await resetApp.ShowAsync();
		}

		private void generalPage_Loaded(object sender, RoutedEventArgs e)
		{
			settingTitle.Text = "Settings";
		}

		private void sessionPage_Loaded(object sender, RoutedEventArgs e)
		{
			settingTitle.Text = "Session";
		}

		private void themePage_Loaded(object sender, RoutedEventArgs e)
		{
			settingTitle.Text = "Theme";
		}

		private void statPage_Loaded(object sender, RoutedEventArgs e)
		{
			settingTitle.Text = "Stat";
		}

		private void aboutPage_Loaded(object sender, RoutedEventArgs e)
		{
			settingTitle.Text = "About";
		}

		private async void darkTheme_Click(object sender, RoutedEventArgs e)
		{
			if (darkTheme.IsChecked == true && Utilities.currentSettings.theme != ApplicationTheme.Dark)
			{
				Utilities.currentSettings.theme = ApplicationTheme.Dark;
				Settings.Save();
				await restartToApply.ShowAsync();
			}
		}

		private void darkTheme_Loaded(object sender, RoutedEventArgs e)
		{
			darkTheme.IsChecked = Utilities.currentSettings.theme == ApplicationTheme.Dark;
		}

		private async void lightTheme_Click(object sender, RoutedEventArgs e)
		{
			if (lightTheme.IsChecked == true && Utilities.currentSettings.theme != ApplicationTheme.Light)
			{
				Utilities.currentSettings.theme = ApplicationTheme.Light;
				Settings.Save();
				await restartToApply.ShowAsync();
			}
		}

		private void lightTheme_Loaded(object sender, RoutedEventArgs e)
		{
			lightTheme.IsChecked = Utilities.currentSettings.theme == ApplicationTheme.Light;
		}

		private void titleBar_Loaded(object sender, RoutedEventArgs e)
		{
			if (Utilities.isSmallLoaderMode)
			{
				titleBar.Visibility = Visibility.Collapsed;
				settingsContent.Margin = new Thickness(0);
			}
			else if (!Utilities.isSmallLoaderMode)
			{
				titleBar.Visibility = Visibility.Visible;
				settingsContent.Margin = new Thickness(0, 50, 0, 0);
			}
		}

		//private async void comboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
		//{
		//	if (comboBox2.SelectedIndex == 0)
		//	{
		//		Utilities.currentSettings.theme = ApplicationTheme.Dark;
		//	}
		//	else if (comboBox2.SelectedIndex == 1)
		//	{
		//		Utilities.currentSettings.theme = ApplicationTheme.Light;
		//	}
		//	Settings.Save();
		//	if (Utilities.currentSettings.theme != Application.Current.RequestedTheme)
		//	{
		//		await restartToApply.ShowAsync();
		//	}
		//}

		//private void comboBox2_Loaded(object sender, RoutedEventArgs e)
		//{
		//	comboBox2.SelectedIndex = (int)Application.Current.RequestedTheme;
		//}
	}
}
