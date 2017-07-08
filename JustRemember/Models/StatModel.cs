﻿using JustRemember.Helpers;
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
		public List<choiceInfoUnDic> choiceInfo2
		{
			get
			{
				var choice2 = new List<choiceInfoUnDic>();
				foreach (var ch in choiceInfo)
				{
					choice2.Add(new choiceInfoUnDic(ch));
				}
				return choice2;
			}
		}

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
	}

	public class choiceInfoUnDic
	{
		public int choice { get; set; }
		public int limit { get; set; }
		public bool choice1 { get; set; }
		public bool choice2 { get; set; }
		public bool choice3 { get; set; }
		public bool choice4 { get; set; }
		public bool choice5 { get; set; }
		public List<bool> origin { get; set; }
		
		public GridLength showChoice4Grid { get => new GridLength(limit >= 4 ? 1 : 0, GridUnitType.Star); }
		
		public GridLength showChoice5Grid { get => new GridLength(limit >= 5 ? 1 : 0, GridUnitType.Star); }

		public choiceInfoUnDic()
		{
			choice = 0;
			limit = 3;
			choice1 = true;
			choice2 = choice3 = choice4 = choice5 = false;
			origin = new List<bool>();
			origin.Add(false);
			origin.Add(false);
			origin.Add(false);
		}

		public choiceInfoUnDic(KeyValuePair<int,List<bool>> info)
		{
			choice = info.Key;
			limit = info.Value.Count - 1;
			choice1 = info.Value[0];
			choice2 = info.Value[1];
			choice3 = info.Value[2];
			choice4 = limit >= 3 ? info.Value[3] : false;
			choice5 = limit >= 4 ? info.Value[4] : false;
			origin = new List<bool>();
			origin = info.Value;
		}
	}

	public enum matchMode
	{
		Easy,
		Normal,
		Hard
	}
}