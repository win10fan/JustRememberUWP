using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using Windows.UI.Xaml;

namespace JustRemember_UWP
{
	public class Settings
	{
		public string language { get; set; }
		public ApplicationTheme theme { get; set; }
		public bool isLimitTime { get; set; }
		public bool showWrongContent { get; set; }
        public challageMode defaultMode { get; set; }
        public float limitTime { get; set; }
        public int totalChoice { get; set; }
        public int displayTextSize { get; set; }
        public List<statInfo> stat { get; set; }
        public int defaultSeed { get; set; }
        public bool autoScrollContent { get; set; }

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
            defaultSeed = -1;
            autoScrollContent = true;
			//notes = new List<Note>();
			//sessions = new List<SessionInfo>();
		}

        public static void Save(Settings content)
        {
            Utilities.currentSettings = content;
            if (Utilities.initialize)
            {
                if (File.Exists(Utilities.savedPath))
                {
                    File.Delete(Utilities.savedPath);
                }
                using (StreamWriter write = new StreamWriter(File.Create(Utilities.savedPath)))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(Settings));
                    xs.Serialize(write, content);
                }
            }
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

        public static Settings Load()
        {
            if (!Utilities.initialize) { return null; }
            if (File.Exists(Utilities.savedPath))
            {
                return Load(Utilities.savedPath);
            }
            return new Settings();
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