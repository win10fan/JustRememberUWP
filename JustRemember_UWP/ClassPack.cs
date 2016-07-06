using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml.Serialization;
using Windows.UI;
using Windows.Storage;
using System;
using System.Threading.Tasks;

namespace JustRemember_UWP
{
	public static class Utilities
	{
		public static T ParseEnum<T>(string value)
		{
			return (T)System.Enum.Parse(typeof(T), value, true);
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

		public static string ReplaceForMatch(this string content)
		{
			string result = content;
			result = result.Trim();

			result = result.Replace(" ", "█ ");
			result = Regex.Replace(result, "\n", "▼ ");
			result = Regex.Replace(result, "\t", "→ ");
			return result;
		}

		public static string ToStringAsTime(this float total)
		{
			System.TimeSpan ttlspan = System.TimeSpan.FromSeconds(total);
			return string.Format("{0}:{1}", ttlspan.Minutes, ttlspan.Seconds);
		}

		public static string ObscureText(this string text)
		{
			return ObscureText(text, false);
		}

		public static string ObscureText(this string text, bool addColorTag)
		{
			//TODO:Make it support richtext
			string res = "";
			for (int i = 0; i < text.Length; i++)
			{
				res += "?";
			}
			if (addColorTag)
			{
				return string.Format("<color=#{1}>{0}</color>", res, systemAccent.ToString());
			}
			return res;
		}

		public static string Translate(this cmd command)
		{
			switch (command)
			{
				case cmd.space:
					return " ";
				case cmd.tab:
					return "\t";
				case cmd.newline:
					return "\n";
				default:
					return " ";
			}
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

		public static Color systemAccent;
		public static Settings currentSettings;
		public static string savedPath;
		public static bool initialize;
	}
	
	public class statInfo
	{
		public string dateandTime;
		public int totalWords;
		public int totalWrong;
		public int totalChoice;
		public List<int> wrongPerchoice = new List<int>();
		public bool useTimeLimit;
		public float totalTime;
		public float totalLimitTime;
		public challageMode currentMode;
		public string noteTitle;
	}
	
	public struct textlist
	{
		public string Text;
		public cmd Command;
		public textlist(string text)
		{
			Text = text;
			Command = cmd.space;
		}

		public textlist(string text, cmd command)
		{
			Text = text;
			Command = command;
		}

		public override string ToString()
		{
			return Text;
		}
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

		public static async Task<Note> Load(string filename)
		{
			Note info = new Note();
			info.Title = filename;
			StorageFolder folder = KnownFolders.DocumentsLibrary;
			StorageFile file = await folder.GetFileAsync(filename);
			if (File.Exists(file.Path))
			{
				info.Content = await FileIO.ReadTextAsync(file);
				return info;
			}
			else
			{
				return new Note();
			}
		}
	}

	public class SessionInfo
	{
		public string LastTitle;
		public statInfo current;
		public int currentAt;
		public int lastCorrect;
		public List<textlist> lastTextList = new List<textlist>();
		public List<string> choiceList = new List<string>();
		public float progressFillAmount;
		public List<string> lastChoices = new List<string>();
		public string lastTimeDisplay;

		public SessionInfo()
		{
			current = new statInfo();
			currentAt = -1;
			lastCorrect = 1;
			lastTextList = new List<textlist>();
			choiceList = new List<string>();
			LastTitle = "";
			progressFillAmount = 0;
			lastChoices = new List<string>();
			lastTimeDisplay = "";
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

		public static void Save(List<SessionInfo> now, string path)
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			XmlSerializer xser = new XmlSerializer(typeof(List<SessionInfo>));
			//string content = JsonUtility.ToJson(now);
			using (StreamWriter write = new StreamWriter(File.Create(path)))
			{
				//write.Write(xser.ser);
				xser.Serialize(write, now);
			}
		}

		public static List<SessionInfo> Load(string path)
		{
			if (!File.Exists(path))
			{
				return new List<SessionInfo>();
			}
			XmlSerializer xs = new XmlSerializer(typeof(List<SessionInfo>));
			//List<SessionInfo> allses = new List<SessionInfo>();
			//SessionInfo sif = new SessionInfo();
			using (var fs = File.Open(path, FileMode.Open))
			{
				return (List<SessionInfo>)xs.Deserialize(fs);
				//StreamReader read = new StreamReader(fs);
				//sif = JsonUtility.FromJson<SessionInfo>(read.ReadToEnd());
				//allses.Add(sif);
			}
		}
	}

	public enum themeMode
	{
		Accent,
		Legacy
	}

	public enum themeBackground
	{
		Light,
		Dark
	}

	public enum cmd
	{
		space,
		tab,
		newline
	}

	public enum Operator
	{
		New,
		Open,
		Save,
		SaveNew,
		Quit
	}

	public enum ThemeItem
	{
		Button,
		ButtonLabel,
		Titlebar,
		TitlebarLabel,
		Background,
		Foreground,
		Accent,
		Undef
	}

	public enum challageMode
	{
		Easy,
		Normal,
		Hard
	}

	public enum Rotation
	{
		Portrait,
		LandscapeLeft,
		LandscapeRight,
		AutoRotation
	}

	public enum Action
	{
		Quit,
		ResetStat,
		ResetMatch,
		BackToMain,
		BackToPervious,
		Delete
	}

	public enum PageName
	{
		main = 0,
		pause = 1,
		file = 2,
		set = 3,
		end = 4,
		over = 5,
		confirm = 6,
		edit = 7,
		home = 8,
		session = 9
	}
}