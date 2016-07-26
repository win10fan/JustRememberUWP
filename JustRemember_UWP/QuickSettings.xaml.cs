using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace JustRemember_UWP
{
	public sealed partial class QuickSettings : Page
	{
		public QuickSettings()
		{
			InitializeComponent();
		}

		private void slider_Loaded(object sender, RoutedEventArgs e)
		{
			slider.Value = Utilities.currentSettings.displayTextSize;
		}

		private void slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
		{
			Utilities.currentSettings.displayTextSize = (int)slider.Value;
			Settings.Save();
		}

		private void slider1_Loaded(object sender, RoutedEventArgs e)
		{
			slider1.Value = Utilities.currentSettings.totalChoice;
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
			if (toggleSwitch2 != null)
			{
				toggleSwitch2.IsEnabled = Utilities.currentSettings.defaultMode == challageMode.Easy;
			}
			Settings.Save();
		}

		private void comboBox1_Loaded(object sender, RoutedEventArgs e)
		{
			comboBox1.SelectedIndex = (int)Utilities.currentSettings.defaultMode;
		}

		private void toggleSwitch2_Toggled(object sender, RoutedEventArgs e)
		{
			Utilities.currentSettings.showWrongContent = toggleSwitch2.IsOn;
			Settings.Save();
		}

		private void toggleSwitch2_Loaded(object sender, RoutedEventArgs e)
		{
			toggleSwitch2.IsEnabled = Utilities.currentSettings.defaultMode == challageMode.Easy;
			toggleSwitch2.IsOn = Utilities.currentSettings.showWrongContent;
		}
	}
}