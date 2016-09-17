using System;
using System.IO;
using System.Text.RegularExpressions;
using Windows.Globalization;
using Windows.Storage;
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
            stats = new StatList();
            stats.Stats = StatList.Load();
            currentConfig = Settings.Load();
            DialogPrep();
			InitializeComponent();
		}
        public StatList stats;
        public Settings currentConfig;

		private void button_Click(object sender, RoutedEventArgs e)
		{
			Frame.Navigate(typeof(MainPage));
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
            GotoEnd.IsChecked = currentConfig.AfterFinalChoice == afterEnd.gotoEnd;
            GotoEnd.Checked += GotoEnd_Checked;
        }

        private void GotoEnd_Checked(object sender, RoutedEventArgs e)
        {
            currentConfig.AfterFinalChoice = afterEnd.gotoEnd;
            Settings.Save(currentConfig);
            if (statdecide1 != null)
            {
                statdecide1.Visibility = currentConfig.AfterFinalChoice != afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
                saveAll.Visibility = currentConfig.AfterFinalChoice != afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
                discardAll.Visibility = currentConfig.AfterFinalChoice != afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void resetMatch_Loaded(object sender, RoutedEventArgs e)
        {
            resetMatch.IsChecked = currentConfig.AfterFinalChoice == afterEnd.restartMatch;
            resetMatch.Checked += ResetMatch_Checked;
        }

        private void ResetMatch_Checked(object sender, RoutedEventArgs e)
        {
            currentConfig.AfterFinalChoice = afterEnd.restartMatch;
            Settings.Save(currentConfig);
            if (statdecide1 != null)
            {
                statdecide1.Visibility = currentConfig.AfterFinalChoice != afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
                saveAll.Visibility = currentConfig.AfterFinalChoice != afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
                discardAll.Visibility = currentConfig.AfterFinalChoice != afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void GotoMain_Loaded(object sender, RoutedEventArgs e)
        {
            GotoMain.IsChecked = currentConfig.AfterFinalChoice == afterEnd.gotoMain;
            GotoMain.Checked += GotoMain_Checked;
        }

        private void GotoMain_Checked(object sender, RoutedEventArgs e)
        {
            currentConfig.AfterFinalChoice = afterEnd.gotoMain;
            Settings.Save(currentConfig);
            if (statdecide1 != null)
            {
                statdecide1.Visibility = currentConfig.AfterFinalChoice != afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
                saveAll.Visibility = currentConfig.AfterFinalChoice != afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
                discardAll.Visibility = currentConfig.AfterFinalChoice != afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void saveAll_Loaded(object sender, RoutedEventArgs e)
        {
            saveAll.Visibility = currentConfig.AfterFinalChoice == afterEnd.gotoEnd ? Visibility.Collapsed : Visibility.Visible;
            saveAll.IsChecked = currentConfig.TodoWithStat == ifNotGotoEnd.saveAllStat;
            saveAll.Checked += SaveAll_Checked;
        }

        private void SaveAll_Checked(object sender, RoutedEventArgs e)
        {
            currentConfig.TodoWithStat = ifNotGotoEnd.saveAllStat;
            Settings.Save(currentConfig);
            if (statdecide1 != null)
            {
                statdecide1.Visibility = currentConfig.AfterFinalChoice != afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
                saveAll.Visibility = currentConfig.AfterFinalChoice != afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
                discardAll.Visibility = currentConfig.AfterFinalChoice != afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void discardAll_Loaded(object sender, RoutedEventArgs e)
        {
            discardAll.Visibility = currentConfig.AfterFinalChoice == afterEnd.gotoEnd ? Visibility.Collapsed : Visibility.Visible;
            discardAll.IsChecked = currentConfig.TodoWithStat == ifNotGotoEnd.discardAllStat;
            discardAll.Checked += DiscardAll_Checked;
        }

        private void DiscardAll_Checked(object sender, RoutedEventArgs e)
        {
            currentConfig.TodoWithStat = ifNotGotoEnd.discardAllStat;
            Settings.Save(currentConfig);
            if (statdecide1 != null)
            {
                statdecide1.Visibility = currentConfig.AfterFinalChoice != afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
                saveAll.Visibility = currentConfig.AfterFinalChoice != afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
                discardAll.Visibility = currentConfig.AfterFinalChoice != afterEnd.gotoEnd ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void statdecide1_Loaded(object sender, RoutedEventArgs e)
        {
            statdecide1.Visibility = currentConfig.AfterFinalChoice == afterEnd.gotoEnd ? Visibility.Collapsed : Visibility.Visible;
        }

        private void hintAtFirst_Loaded(object sender, RoutedEventArgs e)
        {
            hintAtFirst.IsOn = currentConfig.hintAtFirstchoice;
            hintAtFirst.Toggled += HintAtFirst_Toggled;
        }

        private void HintAtFirst_Toggled(object sender, RoutedEventArgs e)
        {
            currentConfig.hintAtFirstchoice = hintAtFirst.IsOn;
            Settings.Save(currentConfig);
        }

        private void noStatNotif_Loaded(object sender, RoutedEventArgs e)
        {
            noStatNotif.FontSize = 18;
            if (stats.Stats.Count >= 1)
            {
                noStatNotif.Text = $"Total stat: {stats.Stats.Count}";
            }
            else
            {
                noStatNotif.Text = "No stat found :(";
            }
        }

        private void choiceStyleA_Loaded(object sender, RoutedEventArgs e)
        {
            choiceStyleA.IsChecked = currentConfig.choiceStyle == selectMode.styleA;
            choiceStyleA.Checked += ChoiceStyleA_Checked;
        }

        private void ChoiceStyleA_Checked(object sender, RoutedEventArgs e)
        {
            currentConfig.choiceStyle = selectMode.styleA;
            Settings.Save(currentConfig);
        }

        private void choiceStyleB_Loaded(object sender, RoutedEventArgs e)
        {
            choiceStyleB.IsChecked = currentConfig.choiceStyle == selectMode.styleB;
            choiceStyleB.Checked += ChoiceStyleB_Checked;
        }

        private void ChoiceStyleB_Checked(object sender, RoutedEventArgs e)
        {
            currentConfig.choiceStyle = selectMode.styleB;
            Settings.Save(currentConfig);
        }

        private async void resetSetting_Click(object sender, RoutedEventArgs e)
        {
            await settingReset.ShowAsync();
        }

        public MessageDialog statReset;
        public MessageDialog sessionReset;
        public MessageDialog usernoteReset;
        public MessageDialog settingReset;
        public MessageDialog allReset;

        public void DialogPrep()
        {
            settingReset = new MessageDialog("All settings will revert to default.", "Are you sure?");
            settingReset.Commands.Add(new UICommand("Yes") { Invoked = delegate { ResetApp(resetStyle.setting); } });
            settingReset.Commands.Add(new UICommand("No") { Id = 1 });
            settingReset.CancelCommandIndex = 1;
            //
            statReset = new MessageDialog("All stat will clear to nothing.", "Are you sure?");
            statReset.Commands.Add(new UICommand("Yes") { Invoked = delegate { ResetApp(resetStyle.stat); } });
            statReset.Commands.Add(new UICommand("No") { Id = 1 });
            statReset.CancelCommandIndex = 1;
            //
            sessionReset = new MessageDialog("All unfinished session will get remove.", "Are you sure?");
            sessionReset.Commands.Add(new UICommand("Yes") { Invoked = delegate { ResetApp(resetStyle.session); } });
            sessionReset.Commands.Add(new UICommand("No") { Id = 1 });
            sessionReset.CancelCommandIndex = 1;
            //
            usernoteReset = new MessageDialog("All your note will gone without going back.", "Are you sure?");
            usernoteReset.Commands.Add(new UICommand("Yes") { Invoked = delegate { ResetApp(resetStyle.note); } });
            usernoteReset.Commands.Add(new UICommand("No") { Id = 1 });
            usernoteReset.CancelCommandIndex = 1;
            //
            allReset = new MessageDialog("Everyting will reset to default like reinstall." + Environment.NewLine + "If you want to reset. Application will exit", "Are you sure?");
            allReset.Commands.Add(new UICommand("Yes") { Invoked = delegate { ResetApp(resetStyle.all); } });
            allReset.Commands.Add(new UICommand("No") { Id = 1 });
            allReset.CancelCommandIndex = 1;
        }
        public enum resetStyle { setting, stat, session, note, all }
        public void ResetApp(resetStyle type)
        {
            switch (type)
            {
                case resetStyle.setting:
                    Settings.Save(new Settings());
                    Frame.Navigate(typeof(MainPage));
                    return;
                case resetStyle.stat:
                    DirectoryInfo di = new DirectoryInfo(ApplicationData.Current.LocalFolder.Path + "\\Stat");
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                    return;
                case resetStyle.session:
                    //Remove all sessions
                    return;
                case resetStyle.note:
                    DirectoryInfo dl = new DirectoryInfo(ApplicationData.Current.RoamingFolder.Path + "\\Note");
                    foreach (FileInfo file in dl.GetFiles())
                    {
                        file.Delete();
                    }
                    return;
                case resetStyle.all:
                    DirectoryInfo alldelb = new DirectoryInfo(ApplicationData.Current.LocalFolder.Path);

                    foreach (FileInfo file in alldelb.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in alldelb.GetDirectories())
                    {
                        dir.Delete(true);
                    }

                    DirectoryInfo alldela = new DirectoryInfo(ApplicationData.Current.RoamingFolder.Path);
                    foreach (FileInfo file in alldela.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in alldela.GetDirectories())
                    {
                        dir.Delete(true);
                    }
                    Application.Current.Exit();
                    return;
            }
        }

        private async void resetstat_Click(object sender, RoutedEventArgs e)
        {
            await statReset.ShowAsync();
        }

        private async void resetsession_Click(object sender, RoutedEventArgs e)
        {
            await sessionReset.ShowAsync();
        }

        private async void resetusernote_Click(object sender, RoutedEventArgs e)
        {
            await usernoteReset.ShowAsync();
        }

        private async void resetall_Click(object sender, RoutedEventArgs e)
        {
            await allReset.ShowAsync();
        }

        private void Image_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (choiceStyleB.IsChecked == true)
            {
                choiceStyleA.IsChecked = true;
                choiceStyleB.IsChecked = false;
            }
        }

        private void Image_Tapped_1(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (choiceStyleA.IsChecked == true)
            {
                choiceStyleB.IsChecked = true;
                choiceStyleA.IsChecked = false;
            }
        }
    }
}