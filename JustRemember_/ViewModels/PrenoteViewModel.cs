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
		public AppConfigModel config;

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

		public ObservableCollection<PrenoteModel> notes;

		public ICommand navTo;
		public ICommand navUp;

		public async void Initialize()
		{
			basePath = (StorageFolder)await basePath.TryGetItemAsync("Prenote");
			if (basePath == null)
			{
				basePath = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Prenote");
			}
			Navigate(basePath.Path);
			config = await AppConfigModel.Load2();
			//Command
			navTo = new RelayCommand<RoutedEventArgs>(NAVTO);
			navUp = new RelayCommand<RoutedEventArgs>(NAVUP);
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
					SessionModel.generate(await NoteModel.GetOneNoteButNotMicrosoftOneNoteButOneOfANoteWithParticularPath(notes[v.FileList.SelectedIndex].Fullpath));
			}
		}

		
	}
}
