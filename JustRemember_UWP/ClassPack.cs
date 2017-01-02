using System.Collections.Generic;
using System.Linq;
using System.IO;
using Windows.Storage;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml.Controls;

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
			return $"{ttlspan.Minutes:00}:{ttlspan.Seconds:00}";
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
		public static SelectorItem selected;
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

    public enum selectMode
    {
        styleA,
        styleB,
        styleC
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

        public string ItemType
        {
            get
            {
                return Type.ToString();
            }
        }

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

        public string Content
        {
            get
            {
                if (itSession)
                {
                    return "";
                }
                return content_note.Content;
            }
        }

        public string Date
        {
            get
            {
                if (itNote)
                {
                    return "";
                }
                return content_ses.current.dateandTime;
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
			Type = selectorType.Note;
			content_note = new Note();
		}

		public SelectorItem(Note content)
		{
			Type = selectorType.Note;
			content_note = content;
		}

		public SelectorItem(SessionInfo content)
		{
			Type = selectorType.Session;
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

        //Binding data
        public Visibility isItNote
        {
            get
            {
                return Type == selectorType.Note ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility isItSession
        {
            get
            {
                return Type == selectorType.Session ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        bool itSession { get { return Type == selectorType.Session; } }
        bool itNote { get { return Type == selectorType.Note; } }

        //Session data
        public string timeDetail
        {
            get
            {
                if (itNote)
                {
                    return "";
                }
                return content_ses.current.timeProgress;
            }
        }

        public string progressDetail
        {
            get
            {
                if (itNote)
                {
                    return "";
                }
                return $"{content_ses.currentAt}/{content_ses.current.totalWords} (x:{content_ses.current.totalWrong})";
            }
        }

        public int timePG
        {
            get
            {
                if (itNote)
                {
                    return 0;
                }
                return content_ses.current.timeInPercent;
            }
        }

        public int wordPG
        {
            get
            {
                if (itNote)
                {
                    return 0;
                }
                float cache = (content_ses.currentAt / content_ses.current.totalWords) * 100;
                return Convert.ToInt32(cache);
            }
        }

        public Visibility isClickMode
        {
            get
            {
                if (itNote)
                {
                    return Visibility.Collapsed;
                }
                if (content_ses.choiceType != selectMode.styleC)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility isWriteMode
        {
            get
            {
                if (itNote) { return Visibility.Collapsed; }
                if (content_ses.choiceType == selectMode.styleC)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public string ch1 { get { if (itNote) { return ""; } if (content_ses.current.totalChoice >= 1) { return content_ses.choiceList[0]; } return ""; } }
        public string ch2 { get { if (itNote) { return ""; } if (content_ses.current.totalChoice >= 2) { return content_ses.choiceList[1]; } return ""; } }
        public string ch3 { get { if (itNote) { return ""; } if (content_ses.current.totalChoice >= 3) { return content_ses.choiceList[2]; } return ""; } }
        public string ch4 { get { if (itNote) { return ""; } if (content_ses.current.totalChoice >= 4) { return content_ses.choiceList[3]; } return ""; } }
        public string ch5 { get { if (itNote) { return ""; } if (content_ses.current.totalChoice >= 5) { return content_ses.choiceList[4]; } return ""; } }

        public SolidColorBrush ch1Col { get { if (itNote) { return null; } if (content_ses.current.totalChoice >= 1) { return content_ses.lastChoicesHide[1] ? new SolidColorBrush((Color)App.Current.Resources["SystemAccentColor"]) : new SolidColorBrush((Color)App.Current.Resources["BackButtonForegroundThemeBrush"]); } return null; } }
        public SolidColorBrush ch2Col { get { if (itNote) { return null; } if (content_ses.current.totalChoice >= 2) { return content_ses.lastChoicesHide[2] ? new SolidColorBrush((Color)App.Current.Resources["SystemAccentColor"]) : new SolidColorBrush((Color)App.Current.Resources["BackButtonForegroundThemeBrush"]); } return null; } }
        public SolidColorBrush ch3Col { get { if (itNote) { return null; } if (content_ses.current.totalChoice >= 3) { return content_ses.lastChoicesHide[3] ? new SolidColorBrush((Color)App.Current.Resources["SystemAccentColor"]) : new SolidColorBrush((Color)App.Current.Resources["BackButtonForegroundThemeBrush"]); } return null; } }
        public SolidColorBrush ch4Col { get { if (itNote) { return null; } if (content_ses.current.totalChoice >= 4) { return content_ses.lastChoicesHide[4] ? new SolidColorBrush((Color)App.Current.Resources["SystemAccentColor"]) : new SolidColorBrush((Color)App.Current.Resources["BackButtonForegroundThemeBrush"]); } return null; } }
        public SolidColorBrush ch5Col { get { if (itNote) { return null; } if (content_ses.current.totalChoice >= 5) { return content_ses.lastChoicesHide[5] ? new SolidColorBrush((Color)App.Current.Resources["SystemAccentColor"]) : new SolidColorBrush((Color)App.Current.Resources["BackButtonForegroundThemeBrush"]); } return null; } }

        public Visibility ch1Hid { get { if (itNote) { return Visibility.Collapsed; } if (content_ses.current.totalChoice >= 1) { return Visibility.Visible; } return Visibility.Collapsed; } }
        public Visibility ch2Hid { get { if (itNote) { return Visibility.Collapsed; } if (content_ses.current.totalChoice >= 2) { return Visibility.Visible; } return Visibility.Collapsed; } }
        public Visibility ch3Hid { get { if (itNote) { return Visibility.Collapsed; } if (content_ses.current.totalChoice >= 3) { return Visibility.Visible; } return Visibility.Collapsed; } }
        public Visibility ch4Hid { get { if (itNote) { return Visibility.Collapsed; } if (content_ses.current.totalChoice >= 4) { return Visibility.Visible; } return Visibility.Collapsed; } }
        public Visibility ch5Hid { get { if (itNote) { return Visibility.Collapsed; } if (content_ses.current.totalChoice >= 5) { return Visibility.Visible; } return Visibility.Collapsed; } }

        public string writeInput
        {
            get
            {
                if (itNote) { return ""; }
                return content_ses.lastChoices[0];
            }
        }
    }

	public enum selectorType
	{
		Note,
		Session
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
		public double progressFillAmount;
		public List<string> lastChoices = new List<string>();
        public List<bool> lastChoicesHide = new List<bool>();
        public selectMode choiceType;
        //public string displayText;
        public Note nowPlayed;
        public Match.mode lastMode;

		public SessionInfo()
		{
			current = new statInfo();
			currentAt = -1;
			lastCorrect = 1;
			lastTextList = new List<textlist>();
			choiceList = new List<string>();
			progressFillAmount = 0;
			lastChoices = new List<string>();
            //displayText = "";
            choiceType = selectMode.styleA;
            lastChoicesHide = new List<bool>();
            nowPlayed = new Note();
            lastMode = Match.mode.normal;
		}

        public SessionInfo(Match info, statInfo currentStat)
        {
            current = currentStat;
            currentAt = info.currentProgress;
            lastCorrect = info.currentValidChoice;
            lastTextList = info.textList;
            choiceList = info.choiceList;
            progressFillAmount = info.progressCounter.Value;
            //displayText = info.dpTxt.Text;
            nowPlayed = info.nowPlaying;
            lastMode = info.currentChoiceMode;
            //Find current choice style
            if (info.choicesListHolder.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                choiceType = selectMode.styleA;
            }
            else if (info.choicesListHolderB.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                choiceType = selectMode.styleB;
            }
            else if (info.choicesListHolderC.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                choiceType = selectMode.styleC;
            }
            switch (choiceType)
            {
                case selectMode.styleA:
                    lastChoicesHide = new List<bool>();
                    lastChoices = new List<string>();
                    lastChoicesHide.Add(info.chImportant.Visibility == Windows.UI.Xaml.Visibility.Visible);
                    lastChoicesHide.Add(info.ch0.Visibility == Windows.UI.Xaml.Visibility.Visible);
                    lastChoicesHide.Add(info.ch1.Visibility == Windows.UI.Xaml.Visibility.Visible);
                    lastChoicesHide.Add(info.ch2.Visibility == Windows.UI.Xaml.Visibility.Visible);
                    lastChoicesHide.Add(info.ch3.Visibility == Windows.UI.Xaml.Visibility.Visible);
                    lastChoicesHide.Add(info.ch4.Visibility == Windows.UI.Xaml.Visibility.Visible);
                    lastChoices.Add(info.ch0.Content.ToString());
                    lastChoices.Add(info.ch1.Content.ToString());
                    lastChoices.Add(info.ch2.Content.ToString());
                    lastChoices.Add(info.ch3.Content.ToString());
                    lastChoices.Add(info.ch4.Content.ToString());
                    break;
                case selectMode.styleB:
                    lastChoicesHide = new List<bool>();
                    lastChoices = new List<string>();
                    lastChoicesHide.Add(info.chImportantb.Visibility == Windows.UI.Xaml.Visibility.Visible);
                    lastChoicesHide.Add(info.ch0b.Visibility == Windows.UI.Xaml.Visibility.Visible);
                    lastChoicesHide.Add(info.ch1b.Visibility == Windows.UI.Xaml.Visibility.Visible);
                    lastChoicesHide.Add(info.ch2b.Visibility == Windows.UI.Xaml.Visibility.Visible);
                    lastChoicesHide.Add(info.ch3b.Visibility == Windows.UI.Xaml.Visibility.Visible);
                    lastChoicesHide.Add(info.ch4b.Visibility == Windows.UI.Xaml.Visibility.Visible);
                    lastChoices.Add(info.choiceInfob.Text);
                    break;
                case selectMode.styleC:
                    lastChoicesHide = new List<bool>();
                    lastChoices = new List<string>();
                    lastChoicesHide.Add(info.chImportantc.Visibility == Windows.UI.Xaml.Visibility.Visible);
                    lastChoicesHide.Add(info.resultWrite.Visibility == Windows.UI.Xaml.Visibility.Visible);
                    lastChoicesHide.Add(info.resultWriteSubmit.Visibility == Windows.UI.Xaml.Visibility.Visible);
                    lastChoices.Add(info.resultWrite.Text);
                    break;
            }
        }
        
		public static implicit operator SessionInfo(SelectorItem input)
		{
			return input.content_ses;
		} 

		public static async void Save(List<SessionInfo> now)
		{
            StorageFolder folder = ApplicationData.Current.RoamingFolder;
            var file = await folder.TryGetItemAsync("session.json");
            if (file == null)
            {
                //not exist
                file = await folder.CreateFileAsync("session.json");
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<SessionInfo>));
                using (Stream write = new FileStream(file.Path,FileMode.OpenOrCreate))
                {
                    ser.WriteObject(write, now);
                }
            }
            else
            {
                //exist
                await file.DeleteAsync();
                file = await folder.CreateFileAsync("session.json");
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<SessionInfo>));
                using (Stream write = new FileStream(file.Path, FileMode.OpenOrCreate))
                {
                    ser.WriteObject(write, now);
                }
            }
		}

        public static async void Load()
        {
            StorageFolder folder = ApplicationData.Current.RoamingFolder;
            var file = await folder.TryGetItemAsync("session.json");
            if (file == null)
            {
                Utilities.sessions = new List<SessionInfo>();
            }
            else
            {
                List<SessionInfo> allses = new List<SessionInfo>();
                DataContractJsonSerializer info = new DataContractJsonSerializer(typeof(List<SessionInfo>));
                using (Stream stream = new FileStream(file.Path, FileMode.Open))
                {
                    allses = (List<SessionInfo>)info.ReadObject(stream);
                }
                Utilities.sessions = allses;
            }
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
            Directory.CreateDirectory(deployPath);
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