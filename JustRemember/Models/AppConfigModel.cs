using JustRemember.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace JustRemember.Models
{
	public class AppConfigModel
	{
		public int language { get; set; }
		public bool isItLightTheme { get; set; }
		public bool isLimitTime { get; set; }
		public bool obfuscateWrongText { get; set; }
		public matchMode defaultMode { get; set; }
		public double limitTime { get; set; }
		public int totalChoice { get; set; }
		public int displayTextSize { get; set; }
		public bool useSeed { get; set; }
		public int defaultSeed { get; set; }
		public bool autoScrollContent { get; set; }
		public whenFinalChoice AfterFinalChoice { get; set; }
		/// <summary>
		/// If "AfterFinalChoice" is not "EndPage" always save stat or discard
		/// </summary>
		public bool saveStatAfterEnd { get; set; }
		public bool hintAtFirstchoice { get; set; }
		public choiceDisplayMode choiceStyle { get; set; }
		public bool antiSpamChoice { get; set; }

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
				string oldset = await FileIO.ReadTextAsync((StorageFile)tfile);
				if (oldset == settings)
				{
					return;
				}
				else
				{
					await tfile.DeleteAsync();
					file = await ApplicationData.Current.LocalFolder.CreateFileAsync("appconfig");
				}
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
}