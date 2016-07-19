using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using Windows.UI.Xaml;

namespace JustRemember_UWP
{
	public class Settings
	{
		public string language;
		public ApplicationTheme theme;
		public bool isLimitTime;
		public bool showWrongContent;
		public challageMode defaultMode;
		public float limitTime;
		public int totalChoice;
		public int displayTextSize;
		public List<statInfo> stat;

		//public List<Note> notes;
		//public List<SessionInfo> sessions;
		public Settings() //Default setting
		{
			language = "en-US";
			theme = ApplicationTheme.Dark;
			isLimitTime = false;
			showWrongContent = false;
			defaultMode = challageMode.Easy;
			limitTime = 30;
			totalChoice = 3;
			displayTextSize = 14;
			stat = new List<statInfo>();
			//notes = new List<Note>();
			//sessions = new List<SessionInfo>();
		}
		
		public static void Save(string path, Settings content)
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			using (StreamWriter write = new StreamWriter(File.Create(path)))
			{
				XmlSerializer xs = new XmlSerializer(typeof(Settings));
				xs.Serialize(write, content);
			}
		}

		public static void Save()
		{
			if (Utilities.initialize)
			{
				Save(Utilities.savedPath, Utilities.currentSettings);
			}
		}

		public static Settings Load(string path)
		{
			if (File.Exists(path))
			{
				using (var fs = File.Open(path, FileMode.Open))
				{
					StreamReader read = new StreamReader(fs);
					XmlSerializer xs = new XmlSerializer(typeof(Settings));
					return (Settings)xs.Deserialize(fs);
				}
			}
			else
			{
				new Settings();
			}
			return new Settings();
		}
	}
}