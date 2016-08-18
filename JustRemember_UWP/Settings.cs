using System.IO;
using Windows.UI.Xaml;
using System;

namespace JustRemember_UWP
{
	public class Settings
	{
		public int language { get; set; }
		public ApplicationTheme theme { get; set; }
		public bool isLimitTime { get; set; }
		public bool showWrongContent { get; set; }
        public challageMode defaultMode { get; set; }
        public float limitTime { get; set; }
        public int totalChoice { get; set; }
        public int displayTextSize { get; set; }
        public int defaultSeed { get; set; }
        public bool autoScrollContent { get; set; }
        public afterEnd AfterFinalChoice { get; set; }
        public ifNotGotoEnd TodoWithStat { get; set; }
        //TODO:Add session system
        public Settings() //Default setting
		{
			language = 0;
			theme = ApplicationTheme.Dark;
			isLimitTime = false;
			showWrongContent = false;
			defaultMode = challageMode.Easy;
			limitTime = 30;
			totalChoice = 3;
			displayTextSize = 14;
            defaultSeed = -1;
            autoScrollContent = true;
            AfterFinalChoice = afterEnd.gotoEnd;
            TodoWithStat = ifNotGotoEnd.saveAllStat;
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
                File.WriteAllText(Utilities.savedPath, Serialize(content));
                //using (StreamWriter write = new StreamWriter(File.Create(Utilities.savedPath)))
                //{
                //    XmlSerializer xs = new XmlSerializer(typeof(Settings));
                //    xs.Serialize(write, content);
                //}
            }
        }

        public static string Serialize(Settings content)
        {
            string value = "";
            value += $"{nameof(language)}={content.language}{Environment.NewLine}";
            value += $"{nameof(theme)}={content.theme.ToString()}{Environment.NewLine}";
            value += $"{nameof(isLimitTime)}={StringSerializeHelper.BoolToString(content.isLimitTime)}{Environment.NewLine}";
            value += $"{nameof(showWrongContent)}={StringSerializeHelper.BoolToString(content.showWrongContent)}{Environment.NewLine}";
            value += $"{nameof(defaultMode)}={content.defaultMode}{Environment.NewLine}";
            value += $"{nameof(limitTime)}={content.limitTime}{Environment.NewLine}";
            value += $"{nameof(totalChoice)}={content.totalChoice}{Environment.NewLine}";
            value += $"{nameof(displayTextSize)}={content.displayTextSize}{Environment.NewLine}";
            value += $"{nameof(defaultSeed)}={content.defaultSeed}{Environment.NewLine}";
            value += $"{nameof(autoScrollContent)}={StringSerializeHelper.BoolToString(content.autoScrollContent)}{Environment.NewLine}";
            value += $"{nameof(AfterFinalChoice)}={content.AfterFinalChoice.ToString()}{Environment.NewLine}";
            value += $"{nameof(TodoWithStat)}={content.TodoWithStat.ToString()}";
            return value;
        }

        public static Settings DeSerialize(string content)
        {
            Settings value = new Settings();
            string line;

            StringReader file = new StringReader(content);
            while ((line = file.ReadLine()) != null)
            {
                if (line.StartsWith(nameof(language))) { value.language = StringSerializeHelper.GetInt(line, nameof(language)); }
                else if (line.StartsWith(nameof(theme))) { value.theme = StringSerializeHelper.GetEnum<ApplicationTheme>(line, nameof(theme)); }
                else if (line.StartsWith(nameof(isLimitTime))) { value.isLimitTime = StringSerializeHelper.GetBool(line, nameof(isLimitTime)); }
                else if (line.StartsWith(nameof(showWrongContent))) { value.showWrongContent = StringSerializeHelper.GetBool(line, nameof(showWrongContent)); }
                else if (line.StartsWith(nameof(defaultMode))) { value.defaultMode = StringSerializeHelper.GetEnum<challageMode>(line, nameof(defaultMode)); }
                else if (line.StartsWith(nameof(limitTime))) { value.limitTime = StringSerializeHelper.GetFloat(line, nameof(limitTime)); }
                else if (line.StartsWith(nameof(totalChoice))) { value.totalChoice = StringSerializeHelper.GetInt(line, nameof(totalChoice)); }
                else if (line.StartsWith(nameof(displayTextSize))) { value.displayTextSize = StringSerializeHelper.GetInt(line, nameof(displayTextSize)); }
                else if (line.StartsWith(nameof(defaultSeed))) { value.defaultSeed = StringSerializeHelper.GetInt(line, nameof(defaultSeed)); }
                else if (line.StartsWith(nameof(autoScrollContent))) { value.autoScrollContent = StringSerializeHelper.GetBool(line, nameof(autoScrollContent)); }
                else if (line.StartsWith(nameof(AfterFinalChoice))) { value.AfterFinalChoice = StringSerializeHelper.GetEnum<afterEnd>(line, nameof(AfterFinalChoice)); }
                else if (line.StartsWith(nameof(TodoWithStat))) { value.TodoWithStat = StringSerializeHelper.GetEnum<ifNotGotoEnd>(line, nameof(TodoWithStat)); }
            }
            return value;
        }

        public static void Save(string path, Settings content)
		{
            if (File.Exists(path))
			{
				File.Delete(path);
			}
            File.WriteAllText(path, Serialize(content));
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
                return DeSerialize(File.ReadAllText(path));
				//using (var fs = File.Open(path, FileMode.Open))
				//{
				//	StreamReader read = new StreamReader(fs);
				//	XmlSerializer xs = new XmlSerializer(typeof(Settings));
				//	return (Settings)xs.Deserialize(fs);
				//}
			}
			else
			{
				new Settings();
			}
			return new Settings();
		}
	}
}