using JustRemember_.Models;
using JustRemember_.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace JustRemember_.ViewModels
{
	public class PrenoteViewModel : INotifyPropertyChanged
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

		public StorageFolder basePath = ApplicationData.Current.LocalFolder;

		string _pt;
		public string Path
		{
			get => _pt;
			set => Set(ref _pt, value);
		}

		public ObservableCollection<string> PathsSplit
		{
			get
			{
				var pth = Path.GetBreadcrumbPath();
				var val = new ObservableCollection<string>(pth);
				return val;
			}
		}

		public ObservableCollection<PrenoteModel> notes;

		public async void Initialize()
		{
			basePath = (StorageFolder)await basePath.TryGetItemAsync("Prenote");
			if (basePath == null)
			{
				basePath = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Prenote");
			}
			Path = "\\";
			notes = PrenoteModel.GetChild(basePath);
			Path = basePath.Path;
		}
	}
}
