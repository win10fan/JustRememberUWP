using JustRemember.Helpers;
using JustRemember.Models;
using JustRemember.Services;
using JustRemember.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using System;

namespace JustRemember.ViewModels
{
	public class SavedSessionViewModel : INotifyPropertyChanged
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
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SessionCount)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(isSelected)));
		}
		
		ObservableCollection<SessionModel> _ss = new ObservableCollection<SessionModel>();
		public ObservableCollection<SessionModel> SavedSessions
		{
			get { return _ss; }
			set { Set(ref _ss, value); }
		}

		public ICommand UpdateSelection { get; private set; }
		public ICommand DoubleTappedSelection { get; private set; }
		public ICommand GoToSetting { get; private set; }
		public ICommand GoToBundledMemoes { get; private set; }
		public ICommand RefreshSessionList { get; private set; }
		public ICommand OpenSession { get; private set; }
		public ICommand DeleteSession { get; private set; }
		public ICommand DeSelectSession { get; private set; }
		public ICommand GoToAudioSplitter { get; private set; }

		public SavedSessionViewModel()
		{
			UpdateSelection = new RelayCommand<SelectionChangedEventArgs>(UPDATESELECTION);
			DoubleTappedSelection = new RelayCommand<DoubleTappedRoutedEventArgs>(DOUBLETAPPEDSELECTION);
			GoToSetting = new RelayCommand<RoutedEventArgs>(GOTOSETTING);
			GoToBundledMemoes = new RelayCommand<RoutedEventArgs>(GOTOBUNDLEDMEMOES);
			RefreshSessionList = new RelayCommand<RoutedEventArgs>(REFRESHSESSIONLIST);
			OpenSession = new RelayCommand<RoutedEventArgs>(OPENSESSION);
			DeleteSession = new RelayCommand<RoutedEventArgs>(DELETESESSION);
			DeSelectSession = new RelayCommand<RoutedEventArgs>(DESELECTSESSION);
			GoToAudioSplitter = new RelayCommand<RoutedEventArgs>(GOTOAUDIOSPLITTER);
		}

		private void GOTOAUDIOSPLITTER(RoutedEventArgs obj)
		{
			NavigationService.Navigate<AudioDescription>();
		}
		
		private void DESELECTSESSION(RoutedEventArgs obj)
		{
			selectedIndex = -1;
			OnPropertyChanged(nameof(isSelected));
		}

		private async void DELETESESSION(RoutedEventArgs obj)
		{
			await SavedSessionModel.Delete(SavedSessions[selectedIndex].GeneratedName);
			SavedSessions.RemoveAt(selectedIndex);
		}

		private void OPENSESSION(RoutedEventArgs obj)
		{
			if (selectedIndex == -1)
			{
				return;
			}
			else
			{
				KickToMatchWithSession();
			}
		}

		private async void REFRESHSESSIONLIST(RoutedEventArgs obj)
		{
			SavedSessions = await SavedSessionModel.Load();
		}

		private void GOTOBUNDLEDMEMOES(RoutedEventArgs obj)
		{
			NavigationService.Navigate<PrenoteView>();
		}

		private void GOTOSETTING(RoutedEventArgs obj)
		{
			NavigationService.Navigate<AppConfigView>();
		}

		int _indx = -1;
		public int selectedIndex
		{
			get => _indx;
			set => Set(ref _indx, value);
		}

		private void DOUBLETAPPEDSELECTION(DoubleTappedRoutedEventArgs obj)
		{
			if (selectedIndex == -1)
			{
				return;
			}
			else
			{
				KickToMatchWithSession();
			}
		}

		private void UPDATESELECTION(SelectionChangedEventArgs obj)
		{
			OnPropertyChanged(nameof(isSelected));
		}

		public async void Initialize()
		{
			SavedSessions = await SavedSessionModel.Load();
		}

		public async void KickToMatchWithSession()
		{
			SessionModel send = SavedSessions[selectedIndex];
			if (send.isNew) { send.isNew = false; }
			await SavedSessionModel.Delete(send.GeneratedName);
			NavigationService.Navigate<Match>(send);
		}

		public string SessionCount
		{
			get
			{
				if (SavedSessions?.Count < 1)
				{
					return App.language.GetString("Home_session_no");
				}
				return string.Format(
					App.language.GetString("Home_session_count_format"),
					SavedSessions?.Count,
					SavedSessions?.Count == 1 ? App.language.GetString("Home_session_single") : App.language.GetString("Home_note_prural"));
			}
		}

		public Visibility isSelected
		{
			get
			{
				if (selectedIndex < 0)
				{
					return Visibility.Collapsed;
				}
				return Visibility.Visible;
			}
		}
	}
}