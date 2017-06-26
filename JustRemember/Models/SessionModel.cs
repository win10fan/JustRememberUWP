using JustRemember.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace JustRemember.Models
{
	public class SessionModel
	{
		public bool noteWhiteSpaceMode { get; set; } //True = Begin with white space on every items | false = Begin with text on all items
		public int currentChoice { get; set; }
		public int maxChoice { get; set; }
		public ObservableCollection<SelectedChoices> selectedChoices { get; set; } = new ObservableCollection<SelectedChoices>();
		public bool isNew { get; set; }

		//save separately
		[JsonIgnore]
		public StatModel StatInfo { get; set; }
		[JsonIgnore]
		public ObservableCollection<ChoiceSet> choices { get; set; } = new ObservableCollection<ChoiceSet>();
		[JsonIgnore]
		public NoteModel SelectedNote { get; set; }

		//Can re-created later
		[JsonIgnore]
		public ObservableCollection<TextList> texts { get; set; } = new ObservableCollection<TextList>();

		//Getter only
		[JsonIgnore]
		public string GeneratedName
		{
			get
			{
				var time = StatInfo.beginTime;
				return $"{time.Year}{time.Month}{time.Day}-{time.Hour}{time.Minute}{time.Second}.info";
			}
		}

		[JsonIgnore]
		public int totalChoices
		{
			get
			{
				return texts.Count;
			}
		}

		[JsonIgnore]
		public int choiceProgress
		{
			get
			{
				float a = (float)currentChoice / (float)totalChoices;
				return (int)(a * 100);
			}
		}

		public SessionModel()
		{
			SelectedNote = new NoteModel();
			noteWhiteSpaceMode = false;
			StatInfo = new StatModel();
			currentChoice = 0;
			texts = new ObservableCollection<TextList>();
			choices = new ObservableCollection<ChoiceSet>();
			maxChoice = 3;
			selectedChoices = new ObservableCollection<SelectedChoices>();
		}

		//private static 
		public static SessionModel generate(NoteModel item)
		{
			return gen(item);
		}

		public static SessionModel generate(SessionModel item)
		{
			return gen(item);
		}

		public static SessionModel generate(NoteModel item,SessionModel old)
		{
			//Wanted to restart
			List<object> trade = new List<object>
			{
				old,
				item
			};
			return gen(trade);
		}

		private static SessionModel gen(object item)
		{
			AppConfigModel Config = App.Config;
			NoteModel fromNote = new NoteModel();
			SessionModel current = new SessionModel();
			if (item is NoteModel)
			{
				current = new SessionModel()
				{
					selectedChoices = new ObservableCollection<SelectedChoices>(),
					isNew = true,
					maxChoice = Config.totalChoice,
					currentChoice = 0
				};
				//Get new one
				fromNote = (NoteModel)item;
				//Initialize Session
				current.SelectedNote = fromNote;
				current.texts = TextList.Extract(current.SelectedNote.Content, out bool? nW);
				current.noteWhiteSpaceMode = nW == true;				
				current.choices = generateChoice(current.texts,current.maxChoice);
				current.StatInfo = refreshStat(current);
			}
			else if (item is SessionModel)
			{
				//Restore session
				current = (SessionModel)item;
				current.isNew = false;
			}
			else if (item is List<object>)
			{
				current = (SessionModel)((List<object>)item)[0];
				current.choices.Clear();
				current.choices = generateChoice(current.texts, current.maxChoice);
				current.StatInfo = refreshStat(current);
				current.isNew = true;
				current.currentChoice = 0;
				current.selectedChoices.Clear();
			}
			return current;
		}

		private static ObservableCollection<ChoiceSet> generateChoice(ObservableCollection<TextList> texts, int maxChoice)
		{
			HashSet<string> hashed = new HashSet<string>();
			ObservableCollection<string> chooseAble = new ObservableCollection<string>();
			foreach (TextList t in texts)
			{
				hashed.Add(t.actualText);
			}
			chooseAble = new ObservableCollection<string>(hashed);
			var choices = new ObservableCollection<ChoiceSet>();
			//--Generate choice
			Random validChoice = new Random();
			Random choiceTexts = new Random();
			if (App.Config.defaultSeed != -1)
			{
				validChoice = new Random(App.Config.defaultSeed);
				choiceTexts = new Random(App.Config.defaultSeed);
			}
			for (int i = 0; i < texts.Count; i++)
			{
				ChoiceSet c = new ChoiceSet()
				{
					choices = new List<string>(),

				};
				//Get valid number first
				int valid = validChoice.Next(0, (maxChoice) * 100);
				valid = valid / 100;
				c.corrected = valid.Clamp(0, maxChoice - 1);
				//Generate choice text depend on difficult
				for (int cnt = 0; cnt < maxChoice; cnt++)
				{
					if (cnt == c.corrected)
					{
						c.choices.Add(texts[i].actualText);
					}
					c.choices.Add("");
				}
				List<string> cache;
				switch (App.Config.defaultMode)
				{
					case matchMode.Easy:
						//Generate random text in a whole array
						cache = new List<string>(chooseAble);
						cache.Remove(texts[i].actualText);
						for (int num = maxChoice; num > -1; num--)
						{
							if (num == c.corrected)
							{
								continue;
							}
							int range = choiceTexts.Next(0, cache.Count - 1);
							c.choices[num] = cache[range];
							cache.RemoveAt(range);
						}
						break;
					case matchMode.Normal:
						//Generate chocie that near current choice within range of 1/4
						for (int num = maxChoice; num > -1; num--)
						{
							if (num == c.corrected)
							{
								continue;
							}
							if ((chooseAble.Count - 1) < 30)
							{
								int range = choiceTexts.Next(0, chooseAble.Count - 1);
								c.choices[num] = chooseAble[range];
							}
							else
							{
								int half = (chooseAble.Count - 1) / 2;
								int quater = half / 2;
								int quaterPastHalf = half + quater;
								if (i <= quater)
								{
									int range = choiceTexts.Next(0, half);
									cache = new List<string>(chooseAble);
									cache.Remove(texts[i].actualText);
									c.choices[num] = cache[range];
								}
								else if (i > quater && i < quaterPastHalf)
								{
									int range = choiceTexts.Next(quater, quaterPastHalf);
									cache = new List<string>(chooseAble);
									cache.Remove(texts[i].actualText);
									c.choices[num] = cache[range];
								}
								else if (i >= quaterPastHalf)
								{
									int range = choiceTexts.Next(half, chooseAble.Count - 1);
									cache = new List<string>(chooseAble);
									cache.Remove(texts[i].actualText);
									c.choices[num] = cache[range];
								}
							}
						}
						break;
					case matchMode.Hard:
						//Generate in range near current with in range 10
						for (int num = maxChoice; num > -1; num--)
						{
							if (num == c.corrected)
							{
								continue;
							}
							int min = i - 10;
							int max = i + 10;
							int range = choiceTexts.Next(
								min.Clamp(0, chooseAble.Count - 1),
								max.Clamp(0, chooseAble.Count - 1));
							cache = new List<string>(chooseAble);
							cache.Remove(texts[i].actualText);
							c.choices[num] = cache[range];
						}
						break;
				}
				//Put choice in choice list
				choices.Add(c);
			}
			if (App.Config.hintAtFirstchoice)
			{
				choices[0].choices[choices[0].corrected] = $">>{choices[0].choices[choices[0].corrected]}<<";
			}
			return choices;
		}

		private static StatModel refreshStat(SessionModel current)
		{
			var Config = App.Config;
			var StatInfo = new StatModel()
			{
				beginTime = DateTime.Now,
				NoteWordCount = current.texts.Count - 1,
				configChoice = Config.totalChoice,
				choiceInfo = new Dictionary<int, List<bool>>(),
				setMode = Config.defaultMode,
				noteTitle = current.SelectedNote.Title,
				totalTimespend = TimeSpan.FromMilliseconds(0),
				isTimeLimited = Config.isLimitTime,
				totalLimitTime = Config.isLimitTime ? TimeSpan.FromSeconds(Config.limitTime) : TimeSpan.MinValue
			};
			StatInfo.correctedChoice = new List<int>();
			foreach (var item in current.choices)
			{
				current.StatInfo.correctedChoice.Add(item.corrected);
			}
			for (int i = 0; i < current.texts.Count; i++)
			{
				List<bool> wrongIfo = new List<bool>();
				for (int c = 0; c != current.maxChoice; c++)
				{
					wrongIfo.Add(false);
				}
				StatInfo.choiceInfo.Add(i, wrongIfo);
			}
			return StatInfo;
		}
	}

	/// <summary>
	/// Contain snip of text from note content | whice later use to generate choice
	/// </summary>
	public class TextList
	{
		public string actualText { get; set; }//This one has no white space
		public string text { get; set; }
		public string whitespace { get; set; }
		public bool isWhitespace { get; set; }

		public TextList()
		{
			actualText = "";
			text = "";
			whitespace = "";
			isWhitespace = false;
		}

		public static ObservableCollection<PreDeterminiteText> ExtractContent(string content)
		{
			ObservableCollection<PreDeterminiteText> item = new ObservableCollection<PreDeterminiteText>();
			if (content.Length < 1) { return item; }
			foreach (char c in content)
			{
				PreDeterminiteText pd = new PreDeterminiteText();
				if (char.IsWhiteSpace(c))
				{
					pd.piece = c.ToString();
					pd.isWhiteSpace = true;
					item.Add(pd);
				}
				else
				{
					pd.piece = c.ToString();
					pd.isWhiteSpace = false;
					item.Add(pd);
				}
			}
			return item;
		}

		public static ObservableCollection<TextList> Extract(string content)
		{
			var itm = Extract(content, out bool? oldAnyway);
			return itm;
		}

		public static ObservableCollection<TextList> Extract(string content, out bool? mode)
		{
			ObservableCollection<PreDeterminiteText> basicSort = ExtractContent(content);
			PreDeterminiteText prev = null;
			mode = null; //True = Begin with white space on every items | false = Begin with text on all items
			int lastID = 0;
			foreach (var pd in basicSort)
			{
				if (mode == null)
				{
					mode = pd.isWhiteSpace;
				}
				if (prev == null)
				{
					//First item
					pd.groupID = lastID;
					prev = pd;
					continue;
				}
				//Every items
				if (mode == true)
				{
					if (prev.isWhiteSpace && pd.isWhiteSpace)
					{
						pd.groupID = lastID;
						prev = pd;
						continue;
					}
					if (prev.isWhiteSpace && !pd.isWhiteSpace)
					{
						pd.groupID = lastID;
						prev = pd;
						continue;
					}
					if (!prev.isWhiteSpace && !pd.isWhiteSpace)
					{
						pd.groupID = lastID;
						prev = pd;
						continue;
					}
					if (!prev.isWhiteSpace && pd.isWhiteSpace)
					{
						lastID += 1;
						pd.groupID = lastID;
						prev = pd;
						continue;
					}
				}
				if (mode == false)
				{
					if (!prev.isWhiteSpace && !pd.isWhiteSpace)
					{
						pd.groupID = lastID;
						prev = pd;
						continue;
					}
					if (!prev.isWhiteSpace && pd.isWhiteSpace)
					{
						//Get some white space after
						pd.groupID = lastID;
						prev = pd;
						continue;
					}
					if (prev.isWhiteSpace && pd.isWhiteSpace)
					{
						//Still white space | keep adding
						pd.groupID = lastID;
						prev = pd;
						continue;
					}
					if (prev.isWhiteSpace && !pd.isWhiteSpace)
					{
						//Start new character | change group ID
						lastID += 1;
						pd.groupID = lastID;
						prev = pd;
						continue;
					}
				}
			}
			ObservableCollection<TextList> list = new ObservableCollection<TextList>();
			for (int i = 0; i < lastID + 1; i++) { list.Add(new TextList()); }
			foreach (var pd in basicSort)
			{
				list[pd.groupID].text += pd.piece;
				if (!pd.isWhiteSpace)
				{
					list[pd.groupID].actualText += pd.piece;
				}
			}
			return list;
		}
	}

	public class PreDeterminiteText
	{
		public string piece;
		public bool isWhiteSpace;
		public int groupID;

		public PreDeterminiteText()
		{
			piece = "";
			isWhiteSpace = false;
			groupID = 0;
		}
	}

	/// <summary>
	/// Choice that generate on this session
	/// </summary>
	public class ChoiceSet
	{
		public List<string> choices;
		public int corrected;

		public ChoiceSet()
		{
			choices = new List<string>();
			corrected = 0;
		}
	}

	public class SelectedChoices
	{
		public bool isItWrong { get; set; }
		public string finalText { get; set; } //This already have white space combine

		public SelectedChoices()
		{
			isItWrong = false;
			finalText = "";
		}

		[JsonIgnore]
		public SolidColorBrush mark
		{
			get
			{
				if (isItWrong)
				{
					return new SolidColorBrush(Colors.Red);
				}
				if (Application.Current.RequestedTheme == ApplicationTheme.Light)
				{
					return new SolidColorBrush(Colors.Black);
				}
				return new SolidColorBrush(Colors.White);
			}
		}
	}

	public static class ExtendFunc
	{
		public static string obfuscateText(this string text)
		{
			string res = "";
			for (int i = 0; i < text.Length; i++)
			{
				res += "?";
			}
			return res;
		}

		public static int Clamp(this int value, int min, int max)
		{
			if (value < min) { return min; }
			if (value > max) { return max; }
			return value;
		}
	}
}