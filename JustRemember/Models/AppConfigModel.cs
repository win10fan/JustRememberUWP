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
		public string customChoiceHeader { get => _chdr == null ? "ABCDE" : _chdr; set => Set(ref _chdr, value); }

		double hrs = -1;
		public double halfResolution { get => hrs; set => Set(ref hrs, value); }

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
			AppConfigModel cfg = new AppConfigModel();
			cfg.language = get<int>(nameof(language));
			cfg.isItLightTheme = get<bool>(nameof(isItLightTheme));
			cfg.isLimitTime = get<bool>(nameof(isLimitTime));
			cfg.obfuscateWrongText = get<bool>(nameof(obfuscateWrongText));
			cfg.defaultMode = (matchMode)get<int>(nameof(defaultMode));
			cfg.limitTime = get<double>(nameof(limitTime));
			cfg.totalChoice = get<int>(nameof(totalChoice));
			cfg.displayTextSize = get<int>(nameof(displayTextSize));
			cfg.useSeed = get<bool>(nameof(useSeed));
			cfg.defaultSeed = get<int>(nameof(defaultSeed));
			cfg.autoScrollContent = get<bool>(nameof(autoScrollContent));
			cfg.AfterFinalChoice = (whenFinalChoice)get<int>(nameof(AfterFinalChoice));
			cfg.saveStatAfterEnd = get<bool>(nameof(saveStatAfterEnd));
			cfg.hintAtFirstchoice = get<bool>(nameof(hintAtFirstchoice));
			cfg.antiSpamChoice = get<bool>(nameof(antiSpamChoice));
			cfg.randomizeQA = (randomQA)get<int>(nameof(randomizeQA));
			cfg.reverseDictionary = get<bool>(nameof(reverseDictionary));
			cfg.AnswerPosition = (answerPosition)get<int>(nameof(AnswerPosition));
			cfg.questionSeparator = (QuestionSeparator)get<int>(nameof(questionSeparator));
			cfg.spaceAfterSeparator = (SpaceAfterSeparator)get<int>(nameof(spaceAfterSeparator));
			cfg.useAd = get<bool>(nameof(useAd));
			cfg.showDebugging = get<bool>(nameof(showDebugging));
			cfg.customChoiceHeader = get<string>(nameof(customChoiceHeader));
			cfg.halfResolution = get<double>(nameof(halfResolution));
			return cfg;
		}

		static T get<T>(string key)
		{
			if (!App._cfg.Values.ContainsKey(key))
			{
				App._cfg.Values.Add(key, default(T));
			}
			return (T)App._cfg.Values[key];
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