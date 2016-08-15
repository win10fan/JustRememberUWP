using System;
using Windows.Globalization;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace JustRemember_UWP
{
	public sealed partial class Setting : Page
	{
		public Setting()
		{
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
            currentConfig = Settings.Load();
			InitializeComponent();
		}
		public MessageDialog resetStat;
		public MessageDialog resetApp;
		public MessageDialog restartToApply;
        public Settings currentConfig;

		private void button_Click(object sender, RoutedEventArgs e)
		{
			Frame.Navigate(typeof(MainPage));
		}
        
		private async void button1_Click(object sender, RoutedEventArgs e)
		{
			await resetStat.ShowAsync();
		}

		private async void button2_Click(object sender, RoutedEventArgs e)
		{
			await resetApp.ShowAsync();
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

        private void darkTheme_Checked(object sender, RoutedEventArgs e)
        {
            currentConfig.theme = ApplicationTheme.Dark;
            Settings.Save(currentConfig);
            if (themeNotify != null)
            {
                themeNotify.Visibility = Application.Current.RequestedTheme == currentConfig.theme ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        
        private void darkTheme_Loaded(object sender, RoutedEventArgs e)
        {
            if (currentConfig.theme == ApplicationTheme.Dark)
            {
                darkTheme.IsChecked = true;
            }
            else
            {
                darkTheme.IsChecked = false;
            }
        }

        private void lightTheme_Loaded(object sender, RoutedEventArgs e)
        {
            if (currentConfig.theme == ApplicationTheme.Light)
            {
                lightTheme.IsChecked = true;
            }
            else
            {
                lightTheme.IsChecked = false;
            }
        }

        private void lightTheme_Checked(object sender, RoutedEventArgs e)
        {
            currentConfig.theme = ApplicationTheme.Light;
            Settings.Save(currentConfig);
            if (themeNotify != null)
            {
                themeNotify.Visibility = Application.Current.RequestedTheme == currentConfig.theme ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void cdpSize_Loaded(object sender, RoutedEventArgs e)
        {
            cdpSize.Value = currentConfig.displayTextSize;
            cdpSize.Minimum = 12;
            cdpSize.Maximum = 72;
        }

        private void cdpSize_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            currentConfig.displayTextSize = Convert.ToInt32(cdpSize.Value);
            Settings.Save(currentConfig);
        }

        private void themeNotify_Loaded(object sender, RoutedEventArgs e)
        {
            themeNotify.Visibility = Application.Current.RequestedTheme == currentConfig.theme ? Visibility.Collapsed : Visibility.Visible;
        }

        private void totalChoicie_Loaded(object sender, RoutedEventArgs e)
        {
            totalChoicie.Minimum = 3;
            totalChoicie.Maximum = 5;
            totalChoicie.Value = currentConfig.totalChoice;
            totalChoicie.ValueChanged += totalChoicie_ValueChanged;
        }

        private void totalChoicie_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            currentConfig.totalChoice = Convert.ToInt32(totalChoicie.Value);
            Settings.Save(currentConfig);
        }

        private void useTimelimit_Loaded(object sender, RoutedEventArgs e)
        {
            useTimelimit.IsOn = currentConfig.isLimitTime;
        }

        private void useTimelimit_Toggled(object sender, RoutedEventArgs e)
        {
            currentConfig.isLimitTime = useTimelimit.IsOn;
            Settings.Save(currentConfig);
            if (minuteSet != null) { minuteSet.IsEnabled = currentConfig.isLimitTime; }
            if (secondSet != null) { secondSet.IsEnabled = currentConfig.isLimitTime; }
        }

        private void minuteSet_Loaded(object sender, RoutedEventArgs e)
        {
            minuteSet.Minimum = 0;
            minuteSet.Maximum = 30;
            minuteSet.Value = TimeSpan.FromSeconds(currentConfig.limitTime).Minutes;
            minuteSet.IsEnabled = currentConfig.isLimitTime;
        }

        private void minuteSet_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            currentConfig.limitTime = Convert.ToSingle((minuteSet.Value * 60) + secondSet.Value);
            Settings.Save(currentConfig);
            if (minuteSet.Value == 0)
            {
                secondSet.Minimum = 30;
                secondSet.Maximum = 60;
            }
            else if (minuteSet.Value >= 1 && minuteSet.Value <= 29)
            {
                secondSet.Minimum = 0;
                secondSet.Maximum = 60;
            }
            else if (minuteSet.Value == 30)
            {
                secondSet.Minimum = 0;
                secondSet.Maximum = 0;
            }
        }

        private void secondSet_Loaded(object sender, RoutedEventArgs e)
        {
            secondSet.Value = TimeSpan.FromSeconds(currentConfig.limitTime).Seconds;
            minuteSet.ValueChanged += minuteSet_ValueChanged;
            secondSet.ValueChanged += secondSet_ValueChanged;
            secondSet.IsEnabled = currentConfig.isLimitTime;
        }

        private void secondSet_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            currentConfig.limitTime = Convert.ToSingle((minuteSet.Value * 60) + secondSet.Value);
            Settings.Save(currentConfig);
        }

        private void difEasy_Loaded(object sender, RoutedEventArgs e)
        {
            difEasy.IsChecked = currentConfig.defaultMode == challageMode.Easy;
        }

        private void difEasy_Checked(object sender, RoutedEventArgs e)
        {
            currentConfig.defaultMode = challageMode.Easy;
            Settings.Save(currentConfig);
            if (showWrong != null)
            {
                showWrong.IsEnabled = currentConfig.defaultMode == challageMode.Easy;
            }
        }

        private void difNorm_Loaded(object sender, RoutedEventArgs e)
        {
            difNorm.IsChecked = currentConfig.defaultMode == challageMode.Normal;
        }

        private void difNorm_Checked(object sender, RoutedEventArgs e)
        {
            currentConfig.defaultMode = challageMode.Normal;
            Settings.Save(currentConfig);
            if (showWrong != null)
            {
                showWrong.IsEnabled = currentConfig.defaultMode == challageMode.Easy;
            }
        }

        private void difHard_Loaded(object sender, RoutedEventArgs e)
        {
            difHard.IsChecked = currentConfig.defaultMode == challageMode.Hard;
        }

        private void difHard_Checked(object sender, RoutedEventArgs e)
        {
            currentConfig.defaultMode = challageMode.Hard;
            Settings.Save(currentConfig);
            if (showWrong != null)
            {
                showWrong.IsEnabled = currentConfig.defaultMode == challageMode.Easy;
            }
        }

        private void showWrong_Loaded(object sender, RoutedEventArgs e)
        {
            showWrong.IsOn = currentConfig.showWrongContent;
            showWrong.IsEnabled = currentConfig.defaultMode == challageMode.Easy;
        }

        private void showWrong_Toggled(object sender, RoutedEventArgs e)
        {
            currentConfig.showWrongContent = showWrong.IsOn;
            Settings.Save(currentConfig);
        }

        private void defSeed_Loaded(object sender, RoutedEventArgs e)
        {
            defSeed.IsOn = currentConfig.defaultSeed != -1;
        }

        private void defSeed_Toggled(object sender, RoutedEventArgs e)
        {
            if (defSeed.IsOn == false)
            {
                currentConfig.defaultSeed = -1;
                defSeedNotify.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (currentConfig.defaultSeed < 0) { currentConfig.defaultSeed = 0; }
                defSeedNotify.Visibility = Visibility.Visible;
            }
            if (defSeedInput != null)
            {
                defSeedInput.Visibility = currentConfig.defaultSeed == -1 ? Visibility.Collapsed : Visibility.Visible;
                defSeedInput.Text = currentConfig.defaultSeed.ToString();
                defSeedAccept.Visibility = currentConfig.defaultSeed == -1 ? Visibility.Collapsed : Visibility.Visible;
            }
            Settings.Save(currentConfig);
        }

        private void defSeedInput_Loaded(object sender, RoutedEventArgs e)
        {
            defSeedInput.Visibility = currentConfig.defaultSeed == -1 ? Visibility.Collapsed : Visibility.Visible;
            if (defSeedInput.Visibility == Visibility.Visible)
            {
                defSeedInput.Text = currentConfig.defaultSeed.ToString();
            }
        }

        private void defSeedAccept_Click(object sender, RoutedEventArgs e)
        {
            currentConfig.defaultSeed = int.Parse(defSeedInput.Text);
            Settings.Save(currentConfig);
        }

        private void defSeedAccept_Loaded(object sender, RoutedEventArgs e)
        {
            defSeedAccept.Visibility = currentConfig.defaultSeed == -1 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void defSeedNotify_Loaded(object sender, RoutedEventArgs e)
        {
            defSeedNotify.Visibility = currentConfig.defaultSeed == -1 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void autoScrollContent_Loaded(object sender, RoutedEventArgs e)
        {
            autoScrollContent.IsOn = currentConfig.autoScrollContent;
            autoScrollContent.Toggled += AutoScrollContent_Toggled;
        }

        private void AutoScrollContent_Toggled(object sender, RoutedEventArgs e)
        {
            currentConfig.autoScrollContent = autoScrollContent.IsOn;
            Settings.Save(currentConfig);
        }

        private void langList_Loaded(object sender, RoutedEventArgs e)
        {
            langList.SelectedIndex = currentConfig.language;
            langList.SelectionChanged += LangList_SelectionChanged;
        }

        private void LangList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentConfig.language = langList.SelectedIndex;
            Settings.Save(currentConfig);
            ApplicationLanguages.PrimaryLanguageOverride = Utilities.lang[currentConfig.language];
            Frame.CacheSize = 0;
        }

        private void GotoEnd_Loaded(object sender, RoutedEventArgs e)
        {
            GotoEnd.IsChecked = currentConfig.AfterFinalChoice == Settings.afterEnd.gotoEnd;
            GotoEnd.Checked += GotoEnd_Checked;
        }

        private void GotoEnd_Checked(object sender, RoutedEventArgs e)
        {
            currentConfig.AfterFinalChoice = Settings.afterEnd.gotoEnd;
            Settings.Save(currentConfig);
            if (statdecide1 != null)
            {
                statdecide1.Visibility = currentConfig.AfterFinalChoice != Settings.afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
                saveAll.Visibility = currentConfig.AfterFinalChoice != Settings.afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
                discardAll.Visibility = currentConfig.AfterFinalChoice != Settings.afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void resetMatch_Loaded(object sender, RoutedEventArgs e)
        {
            resetMatch.IsChecked = currentConfig.AfterFinalChoice == Settings.afterEnd.restartMatch;
            resetMatch.Checked += ResetMatch_Checked;
        }

        private void ResetMatch_Checked(object sender, RoutedEventArgs e)
        {
            currentConfig.AfterFinalChoice = Settings.afterEnd.restartMatch;
            Settings.Save(currentConfig);
            if (statdecide1 != null)
            {
                statdecide1.Visibility = currentConfig.AfterFinalChoice != Settings.afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
                saveAll.Visibility = currentConfig.AfterFinalChoice != Settings.afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
                discardAll.Visibility = currentConfig.AfterFinalChoice != Settings.afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void GotoMain_Loaded(object sender, RoutedEventArgs e)
        {
            GotoMain.IsChecked = currentConfig.AfterFinalChoice == Settings.afterEnd.gotoMain;
            GotoMain.Checked += GotoMain_Checked;
        }

        private void GotoMain_Checked(object sender, RoutedEventArgs e)
        {
            currentConfig.AfterFinalChoice = Settings.afterEnd.gotoMain;
            Settings.Save(currentConfig);
            if (statdecide1 != null)
            {
                statdecide1.Visibility = currentConfig.AfterFinalChoice != Settings.afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
                saveAll.Visibility = currentConfig.AfterFinalChoice != Settings.afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
                discardAll.Visibility = currentConfig.AfterFinalChoice != Settings.afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void saveAll_Loaded(object sender, RoutedEventArgs e)
        {
            saveAll.Visibility = currentConfig.AfterFinalChoice == Settings.afterEnd.gotoEnd ? Visibility.Collapsed : Visibility.Visible;
            saveAll.IsChecked = currentConfig.TodoWithStat == Settings.ifNotGotoEnd.saveAllStat;
            saveAll.Checked += SaveAll_Checked;
        }

        private void SaveAll_Checked(object sender, RoutedEventArgs e)
        {
            currentConfig.TodoWithStat = Settings.ifNotGotoEnd.saveAllStat;
            Settings.Save(currentConfig);
            if (statdecide1 != null)
            {
                statdecide1.Visibility = currentConfig.AfterFinalChoice != Settings.afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
                saveAll.Visibility = currentConfig.AfterFinalChoice != Settings.afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
                discardAll.Visibility = currentConfig.AfterFinalChoice != Settings.afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void discardAll_Loaded(object sender, RoutedEventArgs e)
        {
            discardAll.Visibility = currentConfig.AfterFinalChoice == Settings.afterEnd.gotoEnd ? Visibility.Collapsed : Visibility.Visible;
            discardAll.IsChecked = currentConfig.TodoWithStat == Settings.ifNotGotoEnd.discardAllStat;
            discardAll.Checked += DiscardAll_Checked;
        }

        private void DiscardAll_Checked(object sender, RoutedEventArgs e)
        {
            currentConfig.TodoWithStat = Settings.ifNotGotoEnd.discardAllStat;
            Settings.Save(currentConfig);
            if (statdecide1 != null)
            {
                statdecide1.Visibility = currentConfig.AfterFinalChoice != Settings.afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
                saveAll.Visibility = currentConfig.AfterFinalChoice != Settings.afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
                discardAll.Visibility = currentConfig.AfterFinalChoice != Settings.afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void statdecide1_Loaded(object sender, RoutedEventArgs e)
        {
            statdecide1.Visibility = currentConfig.AfterFinalChoice == Settings.afterEnd.gotoEnd ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}