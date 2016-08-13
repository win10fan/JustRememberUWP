using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace JustRemember_UWP
{
	public sealed partial class Match : Page
	{
		public DispatcherTimer timerNow;
		public Match()
		{
			InitializeComponent();
			timerNow = new DispatcherTimer();
			timerNow.Interval = TimeSpan.FromMilliseconds(100);
			timerNow.Tick += TimerNow_Tick;
			timerNow.Start();
			//Over dialog
			overMSG = new MessageDialog("Try again?", "Time up.");
			overMSG.Commands.Add(new UICommand("OK"){ Invoked = delegate { ResetRound(); } });
			//Load progress lose dialog
			lostPG = new MessageDialog("If you continue.\nCurrent progress will get replaced.\nContinue?", "Warning!");
			lostPG.Commands.Add(new UICommand("Continue") { Invoked = delegate { Utilities.isSmallLoaderMode = false; Frame.Navigate(typeof(MainPage)); } });
			lostPG.Commands.Add(new UICommand("Cancel") { Id = 0 });
			lostPG.CancelCommandIndex = 0;
			//Load other file dialog
			loadotherDiag = new MessageDialog("You about to lose current progress.\nPress \"OK\" to continue", "Warning!");
			loadotherDiag.Commands.Add(new UICommand("OK") { Invoked = delegate { Frame.Navigate(typeof(Selector)); } });
			loadotherDiag.Commands.Add(new UICommand("Cancel") { Id = 0 });
			loadotherDiag.CancelCommandIndex = 0;
			//
			resetM = new MessageDialog("You about to lose current progress.\nPress \"OK\" to continue", "Warning!");
			resetM.Commands.Add(new UICommand("OK") { Invoked = delegate { ResetRound(); } });
			resetM.Commands.Add(new UICommand("Cancel") { Id = 0 });
			resetM.CancelCommandIndex = 0;
			//ExitD
			exitD = new MessageDialog("Did you really want to exit?\nYou will lose current progress");
			exitD.Commands.Add(new UICommand("OK") { Invoked = delegate { Application.Current.Exit(); } });
			exitD.Commands.Add(new UICommand("Cancel") { Id = 0 });
			exitD.CancelCommandIndex = 0;
			//Load file
			LoadFile(Utilities.selected.Title, Utilities.selected.Content);
			randomEngine = new Random();
		}

		MessageDialog resetM;
		MessageDialog overMSG;
		MessageDialog lostPG;
		MessageDialog loadotherDiag;
		MessageDialog exitD;
		Random randomEngine;
		#region Match component
		public int currentProgress
		{
			get
			{
				return now;
			}
			set
			{
				if (value == now)
				{
					return;
				}
				if (now < value)
				{
					now = value;
				}
			}
		}
		int now;
		public string currentFilename;
		public List<textlist> textList = new List<textlist>();
		public List<string> choiceList = new List<string>();
		#endregion
		#region UI Controller
		bool _pse;
		public bool Pause
		{
			get
			{
				return _pse;
			}
			set
			{
				_pse = value;
				if (value)
				{
					pauseInfo.Text = "Paused...";
				}
				else
				{
					pauseInfo.Text = currentFilename;
				}
			}
		}
		#endregion
		
		public void LoadFile(string content)
		{
			LoadFile("", content);
		}
		
		textlist working = new textlist();
		List<textlist> workingResult = new List<textlist>();
		bool morethen1 = false;
		textMode workingType;
		bool oneTime = false;
		bool splitNow = false;
		public List<textlist> replace4Match(string content)
		{
			if (!oneTime)
			{
				if (char.IsWhiteSpace(content[0]))
				{
					workingType = textMode.WhiteSpace;
				}
				else
				{
					workingType = textMode.Char;
				}
				working = new textlist();
				working.Text = "";
				working.Commands = "";
				workingResult = new List<textlist>();
				morethen1 = false;
				workingType = textMode.Char;
				oneTime = true;
			}
			//Final
			if (content.Length == 0)
			{
				if (morethen1) { return workingResult; }
				else if (!morethen1) { return new List<textlist>(); }
			}
			//Woring loop
			if (workingType == textMode.Char)
			{
				//Char mode: Start with char. End with command;
				//If request split detect...
				if (splitNow)
				{
					working.Mode = workingType;
					workingResult.Add(working);
					working.Commands = "";
					working.Text = "";
					splitNow = false;
				}
				//If currently char. Keep adding.
				if (!char.IsWhiteSpace(content[0]))
				{
					working.Text += content[0];
					replace4Match(content.Remove(0, 1));
				}
				else if (char.IsWhiteSpace(content[0]))
				{
					//If now is white space...
					//Check what next
					//working.Commands.Add(content[0]);
					if (content.Length > 1)
					{
						if (!char.IsWhiteSpace(content[1]))
						{
							//If next is char.. Request to split now.
							if (content[0] == ' ')
							{
								working.Commands += char.MinValue;
							}
							else
							{
								working.Commands += content[0];
							}
							splitNow = true;
						}
					}
					replace4Match(content.Remove(0, 1));
				}
			}
			else
			{
				//End with char
				//Check if it white space
				//whitespace mode: Start with command. End with char;
				//If request split detect...
				if (splitNow)
				{
					working.Mode = workingType;
					workingResult.Add(working);
					working.Commands = "";
					working.Text = "";
					splitNow = false;
				}
				//If currently whitespace. Keep adding.
				if (char.IsWhiteSpace(content[0]))
				{
					working.Commands += content[0];
					replace4Match(content.Remove(0, 1));
				}
				else if (!char.IsWhiteSpace(content[0]))
				{
					//If now is char...
					//Check what next
					//working.Commands.Add(content[0]);
					if (content.Length > 1)
					{
						if (char.IsWhiteSpace(content[1]))
						{
							//If next is whitespace.. Request to split now.
							if (content[0] == ' ')
							{
								working.Commands += char.MinValue;
							}
							else
							{
								working.Commands += content[0];
							}
							splitNow = true;
						}
					}
					replace4Match(content.Remove(0, 1));
				}
			}
			//Reset
			oneTime = false;
			return workingResult;
		}

		public void LoadFile(string filename, string content)
		{
			//Update title
			currentFilename = filename;
			//Load file normal
			textList = replace4Match(content);
			//Add lastone
			if (workingType == textMode.Char)
			{
				workingResult.Add(working);
			}
			var plainTxt = new List<string>();
			foreach (textlist t in textList)
			{
				plainTxt.Add(t.ToString());
			}
			HashSet<string> hash = new HashSet<string>(plainTxt);
			choiceList.Clear();
			choiceList.AddRange(hash);
			//
			ResetRound();
		}

		public void ResetRound()
		{
			Utilities.newStat = new statInfo();
            Utilities.newStat.noteTitle = currentFilename;
			Utilities.newStat.dateandTime = DateTime.Now.ToString(@"dd MM yyyy - hh:mm:ss");
			Utilities.newStat.currentMode = Utilities.currentSettings.defaultMode;
			progressCounter.Value = 0;
			progressCounter.Maximum = textList.Count;
			currentProgress = -2;
			currentChoiceMode = mode.begin;
			CheckTotalChoice();
			dpTxt.Inlines.Clear();
			timeCounterText.Text = "--:--\r\n--:--";
			wrongCounter.Text = "0";
			for (int i = 0; i < textList.Count; i++)
			{
				Utilities.newStat.wrongPerchoice.Add(0);
			}
			Utilities.newStat.totalLimitTime = Utilities.currentSettings.limitTime;
			Utilities.newStat.totalWords = textList.Count;
			Utilities.newStat.totalChoice = Utilities.currentSettings.totalChoice;
			Utilities.newStat.useTimeLimit = Utilities.currentSettings.isLimitTime;
            prev = DateTime.UtcNow;
			//TODO:Make it support seed system
			randomEngine = new Random();
		}

		enum mode { begin, normal, end}
		mode currentChoiceMode;

		public void CheckTotalChoice()
		{
			//Condition
			//First choice
			if (currentChoiceMode != mode.normal)
			{
				ch0.Visibility = ch1.Visibility = ch2.Visibility = ch3.Visibility = ch4.Visibility = Visibility.Collapsed;
				chImportant.Visibility = Visibility.Visible;
			}
			else if (currentChoiceMode == mode.normal)
			{
				//Other
				ch0.Visibility = Utilities.currentSettings.totalChoice >= 1 ? Visibility.Visible : Visibility.Collapsed;
				ch1.Visibility = Utilities.currentSettings.totalChoice >= 2 ? Visibility.Visible : Visibility.Collapsed;
				ch2.Visibility = Utilities.currentSettings.totalChoice >= 3 ? Visibility.Visible : Visibility.Collapsed;
				ch3.Visibility = Utilities.currentSettings.totalChoice >= 4 ? Visibility.Visible : Visibility.Collapsed;
				ch4.Visibility = Utilities.currentSettings.totalChoice >= 5 ? Visibility.Visible : Visibility.Collapsed;
				chImportant.Visibility = Visibility.Collapsed;
			}
		}
        DateTime prev;
		private async void TimerNow_Tick(object sender, object e)
		{
			if (Utilities.newStat == null)
			{
				//Game not ready yet!
				return;
			}
			//Update time
			if (Utilities.currentSettings.isLimitTime && !pauseMenu.IsPaneOpen && otherPage.Visibility == Visibility.Collapsed)
			{
                Utilities.newStat.totalTime += Convert.ToSingle((DateTime.UtcNow - prev).TotalSeconds);
				timeCounterText.Text = Utilities.newStat.totalTime.ToStringAsTime() + System.Environment.NewLine + Utilities.newStat.totalLimitTime.ToStringAsTime();
                prev = DateTime.UtcNow;
			}
			else
			{
				timeCounterText.Text = "--:--" + Environment.NewLine + "--:--";
			}
			wrongCounter.Text = Utilities.newStat.totalWrong.ToString();
			//Update font display size
			dpTxt.FontSize = Utilities.currentSettings.displayTextSize;
			if (Utilities.newStat.useTimeLimit)
			{
				if (Utilities.newStat.totalTime >= Utilities.newStat.totalLimitTime)
				{
					//OVER
					timerNow.Stop();
					await overMSG.ShowAsync();
					timerNow.Start();
				}
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
            if (otherPage.Visibility == Visibility.Visible)
            {
                return;
            }
			pauseMenu.IsPaneOpen = !pauseMenu.IsPaneOpen;
			Pause = pauseMenu.IsPaneOpen;
			if (pauseMenu.IsPaneOpen)
			{
				pauseInfo.Text = Pause ? "Paused..." : currentFilename;
			}
            contentSlider.Value = Utilities.currentSettings.displayTextSize;
            pauseMenu.PaneBackground.Opacity = 1;
            menuBG.Opacity = 1;
        }

		private void HideSecondPage_Click(object sender, RoutedEventArgs e)
		{
			otherPage.Visibility = Visibility.Collapsed;
			HideSecondPage.Visibility = Visibility.Collapsed;
			Utilities.isSmallLoaderMode = false;
		}

		void ActivateOtherpage()
		{
			otherPage.Visibility = Visibility.Visible;
			HideSecondPage.Visibility = Visibility.Visible;
			Utilities.isSmallLoaderMode = otherPage.Visibility == Visibility.Visible;
		}

		private async void loadOther_Click(object sender, RoutedEventArgs e)
		{
            if (otherPage.Visibility == Visibility.Visible) { return; }
			if (currentProgress > -1)
			{
				await loadotherDiag.ShowAsync();
			}
			else
			{
				Frame.Navigate(typeof(Selector));
			}
		}

		private void loadSession_Click(object sender, RoutedEventArgs e)
        {
            if (otherPage.Visibility == Visibility.Visible) { return; }
            if (Utilities.isSmallLoaderMode)
			{
				return;
			}
			ActivateOtherpage();
			//
			////TODO:Load session page
		}

		private void settingPage_Click(object sender, RoutedEventArgs e)
        {
            if (otherPage.Visibility == Visibility.Visible) { return; }
            if (Utilities.isSmallLoaderMode)
			{
				return;
			}
			ActivateOtherpage();
			//
			otherPage.Navigate(typeof(QuickSettings));
		}

		private async void mainmenuPage_Click(object sender, RoutedEventArgs e)
        {
            if (otherPage.Visibility == Visibility.Visible) { return; }
            if (currentProgress < 1 || currentProgress > textList.Count - 1)
			{
				Frame.Navigate(typeof(MainPage));
				Utilities.isSmallLoaderMode = false;
			}
			else
			{
				await lostPG.ShowAsync();
			}
		}

		private async void exitApp_Click(object sender, RoutedEventArgs e)
        {
            if (otherPage.Visibility == Visibility.Visible) { return; }
            await exitD.ShowAsync();
		}

		public int currentValidChoice;

		int GetRealChoice(List<string> ncl)
		{
			int real = 0;
			foreach (string s in ncl)
			{
				if (s == textList[currentProgress].Text)
				{
					return ncl.IndexOf(s);
				}
			}
			return real;
		}

		void SpawnChoice(int num, string customText)
		{
			//Update choice text
			switch (num)
			{
				case 0:
					ch0.Content = customText;
					break;
				case 1:
					ch1.Content = customText;
					break;
				case 2:
					ch2.Content = customText;
					break;
				case 3:
					ch3.Content = customText;
					break;
				case 4:
					ch4.Content = customText;
					break;
				default:
					break;
			}
		}

		void SpawnNewChoices()
		{
			currentValidChoice = randomEngine.Next(0, Utilities.currentSettings.totalChoice - 1);
			List<string> newchoiceL = new List<string>(choiceList);
			newchoiceL.RemoveAt(GetRealChoice(newchoiceL));
			for (int i = 0; i < Utilities.currentSettings.totalChoice; i++)
			{
				if (i == currentValidChoice)
				{
					SpawnChoice(i, textList[currentProgress].Text);
				}
				else
				{
					int rndch = randomEngine.Next(0, newchoiceL.Count);
					SpawnChoice(i, newchoiceL[rndch]);
					newchoiceL.RemoveAt(rndch);
				}
			}
		}

		public void ChooseChoice(int choice)
		{
			if (choice < 0)
			{
				//Command choice
				if (choice == -1)
				{
					currentProgress = 0;
					SpawnNewChoices();
					currentChoiceMode = mode.normal;
					CheckTotalChoice();
				}
				else if (choice == -5)
				{
					Utilities.currentSettings.stat.Add(Utilities.newStat);
					Frame.Navigate(typeof(End));
				}
			}
			//Check choice number range
			else if (choice > -1)
			{
				//Normal
				//
				//Check if right or wrong
				if (choice == currentValidChoice)
				{
					//It right
					//Update text display
					AddMainText(textList[currentProgress]);
					//Consider to move next
					if (currentProgress + 1 > textList.Count - 1)
					{
						//It can't go anymore
						//Spawn Ending choice;
						currentChoiceMode = mode.end;
						CheckTotalChoice();
					}
					else
					{
						//It still can go. Move next
						currentProgress += 1;
						SpawnNewChoices();
						currentChoiceMode = mode.normal;
						CheckTotalChoice();
					}
				}
				else
				{
					//wrong choice
					if (Utilities.newStat.currentMode == challageMode.Easy)
					{
						//Move to next
						Utilities.newStat.wrongPerchoice[currentProgress] += 1;
						wrongCounter.Text = Utilities.newStat.totalWrong.ToString();
						Utilities.newStat.totalTime += 1;
						//UpdateDisplay
						AddMainText(textList[currentProgress]);
						//Move next
						if (currentProgress + 1 > textList.Count - 1)
						{
							//It can't go anymore
							//Spawn Ending choice;
							CheckTotalChoice();
							ch0.Visibility = ch1.Visibility = ch2.Visibility = ch3.Visibility = ch4.Visibility = Visibility.Collapsed;
							chImportant.Visibility = Visibility.Visible;
						}
						else
						{
							//It still can go. Move next
							currentProgress += 1;
							SpawnNewChoices();
						}
					}
					else if (Utilities.newStat.currentMode == challageMode.Normal)
					{
						//Hide wrong choice
						switch (choice)
						{
							case 0:
								ch0.Visibility = Visibility.Collapsed;
								break;
							case 1:
								ch1.Visibility = Visibility.Collapsed;
								break;
							case 2:
								ch2.Visibility = Visibility.Collapsed;
								break;
							case 3:
								ch3.Visibility = Visibility.Collapsed;
								break;
							case 4:
								ch4.Visibility = Visibility.Collapsed;
								break;
						}
						//It wrong
						Utilities.newStat.wrongPerchoice[currentProgress] += 1;
						wrongCounter.Text = Utilities.newStat.totalWrong.ToString();
						Utilities.newStat.totalTime += 1;
					}
					else if (Utilities.newStat.currentMode == challageMode.Hard)
					{
						ResetRound();
					}
				}
				progressCounter.Value = currentProgress;
			}
            //displayTextScroll.(displayTextScroll.ExtentHeight);
            displayTextScroll.ChangeView(displayTextScroll.HorizontalOffset, displayTextScroll.ExtentHeight, displayTextScroll.ZoomFactor);
		}
		//TODO:Add prenote system???!?!?!!!?
		
		void AddMainText(textlist item)
		{
			//New line check
			bool a = item.Commands.Contains('\r');
			bool b = item.Commands.Contains('\n');
			if (item.Mode == textMode.Char)
			{
				//Start with char. End with commands
				_AddText(item);
				//Add commands
				_AddCommands(item, a, b);				
			}
			else
			{
				//Start with commands. End with char
				//Add commands
				_AddCommands(item, a, b);

				//Add text
				_AddText(item);
			}
			
			//Scroll text to bottom
			//TODO:Add setting... Automatic scroll text to bottom
		}

		void _AddText(textlist item)
		{
			if (Utilities.newStat.wrongPerchoice[currentProgress] > 0)
			{
				//If this choice been wrong
				if (Utilities.currentSettings.showWrongContent)
				{
					dpTxt.Inlines.Add(new Run() { Text = item.Text, Foreground = new SolidColorBrush((Color)Resources["SystemAccentColor"]) });
				}
				else
				{
					dpTxt.Inlines.Add(new Run() { Text = item.Text.ObscureText(), Foreground = new SolidColorBrush((Color)Resources["SystemAccentColor"]) });
				}
			}
			else
			{
				//If it not
				dpTxt.Inlines.Add(new Run() { Text = item.Text });
			}
		}

		void _AddCommands(textlist item,bool a, bool b)
		{
			//Line break determinite
			int mode = 0;
			if (a && b) { mode = 0; }
			else if (a && !b) { /*Linux??*/ mode = 1; }
			else if (!a && b) { /*Mac?*/ mode = 2; }
			foreach (char c in item.Commands.ToCharArray())
			{
				if (c == ' ' || c == '\0')
				{
					dpTxt.Inlines.Add(new Run() { Text = " " });
				}
				if (c == '\t')
				{
					dpTxt.Inlines.Add(new Run() { Text = "    " });
				}
				//Break line
				switch (mode)
				{
					case 0:
						if (c == '\r') { dpTxt.Inlines.Add(new LineBreak()); }
						continue;
					case 1:
						if (c == '\r') { dpTxt.Inlines.Add(new LineBreak()); }
						continue;
					case 2:
						if (c == '\n') { dpTxt.Inlines.Add(new LineBreak()); }
						continue;
				}
			}
		}
		
		private void ch0_Click(object sender, RoutedEventArgs e)
		{
            if (pauseMenu.IsPaneOpen) { return; }
			ChooseChoice(0);
		}

		private void ch1_Click(object sender, RoutedEventArgs e)
        {
            if (pauseMenu.IsPaneOpen) { return; }
            ChooseChoice(1);
		}

		private void ch2_Click(object sender, RoutedEventArgs e)
        {
            if (pauseMenu.IsPaneOpen) { return; }
            ChooseChoice(2);
		}

		private void ch3_Click(object sender, RoutedEventArgs e)
        {
            if (pauseMenu.IsPaneOpen) { return; }
            ChooseChoice(3);
		}

		private void ch4_Click(object sender, RoutedEventArgs e)
        {
            if (pauseMenu.IsPaneOpen) { return; }
            ChooseChoice(4);
		}

		private void chImportant_Click(object sender, RoutedEventArgs e)
		{
			if (currentProgress <= 0)
			{
				ChooseChoice(-1);
			}
			else
			{
				ChooseChoice(-5);
			}
		}

		private async void restartMatch_Click(object sender, RoutedEventArgs e)
        {
            if (otherPage.Visibility == Visibility.Visible) { return; }
            await resetM.ShowAsync();
		}

        private void contentSlider_Loaded(object sender, RoutedEventArgs e)
        {
            contentSlider.Value = Utilities.currentSettings.displayTextSize;
            contentSlider.Minimum = 12;
            contentSlider.Maximum = 72;
        }

        private void contentSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            dpTxt.FontSize = Convert.ToInt32(contentSlider.Value);
            Utilities.currentSettings.displayTextSize = Convert.ToInt32(contentSlider.Value);
            Settings.Save();
        }

        private void contentSlider_GotFocus(object sender, RoutedEventArgs e)
        {
            pauseMenu.PaneBackground.Opacity = 0.5;
            menuBG.Opacity = 0.5;
        }
    }
}