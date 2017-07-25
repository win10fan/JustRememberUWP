using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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
			try
			{
				return gen(item);
			}
			catch (ArgumentOutOfRangeException ex1)
			{
				Debug.Write(ex1.Message);
			}
			return null;
		}

		public static SessionModel generate(SessionModel item)
		{
			return gen(item);
		}

		public static SessionModel generate(NoteModel item, SessionModel old)
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
				fromNote = (NoteModel)item;
				if (fromNote.Mode == noteMode.Question)
					return genEx(fromNote);
				else if (fromNote.Mode == noteMode.Volcabulary)
					return genVo(fromNote);
				current = new SessionModel()
				{
					selectedChoices = new ObservableCollection<SelectedChoices>(),
					isNew = true,
					maxChoice = Config.totalChoice,
					currentChoice = 0
				};
				//Initialize Session
				current.SelectedNote = fromNote;
				current.texts = TextList.Extract(current.SelectedNote.Content, out bool? nW);
				current.noteWhiteSpaceMode = nW == true;
				current.choices = generateChoice(current.texts, current.maxChoice);
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
				fromNote = (NoteModel)((List<object>)item)[1];
				if (fromNote.Mode == noteMode.Question)
					return genEx(fromNote);
				else if (fromNote.Mode == noteMode.Volcabulary)
					return genVo(fromNote);
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
			if (App.Config.useSeed)
			{
				validChoice = new Random(App.Config.defaultSeed);
				choiceTexts = new Random(App.Config.defaultSeed);
			}
			for (int i = 0; i < texts.Count; i++)
			{
				ChoiceSet c = new ChoiceSet()
				{
					choices = new List<string>()
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
			if (App.Config.hintAtFirstchoice && App.Config.choiceStyle != choiceDisplayMode.Write)
			{
				choices[0].choices[choices[0].corrected] = $">>{choices[0].choices[choices[0].corrected]}<<";
			}
			return choices;
		}

		private static SessionModel genEx(NoteModel item)
		{
			SessionModel current = new SessionModel();
			current = new SessionModel()
			{
				selectedChoices = new ObservableCollection<SelectedChoices>(),
				isNew = true,
				maxChoice = 5,
				currentChoice = 0,
				noteWhiteSpaceMode = false,
				SelectedNote = item
			};
			//Get new one
			//Initialize Session
			var qas = QuestionDesignHelper.FromString(item.Content, true);
			int re = 1;
			switch (App.Config.randomizeQA)
			{
				case randomQA.OnlyQuestion:
					qas.Shuffle();
					foreach (var q in qas)
					{
						q.Index = re;
						re += 1;
					}
					break;
				case randomQA.All:
					qas.Shuffle();
					foreach (var q in qas)
					{
						q.Index = re;
						re += 1;
						q.Answers[q.Correct] += "";
						q.Answers.Shuffle();
						for (int i = 0; i < q.Answers.Count; i++)
						{
							if (q.Answers[i].EndsWith(""))
							{
								q.Correct = i;
								q.Answers[i] = q.Answers[i].Replace("", "");
							}
						}
					}
					break;
			}
			//Update texts
			foreach (var q in qas)
			{
				q.Question = $"{q.Index}. {q.Question}";
			}
			current.texts = new ObservableCollection<TextList>();
			current.choices = new ObservableCollection<ChoiceSet>
			{
				new ChoiceSet() { choices = new List<string>() { ">>>" }, corrected = 0 }
			};
			for (int i = 0; i < qas.Count; i++)
			{
				if (current.texts.Count == 0)
				{
					//Add first texts
					current.texts.Add(new TextList() { isWhitespace = false, text = $"{qas[i].Question}\r\n" });
					current.choices.Add(new ChoiceSet() { choices = qas[i].Answers.ToList(), corrected = qas[i].Correct });
					continue;
				}
				current.texts.Add(new TextList() { isWhitespace = false, text = $"{qas[i - 1].Answers[qas[i - 1].Correct]}\r\n\r\n{qas[i].Question}\r\n" });
				current.choices.Add(new ChoiceSet() { choices = qas[i].Answers.ToList(), corrected = qas[i].Correct });
			}
			current.StatInfo = refreshStat(current);
			return current;
		}

		private static SessionModel genVo(NoteModel item)
		{
			SessionModel current = new SessionModel()
			{
				selectedChoices = new ObservableCollection<SelectedChoices>(),
				isNew = true,
				maxChoice = 5,
				currentChoice = 0,
				noteWhiteSpaceMode = false,
				SelectedNote = item
			};
			Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
			List<string> keys = new List<string>();
			List<string> lines = item.Content.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
			foreach (var line in lines)
			{
				if (line.StartsWith("#MODE=VOLC")) continue;
				var words = line.Split(' ').ToList();
				string mean = words[0];
				keys.Add(mean);
				words.RemoveAt(0);
				dict.Add(mean, new List<string>());
				foreach (var word in words)
				{
					dict[mean].Add(word);
				}
			}
			if (App.Config.randomizeQA == randomQA.OnlyQuestion)
			{
				dict.Shuffle();
			}
			else if (App.Config.randomizeQA == randomQA.All)
			{
				dict.Shuffle();
				foreach (var itm in dict)
				{
					itm.Value.Shuffle();
				}
			}
			current.texts = new ObservableCollection<TextList>();
			current.choices = new ObservableCollection<ChoiceSet>()
			{
				new ChoiceSet() { choices = new List<string>() { ">>>" }, corrected = 0 }
			};
			for (int i = 0; i < dict.Count; i++)
			{
				string now = keys[i];
				if (i == dict.Count - 1)
				{
					for (int i2 = 0; i2 < dict[now].Count; i2++)
					{
						current.texts.Add(new TextList() { text = $"{dict[now][i2]} " });
					}
					continue;
				}
				string next = keys[i + 1];
				if (i == 0)
					current.texts.Add(new TextList() { text = $"{now}\r\n" });
				for (int i2 = 0; i2 < dict[now].Count; i2++)
				{
					if (i2 == dict[now].Count - 1)
					{
						current.texts.Add(new TextList() { text = $"{dict[now][i2]}\r\n\r\n{next}\r\n" });
						continue;
					}
					current.texts.Add(new TextList() { text = $"{dict[now][i2]} " });
				}
			}
			//Generate choices
			Random rnd = null;
			Random chs = null;
			if (App.Config.useSeed)
			{
				rnd = new Random(App.Config.defaultSeed);
				chs = new Random(App.Config.defaultSeed);
			}
			else
			{
				rnd = new Random();
				chs = new Random();
			}
			for (int i = 0; i < dict.Count; i++)
			{
				for (int nd = 0; nd < dict[keys[i]].Count; nd++)
				{
					ChoiceSet cs = new ChoiceSet()
					{
						corrected = rnd.Next(0, App.Config.totalChoice)
					};
					for (int sub = 0; sub < App.Config.totalChoice; sub++)
					{
						if (sub == cs.corrected)
						{
							cs.choices.Add(dict[keys[i]][nd]);
						}
						int randKey = chs.Next(0, keys.Count);
						while (randKey == i)
						{
							randKey = chs.Next(0, keys.Count);
						}
						int randIndx = chs.Next(0, dict[keys[randKey]].Count);
						cs.choices.Add(dict[keys[randKey]][randIndx]);
					}
					current.choices.Add(cs);
				}
			}

			//Stat
			current.StatInfo = refreshStat(current);
			return current;
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
					return new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"]);
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
		private static Random rng = new Random();

		public static string obfuscateText(this string text)
		{
			string res = "";
			string obc = App.language.GetString("Session_obfuscateChar");
			for (int i = 0; i < text.Length; i++)
			{
				res += obc;
			}
			return res;
		}

		public static int Clamp(this int value, int min, int max)
		{
			if (value < min) { return min; }
			if (value > max) { return max; }
			return value;
		}

		public static void Shuffle<TKey, TValue>(this Dictionary<TKey, TValue> source)
		{
			source.OrderBy(x => rng.Next()).ToDictionary(item => item.Key, item => item.Value);
		}

		public static void Shuffle<T>(this IList<T> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}
	}
}