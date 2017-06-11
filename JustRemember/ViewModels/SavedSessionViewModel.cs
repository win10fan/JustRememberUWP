using JustRemember.Helpers;
using JustRemember.Models;
using JustRemember.Services;
using JustRemember.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace JustRemember.ViewModels
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
		
		public async void Initialize()
		{
			SavedSessions = await SavedSessionModel.Load();
		}

		public void DeSelectSession(RoutedEventArgs obj)
		{
			view.SSL.SelectedIndex = -1;
			OnPropertyChanged(nameof(isSelected));
		}

		public async void DeleteSelectedSession(RoutedEventArgs obj)
		{
			//Get list of files
			await SavedSessionModel.Delete(SavedSessions[view.SSL.SelectedIndex].GeneratedName);
			SavedSessions.RemoveAt(view.SSL.SelectedIndex);
		}
		
		public void OpenSessionMenu(RoutedEventArgs obj)
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

		public async void RefrehListAsync(RoutedEventArgs obj)
		{
			SavedSessions.Clear();
			SavedSessions = await SavedSessionModel.Load();
		}

		public void UpdateSelection(SelectionChangedEventArgs obj)
		{
			OnPropertyChanged(nameof(isSelected));
		}

		public void DoubleClickSession(DoubleTappedRoutedEventArgs arg)
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

		public async void KickToMatchWithSession()
		{
			int sel = view.SSL.SelectedIndex;
			SessionModel send = SavedSessions[sel];
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