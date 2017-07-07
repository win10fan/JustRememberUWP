using JustRemember.Helpers;
using JustRemember.Models;
using JustRemember.Services;
using JustRemember.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Xaml;
using static JustRemember.Services.PrenoteService;

namespace JustRemember.ViewModels
{
	public class PrenoteViewModel : INotifyPropertyChanged
	{
		public PrenoteView v;
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

		public StorageFolder basePath = ApplicationData.Current.LocalFolder;
		
		string _pt;
		public string Path
		{
			get => _pt;
			set
			{
				Set(ref _pt, value);
				OnPropertyChanged("PathsSplit");
			}
		}

		int _mon = -1;
		public int selectMon { get => _mon; set { Set(ref _mon, value); OnPropertyChanged(nameof(isFileSelected)); OnPropertyChanged(nameof(isOpenAbleSelected)); } }

		public ObservableCollection<PathDir> PathsSplit
		{
			get
			{
				var pth = Path.GetBreadcrumbPath().Reverse();
				var val = new ObservableCollection<PathDir>(pth);
				return val;
			}
		}

		public async void Navigate(string path)
		{
			basePath = await StorageFolder.GetFolderFromPathAsync(path);
			Path = basePath.Path;
			notes = await PrenoteModel.GetChild(basePath);
			isMoreUp = new DirectoryInfo(basePath.Path).Name != "Prenote";
			OnPropertyChanged(nameof(notes));
		}

		ObservableCollection<PrenoteModel> _n;
		public ObservableCollection<PrenoteModel> notes
		{
			get => _n;
			set
			{
				Set(ref _n, value);
				OnPropertyChanged(nameof(isDeployingFromExt));
			}
		}

		public Visibility isDeployingFromExt
		{
			get
			{
				if (App.isDeploying) { deployCheck?.Start(); }
				return App.isDeploying ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		public ICommand navTo;
		public ICommand navUp;

		DispatcherTimer deployCheck;
		public async void Initialize()
		{
			basePath = (StorageFolder)await basePath.TryGetItemAsync("Prenote");
			if (basePath == null)
			{
				basePath = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Prenote");
			}
			Navigate(basePath.Path);
			//Command
			navTo = new RelayCommand<RoutedEventArgs>(NAVTO);
			navUp = new RelayCommand<RoutedEventArgs>(NAVUP);
			deployCheck = new DispatcherTimer()
			{
				Interval = TimeSpan.FromSeconds(3)
			};
			deployCheck.Tick += DeployCheck_Tick;
		}

		private void DeployCheck_Tick(object sender, object e)
		{
			if (App.isDeploying)
			{
				OnPropertyChanged(nameof(isDeployingFromExt));
				Refresh();
			}
			else { deployCheck.Stop(); }
		}

		bool _canUp;
		public bool isMoreUp
		{
			get => _canUp;
			set => Set(ref _canUp, value);
		}

		public void NavToAccordingToWhatYouBeenClickOnPathList(int selected)
		{
			Navigate(PathsSplit[selected].FullPath);
		}

		private void NAVUP(RoutedEventArgs obj)
		{
			DirectoryInfo dir = new DirectoryInfo(basePath.Path);
			if (dir.Name == "Prenote")
			{
				NavigationService.GoBack();
				return;
			}
			Navigate(dir.Parent.FullName);
		}

		private async void NAVTO(RoutedEventArgs obj)
		{
			if (v.FileList.SelectedIndex > -1)
			{
				if (!notes[v.FileList.SelectedIndex].isFile)
					Navigate(notes[v.FileList.SelectedIndex].Fullpath);
				else
				{
					NavigationService.Navigate<Match>(SessionModel.generate(await NoteModel.GetOneNoteButNotMicrosoftOneNoteButOneOfANoteWithParticularPath(notes[v.FileList.SelectedIndex].Fullpath)));
				}
			}
		}

		public Visibility isOpenAbleSelected
		{
			get
			{
				return v.FileList.SelectedIndex > -1 ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		public Visibility isFileSelected
		{
			get
			{
				return v.FileList.SelectedIndex > -1 ? (notes[v.FileList.SelectedIndex].isFile ? Visibility.Visible : Visibility.Collapsed) : Visibility.Collapsed;
			}
		}

		public void Refresh()
		{
			Navigate(basePath.Path);
		}

		public async void Edit()
		{
			NoteModel note = await NoteModel.GetOneNoteButNotMicrosoftOneNoteButOneOfANoteWithParticularPath(notes[v.FileList.SelectedIndex].Fullpath);
			NavigationService.Navigate<NoteEditorView>(note);
		}
	}
}