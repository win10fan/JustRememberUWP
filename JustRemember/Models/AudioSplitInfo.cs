using JustRemember.Helpers;
using Newtonsoft.Json;
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

		public static async void Save(StorageFile location, AudioSplitInfo info)
		{
			await FileIO.WriteTextAsync(location, await Json.StringifyAsync(info));
		}

		public static async Task<AudioSplitInfo> Load(StorageFile location)
		{
			string info = await FileIO.ReadTextAsync(location);
			return await Json.ToObjectAsync<AudioSplitInfo>(info);
		}
	}

	public class AudioDescriptor
	{
		public string audioName;
		public ObservableCollection<AudioSplitItem> Splits { get; set; }

		public AudioDescriptor() { audioName = ""; Splits = new ObservableCollection<AudioSplitItem>(); }

		public AudioDescriptor(ObservableCollection<AudioSplitItem> sif, string name)
		{
			audioName = name;
			Splits = sif;
		}

		public async Task<StorageFile> GetAudio()
		{
			return await (await ApplicationData.Current.RoamingFolder.TryGetItemAsync("Description") as StorageFolder).GetFileAsync(audioName);
		}
	}

	public class AudioSplitItem : Observable
	{
		[JsonIgnore]
		string word = "";
		TimeSpan time = TimeSpan.FromSeconds(-1);
		[JsonIgnore]
		public string timeDisplay
		{
			get
			{
				if (time == TimeSpan.FromSeconds(-1))
				{
					return "Not set";
				}
				return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}:{time.Milliseconds:0000}:{time.Ticks}";
			}
		}
		[JsonIgnore]
		public bool isDefined
		{
			get => Time == TimeSpan.FromSeconds(-1);
		}
		[JsonIgnore]
		public bool isNotDefined
		{
			get => !isDefined;
		}

		[JsonIgnore]
		public string RID { get; set; }
		
		public string Word
		{
			get => word; set => Set(ref word, value);
		}

		public TimeSpan Time
		{
			get => time; set
			{
				Set(ref time, value);
				OnPropertyChanged(nameof(timeDisplay));
				OnPropertyChanged(nameof(isDefined));
				OnPropertyChanged(nameof(isNotDefined));
			}
		}

		public AudioSplitItem()
		{
			word = "";
			time = TimeSpan.FromSeconds(-1);
			RID = QuestionDesignHelper.getRandom(true);
		}

		public static int Hunt(IList<AudioSplitItem> splits, string id)
		{
			foreach (var item in splits)
			{
				if (item.RID == id)
				{
					return splits.IndexOf(item);
				}
			}
			return -1;
		}

		public AudioSplitItem(string dw)
		{
			word = dw;
			time = TimeSpan.FromSeconds(-1);
			RID = QuestionDesignHelper.getRandom(true);
		}
	}
}