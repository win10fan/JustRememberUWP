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
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(currentChoice)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(isPausing)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(totalWrong)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(totalText)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(choicesSelected)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(sessionButtonColor)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(isItBottomMode)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(isItCenterMode)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(isItWriteMode)));
			//Update choice buttons
			//Update choice visibility
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Choice0Display)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Choice1Display)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Choice2Display)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Choice3Display)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Choice4Display)));
			//Update choice text
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Choice0Content)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Choice1Content)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Choice2Content)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Choice3Content)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Choice4Content)));
			if (Config?.autoScrollContent == true)
			{
				view?.displayTexts?.ChangeView(view.displayTexts.HorizontalOffset, view.displayTexts.ExtentHeight, view.displayTexts.ZoomFactor);
			}
			Config?.Save();
		}

		public string totalText
		{
			get
			{
				if (current == null)
				{
					return "??";
				}
				return current.texts.Count.ToString();
			}
		}

		public void Update()
		{
			OnPropertyChanged("current");
		}

		SessionModel _session;
		public SessionModel current
		{
			get => _session;
			set => Set(ref _session, value);
		}

		AppConfigModel _c = new AppConfigModel();
		public AppConfigModel Config
		{
			get => _c;
			set => Set(ref _c, value);
		}

		public Match view;

		ThreadPoolTimer timer;
		void SetTimer()
		{
			if (current.StatInfo.isTimeLimited)
			{
				timer = ThreadPoolTimer.CreatePeriodicTimer((t) =>
				{
		//Count timer
		current.StatInfo.totalTimespend.Add(TimeSpan.FromSeconds(1));
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(spendedTime)));
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(totalLimitTime)));
				}, TimeSpan.FromSeconds(1));
			}
			else
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(spendedTime)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(totalLimitTime)));
			}
		}

		/// <summary>
		/// This choice use for binding on display which can't be -1
		/// </summary>
		public int currentChoice
		{
			get
			{
				if (current == null) { return 0; }
				if (current.currentChoice < 0)
				{
					return 0;
				}
				if (current.currentChoice > current.texts.Count - 1)
				{
					return current.texts.Count - 1;
				}
				return current.currentChoice;
			}
			set
			{
				if ((value - current.currentChoice) > 1)
				{
					value = current.currentChoice + 1;
					current.currentChoice = value;
				}
			}
		}

		public void InitializeCommands()
		{
			while (current == null)
			{
				Task.Delay(5);
			}
			OnPropertyChanged(nameof(current));
			//Dialogs
			wantToLeave = new MessageDialog("Do you want to leave this session?", "Confirm");
			wantToLeave.Commands.Add(new UICommand()
			{
				Invoked = delegate { NavigationService.Navigate<MainPage>(); },
				Label = "Yes"
			});
			wantToLeave.Commands.Add(new UICommand("No"));
		}

		public async void SaveToSessionList()
		{
			if (isSessionSaved) { return; }
			isSessionSaved = true;
			await SavedSessionModel.AddNew(current);
		}

		public MessageDialog wantToLeave;
		public async void KickToMainPage()
		{
			if (isSessionSaved)
			{
				NavigationService.Navigate<MainPage>();
				return;
			}
			await wantToLeave.ShowAsync();
		}

		public bool isSessionSaved;

		public Visibility sessionButtonColor
		{
			get
			{
				if (isSessionSaved) { return Visibility.Visible; }
				return Visibility.Collapsed;
			}
		}

		public ObservableCollection<SelectedChoices> choicesSelected
		{
			get
			{
				return current.selectedChoices;
			}
			set
			{
				current.selectedChoices = value;
			}
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

		public void UpdateText(bool IsItRightChoice)
		{
			//Check if choice has been added
			if (current.selectedChoices.Count - 1 < current.currentChoice)
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
			Update();
			//view.controls.ItemsSource = choicesSelected;
			latestChoices = current.selectedChoices[currentChoice];
		}

		public void Choose(int choice)
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
							//Let UI show final button
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
				//TODO:Update display text in MVVM method
				UpdateText(true);
				//Check that choice that has been selected is final
				if (currentChoice + 1 > current.texts.Count)
				{
					//This is the end
					//Let UI update
					OnPropertyChanged("current");
				}
				else
				{
					//Keep on struggle to the next choice
					currentChoice += 1;
					OnPropertyChanged("current");
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
				current.currentChoice += 1;
			}
			totalWrong = current.StatInfo.GetTotalWrong();
			OnPropertyChanged("current");
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

		//Choice text
		public string Choice0Content
		{
			get
			{
				return current?.choices[currentChoice].choices[0];
			}
		}

		public string Choice1Content
		{
			get
			{
				return current?.choices[currentChoice].choices[1];
			}
		}

		public string Choice2Content
		{
			get
			{
				return current?.choices[currentChoice].choices[2];
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
				return current?.choices[currentChoice].choices[3];
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
				return current?.choices[currentChoice].choices[4];
			}
		}

		public void RestoreSession()
		{
			if (App.Config == null)
			{
				Debug.Write("Fuuuuuuu");
				Application.Current.Exit();
			}
			Config = App.Config;
			InitializeCommands();
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
					timer?.Cancel();
					view.UnPause.Stop();
					view.Pause.Begin();
					view.displayTXT.VerticalAlignment = VerticalAlignment.Top;
				}
				else
				{
					SetTimer();
					view.Pause?.Stop();
					view.UnPause?.Begin();
					view.displayTXT.VerticalAlignment = VerticalAlignment.Bottom;
				}
				Set(ref _p, value);
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
			}
		}

		public string spendedTime
		{
			get
			{
				if (current == null) { return "--:--"; }
				if (current?.StatInfo?.isTimeLimited == false) { return "--:--"; }
				return $"{current?.StatInfo?.totalTimespend.Minutes:00}:{current.StatInfo?.totalTimespend.Seconds}";
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
	}

	public static class ExtendFunc
	{
		public static string obfuscateText(this string text)
		{
			string res = "";
			for (int i = 0; i < text.Length; i++)
			{
				res += "?";
			}
			return res;
		}

		public static int Clamp(this int value, int min, int max)
		{
			if (value < min) { return min; }
			if (value > max) { return max; }
			return value;
		}
	}
}