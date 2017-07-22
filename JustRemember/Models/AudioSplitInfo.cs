using JustRemember.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace JustRemember.Models
{
    public class AudioSplitInfo
    {
		public string fileName { get; set; }
		public ObservableCollection<TimeSpan> splits { get; set; }

		public AudioSplitInfo()
		{
			fileName = "";
			splits = new ObservableCollection<TimeSpan>();
		}

		public static async void Save(StorageFile location,AudioSplitInfo info)
		{
			await FileIO.WriteTextAsync(location, await Json.StringifyAsync(info));
		}

		public static async Task<AudioSplitInfo> Load(StorageFile location)
		{
			string info = await FileIO.ReadTextAsync(location);
			return await Json.ToObjectAsync<AudioSplitInfo>(info);
		}
    }

	public class AudioSplitItem : Observable
	{
		string word = "";
		TimeSpan time = TimeSpan.FromSeconds(-1);

		public string Word
		{
			get => word; set => Set(ref word, value);
		}

		public TimeSpan Time
		{
			get => time; set
			{
				Set(ref time, value);
				OnPropertyChanged(nameof(isDefined));
			}
		}

		public Visibility isDefined
		{
			get => Time == TimeSpan.FromSeconds(-1) ? Visibility.Visible : Visibility.Collapsed;
		}

		public AudioSplitItem()
		{
			word = "";
			time = TimeSpan.FromSeconds(-1);
		}

		public AudioSplitItem(string dw)
		{
			word = dw;
			time = TimeSpan.FromSeconds(-1);
		}
	}
}
