using JustRemember.Helpers;
using JustRemember.Models;
using JustRemember.Services;
using JustRemember.Views;
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

		public void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		
		public AppConfigModel config
		{
			get => App.Config;
		}

		public ICommand ResetConfig;
		public ICommand ResetStat;
		public ICommand ResetSessions;
		public ICommand ResetNotes;
		public ICommand ResetAll;
		public ICommand ToggleAntiSpam;

		public void Initialie()
		{
			stats = App.Stats;
			ResetConfig = new RelayCommand<RoutedEventArgs>(RESETCONFIG);
			ResetStat = new RelayCommand<RoutedEventArgs>(RESETSTAT);
			ResetSessions = new RelayCommand<RoutedEventArgs>(RESETSESSIONS);
			ResetNotes = new RelayCommand<RoutedEventArgs>(RESETNOTES);
			ResetAll = new RelayCommand<RoutedEventArgs>(RESETALL);
			ToggleAntiSpam = new RelayCommand<RoutedEventArgs>(TOGGLEANTISPAM);
			saver = new DispatcherTimer()
			{
				Interval = TimeSpan.FromSeconds(5)
			};
			saver.Tick += Saver_Tick;
			saver.Start();
		}

		private async void Saver_Tick(object sender, object e)
		{
			if (AppConfigModel.isDirty)
				await config.Save();
		}
		DispatcherTimer saver;

		private void TOGGLEANTISPAM(RoutedEventArgs obj)
		{
			config.antiSpamChoice = !config.antiSpamChoice;
		}

		public void RESETCONFIG(RoutedEventArgs obj)
		{
			App.Config = new AppConfigModel();
			AppConfigModel.isDirty = true;
			NavigationService.GoBack();
		}
		public async void RESETSTAT(RoutedEventArgs obj)
		{
			StorageFolder fol = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("Stat");
			if (fol != null)
			{
				await fol.DeleteAsync(StorageDeleteOption.PermanentDelete);
			}
			App.Stats.Clear();
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
			App.Config = new AppConfigModel();
			AppConfigModel.isDirty = true;
			App.Stats.Clear();
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
			get => (int)config.choiceStyle;
			set
			{
				config.choiceStyle = (choiceDisplayMode)value;
				OnPropertyChanged(nameof(choiceMode));
			}
		}

		public int FontSize
		{
			get => config.displayTextSize;
			set
			{
				config.displayTextSize = value;
				OnPropertyChanged(nameof(FontSize));
			}
		}

		public bool useSeed
		{
			get => config.useSeed;
			set
			{
				config.useSeed = value;
				OnPropertyChanged(nameof(seedUI));
				OnPropertyChanged(nameof(useSeed));
			}
		}

		public Visibility seedUI
		{
			get => useSeed ? Visibility.Visible : Visibility.Collapsed;
		}

		public bool useLight
		{
			get => config.isItLightTheme;
			set
			{
				config.isItLightTheme = value;
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
			get => config.autoScrollContent;
			set
			{
				config.autoScrollContent = value;
				OnPropertyChanged(nameof(autoScroll));
			}
		}

		public int afterEnd { get => (int)config.AfterFinalChoice; set { config.AfterFinalChoice = (whenFinalChoice)value; OnPropertyChanged(nameof(showNotEndPage)); } }
		
		public bool saveAllStat { get => config.saveStatAfterEnd; set { config.saveStatAfterEnd = value; OnPropertyChanged(nameof(saveAllStat)); OnPropertyChanged(nameof(saveAllString)); } }

		public string saveAllString { get => config.saveStatAfterEnd ? App.language.GetString("Config_saveall") : App.language.GetString("Config_delall"); }

		public Visibility showNotEndPage
		{
			get => config.AfterFinalChoice != whenFinalChoice.EndPage ? Visibility.Visible : Visibility.Collapsed;
		}

		public int totalChoice
		{
			get => config.totalChoice; set { config.totalChoice = value; OnPropertyChanged(nameof(totalChoice)); }
		}

		public bool useTimeLimit
		{
			get => config.isLimitTime; set { config.isLimitTime = value; OnPropertyChanged(nameof(useTimeLimit)); OnPropertyChanged(nameof(showTimePicker)); }
		}

		public Visibility showTimePicker
		{
			get => useTimeLimit ? Visibility.Visible : Visibility.Collapsed;
		}

		public int difficultSet { get => (int)config.defaultMode; set { config.defaultMode = (matchMode)value; OnPropertyChanged(nameof(isEasySelected)); } }
		
		public bool hintOnFirst { get => config.hintAtFirstchoice; set { config.hintAtFirstchoice = value; OnPropertyChanged(nameof(hintOnFirst)); } }
		public bool showWrongContent { get => config.obfuscateWrongText; set { config.obfuscateWrongText = value; OnPropertyChanged(nameof(showWrongContent)); } }
		public Visibility isEasySelected { get { return config.defaultMode == matchMode.Easy ? Visibility.Visible : Visibility.Collapsed; } }
		
		TimeSpan _lim;
		public TimeSpan timeLimit
		{
			get
			{
				return TimeSpan.FromMinutes(config.limitTime);
			}
			set
			{
				Set(ref _lim, value);
				config.limitTime =_lim.TotalMinutes;
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
				if (stats?.Count < 1)
				{
					return App.language.GetString("Config_stat_no");
				}
				return string.Format(App.language.GetString("Config_stat_count_format"),
					stats?.Count,
					stats?.Count == 1 ? App.language.GetString("Config_stat_single") : App.language.GetString("Config_stat_prural"));
			}
		}

		int _sel = -1;
		public int IselectedStat
		{
			get => _sel;
			set
			{
				Set(ref _sel, value);
				OnPropertyChanged(nameof(selectedStat));
				OnPropertyChanged(nameof(isSelected));
			}
		}

		public Visibility isSelected
		{
			get => IselectedStat == -1 ? Visibility.Collapsed : Visibility.Visible;
		}
		
		public StatModel selectedStat
		{
			get
			{
				if (stats == null) { return new StatModel(); }
				if (stats.Count < 1) { return new StatModel(); }
				if (IselectedStat < 0) { return new StatModel(); }
				return stats[IselectedStat];
			}
		}

		PackageId app { get => Package.Current.Id; }

		public string AppName { get => app.Name; }
		public string AppMaker { get => app.Publisher; }
		public string AppDesc { get => "some app description here"; }
		public string AppVersion { get => $"{app.Version.Major}.{app.Version.Minor}.{app.Version.Revision} build {app.Version.Build}"; }

		bool applying;
		string tmp;
		public string seedValue
		{
			get => applying ? tmp : config.defaultSeed.ToString();
			set
			{
				applying = true;
				tmp = value;
				for (int i = tmp.Length - 1; i > -1; i--)
				{
					if (i == -1) { break; }
					if (i == 0)
					{
						if (value[i] == '-')
						{
							break;
						}
					}
					if (!char.IsNumber(value[i]))
					{
						tmp = tmp.Remove(i, 1);
						OnPropertyChanged(nameof(seedValue));
					}
				}
				applying = false;
				value = tmp;
				config.defaultSeed = int.Parse(value);
				OnPropertyChanged(nameof(seedValue));
			}
		}
		
		public int language
		{
			get => config.language;
			set { config.language = value; AppConfigModel.SetLanguage(value); }
		}
	}
}