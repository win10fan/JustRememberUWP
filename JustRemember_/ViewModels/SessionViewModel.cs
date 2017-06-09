using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.System.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Windows.UI.Xaml;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Documents;
using Windows.UI.Popups;
using JustRemember.Models;
using JustRemember.Views;
using JustRemember.Services;
using System.Windows.Input;
using JustRemember.Helpers;

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

		protected void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			if (propertyName == nameof(isSessionSaved))
			{
				OnPropertyChanged(nameof(isSessionSaved));
				OnPropertyChanged(nameof(isSessionNotSaved));
			}
		}

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
			timerUI.Start();
		}

		#endregion

		#region Binding Property
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
				current.currentChoice = value;
				//Update binding value when choice changed to next
				OnPropertyChanged(nameof(currentDisplayChoice));
			}
		}

		public int totalChoice
		{
			get { return current.texts.Count; }
		}

		bool issave;
		public bool isSessionNotSaved
		{
			get
			{
				if (currentChoice < 1 || currentChoice > current.texts.Count - 2)
				{
					return false;
				}
				return !issave;
			}
		}
		public bool isSessionSaved
		{
			get
			{
				if (currentChoice < 1 || currentChoice > current.texts.Count - 2)
				{
					return true;
				}
				return issave;
			}
			set
			{
				Set(ref issave, value);
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
				OnPropertyChanged(nameof(isPausing));
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

		public Visibility Choice0Display
		{
			get
			{
				if (current == null) { return Visibility.Visible; }
				//Pass the last choice
				//Current choice is already choose and it wrong
				else if (current.StatInfo.choiceInfo[currentChoice][0])//If it true already answer | Which mean hide it
				{
					return Visibility.Collapsed;
				}
				else
				{
					return Visibility.Visible;
				}
			}
		}

		public Visibility Choice1Display
		{
			get
			{
				if (current == null) { return Visibility.Visible; }
				//Pass the last choice
				//Current choice is already choose and it wrong
				else if (current.StatInfo.choiceInfo[currentChoice][1])//If it true already answer | Which mean hide it
				{
					return Visibility.Collapsed;
				}
				else
				{
					return Visibility.Visible;
				}
			}
		}

		public Visibility Choice2Display
		{
			get
			{
				if (current == null) { return Visibility.Visible; }
				//Pass the last choice
				//Current choice is already choose and it wrong
				else if (current.StatInfo.choiceInfo[currentChoice][2])//If it true already answer | Which mean hide it
				{
					return Visibility.Collapsed;
				}
				else
				{
					return Visibility.Visible;
				}
			}
		}

		public Visibility Choice3Display
		{
			get
			{
				if (current == null) { return Visibility.Visible; }
				//Pass the last choice
				//Current choice is already choose and it wrong
				if (current.maxChoice < 4) { return Visibility.Collapsed; }
				else if (current.currentChoice > current.choices.Count - 1)
				{
					return Visibility.Collapsed;
				}
				//Current choice is already choose and it wrong
				else if (current.StatInfo.choiceInfo[currentChoice][3])//If it true already answer | Which mean hide it
				{
					return Visibility.Collapsed;
				}
				else
				{
					return Visibility.Visible;
				}
			}
		}

		public Visibility Choice4Display
		{
			get
			{
				if (current == null) { return Visibility.Collapsed; }
				if (current.maxChoice < 5) { return Visibility.Collapsed; }
				//Current choice is already choose and it wrong
				else if (current.StatInfo.choiceInfo[currentChoice][4])//If it true already answer | Which mean hide it
				{
					return Visibility.Collapsed;
				}
				else
				{
					return Visibility.Visible;
				}
			}
		}

		////Choice text
		public string Choice0Content
		{
			get
			{
				string title = "";
				if (Config.choiceStyle == choiceDisplayMode.Bottom)
				{
					title = "1: ";
				}
				return $"{title}{current?.choices[currentDisplayChoice].choices[0]}";
			}
		}

		public string Choice1Content
		{
			get
			{
				string title = "";
				if (Config.choiceStyle == choiceDisplayMode.Bottom)
				{
					title = "2: ";
				}
				return $"{title}{current?.choices[currentDisplayChoice].choices[1]}";
			}
		}

		public string Choice2Content
		{
			get
			{
				string title = "";
				if (Config.choiceStyle == choiceDisplayMode.Bottom)
				{
					title = "3: ";
				}
				return $"{title}{current?.choices[currentDisplayChoice].choices[2]}";
			}
		}

		public string Choice3Content
		{
			get
			{
				if (Choice3Display == Visibility.Collapsed)
				{
					return "";
				}
				string title = "";
				if (Config.choiceStyle == choiceDisplayMode.Bottom)
				{
					title = "4: ";
				}
				return $"{title}{current?.choices[currentDisplayChoice].choices[3]}";
			}
		}

		public string Choice4Content
		{
			get
			{
				if (Choice4Display == Visibility.Collapsed)
				{
					return "";
				}
				string title = "";
				if (Config.choiceStyle == choiceDisplayMode.Bottom)
				{
					title = "5: ";
				}
				return $"{title}{current?.choices[currentDisplayChoice].choices[4]}";
			}
		}

		public string totalLimitTime
		{
			get
			{
				if (current == null) { return "--:--"; }
				if (current?.StatInfo?.isTimeLimited == false) { return "--:--"; }
				return $"{current?.StatInfo?.totalLimitTime.Minutes:00}:{current.StatInfo?.totalLimitTime.Seconds}";
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

		public ICommand BackToMainMenu;
		public ICommand SaveSession;
		public ICommand PauseFunc;
		public ICommand UnPauseFunc;
		public ICommand Choose1;
		public ICommand Choose2;
		public ICommand Choose3;
		public ICommand Choose4;
		public ICommand Choose5;

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

			//Dialogs
			confirm = new MessageDialog("Do you want to leave this session?", "Confirm");
			confirm.Commands.Add(new UICommand()
			{
				Invoked = delegate { NavigationService.GoBack(); },
				Label = "Yes"
			});
			confirm.Commands.Add(new UICommand("No"));
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
			if (isSessionSaved) { return; }
			isSessionSaved = true;
			await SavedSessionModel.AddNew(current);
		}

		private async void BACKTOMAINMENU(RoutedEventArgs obj)
		{
			if (isSessionNotSaved)
			{
				NavigationService.GoBack();
				return;
			}
			await confirm.ShowAsync();
		}
		#endregion

		#region Function
		DispatcherTimer timerUI;
		void SetTimer()
		{
			if (current.StatInfo.isTimeLimited)
			{
				timerUI = new DispatcherTimer()
				{
					Interval = TimeSpan.FromSeconds(1)
				};
				timerUI.Tick += TimerUI_Tick;
			}
			else
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(spendedTime)));
			}
		}

		private void TimerUI_Tick(object sender, object e)
		{
			spendTime = spendTime.Add(TimeSpan.FromSeconds(1));
		}

		public async void Choose(int choice)
		{
			//Normal choice			
			if (choice != current.choices[currentChoice].corrected)
			{
				//Wrong choice
				switch (current.StatInfo.setMode)
				{
					case matchMode.Easy:
						//Mark this choice as wrong
						current.StatInfo.choiceInfo[currentChoice][choice] = true;
						//Advance time up little bit as wrong choice has been selected
						current.StatInfo.totalTimespend.Add(TimeSpan.FromSeconds(2));
						//Update choice text
						UpdateText(false);
						//Update progress
						if (current.currentChoice + 1 == current.texts.Count)
						{
							//TODO:Go to end page
							break;
						}
						break;
					case matchMode.Normal:
						//Mark selected choice as wrong
						current.StatInfo.choiceInfo[currentChoice][choice] = true;
						//Advance time up little bit as wrong choice has been selected
						current.StatInfo.totalTimespend.Add(TimeSpan.FromSeconds(5));
						break;
					case matchMode.Hard:
						//Reset round
						//TODO:Reset match
						break;
				}
			}
			else if (choice == current.choices[currentChoice].corrected)
			{
				//Corrent choice
				//Update display text
				UpdateText(true);
				//Check that choice that has been selected is final
				if (currentChoice + 1 > current.texts.Count)
				{
					//This is the end
					//TODO:Go to end page
				}
			}
			//
			if (current.currentChoice > current.texts.Count - 1)
			{
				//TODO:Do what depend on setting
				switch (Config.AfterFinalChoice)
				{
					case whenFinalChoice.EndPage:
						//TODO:Kick user to end page
						break;
					case whenFinalChoice.Restart:
						//TODO:Restart match
						break;
					case whenFinalChoice.BackHome:
						//TODO:Return to main page
						break;
				}
			}
			else
			{
				currentChoice += 1;
			}
			totalWrong = current.StatInfo.GetTotalWrong();
			//Update choice text
			OnPropertyChanged(nameof(Choice0Content));
			OnPropertyChanged(nameof(Choice1Content));
			OnPropertyChanged(nameof(Choice2Content));
			OnPropertyChanged(nameof(Choice3Content));
			OnPropertyChanged(nameof(Choice4Content));
			issave = await SavedSessionModel.isExist(current);
			OnPropertyChanged(nameof(isSessionNotSaved));
		}

		/// <summary>
		/// Update display text
		/// </summary>
		public void UpdateText(bool IsItRightChoice)
		{
			int currentChoiceInternal = currentChoice - 1;
			if (currentChoiceInternal < 0) { currentChoiceInternal = 0; }
			//Check if choice has been added
			while (currentChoiceInternal > current.selectedChoices.Count - 1)
			{
				//No choice made yet
				current.selectedChoices.Add(new SelectedChoices());
				OnPropertyChanged("choicesSelected");
			}
			//
			if (IsItRightChoice)
			{
				if (current.StatInfo.setMode == matchMode.Easy)
				{
					//Choose the right choice in easy
					//Update that selected
					current.selectedChoices[currentChoiceInternal].finalText = current.texts[currentChoiceInternal].text;
				}
				if (current.StatInfo.setMode == matchMode.Normal)
				{
					//Choose the right choice in Normal
					//Check if it was right on the first time or not
					bool wrongBefore = false;
					foreach (bool v in current.StatInfo.choiceInfo[currentChoiceInternal])
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
						current.selectedChoices[currentChoiceInternal].finalText = current.texts[currentChoiceInternal].text.obfuscateText();
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
						current.selectedChoices[currentChoiceInternal].finalText = current.texts[currentChoiceInternal].text.obfuscateText();
					}
					else
					{
						current.selectedChoices[currentChoiceInternal].finalText = current.texts[currentChoiceInternal].text;
					}
					current.selectedChoices[currentChoiceInternal].isItWrong = true;
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
			latestChoices = current.selectedChoices[currentChoiceInternal];
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
		}

		public void AddTextDisplay(int at)
		{
			view.displayTXT.Inlines.Add(new Run
			{
				Text = current.selectedChoices[at].finalText,
				Foreground = current.selectedChoices[at].mark
			});
		}
		#endregion
	}
}