using JustRemember.Helpers;
using JustRemember.Models;
using JustRemember.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml;

namespace JustRemember.ViewModels
{
	public class AppConfigViewModel : INotifyPropertyChanged
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
		}

		AppConfigModel cf;
		public AppConfigModel config
		{
			get => cf;
			set
			{
				Set(ref cf, value);
			}
		}

		public ICommand ResetConfig;
		public ICommand ResetStat;
		public ICommand ResetSessions;
		public ICommand ResetNotes;
		public ICommand ResetAll;

		public void Initialie()
		{
			config = App.Config;
			stats = App.Stats;
			ResetConfig = new RelayCommand<RoutedEventArgs>(RESETCONFIG);
			ResetStat = new RelayCommand<RoutedEventArgs>(RESETSTAT);
			ResetSessions = new RelayCommand<RoutedEventArgs>(RESETSESSIONS);
			ResetNotes = new RelayCommand<RoutedEventArgs>(RESETNOTES);
			ResetAll = new RelayCommand<RoutedEventArgs>(RESETALL);
		}

		public async void RESETCONFIG(RoutedEventArgs obj)
		{
			AppConfigModel res = new AppConfigModel();
			await res.Save();
			NavigationService.GoBack();
		}
		public async void RESETSTAT(RoutedEventArgs obj)
		{
			StorageFolder fol = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("Stat");
			if (fol != null)
			{
				await fol.DeleteAsync(StorageDeleteOption.PermanentDelete);
			}
			NavigationService.GoBack();
		}
		public async void RESETSESSIONS(RoutedEventArgs obj)
		{
			StorageFolder fol = (StorageFolder)await ApplicationData.Current.RoamingFolder.TryGetItemAsync("Sessions");
			if (fol != null)
			{
				await fol.DeleteAsync(StorageDeleteOption.PermanentDelete);
			}
			NavigationService.GoBack();
		}
		public async void RESETNOTES(RoutedEventArgs obj)
		{
			StorageFolder fol = (StorageFolder)await ApplicationData.Current.RoamingFolder.TryGetItemAsync("Notes");
			if (fol != null)
			{
				await fol.DeleteAsync(StorageDeleteOption.PermanentDelete);
			}
			NavigationService.GoBack();
		}
		public async void RESETALL(RoutedEventArgs obj)
		{
			AppConfigModel res = new AppConfigModel();
			await res.Save();
			StorageFolder fol = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("Stat");
			if (fol != null)
			{
				await fol.DeleteAsync(StorageDeleteOption.PermanentDelete);
			}
			fol = (StorageFolder)await ApplicationData.Current.RoamingFolder.TryGetItemAsync("Sessions");
			if (fol != null)
			{
				await fol.DeleteAsync(StorageDeleteOption.PermanentDelete);
			}
			fol = (StorageFolder)await ApplicationData.Current.RoamingFolder.TryGetItemAsync("Notes");
			if (fol != null)
			{
				await fol.DeleteAsync(StorageDeleteOption.PermanentDelete);
			}
			NavigationService.GoBack();
		}

		public int choiceMode
		{
			get => (int)cf.choiceStyle;
			set
			{
				cf.choiceStyle = (choiceDisplayMode)value;
				OnPropertyChanged(nameof(choiceMode));
			}
		}

		public int FontSize
		{
			get => cf.displayTextSize;
			set
			{
				cf.displayTextSize = value;
				OnPropertyChanged(nameof(FontSize));
			}
		}

		public bool useSeed
		{
			get => cf.defaultSeed != -1;
			set
			{
				cf.defaultSeed = value ? (cf.defaultSeed == -1 ? 0 : cf.defaultSeed) : -1;
				OnPropertyChanged(nameof(useSeed));
				OnPropertyChanged(nameof(seedUI));
			}
		}

		public Visibility seedUI
		{
			get => useSeed ? Visibility.Visible : Visibility.Collapsed;
		}

		public bool useLight
		{
			get => cf.isItLightTheme;
			set
			{
				cf.isItLightTheme = value;
				UpdateTheme(value);
				OnPropertyChanged(nameof(useLight));
			}
		}

		public async void UpdateTheme(bool light)
		{
			await ThemeSelectorService.SetThemeAsync(light ? ElementTheme.Light : ElementTheme.Dark);
		}

		public bool autoScroll
		{
			get => cf.autoScrollContent;
			set
			{
				cf.autoScrollContent = value;
				OnPropertyChanged(nameof(autoScroll));
			}
		}

		public bool afterEndIsIt1 { get => cf.AfterFinalChoice == whenFinalChoice.EndPage; set { if (value) { cf.AfterFinalChoice = whenFinalChoice.EndPage; OnPropertyChanged("afterEndIsIt1"); OnPropertyChanged("afterEndIsIt2"); OnPropertyChanged("afterEndIsIt3"); OnPropertyChanged(nameof(showNotEndPage)); } } }
		public bool afterEndIsIt2 { get => cf.AfterFinalChoice == whenFinalChoice.Restart; set { if (value) { cf.AfterFinalChoice = whenFinalChoice.Restart; OnPropertyChanged("afterEndIsIt1"); OnPropertyChanged("afterEndIsIt2"); OnPropertyChanged("afterEndIsIt3"); OnPropertyChanged(nameof(showNotEndPage)); } } }
		public bool afterEndIsIt3 { get => cf.AfterFinalChoice == whenFinalChoice.BackHome; set { if (value) { cf.AfterFinalChoice = whenFinalChoice.BackHome; OnPropertyChanged("afterEndIsIt1"); OnPropertyChanged("afterEndIsIt2"); OnPropertyChanged("afterEndIsIt3"); OnPropertyChanged(nameof(showNotEndPage)); } } }

		public bool saveAllStat { get => cf.saveStatAfterEnd; set { cf.saveStatAfterEnd = value; OnPropertyChanged(nameof(saveAllStat)); } }
		public bool deleteAllStat { get => !cf.saveStatAfterEnd; set { cf.saveStatAfterEnd = !value; OnPropertyChanged(nameof(deleteAllStat)); } }

		public Visibility showNotEndPage
		{
			get => cf.AfterFinalChoice != whenFinalChoice.EndPage ? Visibility.Visible : Visibility.Collapsed;
		}

		public int totalChoice
		{
			get => cf.totalChoice; set { cf.totalChoice = value; OnPropertyChanged(nameof(totalChoice)); }
		}

		public bool useTimeLimit
		{
			get => cf.isLimitTime; set { cf.isLimitTime = value; OnPropertyChanged(nameof(useTimeLimit)); OnPropertyChanged(nameof(showTimePicker)); }
		}

		public Visibility showTimePicker
		{
			get => useTimeLimit ? Visibility.Visible : Visibility.Collapsed;
		}

		public bool isItEasy { get => cf.defaultMode == matchMode.Easy; set { if (value) { cf.defaultMode = matchMode.Easy; UpdateDifficultProperty(); } } }
		public bool isItNormal { get => cf.defaultMode == matchMode.Normal; set { if (value) { cf.defaultMode = matchMode.Normal; UpdateDifficultProperty(); } } }
		public bool isItHard { get => cf.defaultMode == matchMode.Hard; set { if (value) { cf.defaultMode = matchMode.Hard; UpdateDifficultProperty(); } } }

		public bool hintOnFirst { get => cf.hintAtFirstchoice; set { cf.hintAtFirstchoice = value; OnPropertyChanged(nameof(hintOnFirst)); } }
		public bool showWrongContent { get => cf.obfuscateWrongText; set { cf.obfuscateWrongText = value; OnPropertyChanged(nameof(showWrongContent)); } }
		public Visibility isEasySelected { get { return isItEasy ? Visibility.Visible : Visibility.Collapsed; } }

		void UpdateDifficultProperty()
		{
			OnPropertyChanged(nameof(isItEasy));
			OnPropertyChanged(nameof(isItNormal));
			OnPropertyChanged(nameof(isItHard));
			OnPropertyChanged(nameof(isEasySelected));
		}

		TimeSpan _lim;
		public TimeSpan timeLimit
		{
			get
			{
				return TimeSpan.FromMinutes(cf.limitTime);
			}
			set
			{
				Set(ref _lim, value);
				cf.limitTime =_lim.TotalMinutes;
			}
		}

		ObservableCollection<StatModel> _stats;
		public ObservableCollection<StatModel> stats
		{
			get => _stats;
			set { Set(ref _stats, value); OnPropertyChanged(nameof(statCount)); }
		}

		public string statCount
		{
			get
			{
				if (stats == null) { return "Stat list is empty"; }
				if (stats.Count == 0) { return "Stat list is empty"; }
				string s = stats.Count == 1 ? "" : "s";
				string stat = $"{stats.Count} stat{s}";
				return stat;
			}
		}

		int _sel = -1;
		public int IselectedStat
		{
			get => _sel;
			set => Set(ref _sel, value);
		}

		public StatModel selectedStat
		{
			get
			{
				if (stats == null) { return new StatModel(); }
				if (stats.Count < 1) { return new StatModel(); }
				return stats[IselectedStat];
			}
		}

		PackageId app { get => Package.Current.Id; }

		public string AppName { get => Package.Current.DisplayName; }
		public string AppMaker { get => Package.Current.PublisherDisplayName; }
		public string AppRunOn { get => app.Architecture.ToString(); }
		public string AppVersion { get => $"{app.Version.Major}.{app.Version.Minor}.{app.Version.Revision}{app.Version.Build}"; }

		public string seedValue
		{
			get => cf.defaultSeed.ToString();
			set => int.Parse(value);
		}
	}
}
