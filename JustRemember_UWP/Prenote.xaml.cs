using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace JustRemember_UWP
{
	public sealed partial class Prenote : Page
	{
		public Prenote()
		{
			InitializeComponent();
			Prenotes = new PrenoteList();
		}
		
		public PrenoteList Prenotes;
        int level = 0;
		
		private async void Button_Click(object sender, RoutedEventArgs e)
		{
            if (level == 0)
            {
                Frame.GoBack();
                return;
            }
			await Prenotes.NavigateUpAsync();
		}
		
		private async void Page_Loaded(object sender, RoutedEventArgs e)
		{
			await Prenotes.NavigateAsync("Prenote");
		}
		public PrenoteInfo selectedPrenote;
		private async void openFolder_Click(object sender, RoutedEventArgs e)
		{
            level += 1;
            if (selectionList.SelectedIndex < 0) { return; }
			if (selectedPrenote != null)
			{
				await Prenotes.NavigateAsync(selectedPrenote.Name);
			}
		}

		private void openFile_Click(object sender, RoutedEventArgs e)
        {
            if (selectionList.SelectedIndex < 0) { return; }
            Note selected = new Note();
			selected.Title = selectedPrenote.Name;
			selected.Content = selectedPrenote.Content;
			Utilities.selected = selected;
			Frame.Navigate(typeof(Match));
		}

		private void AppBarButton_Click(object sender, RoutedEventArgs e)
		{
            Frame.GoBack();
		}

		private void selectionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (selectionList.SelectedIndex >= 0)
			{
				selectedPrenote = Prenotes.Prenotes[selectionList.SelectedIndex];
				openFolder.Visibility = selectedPrenote.isFile ? Visibility.Collapsed : Visibility.Visible;
				openFile.Visibility = selectedPrenote.isFile ? Visibility.Visible : Visibility.Collapsed;
			}
		}
	}

	public class PrenoteList
	{
		public StorageFolder currentFolder = ApplicationData.Current.LocalFolder;
		public static readonly StorageFolder baseFolder = ApplicationData.Current.LocalFolder;
		private ObservableCollection<PrenoteInfo> prenotes = new ObservableCollection<PrenoteInfo>();
		public ObservableCollection<PrenoteInfo> Prenotes
		{
			get
			{
				return prenotes;
			}
		}

		public PrenoteList() { }
		
		public async Task NavigateAsync(string folderName)
		{
			prenotes.Clear();
			currentFolder = await currentFolder.GetFolderAsync(folderName);
			var folders = await currentFolder.GetFoldersAsync();
			foreach (var folder in folders)
			{
				PrenoteInfo info = await PrenoteInfo.GetPrenoteAsync(folder);
				prenotes.Add(info);
			}
			var files = await currentFolder.GetFilesAsync();
			foreach (var file in files)
			{
				PrenoteInfo info = await PrenoteInfo.GetPrenoteAsync(file);
				prenotes.Add(info);
			}
		}

		public bool allowUpper
		{
			get
			{
				return currentFolder.Name != "Prenote";
			}
		}

		public async Task NavigateUpAsync()
		{
			prenotes.Clear();
			if (currentFolder.Name == "Prenote") { return; }
			currentFolder = await currentFolder.GetParentAsync();
			var folders = await currentFolder.GetFoldersAsync();
			foreach (var folder in folders)
			{
				PrenoteInfo info = await PrenoteInfo.GetPrenoteAsync(folder);
				prenotes.Add(info);
			}
			var files = await currentFolder.GetFilesAsync();
			foreach (var file in files)
			{
				PrenoteInfo info = await PrenoteInfo.GetPrenoteAsync(file);
				prenotes.Add(info);
			}
		}
	}

	public class PrenoteInfo
	{
		public string Fullpath { get; set; } = "";
		public string Name { get; set; } = "";
		public string Content { get; set; } = "";
		public string Icon { get; set; } = "?";
		public bool isFile { get; set; } = false;

		private PrenoteInfo() { }
		
		public static async Task<PrenoteInfo> GetPrenoteAsync(string fullpath)
		{
			PrenoteInfo info = new PrenoteInfo();
			if (fullpath.EndsWith(".txt"))
			{
				StorageFile file = await StorageFile.GetFileFromPathAsync(fullpath);
				info.Fullpath = file.Path;
				info.Name = file.Name;
				info.isFile = true;
				info.Icon = "";
				info.Content = await FileIO.ReadTextAsync(file);
				return info;
			}
			else
			{
				StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(fullpath);
				info.Fullpath = folder.Path;
				info.Name = folder.Name;
				info.isFile = false;
				info.Icon = "";
				info.Content = "";
				return info;
			}
		}

		public static async Task<PrenoteInfo> GetPrenoteAsync(StorageFile file)
		{
			PrenoteInfo info = new PrenoteInfo();
			info.Fullpath = file.Path;
			info.Name = file.Name;
			info.isFile = true;
			info.Icon = "";
			info.Content = await FileIO.ReadTextAsync(file);
			return info;
		}

		public static async Task<PrenoteInfo> GetPrenoteAsync(StorageFolder folder)
		{
			PrenoteInfo info = new PrenoteInfo();
			info.Fullpath = folder.Path;
			info.Name = folder.Name;
			info.isFile = false;
			info.Icon = "";
			info.Content = "";
			await Task.Delay(1);
			return info;
		}
	}
}