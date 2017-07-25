using JustRemember.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;

namespace JustRemember.Models
{
	public class AppConfigModel
	{
		public void Set<T>(ref T container, T value,[CallerMemberName]string name = null)
		{
			if (!Equals(container, value))
			{
				container = value;
				if (App._cfg.Values.ContainsKey(name) && isInitialize)
				{
					App._cfg.Values[name] = value;
				}
			}
		}

		[JsonIgnore]
		public bool isInitialize { get; set; }

		bool isn;
		public bool isOpenBefore
		{
			get => isn;
			set => Set(ref isn, value);
		}

		int lang;
		public int language
		{
			get => lang;
			set => Set(ref lang, value);
		}

		bool light;
		public bool isItLightTheme
		{
			get => light;
			set => Set(ref light, value);
		}
		
		bool limittime;
		public bool isLimitTime
		{
			get => limittime;
			set => Set(ref limittime, value);
		}

		bool hidewrong;
		public bool obfuscateWrongText
		{
			get => hidewrong;
			set => Set(ref hidewrong, value);
		}

		int mode;
		public matchMode defaultMode
		{
			get => (matchMode)mode;
			set => Set(ref mode, (int)value);
		}

		double limits;
		public double limitTime
		{
			get => limits;
			set => Set(ref limits, value);
		}

		int choices;
		public int totalChoice
		{
			get => choices;
			set => Set(ref choices, value);
		}

		int size;
		public int displayTextSize
		{
			get => size;
			set => Set(ref size, value);
		}

		bool seed;
		public bool useSeed
		{
			get => seed;
			set => Set(ref seed, value);
		}

		int seedi;
		public int defaultSeed
		{
			get => seedi;
			set => Set(ref seedi, value);
		}

		bool scroll;
		public bool autoScrollContent
		{
			get => scroll;
			set => Set(ref scroll, value);
		}

		int end;
		public whenFinalChoice AfterFinalChoice
		{
			get => (whenFinalChoice)end;
			set => Set(ref end, (int)value);
		}

		bool savestat;
		/// <summary>
		/// If "AfterFinalChoice" is not "EndPage" always save stat or discard
		/// </summary>
		public bool saveStatAfterEnd
		{
			get => savestat;
			set => Set(ref savestat, value);
		}

		bool hint;
		public bool hintAtFirstchoice
		{
			get => hint;
			set => Set(ref hint, value);
		}

		int style;
		public choiceDisplayMode choiceStyle
		{
			get => (choiceDisplayMode)style;
			set => Set(ref style, (int)value);
		}

		bool nospam;
		public bool antiSpamChoice
		{
			get => nospam;
			set => Set(ref nospam, value);
		}

		int _randQA;
		public randomQA randomizeQA
		{
			get => (randomQA)_randQA;
			set => Set(ref _randQA, (int)value);
		}

		bool _frontback;
		public bool reverseDictionary
		{
			get => _frontback;
			set => Set(ref _frontback, value);
		}

		int _ap, _qs, _sa;
		public answerPosition AnswerPosition { get => (answerPosition)_ap; set => Set(ref _ap, (int)value); }
		public QuestionSeparator questionSeparator { get => (QuestionSeparator)_qs; set => Set(ref _qs, (int)value); }
		public SpaceAfterSeparator spaceAfterSeparator { get => (SpaceAfterSeparator)_sa; set => Set(ref _sa, (int)value); }

		bool _ads;
		public bool useAd { get => _ads; set => Set(ref _ads, value); }

		bool _dbg;
		public bool showDebugging { get => _dbg; set => Set(ref _dbg, value); }

		string _chdr;
		public string customChoiceHeader { get => _chdr ?? "ABCDE"; set => Set(ref _chdr, value); }

		int hrs = -1;
		public int halfResolution { get => hrs; set => Set(ref hrs, value); }

		[JsonIgnore]
		public int randomizeQAInt
		{
			get => _randQA;
			set => Set(ref _randQA, value);
		}

		[JsonIgnore]
		public static List<string> languages = new List<string>()
		{
			"en",
			"th-TH"
		};

		public static AppConfigModel GetSettings()
		{
			AppConfigModel cfg = new AppConfigModel()
			{
				language = get<int>(nameof(language)),
				isItLightTheme = get<bool>(nameof(isItLightTheme)),
				isLimitTime = get<bool>(nameof(isLimitTime)),
				obfuscateWrongText = get<bool>(nameof(obfuscateWrongText)),
				defaultMode = (matchMode)get<int>(nameof(defaultMode)),
				limitTime = get<double>(nameof(limitTime)),
				totalChoice = get<int>(nameof(totalChoice)),
				displayTextSize = get<int>(nameof(displayTextSize)),
				useSeed = get<bool>(nameof(useSeed)),
				defaultSeed = get<int>(nameof(defaultSeed)),
				autoScrollContent = get<bool>(nameof(autoScrollContent)),
				AfterFinalChoice = (whenFinalChoice)get<int>(nameof(AfterFinalChoice)),
				saveStatAfterEnd = get<bool>(nameof(saveStatAfterEnd)),
				hintAtFirstchoice = get<bool>(nameof(hintAtFirstchoice)),
				antiSpamChoice = get<bool>(nameof(antiSpamChoice)),
				randomizeQA = (randomQA)get<int>(nameof(randomizeQA)),
				reverseDictionary = get<bool>(nameof(reverseDictionary)),
				AnswerPosition = (answerPosition)get<int>(nameof(AnswerPosition)),
				questionSeparator = (QuestionSeparator)get<int>(nameof(questionSeparator)),
				spaceAfterSeparator = (SpaceAfterSeparator)get<int>(nameof(spaceAfterSeparator)),
				useAd = get<bool>(nameof(useAd)),
				showDebugging = get<bool>(nameof(showDebugging)),
				customChoiceHeader = get<string>(nameof(customChoiceHeader)),
				halfResolution = get<int>(nameof(halfResolution))
			};
			return cfg;
		}

		static T get<T>(string key)
		{
			if (!App._cfg.Values.ContainsKey(key))
			{
				AddMissing(key);
			}
			return (T)App._cfg.Values[key];
		}

		static void AddMissing(string key)
		{
			switch (key)
			{
				case "language": App._cfg.Values.Add(key, 0); return;
				case "isItLightTheme": App._cfg.Values.Add(key, false); return;
				case "isLimitTime": App._cfg.Values.Add(key, false); return;
				case "obfuscateWrongText": App._cfg.Values.Add(key, false); return;
				case "defaultMode": App._cfg.Values.Add(key, (int)matchMode.Easy); return;
				case "limitTime": App._cfg.Values.Add(key, TimeSpan.FromMinutes(5).TotalSeconds); return;
				case "totalChoice": App._cfg.Values.Add(key, 3); return;
				case "displayTextSize": App._cfg.Values.Add(key, 18); return;
				case "useSeed": App._cfg.Values.Add(key, false); return;
				case "defaultSeed": App._cfg.Values.Add(key, -1); return;
				case "autoScrollContent": App._cfg.Values.Add(key, true); return;
				case "AfterFinalChoice": App._cfg.Values.Add(key, (int)whenFinalChoice.EndPage); return;
				case "saveStatAfterEnd": App._cfg.Values.Add(key, true); return;
				case "hintAtFirstchoice": App._cfg.Values.Add(key, true); return;
				case "antiSpamChoice": App._cfg.Values.Add(key, true); return;
				case "randomizeQA": App._cfg.Values.Add(key, (int)randomQA.No); return;
				case "reverseDictionary": App._cfg.Values.Add(key, false); return;
				case "AnswerPosition": App._cfg.Values.Add(key, (int)answerPosition.Bottom); return;
				case "questionSeparator": App._cfg.Values.Add(key, (int)QuestionSeparator.Dot); return;
				case "spaceAfterSeparator": App._cfg.Values.Add(key, (int)SpaceAfterSeparator.Yes); return;
				case "useAd": App._cfg.Values.Add(key, true); return;
				case "showDebugging": App._cfg.Values.Add(key, false); return;
				case "customChoiceHeader": App._cfg.Values.Add(key, "ABCDE"); return;
				case "halfResolution": App._cfg.Values.Add(key, -1); return;
				default: return;
			}
		}

		public static void ResetConfig()
		{
			App._cfg.Values.Clear();
			GetSettings();
		}

		public AppConfigModel()
		{
			//Provide default setting
			language = 0;
			isItLightTheme = false;
			isLimitTime = false;
			obfuscateWrongText = false;
			defaultMode = matchMode.Easy;
			limitTime = TimeSpan.FromMinutes(5).TotalSeconds;
			totalChoice = 3;
			displayTextSize = 18;
			useSeed = false;
			defaultSeed = -1;
			autoScrollContent = true;
			AfterFinalChoice = whenFinalChoice.EndPage;
			saveStatAfterEnd = true;
			hintAtFirstchoice = true;
			antiSpamChoice = true;
			randomizeQA = randomQA.No;
			reverseDictionary = false;
			AnswerPosition = answerPosition.Bottom;
			questionSeparator = QuestionSeparator.Dot;
			spaceAfterSeparator = SpaceAfterSeparator.Yes;
			useAd = true;
			showDebugging = false;
			customChoiceHeader = "ABCDE";
			halfResolution = -1;
		}
	}

	public enum whenFinalChoice
	{
		EndPage,
		Restart,
		BackHome
	}

	public enum choiceDisplayMode
	{
		Center,
		Bottom,
		Write
	}

	public enum randomQA
	{
		No,
		OnlyQuestion,
		All
	}

	public enum answerPosition
	{
		BehindAnswer,
		Bottom
	}

	public enum QuestionSeparator
	{
		Dot,
		Bracket
	}

	public enum SpaceAfterSeparator
	{
		Yes,
		No
	}
}