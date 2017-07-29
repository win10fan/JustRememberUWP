using System;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Documents;
using Windows.UI.Popups;
using JustRemember.Models;
using JustRemember.Views;
using JustRemember.Services;
using System.Windows.Input;
using JustRemember.Helpers;
using System.Globalization;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace JustRemember.ViewModels
{
	public class SessionViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
		{
			if (Equals(storage, value))
			{
				return;
			}
			storage = value;
			OnPropertyChanged(propertyName);
		}

		protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		public Match view;

		SessionModel _ses;
		public SessionModel current
		{
			get => _ses;
			set
			{
				Set(ref _ses, value);
			}
		}

		public AppConfigModel Config
		{
			get => App.Config;
			set => App.Config = value;
		}

		#region Initialize
		public void RestoreSession()
		{
			isStillNotSaveSession = true;
			if (!current.isNew)
			{
				//This is an old session saved
				//Need to restore it
				if (!current.isNew)
				{
					//Reload text display
					for (int i = 0; i < currentChoice; i++)
					{
						AddTextDisplay(i);
					}
				}
			}
			SetTimer();
			isPausing = false;
			UNPAUSEFUNC(null);
			InitializeCommands();
			GetDesc();
			if (current.isNew)
				UpdateUI(1, 0);
			else
				UpdateUI(currentDisplayChoice, currentChoice);
			if (Config.antiSpamChoice)
			{
				lastChoose = DateTime.Now;
			}
			if (current.StatInfo.correctedChoice.Count < 1)
			{
				foreach (var c in current.choices)
				{
					current.StatInfo.correctedChoice.Add(c.corrected);
				}
			}
		}

		public AudioDescriptor Description;
		public async void GetDesc()
		{
			if (current.SelectedNote.hasDesc)
			{
				if (current.SelectedNote.isPrenote)
				{
					Description = await Json.ToObjectAsync<AudioDescriptor>(await FileIO.ReadTextAsync(await current.SelectedNote.GetDescription()));
					Play(current.SelectedNote.descAudi);
					InitializeQTM();
					return;
				}
				Description = await Json.ToObjectAsync<AudioDescriptor>(await FileIO.ReadTextAsync(await current.SelectedNote.GetDescription()));
				StorageFile audi = await (await ApplicationData.Current.RoamingFolder.GetFolderAsync("Description")).GetFileAsync(Description.audioName);
				Play(audi);
				InitializeQTM();
			}
		}
		#endregion

		#region Binding Property
		public bool isMuted
		{
			get => Config.MuteDescription;
			set
			{
				Config.MuteDescription = value;
				player.Volume = value ? 0 : 100;
				OnPropertyChanged(nameof(isMuted));
			}
		}

		public GridLength choice4Avialable
		{
			get => Choice3Display == Visibility.Visible ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
		}

		public GridLength choice5Avialable
		{
			get => Choice4Display == Visibility.Visible ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
		}
		
		public string writeUp
		{
			get => "";
			set
			{
				if (value.Length == current.choices[currentChoice].choices[current.choices[currentChoice].corrected].Length)
				{
					if (value == current.choices[currentChoice].choices[current.choices[currentChoice].corrected])
					{
						Choose(current.choices[currentChoice].corrected);
					}
					else
					{
						Choose(GetNonCorrect());
					}
					value = "";
					view.writebx.Focus(FocusState.Keyboard);
					view.writebx.Text = "";
				}
				else
				{
					Choose(GetNonCorrect());
				}
			}
		}

		public int GetNonCorrect()
		{
			Random r = new Random();
			int i = r.Next(0, current.maxChoice);
			while (i == current.choices[currentChoice].corrected)
			{
				i = r.Next(0, current.maxChoice);
			}
			return i;
		}

		public string beginTimeSTR
		{
			get
			{
				if (current == null) { return ""; }
				CultureInfo culture = CultureInfo.CurrentCulture;
				return current.StatInfo.beginTime.ToString("G", culture);
			}
		}

		public Visibility detailStat { get; set; } = Visibility.Collapsed;

		public int FontSize
		{
			get
			{
				return Config.displayTextSize;
			}
			set
			{
				Config.displayTextSize = value;
				OnPropertyChanged(nameof(FontSize));
			}
		}
		/// <summary>
		/// This choice use for binding on display which can't be -1
		/// </summary>
		public int currentDisplayChoice { get { return current.currentChoice + 1; } }

		public int currentChoice
		{
			get
			{
				if (current == null) { return 0; }
				if (current.currentChoice < 0) { return 0; }
				if (current.currentChoice > current.texts.Count - 1) { return current.texts.Count - 1; }
				return current.currentChoice;
			}
			set
			{
				if ((value - current.currentChoice) > 0)
				{
					//Get to next
					UpdateUI(value + 1, value);
				}
				current.currentChoice = value;
				//Update binding value when choice changed to next
				OnPropertyChanged(nameof(currentDisplayChoice));
			}
		}

		public int totalChoice
		{
			get { return current.texts.Count; }
		}
		
		public bool pausingPC
		{
			get
			{
				if (view.scSize >= App.Config.halfResolution)
				{
					return isPausing;
				}
				return false;
			}
			set
			{
				isPausing = value;
			}
		}

		bool _p;
		public bool isPausing
		{
			get
			{
				return _p;
			}
			set
			{
				if (value)
				{

				}
				else
				{

				}
				Set(ref _p, value);
				OnPropertyChanged(nameof(pausingPC));
			}
		}

		public TimeSpan spendTime
		{
			get
			{
				return _ses.StatInfo.totalTimespend;
			}
			set
			{
				Set(ref _ses.StatInfo.totalTimespend, value);
				OnPropertyChanged(nameof(spendedTime));
			}
		}

		public string spendedTime
		{
			get
			{
				if (!current.StatInfo.isTimeLimited) { return "--:--"; }
				return $"{spendTime.Minutes:00}:{spendTime.Seconds:00}";
			}
		}

		public ObservableCollection<SelectedChoices> choicesSelected
		{
			get
			{
				return _ses.selectedChoices;
			}
			set
			{
				_ses.selectedChoices = value;
				OnPropertyChanged(nameof(choicesSelected));
			}
		}

		int wrongCount;
		public int totalWrong
		{
			get
			{
				if (current == null) { return 0; }
				return wrongCount;
			}
			set
			{
				Set(ref wrongCount, value);
				OnPropertyChanged(nameof(totalWrong));
			}
		}

		//Choice visibility
		Visibility _0v, _1v, _2v, _3v, _4v;
		public Visibility Choice0Display { get => _0v; set => Set(ref _0v, value); }
		public Visibility Choice1Display { get => _1v; set => Set(ref _1v, value); }
		public Visibility Choice2Display { get => _2v; set => Set(ref _2v, value); }
		public Visibility Choice3Display { get => _3v; set => Set(ref _3v, value); }
		public Visibility Choice4Display { get => _4v; set => Set(ref _4v, value); }

		//Choice text
		string _0, _1, _2, _3, _4;
		public string Choice0Content
		{
			get => _0;
			set => Set(ref _0, value);
		}

		public string Choice1Content
		{
			get => _1;
			set => Set(ref _1, value);
		}

		public string Choice2Content
		{
			get => _2;
			set => Set(ref _2, value);
		}

		public string Choice3Content
		{
			get => _3;
			set => Set(ref _3, value);
		}

		public string Choice4Content
		{
			get => _4;
			set => Set(ref _4, value);
		}

		public string totalLimitTime
		{
			get
			{
				if (current == null) { return "--:--"; }
				if (current?.StatInfo?.isTimeLimited == false) { return "--:--"; }
				return $"{current?.StatInfo?.totalLimitTime.Minutes:00}:{current.StatInfo?.totalLimitTime.Seconds:00}";
			}
		}

		public Visibility isItCenterMode
		{
			get
			{
				return Config.choiceStyle == choiceDisplayMode.Center ? Visibility.Visible : Visibility.Collapsed;
			}
		}
		public Visibility isItBottomMode
		{
			get
			{
				return Config.choiceStyle == choiceDisplayMode.Bottom ? Visibility.Visible : Visibility.Collapsed;
			}
		}
		public Visibility isItWriteMode
		{
			get
			{
				return Config.choiceStyle == choiceDisplayMode.Write ? Visibility.Visible : Visibility.Collapsed;
			}
		}
		#endregion

		#region Commands & Dialogs
		MessageDialog confirm;
		MessageDialog resetmatch;
		MessageDialog notyet;
		MessageDialog timeup;

		public ICommand BackToMainMenu;
		public ICommand SaveSession;
		public ICommand PauseFunc;
		public ICommand UnPauseFunc;
		public ICommand Choose1;
		public ICommand Choose2;
		public ICommand Choose3;
		public ICommand Choose4;
		public ICommand Choose5;
		public ICommand RestartMatch;
		public ICommand ToggleDetailStatus;
		public ICommand DebugChoose;

		public void InitializeCommands()
		{
			BackToMainMenu = new RelayCommand<RoutedEventArgs>(BACKTOMAINMENU);
			SaveSession = new RelayCommand<RoutedEventArgs>(SAVESESSION);
			PauseFunc = new RelayCommand<RoutedEventArgs>(PAUSEFUNC);
			UnPauseFunc = new RelayCommand<RoutedEventArgs>(UNPAUSEFUNC);
			Choose1 = new RelayCommand<RoutedEventArgs>(CHOOSE1);
			Choose2 = new RelayCommand<RoutedEventArgs>(CHOOSE2);
			Choose3 = new RelayCommand<RoutedEventArgs>(CHOOSE3);
			Choose4 = new RelayCommand<RoutedEventArgs>(CHOOSE4);
			Choose5 = new RelayCommand<RoutedEventArgs>(CHOOSE5);
			RestartMatch = new RelayCommand<RoutedEventArgs>(RESTARTMATCH);
			ToggleDetailStatus = new RelayCommand<RoutedEventArgs>(TOGGLEDETAILSTATUS);
			DebugChoose = new RelayCommand<RoutedEventArgs>(DEBUGCHOOSE);

			//Dialogs
			confirm = new MessageDialog(
				App.language.GetString("Match_dialog_confirm_content"), 
				App.language.GetString("Match_dialog_confirm_title"));
			confirm.Commands.Add(new UICommand()
			{
				Invoked = delegate { NavigationService.GoBack(); },
				Label = App.language.GetString("Match_dialog_yes")
			});
			confirm.Commands.Add(new UICommand(App.language.GetString("Match_dialog_no")));

			resetmatch = new MessageDialog(
				App.language.GetString("Match_dialog_reset_content"),
				App.language.GetString("Match_dialog_reset_title"));
			resetmatch.Commands.Add(new UICommand()
			{
				Invoked = delegate
				{
					Restart();
					ReloadAudio();
				},
				Label = App.language.GetString("Match_dialog_yes")
			});
			resetmatch.Commands.Add(new UICommand(App.language.GetString("Match_dialog_no")));

			notyet = new MessageDialog(App.language.GetString("Match_dialog_notyet_content"));
			notyet.Commands.Add(new UICommand(App.language.GetString("Match_dialog_ok")));

			timeup = new MessageDialog(
				App.language.GetString("Match_dialog_timeup_content"),
				App.language.GetString("Match_dialog_timeup_title"));
			timeup.Commands.Add(new UICommand()
			{
				Invoked = delegate
				{
					Restart();
				},
				Label = App.language.GetString("Match_dialog_ok")
		});
		}
		
		private async void DEBUGCHOOSE(RoutedEventArgs obj)
		{
			Config.antiSpamChoice = false;
			Random rnd = new Random();
			while (currentChoice < totalChoice - 4)
			{
				if (isPausing)
				{
					return;
				}
				Choose(rnd.Next(0, current.maxChoice));
				await Task.Delay(50);
			}
			Config.antiSpamChoice = true;
		}

		private void TOGGLEDETAILSTATUS(RoutedEventArgs obj)
		{
			detailStat = detailStat == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
			OnPropertyChanged(nameof(detailStat));
		}

		private async void RESTARTMATCH(RoutedEventArgs obj)
		{
			if (currentChoice < 2) { await notyet.ShowAsync(); return; }
			if (!isStillNotSaveSession)
			{
				Restart();
				ReloadAudio();
				return;
			}
			else
			{
				await resetmatch.ShowAsync();
			}
		}

		private void CHOOSE1(RoutedEventArgs obj)
		{
			Choose(0);
		}

		private void CHOOSE2(RoutedEventArgs obj)
		{
			Choose(1);
		}

		private void CHOOSE3(RoutedEventArgs obj)
		{
			Choose(2);
		}

		private void CHOOSE4(RoutedEventArgs obj)
		{
			Choose(3);
		}

		private void CHOOSE5(RoutedEventArgs obj)
		{
			Choose(4);
		}

		private async void PAUSEFUNC(RoutedEventArgs obj)
		{
			isPausing = true;
			timerUI?.Stop();
			view.UnPause.Stop();
			view.Pause.Begin();
			await Task.Delay(500);
			view.displayTXT.VerticalAlignment = VerticalAlignment.Top;
		}

		private async void UNPAUSEFUNC(RoutedEventArgs obj)
		{
			isPausing = false;
			timerUI?.Start();
			view.Pause?.Stop();
			view.UnPause?.Begin();
			await Task.Delay(500);
			view.displayTXT.VerticalAlignment = VerticalAlignment.Bottom;
		}

		private async void SAVESESSION(RoutedEventArgs obj)
		{
			if (currentChoice < 2) { await notyet.ShowAsync(); return; }
			if (!isStillNotSaveSession) { return; }
			isStillNotSaveSession = false;
			await SavedSessionModel.AddNew(current);
			await Task.Delay(100);
			OnPropertyChanged(nameof(isStillNotSaveSession));
		}
		public bool isStillNotSaveSession;
		private async void BACKTOMAINMENU(RoutedEventArgs obj)
		{
			if (!isStillNotSaveSession)
			{
				NavigationService.GoBack();
				return;
			}
			await confirm.ShowAsync();
		}
		#endregion

		#region Function
		public DispatcherTimer timerUI;
		void SetTimer()
		{
			timerUI = new DispatcherTimer()
			{
				Interval = TimeSpan.FromSeconds(1)
			};
			timerUI.Tick += TimerUI_Tick;
			timerUI.Start();
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(spendedTime)));
		}

		private async void TimerUI_Tick(object sender, object e)
		{
			if (current.StatInfo.isTimeLimited)
			{
				spendTime = spendTime.Add(TimeSpan.FromSeconds(1));
				if (spendTime > current.StatInfo.totalLimitTime)
				{
					timerUI.Stop();
					await timeup.ShowAsync();
				}
			}
			if (Config.antiSpamChoice)
				OnPropertyChanged(nameof(BlindThePage));
			if (BlindThePage == Visibility.Visible)
			{
				Restart();
			}
		}

		public Visibility BlindThePage
		{
			get
			{
				if (AnnoyPlayer.isPlaying)
				{
					return Visibility.Visible;
				}
				return Visibility.Collapsed;
			}
		}
		
		public DateTime lastChoose;
		public bool allowNext;
		int tooFast = 5;
		public void Choose(int choice)
		{
			if (isPausing) { return; }
			if (Config.antiSpamChoice)
			{ 
				if (tooFast >= 1)
				{
					tooFast -= 1;
					lastChoose = DateTime.Now;
				}
				else if (tooFast <= 0)
				{
					if (DateTime.Now - lastChoose < TimeSpan.FromMilliseconds(150))
					{
						//Snooze
						AnnoyPlayer.Play();
						OnPropertyChanged(nameof(BlindThePage));
					}
					tooFast = 5;
				}
			}
			if (choice != current.choices[currentChoice].corrected)
			{
				//Wrong choice
				switch (current.StatInfo.setMode)
				{
					case matchMode.Easy:
						//Mark this choice as wrong
						current.StatInfo.choiceInfo[currentChoice][choice] = true;
						//Advance time up little bit as wrong choice has been selected
						spendTime.Add(TimeSpan.FromSeconds(1));
						//Update choice text
						UpdateText(false);
						break;
					case matchMode.Normal:
						//Mark selected choice as wrong
						current.StatInfo.choiceInfo[currentChoice][choice] = true;
						//Advance time up little bit as wrong choice has been selected
						spendTime.Add(TimeSpan.FromSeconds(3));
						allowNext = false;
						break;
					case matchMode.Hard:
						//Reset round
						Restart();
						break;
				}
			}
			else if (choice == current.choices[currentChoice].corrected)
			{
				if (current.StatInfo.setMode == matchMode.Normal)
				{
					allowNext = true;
				}
				//Corrent choice
				//Update display text
				UpdateText(true);
			}
			//
			if (current.currentChoice == current.texts.Count - 1)
			{
				//Do what depend on setting
				switch (Config.AfterFinalChoice)
				{
					case whenFinalChoice.EndPage:
						EndMatch();
						break;
					case whenFinalChoice.Restart:
						if (App.Config.saveStatAfterEnd)
						{
							if (!App.Config.useSeed)
							{
								StatModel.Set(current.StatInfo);
							}
						}
						Restart();
						break;
					case whenFinalChoice.BackHome:
						if (App.Config.saveStatAfterEnd)
						{
							if (!App.Config.useSeed)
							{
								StatModel.Set(current.StatInfo);
							}
						}
						NavigationService.Navigate<MainPage>();
						break;
				}
				ReloadAudio();
			}
			else
			{
				if (current.StatInfo.setMode == matchMode.Normal)
				{
					if (allowNext)
					{
						currentChoice += 1;
					}
				}
				else
				{
					currentChoice += 1;
				}
			}
			totalWrong = current.StatInfo.GetTotalWrong();
			//Update choice text
			OnPropertyChanged(nameof(Choice0Content));
			OnPropertyChanged(nameof(Choice1Content));
			OnPropertyChanged(nameof(Choice2Content));
			OnPropertyChanged(nameof(Choice3Content));
			OnPropertyChanged(nameof(Choice4Content));
		}

		async void ReloadAudio()
		{
			StorageFile audi = await (await ApplicationData.Current.RoamingFolder.GetFolderAsync("Description")).GetFileAsync(Description.audioName);
			Play(audi);
		}

		void EndMatch()
		{
			var bkp = current.StatInfo;
			App.cachedSession = SessionModel.generate(current.SelectedNote, current);
			NavigationService.Navigate<End>(bkp);
		}

		/// <summary>
		/// Update display text
		/// </summary>
		public void UpdateText(bool IsItRightChoice)
		{
			while (currentChoice > current.selectedChoices.Count - 1)
			{
				//No choice made yet
				current.selectedChoices.Add(new SelectedChoices());
				OnPropertyChanged(nameof(choicesSelected));
			}
			//
			if (IsItRightChoice)
			{
				if (current.StatInfo.setMode == matchMode.Easy)
				{
					//Choose the right choice in easy
					//Update that selected
					current.selectedChoices[currentChoice].finalText = current.texts[currentChoice].text;
				}
				if (current.StatInfo.setMode == matchMode.Normal)
				{
					//Choose the right choice in Normal
					//Check if it was right on the first time or not
					bool wrongBefore = false;
					foreach (bool v in current.StatInfo.choiceInfo[currentChoice])
					{
						if (v)
						{
							//It has wrong choice before
							wrongBefore = true;
						}
					}
					if (wrongBefore)
					{
						//Obfuscate the text
						current.selectedChoices[currentChoice].finalText = current.texts[currentChoice].text.obfuscateText();
					}
				}
			}
			else
			{
				//Choose the wrong choice
				if (current.StatInfo.setMode == matchMode.Easy)
				{
					//Choose the wrong choice in easy
					//Update that selected
					if (Config.obfuscateWrongText)
					{
						current.selectedChoices[currentChoice].finalText = current.texts[currentChoice].text.obfuscateText();
					}
					else
					{
						current.selectedChoices[currentChoice].finalText = current.texts[currentChoice].text;
					}
					current.selectedChoices[currentChoice].isItWrong = true;
				}
				else
				{
					//Choose wrong choice on normal
					//Hard mode would not be at this point as it force to reset match before anything else
					//Do nothing anyway though
					return;
				}
			}
			//view.controls.ItemsSource = choicesSelected;
			latestChoices = current.selectedChoices[currentChoice];
		}

		SelectedChoices _chLT;
		public SelectedChoices latestChoices
		{
			get
			{
				return _chLT;
			}
			set { _chLT = value; AddTextDisplay(); }
		}

		public void AddTextDisplay()
		{
			view.displayTXT.Inlines.Add(new Run
			{
				Text = latestChoices.finalText,
				Foreground = latestChoices.mark
			});
			if (App.Config.autoScrollContent)
			{
				view.displayTexts.ChangeView(view.displayTexts.HorizontalOffset, view.displayTexts.ExtentHeight, view.displayTexts.ZoomFactor);
			}
		}

		public void AddTextDisplay(int at)
		{
			view.displayTXT.Inlines.Add(new Run
			{
				Text = current.selectedChoices[at].finalText,
				Foreground = current.selectedChoices[at].mark
			});
		}

		string t1 { get => App.language.GetString("Match_choice_1"); }
		string t2 { get => App.language.GetString("Match_choice_2"); }
		string t3 { get => App.language.GetString("Match_choice_3"); }
		string t4 { get => App.language.GetString("Match_choice_4"); }
		string t5 { get => App.language.GetString("Match_choice_5"); }
		/// <summary>
		/// Update the UI including choices visibility etc.
		/// </summary>
		/// <param name="visual">The number of current progress in display</param>
		/// <param name="bg">The number of current progress with -1</param>
		public void UpdateUI(int visual, int bg)
		{
			if (bg == current.texts.Count)
			{
				bg -= 1;
				visual -= 2;
			}
			Choice0Content = current?.choices[bg].choices.Count >= 1 ? (Config.choiceStyle == choiceDisplayMode.Center ? current?.choices[bg].choices[0] : $"{t1}{current?.choices[bg].choices[0]}") : "";
			Choice1Content = current?.choices[bg].choices.Count >= 2 ? (Config.choiceStyle == choiceDisplayMode.Center ? current?.choices[bg].choices[1] : $"{t2}{current?.choices[bg].choices[1]}") : "";
			Choice2Content = current?.choices[bg].choices.Count >= 3 ? (Config.choiceStyle == choiceDisplayMode.Center ? current?.choices[bg].choices[2] : $"{t3}{current?.choices[bg].choices[2]}") : "";
			Choice3Content = current?.choices[bg].choices.Count >= 4 ? (current.maxChoice < 4 ? "" : Config.choiceStyle == choiceDisplayMode.Center ? current?.choices[bg].choices[3] : $"{t4}{current?.choices[bg].choices[3]}") : "";
			Choice4Content = current?.choices[bg].choices.Count >= 5 ? (current.maxChoice < 5 ? "" : Config.choiceStyle == choiceDisplayMode.Center ? current?.choices[bg].choices[4] : $"{t5}{current?.choices[bg].choices[4]}") : "";
			//
			Choice0Display = current?.choices[bg].choices.Count >= 1 ? (current.StatInfo.choiceInfo[bg][0] ? Visibility.Collapsed : Visibility.Visible) : Visibility.Collapsed;
			Choice1Display = current?.choices[bg].choices.Count >= 2 ? (current.StatInfo.choiceInfo[bg][1] ? Visibility.Collapsed : Visibility.Visible) : Visibility.Collapsed;
			Choice2Display = current?.choices[bg].choices.Count >= 3 ? (current.StatInfo.choiceInfo[bg][2] ? Visibility.Collapsed : Visibility.Visible) : Visibility.Collapsed;
			Choice3Display = current?.choices[bg].choices.Count >= 4 ? (current.maxChoice < 4 ? Visibility.Collapsed : (current.StatInfo.choiceInfo[bg][3] ? Visibility.Collapsed : Visibility.Visible)) : Visibility.Collapsed;
			Choice4Display = current?.choices[bg].choices.Count >= 5 ? (current.maxChoice < 5 ? Visibility.Collapsed : (current.StatInfo.choiceInfo[bg][4] ? Visibility.Collapsed : Visibility.Visible)) : Visibility.Collapsed;
			//
			if (Config.choiceStyle == choiceDisplayMode.Bottom)
			{
				OnPropertyChanged(nameof(choice4Avialable));
				OnPropertyChanged(nameof(choice5Avialable));
			}
		}

		public void Restart()
		{
			current.currentChoice = -1;
			ReloadAudio();
			current = SessionModel.generate(current.SelectedNote, current);
			view.displayTXT.Inlines.Clear();
			isPausing = false;
			UNPAUSEFUNC(null);
			UpdateUI(1, 0);
			OnPropertyChanged(nameof(currentDisplayChoice));
			totalWrong = 0;
			OnPropertyChanged(nameof(totalWrong));
		}
		#endregion

		public MediaElement player { get => view.Player; set => view.Player = value; }
		public async void Play(StorageFile file)
		{
			var stream = await file.OpenAsync(FileAccessMode.Read);
			player.SetSource(stream, file.ContentType);
		}

		DispatcherTimer quickerTimer;
		public void InitializeQTM()
		{
			quickerTimer = new DispatcherTimer()
			{
				Interval = TimeSpan.FromMilliseconds(1)
			};
			quickerTimer.Tick += QuickerTimer;
			quickerTimer.Start();
		}

		private void QuickerTimer(object sender, object e)
		{
			if (currentChoice - 1 < 0)
			{
				player.Pause();
				return;
			}
			if (player.Position.TotalMilliseconds > Description.Splits[currentChoice - 1].Time.TotalMilliseconds)
			{
				player.Pause();
				return;
			}
			player.Play();
		}

	}
}