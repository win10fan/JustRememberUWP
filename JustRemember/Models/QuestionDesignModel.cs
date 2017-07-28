using JustRemember.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using static JustRemember.App;

namespace JustRemember.Models
{
	public class QuestionDesignModel : Observable
	{
		public QuestionDesignModel()
		{
			Index = 1;
			Correct = 0;
			Question = "";
			Answers = new ObservableCollection<string>() { "", "", "", "", "" };
			UID = QuestionDesignHelper.getRandom();
		}
		int indx, cor;
		string q;
		ObservableCollection<string> _as;

		public int Index { get => indx; set => Set(ref indx, value); }
		public int Correct { get => cor; set { Set(ref cor, value); } }
		public string Question { get => q; set { Set(ref q, value); } }
		public ObservableCollection<string> Answers { get => _as; set => Set(ref _as, value); }

		[JsonIgnore]
		public Visibility isCanUp
		{
			get
			{
				return Index == 1 ? Visibility.Collapsed : Visibility.Visible;
			}
		}

		public static int maxIndex;

		[JsonIgnore]
		public Visibility isCanDown
		{
			get
			{
				return Index == maxIndex ? Visibility.Collapsed : Visibility.Visible;
			}
		}

		[JsonIgnore]
		public string Answer1
		{
			get => Answers.Count >= 1 ? Answers[0] : "";
			set
			{
				if (Answers.Count >= 1)
					Answers[0] = value;
				else
					nothing = value;
			}
		}

		[JsonIgnore]
		public string Answer2
		{
			get => Answers.Count >= 2 ? Answers[1] : "";
			set
			{
				if (Answers.Count >= 2)
					Answers[1] = value;
				else
					nothing = value;
			}
		}

		[JsonIgnore]
		public string Answer3
		{
			get => Answers.Count >= 3 ? Answers[2] : "";
			set
			{
				if (Answers.Count >= 3)
					Answers[2] = value;
				else
					nothing = value;
			}
		}

		[JsonIgnore]
		public string Answer4
		{
			get => Answers.Count >= 4 ? Answers[3] : "";
			set
			{
				if (Answers.Count >= 4)
					Answers[3] = value;
				else
					nothing = value;
			}
		}

		[JsonIgnore]
		public string Answer5
		{
			get => Answers.Count >= 5 ? Answers[4] : "";
			set
			{
				if (Answers.Count >= 5)
					Answers[4] = value;
				else
					nothing = value;
			}
		}

		[JsonIgnore]
		string nothing;

		[JsonIgnore]
		public bool is1Correct
		{
			get => Correct == 0; set
			{
				if (value) Correct = 0;
				OnPropertyChanged(nameof(is1Correct));
				OnPropertyChanged(nameof(is2Correct));
				OnPropertyChanged(nameof(is3Correct));
				OnPropertyChanged(nameof(is4Correct));
				OnPropertyChanged(nameof(is5Correct));
			}
		}
		[JsonIgnore]
		public bool is2Correct
		{
			get => Correct == 1; set
			{
				if (value) Correct = 1;
				OnPropertyChanged(nameof(is1Correct));
				OnPropertyChanged(nameof(is2Correct));
				OnPropertyChanged(nameof(is3Correct));
				OnPropertyChanged(nameof(is4Correct));
				OnPropertyChanged(nameof(is5Correct));
			}
		}
		[JsonIgnore]
		public bool is3Correct
		{
			get => Correct == 2; set
			{
				if (value) Correct = 2;
				OnPropertyChanged(nameof(is1Correct));
				OnPropertyChanged(nameof(is2Correct));
				OnPropertyChanged(nameof(is3Correct));
				OnPropertyChanged(nameof(is4Correct));
				OnPropertyChanged(nameof(is5Correct));
			}
		}
		[JsonIgnore]
		public bool is4Correct
		{
			get => Correct == 3; set
			{
				if (value) Correct = 3;
				OnPropertyChanged(nameof(is1Correct));
				OnPropertyChanged(nameof(is2Correct));
				OnPropertyChanged(nameof(is3Correct));
				OnPropertyChanged(nameof(is4Correct));
				OnPropertyChanged(nameof(is5Correct));
			}
		}
		[JsonIgnore]
		public bool is5Correct
		{
			get => Correct == 4; set
			{
				if (value) Correct = 4;
				OnPropertyChanged(nameof(is1Correct));
				OnPropertyChanged(nameof(is2Correct));
				OnPropertyChanged(nameof(is3Correct));
				OnPropertyChanged(nameof(is4Correct));
				OnPropertyChanged(nameof(is5Correct));
			}
		}

		[JsonIgnore]
		public string UID { get; private set; }
	}

	public static class QuestionDesignHelper
	{
		public static HashSet<string> ids { get; set; }
		const string atoz = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
		static Random rand = null;

		public static string getRandom()
		{
			return getRandom(false);
		}

		public static string getRandom(bool fromOther)
		{
			if (rand == null)
			{
				rand = new Random();
			}
			int lenght = rand.Next(30, 80);
			string word = "";
			while (lenght > 0)
			{
				word += atoz[rand.Next(0, atoz.Length - 1)];
				lenght -= 1;
			}
			if (ids == null)
				ids = new HashSet<string>();
			if (!fromOther)
			{
				if (ids.Contains(word))
				{
					getRandom();
				}
				ids.Add(word);
			}
			return word;
		}

		const string answerpos = "AnswerPosition";
		const string separator = "Separate";
		const string usesp8spr = "SpaceAfterSeparate";
		const string customhdr = "CustomChoiceHeader";
		//public const string ansSymbol = "ABCDEFG";

		public static ObservableCollection<QuestionDesignModel> FromString(string content)
		{
			return FromString(content, false);
		}

		public static ObservableCollection<QuestionDesignModel> FromString(string content,bool forPlay)
		{
			var items = new ObservableCollection<QuestionDesignModel>();
			List<string> lines = content.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
			answerPosition answerPos = answerPosition.Bottom;
			QuestionSeparator Separate = QuestionSeparator.Dot;
			SpaceAfterSeparator UseSpace = SpaceAfterSeparator.Yes;
			string customChoiceHeader = "ABCDE";
			string[] config = lines[0].Split(' ');
			//Read config
			foreach (string c in config)
			{
				if (c.StartsWith(answerpos))
					answerPos = ParseEnum<answerPosition>(c.Remove(0, c.IndexOf('=') + 1));
				else if (c.StartsWith(separator))
					Separate = ParseEnum<QuestionSeparator>(c.Remove(0, c.IndexOf('=') + 1));
				else if (c.StartsWith(usesp8spr))
					UseSpace = ParseEnum<SpaceAfterSeparator>(c.Remove(0, c.IndexOf('=') + 1));
				else if (c.StartsWith(customhdr))
					customChoiceHeader = c.Substring(c.IndexOf('=') + 1);
			}
			Debug.Write(customChoiceHeader);
			lines.RemoveAt(0);
			//Get answers if it was at bottom
			string answersLine = "";
			if (answerPos == answerPosition.Bottom)
			{
				answersLine = lines.Last();
				lines.RemoveAt(lines.Count - 1);
			}
			//Read q&a
			int index = 0;
			int tilNextPredict = -1;
			bool skippls = false;
			QuestionDesignModel item = new QuestionDesignModel();
			char check = Separate == QuestionSeparator.Dot ? '.' : ')';
			foreach (var line in lines)
			{
				if (skippls)
				{
					if (index != tilNextPredict)
					{
						index += 1;
						continue;
					}
					if (index == tilNextPredict)
					{
						skippls = false;
					}
				}
				bool isquestion = char.IsNumber(line[0]);
				if (isquestion)
				{
					item.Index = int.Parse(line.Substring(0, line.IndexOf(check)));
					item.Question = line.Substring(line.IndexOf(check) + 1).Trim();
					if (answerPos == answerPosition.BehindAnswer)
					{
						item.Correct = customChoiceHeader.IndexOf(item.Question[item.Question.Length - 1]);
						item.Question = item.Question.Substring(0, item.Question.LastIndexOf('=') - 1);
					}
					else
					{
						item.Correct = customChoiceHeader.IndexOf(answersLine[item.Index]);
					}
				}
				else
				{
					int predict = index;
					bool keeploop = !char.IsNumber(lines[predict][0]);
					while (keeploop)
					{
						predict += 1;
						if (predict == lines.Count)
						{
							predict = lines.Count + 1;
							keeploop = false;
							break;
						}
						else
						{
							keeploop = !char.IsNumber(lines[predict][0]);
						}
					}
					tilNextPredict = predict;
					skippls = true;
					List<string> answers = new List<string>();
					for (int i = index; i < predict; i++)
					{
						//Answers
						if (i == lines.Count)
						{
							continue;
						}
						answers.Add(lines[i].Substring(line.IndexOf(check) + 1).Trim());
					}
					if (!forPlay)
					{
						while (answers.Count != 5) { answers.Add(""); }
					}
					item.Answers.Clear();
					answers.ForEach((string s) => item.Answers.Add(s));
					items.Add(item);
					item = new QuestionDesignModel();
				}
				index++;
			}
			if (forPlay) { items.Add(new QuestionDesignModel() { Answers = new ObservableCollection<string>() { "0", "1", "2", "3", "4" }, Question = "1 + 1 = ?", Correct = 2 }); }
			return items;
		}

		public static string ToString(IList<QuestionDesignModel> items)
		{
			return ToString(items, Config.AnswerPosition, Config.questionSeparator, Config.spaceAfterSeparator,Config.customChoiceHeader);
		}

		public static string ToString(IList<QuestionDesignModel> items, answerPosition answerPos, QuestionSeparator Separate, SpaceAfterSeparator UseSpace,string customHeader)
		{
			//Config
			string output = $"MODE=EXAM " +
				$"{answerpos}={answerPos.ToString()} " +
				$"{separator}={Separate.ToString()} " +
				$"{usesp8spr}={UseSpace.ToString()} " +
				$"{customhdr}={customHeader}\r\n";
			//Questions and Answers
			string separ8 = Separate == QuestionSeparator.Dot ? "." : ")";
			string space = UseSpace == SpaceAfterSeparator.Yes ? " " : "";
			string answerLine = ">";
			int emptyQuestion = 0;
			foreach (var item in items)
			{
				if (string.IsNullOrEmpty(item.Question))
				{
					emptyQuestion += 1;
					continue;
				}
				string index = $"{item.Index - emptyQuestion}";
				string after = answerPos == answerPosition.BehindAnswer ? $" A={customHeader[item.Correct]}" : "";
				answerLine += answerPos == answerPosition.Bottom ? $"{customHeader[item.Correct]}" : "";
				output += $"{index}{separ8}{space}{item.Question}{after}\r\n";
				int i = 1;
				foreach (var an in item.Answers)
				{
					if (string.IsNullOrEmpty(an))
					{
						continue;
					}
					string aindex = $"{Convert.ToChar(64 + i)}";
					output += $"{aindex}{separ8}{space}{an}\r\n";
					i += 1;
				}
			}
			if (answerPos == answerPosition.Bottom)
			{
				output += $"\r\n{answerLine}";
			}
			return output;
		}

		public static T ParseEnum<T>(string value)
		{
			return (T)Enum.Parse(typeof(T), value, true);
		}
	}
}