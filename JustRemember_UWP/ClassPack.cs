using System.Collections.Generic;
using System.Linq;
using System.IO;
using Windows.Storage;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization.Json;

namespace JustRemember_UWP
{
	public static class Utilities
	{
		public static T ParseEnum<T>(string value)
		{
			return (T)Enum.Parse(typeof(T), value, true);
		}

		public static IEnumerable<string> GetBreadcrumbPath(this string path)
		{
			string tmp = "";
			if (!path.StartsWith("/"))
			{
				tmp = "/" + path;
			}
			var index = tmp.IndexOf("/");
			var indices = tmp.Select((x, idx) => new { x, idx }).Where(p => p.x == '/' && p.idx > index + 1).Select(p => p.idx);

			foreach (var idx in indices)
			{
				yield return path.Substring(0, idx - 1);
			}
			yield return path;
		}

		public static string ToStringAsTime(this float total)
		{
			TimeSpan ttlspan = TimeSpan.FromSeconds(total);
			return $"{ttlspan.Minutes}:{ttlspan.Seconds}";
		}
        
		public static string ObscureText(this string text)
		{
			string res = "";
			for (int i = 0; i < text.Length; i++)
			{
				res += "?";
			}
			return res;
		}
		
		public static float Clamp(float value,float min,float max)
		{
			if (value <= min)
			{
				value = min;
			}
			else if (value >= max)
			{
				value = max;
			}
			return value;
		}
		
		public static Settings currentSettings
		{
			get
			{
				if (_cs == null)
				{
					savedPath = ApplicationData.Current.LocalFolder.Path + "\\settings.xml";
					Debug.WriteLine(ApplicationData.Current.LocalFolder.Path);
					Debug.WriteLine(savedPath);
					_cs = Settings.Load(Path.Combine(ApplicationData.Current.LocalFolder.Path, "\\settings.xml"));
				}
				return _cs;
			}
			set
			{
				if (_cs == null)
				{
					savedPath = ApplicationData.Current.LocalFolder.Path + "\\settings.xml";
					Debug.WriteLine(ApplicationData.Current.LocalFolder.Path);
					Debug.WriteLine(savedPath);
					_cs = Settings.Load(Path.Combine(ApplicationData.Current.LocalFolder.Path, "\\settings.xml"));
				}
				_cs = value;
			}
		}
		internal static Settings _cs;
		public static string savedPath;
		public static bool initialize;
		public static List<Note> notes;
		public static List<SessionInfo> sessions;
		public static Note selected;
		public static bool isSmallLoaderMode;//Check if page is run in Match page... "OtherPage" frame
		public static statInfo newStat;
        public static readonly string[] lang = new string[] { "en", "th" };
    }

    public class StatList
    {
        public List<statInfo> Stats;
        
        public StatList()
        {
            Stats = new List<statInfo>();
        }
        
        public static List<statInfo> Load()
        {
            List<statInfo> value = new List<statInfo>();
            string path = ApplicationData.Current.LocalFolder.Path + "\\Stat";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return new List<statInfo>();
            }
            string[] files = Directory.GetFiles(path);
            if (files.Length > 0)
            {
                foreach (string s in files)
                {
                    value.Add(statInfo.DeSerialize(File.ReadAllText(s)));
                }
            }
            return value;
        }

        public static void SaveAll(List<statInfo> info)
        {
            string path = ApplicationData.Current.LocalFolder.Path + "\\Stat";
            foreach (var item in info)
            {
                var date = DateTime.ParseExact(item.dateandTime, "dd MM yyyy - hh:mm:ss", CultureInfo.InvariantCulture);
                File.WriteAllText($"{path}\\{date.ToString("dd-MM-yyyy-hh-mm-ss")}.stat", statInfo.Serialize(item));
                Debug.WriteLine($"Path = {path}\\{date.ToString("dd-MM-yyyy-hh-mm-ss")}.stat");
            }
        }

        public static void Save(statInfo info)
        {
            string path = ApplicationData.Current.LocalFolder.Path + "\\Stat";
            var date = DateTime.ParseExact(info.dateandTime, "dd MM yyyy - hh:mm:ss", CultureInfo.InvariantCulture);
            File.WriteAllText($"{path}\\{date.ToString("dd-MM-yyyy-hh-mm-ss")}.stat", statInfo.Serialize(info));
        }
    }

    public class statInfo
    {
        public string dateandTime;
        public int totalWords;
        public int totalChoice;
        public List<int> wrongPerchoice = new List<int>();
        public bool useTimeLimit;
        public float totalTime;
        public float totalLimitTime;
        public challageMode currentMode;
        public string noteTitle;

        public static string Serialize(statInfo info)
        {
            string content = "";
            content += $"{nameof(dateandTime)}={info.dateandTime}{Environment.NewLine}";
            content += $"{nameof(totalWords)}={info.totalWords}{Environment.NewLine}";
            content += $"{nameof(totalChoice)}={info.totalChoice}{Environment.NewLine}";
            content += $"{nameof(wrongPerchoice)}={StringSerializeHelper.ListOfIntToString(info.wrongPerchoice)}{Environment.NewLine}";
            content += $"{nameof(useTimeLimit)}={StringSerializeHelper.BoolToString(info.useTimeLimit)}{Environment.NewLine}";
            content += $"{nameof(totalTime)}={info.totalTime}{Environment.NewLine}";
            content += $"{nameof(totalLimitTime)}={info.totalLimitTime}{Environment.NewLine}";
            content += $"{nameof(currentMode)}={info.currentMode.ToString()}{Environment.NewLine}";
            content += $"{nameof(noteTitle)}={info.noteTitle}";
            return content;
        }

        public static statInfo DeSerialize(string info)
        {
            statInfo value = new statInfo();
            string line;

            StringReader file = new StringReader(info);
            while ((line = file.ReadLine()) != null)
            {
                if (line.StartsWith(nameof(dateandTime)))
                {
                    value.dateandTime = StringSerializeHelper.GetString(line, nameof(dateandTime));
                }
                else if (line.StartsWith(nameof(totalWords)))
                {
                    value.totalWords = StringSerializeHelper.GetInt(line, nameof(totalWords));
                }
                else if (line.StartsWith(nameof(totalChoice)))
                {
                    value.totalChoice = StringSerializeHelper.GetInt(line, nameof(totalChoice));
                }
                else if (line.StartsWith(nameof(wrongPerchoice)))
                {
                    value.wrongPerchoice = StringSerializeHelper.GetListOfInt(line, nameof(wrongPerchoice));
                }
                else if (line.StartsWith(nameof(useTimeLimit)))
                {
                    value.useTimeLimit = StringSerializeHelper.GetBool(line, nameof(useTimeLimit));
                }
                else if (line.StartsWith(nameof(totalTime)))
                {
                    value.totalTime = StringSerializeHelper.GetFloat(line, nameof(totalTime));
                }
                else if (line.StartsWith(nameof(totalLimitTime)))
                {
                    value.totalLimitTime = StringSerializeHelper.GetFloat(line, nameof(totalLimitTime));
                }
                else if (line.StartsWith(nameof(currentMode)))
                {
                    value.currentMode = StringSerializeHelper.GetEnum<challageMode>(line, nameof(currentMode));
                }
                else if (line.StartsWith(nameof(noteTitle)))
                {
                    value.noteTitle = StringSerializeHelper.GetString(line, nameof(noteTitle));
                }
            }
            return value;
        }

        //Read-only values
        public string timeProgress
        {
            get
            {
                if (useTimeLimit)
                {
                    return $"{Utilities.ToStringAsTime(totalTime)}/{Utilities.ToStringAsTime(totalLimitTime)}";
                }
                return "N/A";
            }
        }
        public int timeInPercent
        {
            get
            {
                if (useTimeLimit)
                {
                    float cache = (totalTime / totalLimitTime) * 100;
                    return Convert.ToInt32(cache);
                }
                return 0;
            }
        }
        public string choiceInfo
        {
            get
            {
                return $"{totalWrong} wrong choice out of {totalWords}. Average wrong {wrongPerchoice.Average().ToString("0.00")}";
            }
        }
        public string titleInfo
        {
            get
            {
                return $"{noteTitle} - {dateandTime}";
            }
        }
        public int totalWrong
        {
            get
            {
                int value = 0;
                foreach (int i in wrongPerchoice)
                {
                    value += i;
                }
                return value;
            }
        }
    }

    public static class StringSerializeHelper
    {
        public static string GetString(string line,string command)
        {
            //Pattern {item}={value}
            return line.Substring(command.Length + 1);
        }

        public static int GetInt(string line,string command)
        {
            return int.Parse(line.Substring(command.Length + 1));
        }

        public static string BoolToString(bool value)
        {
            return value ? "1" : "0";
        }

        public static bool GetBool(string line,string command)
        {
            string value = line.Substring(command.Length + 1);
            if (value == "0") { return false; }
            else
            {
                return true;
            }
        }

        public static float GetFloat(string line,string command)
        {
            string value = line.Substring(command.Length + 1);
            return float.Parse(value);
        }

        public static T GetEnum<T>(string line,string command)
        {
            string value = line.Substring(command.Length + 1);
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static string ListOfIntToString(List<int> info)
        {
            string value = "";
            foreach (int i in info)
            {
                value += $"{i}|";
            }
            if (value.EndsWith("|"))
            {
                value = value.Remove(value.Length - 1, 1);
            }
            return value;
        }
        
        public static List<int> GetListOfInt(string line,string command)
        {
            string value = line.Substring(command.Length + 1);
            string[] values = value.Split('|');
            List<int> returned = new List<int>();
            foreach (string s in values)
            {
                returned.Add(int.Parse(s));
            }
            return returned;
        }
    }

    public enum afterEnd
    {
        gotoEnd,
        restartMatch,
        gotoMain
    }
    public enum ifNotGotoEnd
    {
        discardAllStat,
        saveAllStat
    }

    public enum textMode { Char, WhiteSpace }

	public struct textlist
	{
		public static readonly textlist Empty = new textlist("", "\0", textMode.Char);
		public string Text;
		public string Commands;
		public textMode Mode;
		public textlist(string text)
		{
			Text = text;
			Commands = "\0";
			Mode = textMode.Char;
		}

		public textlist(string text, char command)
		{
			Text = text;
			Commands = "";
			Commands += command;
			Mode = textMode.Char;
		}

		public textlist(string text, string commands)
		{
			Text = text;
			Commands = commands;
			Mode = textMode.Char;
		}

		public textlist(string text, string commands,textMode mode)
		{
			Text = text;
			Commands = commands;
			Mode = mode;
		}

		public override string ToString()
		{
			return Text;
		}
	}

	public class SelectorItem
	{
		public selectorType Type { get; set; }
		public string Name
		{
			get
			{
				if (content_ses != null)
				{
					return content_ses.LastTitle;
				}
				else
				{
					return content_note.Title;
				}
			}
		}
		public Note content_note;
		public SessionInfo content_ses;

		public override string ToString()
		{
			return Name;
		}

		public SelectorItem()
		{
			Type = selectorType.note;
			content_note = new Note();
		}

		public SelectorItem(Note content)
		{
			Type = selectorType.note;
			content_note = content;
		}

		public SelectorItem(SessionInfo content)
		{
			Type = selectorType.session;
			content_ses = content;
		}

		public static implicit operator SelectorItem(Note input)
		{
			return new SelectorItem(input);
		}

		public static implicit operator SelectorItem(SessionInfo input)
		{
			return new SelectorItem(input);
		}

		public static SelectorItem GetItems()
		{
			return new SelectorItem();
		}
	}

	public enum selectorType
	{
		file,
		note,
		session
	}

	public struct Note
	{
		public string Title;
		public string Content;

		public Note(string content, string title)
		{
			Title = title;
			Content = content;
		}

		public static implicit operator Note(SelectorItem item)
		{
			return item.content_note;
		}

		public static async void Save(Note saved)
		{
			var path = KnownFolders.DocumentsLibrary;
			var file = await path.CreateFileAsync(saved.Title + ".txt", CreationCollisionOption.ReplaceExisting);
			await FileIO.WriteTextAsync(file, saved.Content);
		}

		public static async void Delete(string filename)
		{
			var path = KnownFolders.DocumentsLibrary;
			var file = await path.TryGetItemAsync(filename);
			if (file != null)
			{
				await file.DeleteAsync();
			}
		}

		public static Note Load(string filename)
		{
			Note info = new Note();
			info.Title = filename;
			string path = Path.Combine(ApplicationData.Current.RoamingFolder.Path, filename);
			if (!File.Exists(path))
			{
				return info;
			}
			using (StreamReader reader = File.OpenText(path))
			{
				info.Content = reader.ReadToEnd();
				return info;
			}
		}

		public static async Task<Note> LoadAsync(string filename)
		{
			Note result = new Note();
			if (filename.Contains(".txt"))
			{
				result.Title = filename.Replace(".txt", "");
			}
			var path = await ApplicationData.Current.RoamingFolder.GetFolderAsync("Note");
			var file = await path.GetFileAsync(filename);
			if (file == null)
			{
				return result;
			}
			else
			{
				result.Content = await FileIO.ReadTextAsync(file);
				return result;
			}
		}

		public static async Task GetNotesList()
		{
			if (!Directory.Exists(ApplicationData.Current.RoamingFolder.Path + "\\Note\\"))
			{
				StorageFolder fol = ApplicationData.Current.RoamingFolder;
				await fol.CreateFolderAsync("Note");
			}
			var files = Directory.GetFiles(ApplicationData.Current.RoamingFolder.Path + "\\Note\\");
			if (files.Length == 0)
			{
                Utilities.notes?.Clear();
				return;
			}
			List<Note> notes = new List<Note>();
			foreach (var f in files)
			{
				FileInfo info = new FileInfo(f);
				if (info.Extension == ".txt")
				{
					Note newone = await LoadAsync(info.Name);
					notes.Add(newone);
				}
			}
			Utilities.notes = notes;
		}

		//public static async Task<List<Note>> GetDocList()
		//{
		//	//Same as get note.. But get in Document path
		//	var files = Directory.GetFiles(KnownFolders.DocumentsLibrary.Path);
		//	List<Note> notes = new List<Note>();
		//	foreach (var f in files)
		//	{

		//	}
		//}
	}

	public class SessionInfo
	{
		public string LastTitle
		{
			get
			{
				return current.noteTitle;
			}
		}
		public statInfo current;
		public int currentAt;
		public int lastCorrect;
		public List<textlist> lastTextList = new List<textlist>();
		public List<string> choiceList = new List<string>();
		public float progressFillAmount;
		public List<string> lastChoices = new List<string>();

		public SessionInfo()
		{
			current = new statInfo();
			currentAt = -1;
			lastCorrect = 1;
			lastTextList = new List<textlist>();
			choiceList = new List<string>();
			progressFillAmount = 0;
			lastChoices = new List<string>();
		}
        
		//public SessionInfo(MainMerge now)
		//{
		//	current = now.newStat;
		//	currentAt = now.currentProgress;
		//	lastCorrect = now.currentValidChoice;
		//	lastTextList = now.textList;
		//	choiceList = now.choiceList;
		//	LastTitle = now.mainPageTitleText.text;
		//	progressFillAmount = now.mainPageProgressbar.fillAmount;
		//	var items = MainMerge.n.mainPageChoiceParent.GetComponentsInChildren<Text>();
		//	if (items != null)
		//	{
		//		foreach (var i in items)
		//		{
		//			lastChoices.Add(i.text);
		//		}
		//	}
		//	//for (int i = 0; i < 10; i++)
		//	//{
		//	//	GameObject go = GameObject.Find("CHSpecific" + i);
		//	//	if (go != null)
		//	//	{
		//	//		Text ct = go.transform.FindChild("ChoiceText").GetComponent<Text>();
		//	//		lastChoices.Add(ct.text);
		//	//	}
		//	//}
		//}

		public static implicit operator SessionInfo(SelectorItem input)
		{
			return input.content_ses;
		} 

		public static void Save(List<SessionInfo> now)
		{
			string path = Path.Combine(ApplicationData.Current.RoamingFolder.Path, "\\Session.xml");
            DataContractJsonSerializer jsoninfo = new DataContractJsonSerializer(typeof(List<SessionInfo>));
            //string content = JsonUtility.ToJson(now);
            using (Stream write = new FileStream(path, FileMode.CreateNew))
            {
                jsoninfo.WriteObject(write, now);
                //write.Write(xser.ser);
            }
		}

        public static List<SessionInfo> Load()
        {
            string path = Path.Combine(ApplicationData.Current.RoamingFolder.Path, "\\Session.xml");
            if (!File.Exists(path))
            {
                return new List<SessionInfo>();
            }
            List<SessionInfo> allses = new List<SessionInfo>();
            //SessionInfo sif = new SessionInfo();
            DataContractJsonSerializer info = new DataContractJsonSerializer(typeof(List<SessionInfo>));
            using (Stream stream = new FileStream(path, FileMode.Open))
            {
                allses = (List<SessionInfo>)info.ReadObject(stream);
            }
            Utilities.sessions = allses;
            return allses;
        }
	}
    
	public enum Operator
	{
		New,
		Open,
		Save,
		SaveNew,
		Quit
	}
	
	public enum challageMode
	{
		Easy,
		Normal,
		Hard
	}

    public static class PrenoteLoader
    {
        public static bool isDeployed
        {
            get
            {
                if (Directory.Exists(ApplicationData.Current.LocalFolder.Path + "\\Prenote"))
                {
                    return true;
                }
                return false;
            }
        }

        public static void DeployPrenote()
        {
            string prenotepath = Windows.ApplicationModel.Package.Current.InstalledLocation.Path + "\\Prenote";
            var files = Directory.GetFiles(prenotepath);
            string deployPath = ApplicationData.Current.LocalFolder.Path + "\\Prenote";
            for (int i = 0;i < files.Length - 1;i++)
            {
                string[] path = Path.GetFileName(files[i]).Split('-');
                if (path.Length == 3)
                {
                    string cachep = $"{deployPath}\\{path[0]}\\{path[1]}";
                    if (!Directory.Exists(cachep))
                    {
                        Directory.CreateDirectory(cachep);
                    }
                    cachep += "\\" + path[2];
                    File.Copy(files[i], cachep);
                }
                //else //Deeper prenote
                //{                    
                //}
            }
        }
    }
}