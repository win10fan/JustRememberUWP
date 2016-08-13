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
            currentSettings = Settings.Load();
            InitializeComponent();
            c3.IsEnabled = currentSettings.totalChoice != 3;
            c4.IsEnabled = currentSettings.totalChoice != 4;
            c5.IsEnabled = currentSettings.totalChoice != 5;
        }
        Settings currentSettings;
        
		private void toggleSwitch_Toggled(object sender, RoutedEventArgs e)
		{
			currentSettings.isLimitTime = toggleSwitch.IsOn;
			Settings.Save();
			slider2.IsEnabled = currentSettings.isLimitTime;
			slider3.IsEnabled = currentSettings.isLimitTime;
		}

		private void toggleSwitch_Loaded(object sender, RoutedEventArgs e)
		{
			toggleSwitch.IsOn = currentSettings.isLimitTime;
		}

		private void slider2_Loaded(object sender, RoutedEventArgs e)
		{
            slider2.Maximum = 30;
			slider2.IsEnabled = currentSettings.isLimitTime;
			slider2.Value = TimeSpan.FromSeconds(currentSettings.limitTime).Minutes;
		}

		private void slider3_Loaded(object sender, RoutedEventArgs e)
		{
			slider3.IsEnabled = currentSettings.isLimitTime;
			slider3.Value = TimeSpan.FromSeconds(currentSettings.limitTime).Seconds;
		}

		private void slider2_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
		{
			currentSettings.limitTime = Convert.ToSingle((slider2.Value * 60) + slider3.Value);
			Settings.Save();
			slider3.Minimum = slider2.Value == 0 ? 30 : 0;
			slider3.Maximum = slider2.Value == 30 ? 0 : 60;
		}

		private void slider3_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
		{
			currentSettings.limitTime = Convert.ToSingle((slider2.Value * 60) + slider3.Value);
			Settings.Save();
		}
        private void difEasy_Loaded(object sender, RoutedEventArgs e)
        {
            difEasy.IsChecked = currentSettings.defaultMode == challageMode.Easy;
        }

        private void difEasy_Checked(object sender, RoutedEventArgs e)
        {
            currentSettings.defaultMode = challageMode.Easy;
            Settings.Save(currentSettings);
            if (toggleSwitch2 != null)
            {
                toggleSwitch2.IsEnabled = currentSettings.defaultMode == challageMode.Easy;
            }
        }

        private void difNorm_Loaded(object sender, RoutedEventArgs e)
        {
            difNorm.IsChecked = currentSettings.defaultMode == challageMode.Normal;
        }

        private void difNorm_Checked(object sender, RoutedEventArgs e)
        {
            currentSettings.defaultMode = challageMode.Normal;
            Settings.Save(currentSettings);
            if (toggleSwitch2 != null)
            {
                toggleSwitch2.IsEnabled = currentSettings.defaultMode == challageMode.Easy;
            }
        }

        private void difHard_Loaded(object sender, RoutedEventArgs e)
        {
            difHard.IsChecked = currentSettings.defaultMode == challageMode.Hard;
        }

        private void difHard_Checked(object sender, RoutedEventArgs e)
        {
            currentSettings.defaultMode = challageMode.Hard;
            Settings.Save(currentSettings);
            if (toggleSwitch2 != null)
            {
                toggleSwitch2.IsEnabled = currentSettings.defaultMode == challageMode.Easy;
            }
        }

        private void toggleSwitch2_Toggled(object sender, RoutedEventArgs e)
		{
			currentSettings.showWrongContent = toggleSwitch2.IsOn;
			Settings.Save();
		}

		private void toggleSwitch2_Loaded(object sender, RoutedEventArgs e)
		{
			toggleSwitch2.IsEnabled = currentSettings.defaultMode == challageMode.Easy;
			toggleSwitch2.IsOn = currentSettings.showWrongContent;
		}

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Setting));
        }

        private void c3_Click(object sender, RoutedEventArgs e)
        {
            currentSettings.totalChoice = 3;
            Settings.Save(currentSettings);
            c3.IsEnabled = currentSettings.totalChoice != 3;
            c4.IsEnabled = currentSettings.totalChoice != 4;
            c5.IsEnabled = currentSettings.totalChoice != 5;
        }

        private void c4_Click(object sender, RoutedEventArgs e)
        {
            currentSettings.totalChoice = 4;
            Settings.Save(currentSettings);
            c3.IsEnabled = currentSettings.totalChoice != 3;
            c4.IsEnabled = currentSettings.totalChoice != 4;
            c5.IsEnabled = currentSettings.totalChoice != 5;
        }

        private void c5_Click(object sender, RoutedEventArgs e)
        {
            currentSettings.totalChoice = 5;
            Settings.Save(currentSettings);
            c3.IsEnabled = currentSettings.totalChoice != 3;
            c4.IsEnabled = currentSettings.totalChoice != 4;
            c5.IsEnabled = currentSettings.totalChoice != 5;
        }
    }
}