using JustRemember_.Helpers;
using JustRemember_.Models;
using JustRemember_.Services;
using JustRemember_.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace JustRemember_.ViewModels
{
	public class SavedSessionViewModel : INotifyPropertyChanged
	{
		public MainPage view;
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
		
		ObservableCollection<SessionModel> _ss;
		public ObservableCollection<SessionModel> SavedSessions
		{
			get { return _ss; }
			set { Set(ref _ss, value); }
		}

		public ICommand openSession;
		public ICommand selectSession;
		public ICommand refreshSession;
		public ICommand openSessionOnMenu;
		public ICommand deleteSession;
		public ICommand deSelect;

		public async void Initialize()
		{
			SavedSessions = await SavedSessionModel.Load();
			//Commands
			openSession = new RelayCommand<DoubleTappedRoutedEventArgs>(DoubleClickSession);
			selectSession = new RelayCommand<SelectionChangedEventArgs>(UpdateSelection);
			refreshSession = new RelayCommand<RoutedEventArgs>(RefrehListAsync);
			openSessionOnMenu = new RelayCommand<RoutedEventArgs>(OpenSessionMenu);
			deleteSession = new RelayCommand<RoutedEventArgs>(DeleteSelectedSession);
			deSelect = new RelayCommand<RoutedEventArgs>(DeSelectSession);
		}

		private void DeSelectSession(RoutedEventArgs obj)
		{
			view.SSL.SelectedIndex = -1;
			OnPropertyChanged(nameof(isSelected));
		}

		private async void DeleteSelectedSession(RoutedEventArgs obj)
		{
			SavedSessions.RemoveAt(view.SSL.SelectedIndex);
			await SavedSessionModel.Save(SavedSessions);
		}

		private void OpenSessionMenu(RoutedEventArgs obj)
		{
			if (view.SSL.SelectedIndex == -1)
			{
				return;
			}
			else
			{
				KickToMatchWithSession();
			}
		}

		private async void RefrehListAsync(RoutedEventArgs obj)
		{
			SavedSessions.Clear();
			SavedSessions = await SavedSessionModel.Load();
		}

		private void UpdateSelection(SelectionChangedEventArgs obj)
		{
			OnPropertyChanged(nameof(isSelected));
		}

		private void DoubleClickSession(DoubleTappedRoutedEventArgs arg)
		{
			if (view.SSL.SelectedIndex == -1)
			{
				return;
			}
			else
			{
				KickToMatchWithSession();
			}
		}

		public void KickToMatchWithSession()
		{
			int sel = view.SSL.SelectedIndex;
			SessionModel send = SavedSessions[sel];
			if (send.isNew) { send.isNew = false; }
			NavigationService.Navigate<Match>(send);
		}

		public string SessionCount
		{
			get
			{
				if (SavedSessions?.Count < 1)
				{
					return "No session saved";
				}
				string s = SavedSessions?.Count > 1 ? "s" : "";
				return $"{SavedSessions?.Count} saved session{s}";
			}
		}

		public Visibility isSelected
		{
			get
			{
				if (view.SSL.SelectedIndex < 0)
				{
					return Visibility.Collapsed;
				}
				return Visibility.Visible;
			}
		}
	}
}