using JustRemember.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace JustRemember.Models
{
	public class StatModel
	{
		public DateTime beginTime;
		public int NoteWordCount;
		public int configChoice;
		public Dictionary<int, List<bool>> choiceInfo;
		public bool isTimeLimited;
		public TimeSpan totalTimespend;
		public TimeSpan totalLimitTime;
		public matchMode setMode;
		public string noteTitle;
		public List<int> correctedChoice;
		
		[JsonIgnore]
		public int totalWrong
		{
			get
			{
				return GetTotalWrong();
			}
		}

		public int GetTotalWrong()
		{
			int wrongCount = 0;
			foreach (var item in choiceInfo)
			{
				foreach (bool vlu in item.Value)
				{
					if (vlu)
					{
						wrongCount += 1;
					}
				}
			}
			return wrongCount;
		}

		[JsonIgnore]
		public Visibility wasTimeLimited
		{
			get
			{
				if (isTimeLimited) { return Visibility.Visible; }
				return Visibility.Collapsed;
			}
		}

		[JsonIgnore]
		public int timeValue
		{
			get
			{
				if (!isTimeLimited) { return 0; }
				double a = totalTimespend.TotalSeconds;
				double b = totalLimitTime.TotalSeconds;
				double val = a / b;
				return (int)(val * 100);
			}
		}

		[JsonIgnore]
		public string begintimeSTR
		{
			get
			{
				if (this == null) { return ""; }
				CultureInfo culture = CultureInfo.CurrentCulture;
				return beginTime.ToString("G", culture);
			}
		}

		[JsonIgnore]
		public int isTimeSlotShow
		{
			get
			{
				return isTimeLimited ? 30 : 0;
			}
		}

		[JsonIgnore]
		public string genName
		{
			get => $"{beginTime.Day}{beginTime.Month}{beginTime.Year}-{beginTime.Hour}{beginTime.Minute}{beginTime.Second}.stat";
		}

		[JsonIgnore]
		public string fileName { get; set; }

		[JsonIgnore]
		public TimeSpan timeLeft { get => isTimeLimited ? totalLimitTime - totalTimespend : TimeSpan.MinValue; }

		[JsonIgnore]
		public string modeTXT
		{
			get
			{
				switch (setMode)
				{
					case matchMode.Easy:
						return App.language.GetString("Stat_easy");
					case matchMode.Normal:
						return App.language.GetString("Stat_normal");
					case matchMode.Hard:
						return App.language.GetString("Stat_hard");
					default:
						return App.language.GetString("Stat_unknow");
				}
			}
		}

		[JsonIgnore]
		public string timeSpendSTR
		{
			get
			{
				string res = "";
				string minute = totalTimespend.Minutes == 1 ? App.language.GetString("Stat_Minute") : App.language.GetString("Stat_Minutes");
				string second = totalTimespend.Seconds == 1 ? App.language.GetString("Stat_Second") : App.language.GetString("Stat_Seconds");
				if (totalTimespend.Minutes >= 1)
				{
					if (totalTimespend.Seconds == 0)
						res += $"{totalTimespend.Minutes} {minute}";
					else
						res += $"{totalTimespend.Minutes} {minute} {totalTimespend.Seconds} {second}";
				}
				else
				{
					res += $"{totalTimespend.Seconds} {second}";
				}
				return res;
			}
		}

		[JsonIgnore]
		public string timeleftSTR
		{
			get
			{
				string res = "";
				string minute = timeLeft.Minutes == 1 ? App.language.GetString("Stat_Minute") : App.language.GetString("Stat_Minutes");
				string second = timeLeft.Seconds == 1 ? App.language.GetString("Stat_Second") : App.language.GetString("Stat_Seconds");
				if (timeLeft.Minutes >= 1)
				{
					if (timeLeft.Seconds == 0)
						res += $"{timeLeft.Minutes} {minute}";
					else
						res += $"{timeLeft.Minutes} {minute} {timeLeft.Seconds} {second}";
				}
				else
				{
					res += $"{timeLeft.Seconds} {second}";
				}
				return res;
			}
		}

		[JsonIgnore]
		public string timeLimitSTR
		{
			get
			{
				string res = "";
				string minute = totalLimitTime.Minutes == 1 ? App.language.GetString("Stat_Minute") : App.language.GetString("Stat_Minutes");
				string second = totalLimitTime.Seconds == 1 ? App.language.GetString("Stat_Second") : App.language.GetString("Stat_Seconds");
				if (totalLimitTime.Minutes >= 1)
				{
					if (totalLimitTime.Seconds == 0)
						res += $"{totalLimitTime.Minutes} {minute}";
					else
						res += $"{totalLimitTime.Minutes} {minute} {totalLimitTime.Seconds} {second}";
				}
				else
				{
					res += $"{totalLimitTime.Seconds} {second}";
				}
				return res;
			}
		}

		//[JsonIgnore]
		//public List<choiceInfoUnDic> choiceInfo2
		//{
		//	get
		//	{
		//		var choice2 = new List<choiceInfoUnDic>();
		//		foreach (var ch in choiceInfo)
		//		{
		//			choice2.Add(new choiceInfoUnDic(ch));
		//		}
		//		return choice2;
		//	}
		//}

		[JsonIgnore]
		public string startedTime
		{
			get { return $"{beginTime.ToString("dd/MM/yy HH:mm:ss")}"; }
		}

		public StatModel()
		{
			beginTime = DateTime.Now;
			NoteWordCount = 10;
			configChoice = 3;
			choiceInfo = new Dictionary<int, List<bool>>();
			isTimeLimited = false;
			totalTimespend = TimeSpan.FromMilliseconds(0);
			totalLimitTime = TimeSpan.FromMinutes(5);
			setMode = matchMode.Easy;
			noteTitle = App.language.GetString("Note_Untitled");
			correctedChoice = new List<int>();
		}

		public static async Task<ObservableCollection<StatModel>> Get()
		{
			var statFol = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("Stat");
			if (statFol == null)
			{
				statFol = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Stat");
				return new ObservableCollection<StatModel>();
			}
			ObservableCollection<StatModel> stats = new ObservableCollection<StatModel>();
			var files = await statFol.GetFilesAsync();
			if (files.Count > 0)
			{
				foreach (var f in files)
				{
					stats.Add(await Json.ToObjectAsync<StatModel>(await FileIO.ReadTextAsync(f)));
					stats[stats.Count - 1].fileName = f.Name;
				}
			}
			return stats;
		}

		public static async void Set(StatModel set)
		{
			var statFol = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("Stat");
			if (statFol == null)
			{
				statFol = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Stat");
			}
			StorageFile statFil = (StorageFile)await statFol.TryGetItemAsync(set.genName);
			if (statFil == null)
			{
				statFil = await statFol.CreateFileAsync(set.genName);
				await FileIO.WriteTextAsync(statFil, await Json.StringifyAsync(set));
			}
			else
			{
				await statFil.DeleteAsync();
				statFil = await statFol.CreateFileAsync(set.genName);
				await FileIO.WriteTextAsync(statFil, await Json.StringifyAsync(set));
			}
			App.Stats.Add(set);
		}

		public static async void Delete(int index)
		{
			var items = await Get();
			var statFol = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("Stat");
			if (statFol == null)
			{
				statFol = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Stat");
			}
			StorageFile statFil = (StorageFile)await statFol.TryGetItemAsync(items[index].fileName);
			if (statFil == null)
			{
				return;
			}
			else
			{
				await statFil.DeleteAsync();
			}
		}
	}
	
	public enum matchMode
	{
		Easy,
		Normal,
		Hard
	}
}