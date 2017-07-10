using JustRemember.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace JustRemember.Models
{
	public class AppConfigModel
	{
		public void Set<T>(ref T container, T value)
		{
			if (!Equals(container, value))
			{
				isDirty = true;
				container = value;
			}
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

		matchMode mode;
		public matchMode defaultMode
		{
			get => mode;
			set => Set(ref mode, value);
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

		whenFinalChoice end;
		public whenFinalChoice AfterFinalChoice
		{
			get => end;
			set => Set(ref end, value);
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

		choiceDisplayMode style;
		public choiceDisplayMode choiceStyle
		{
			get => style;
			set => Set(ref style, value);
		}

		bool nospam;
		public bool antiSpamChoice
		{
			get => nospam;
			set => Set(ref nospam, value);
		}

		randomQA _randQA;
		public randomQA randomizeQA
		{
			get => _randQA;
			set => Set(ref _randQA, value);
		}

		bool _frontback;
		public bool reverseDictionary
		{
			get => _frontback;
			set => Set(ref _frontback, value);
		}

		[JsonIgnore]
		public int randomizeQAInt
		{
			get => (int)_randQA;
			set => Set(ref _randQA, (randomQA)value);
		}

		[JsonIgnore]
		public static List<string> languages = new List<string>()
		{
			"en",
			"th-TH"
		};

		[JsonIgnore]
		public static bool isDirty;

		public static async Task<AppConfigModel> Load2()
		{
			AppConfigModel settings = new AppConfigModel();
			var tf = await ApplicationData.Current.LocalFolder.TryGetItemAsync("appconfig");
			if (tf == null)
			{
				var nit = new AppConfigModel();
				await nit.Save();
				return nit;
			}
			else
			{
				var proper = await tf.GetBasicPropertiesAsync();
				if (proper.Size < 2u)
				{
					await tf.DeleteAsync();
					await Load2();
				}
			}
			StorageFile configPath = await ApplicationData.Current.LocalFolder.GetFileAsync("appconfig");
			string jsonConfig = await FileIO.ReadTextAsync(configPath);
			return await Json.ToObjectAsync<AppConfigModel>(jsonConfig);
		}

		public async Task<AppConfigModel> Load()
		{
			AppConfigModel settings = new AppConfigModel();
			var tf = await ApplicationData.Current.LocalFolder.TryGetItemAsync("appconfig");
			if (tf == null)
			{
				await Save();
				return new AppConfigModel();
			}
			else
			{
				var proper = await tf.GetBasicPropertiesAsync();
				if (proper.Size < 2u)
				{
					await tf.DeleteAsync();
					await Load();
				}
			}
			StorageFile configPath = await ApplicationData.Current.LocalFolder.GetFileAsync("appconfig");
			string jsonConfig = await FileIO.ReadTextAsync(configPath);
			return await Json.ToObjectAsync<AppConfigModel>(jsonConfig);
		}

		[JsonIgnore]
		bool isSaving;
		public async Task Save()
		{
			if (isSaving) { return; }
			isSaving = true;
			string settings = await Json.StringifyAsync(this);
			StorageFile file = null;
			var tfile = await ApplicationData.Current.LocalFolder.TryGetItemAsync("appconfig");
			if (tfile == null)
			{
				file = await ApplicationData.Current.LocalFolder.CreateFileAsync("appconfig");
			}
			else
			{
				await tfile.DeleteAsync();
				file = await ApplicationData.Current.LocalFolder.CreateFileAsync("appconfig");
			}
			await FileIO.WriteTextAsync(file, settings);
			isSaving = false;
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
		}

		public static async void SetLanguage(int selected)
		{
			var folder = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("lang");
			if (folder == null)
			{
				folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("lang");
			}
			else
			{
				if (GetLanguage() == languages[App.Config.language])
				{
					return;
				}
				else
				{
					var files = await folder.GetFilesAsync();
					foreach (var file in files)
					{
						await file.DeleteAsync();
					}
					await folder.DeleteAsync(StorageDeleteOption.PermanentDelete);
					SetLanguage(selected);
				}
			}
			await folder.CreateFileAsync(languages[selected]);
		}

		public static string GetLanguage()
		{
			foreach (var lang in languages)
			{
				if (File.Exists($"{ApplicationData.Current.LocalFolder.Path}\\lang\\{lang}"))
				{
					return languages[languages.IndexOf(lang)];
				}
			}
			return languages[0];
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
}