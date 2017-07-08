using JustRemember.Helpers;
using JustRemember.Models;
using JustRemember.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace JustRemember.Views
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class ExamEdit : Page, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
		{
			if (Equals(storage, value))
			{
				return;
			}

			storage = value;
			OnPropertyChanged(propertyName);
		}

		void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		ExamsEdit _item = new ExamsEdit();
		public ExamsEdit items { get => _item; set => Set(ref _item, value); }

		//ObservableCollection<ExamEditItem> _item;
		//public ObservableCollection<ExamEditItem> items { get => _item; set => Set(ref _item, value); }
		bool _am;
		public bool answerStoreMode { get => _am; set => Set(ref _am, value); }
		
		public ExamEdit()
		{
			placeHolder = new ExamEditItem();
			this.InitializeComponent();
#if DEBUG
			ExamEditItem.InitializeRandomness();
#endif
			InitializeDispatch();		
		}
		public ExamEditItem placeHolder;
		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			if (e.Parameter != null)
			{
				var note = (NoteModel)e.Parameter;
				if (note.Content.StartsWith("MODE=EXAM"))
					items.AddRange(ExamEditItem.FromString(note.Content));
				/*List<string> lines = note.Content.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
				string answers = null;
				if (lines.Last().StartsWith("A="))
				{
					answers = lines.Last().Remove(0, 1);
					lines.RemoveAt(lines.Count - 1);
				}
				lines.RemoveAt(0);
				ExamEditItem item = new ExamEditItem();
				item.Answer.Clear();
				bool qora = false;//true=q false=a
				for (int i = 0; i < lines.Count - 1; i++)
				{
					qora = char.IsNumber(lines[i][0]);
					if (qora)//question
					{
						item.Index = int.Parse(lines[i].Substring(0, lines[i].IndexOf('.')));
						if (string.IsNullOrEmpty(answers))
						{
							item.Corrected = ansSymbol.IndexOf(lines[i][lines[i].LastIndexOf('=') + 1], 0);
							item.Question = lines[i].Remove(0, lines[i].IndexOf('.') + 1).Trim();
							item.Question = item.Question.Remove(item.Question.LastIndexOf('=') - 1, 3);
						}
						else
						{
							item.Question = lines[i].Remove(0, lines[i].IndexOf('.') + 1).Trim();
							item.Corrected = ansSymbol.IndexOf(answers[item.Index].ToString().ToUpper());
						}
						continue;
					}
					else //answer
					{
						item.Answer.Add(lines[i].Remove(0, lines[i].IndexOf('.') + 1).Trim());
						if ((i + 1) <= lines.Count - 1)
						{
							if (char.IsNumber(lines[i + 1][0]))
							{
								//It will be question next finish current examitem
								items.Add(item);
								item = new ExamEditItem();
								item.Answer.Clear();
							}
						}
					}
				}
				item.Answer.Add(lines.Last().Remove(0, lines.Last().IndexOf('.') + 1).Trim());
				items.Add(item);
				item = new ExamEditItem();
				item.Answer.Clear();*/
			}
			await MobileTitlebarService.Refresh("Question desinger");
			base.OnNavigatedTo(e);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			placeHolder.Dispose();
			base.OnNavigatedFrom(e);
		}

		private CoreDispatcher _dispatcher;
		public void InitializeDispatch()
		{
			#region Error Checking & Dispatcher Setup
			// check that we haven't already been initialized
			if (_dispatcher != null)
			{
				//throw new ExtensionManagerException("Extension Manager for " + this.Contract + " is already initialized.");
			}

			// thread that initializes the extension manager has the dispatcher
			_dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;
			#endregion
		}
		
		private void NewQuestion(object sender, RoutedEventArgs e)
		{
			items.Add(new ExamEditItem());
			ReorderQuestions();
		}

		private void DeleteQuestion(object sender, RoutedEventArgs e)
		{
			if (items.Count == 1)
			{
				items[0].Dispose();
				items.Clear();
				return;
			}
			int that = (int)(sender as Button).Tag;
			that -= 1;
			try
			{
				items[that].Dispose();
				items.RemoveAt(that);
			}
			catch (Exception ex)
			{
				Debug.Write(ex.Message);
				that -= 1;
			}
			//Reorder
			ReorderQuestions();
		}

		public void ReorderQuestions()
		{
			OnPropertyChanged(nameof(items));
		}
		
		private void MoveUp(object sender, RoutedEventArgs e)
		{
			int that = (int)(sender as Button).Tag;
			that -= 1;
			items.Move(that, that - 1);
			ReorderQuestions();
		}

		private void MoveDown(object sender, RoutedEventArgs e)
		{
			int that = (int)(sender as Button).Tag;
			that -= 1;
			items.Move(that, that + 1);
			ReorderQuestions();
		}

		int _ind = -1;
		public int selectedIndex { get => _ind; set => Set(ref _ind, value); }

		public Visibility isSelected { get => selectedIndex >= 0 ? Visibility.Visible : Visibility.Collapsed; }

		private void SelectQuestion(object sender, SelectionChangedEventArgs e)
		{
			selectedIndex = (sender as GridView).SelectedIndex;
			OnPropertyChanged(nameof(selectedIndex));
			OnPropertyChanged(nameof(selectedItem));
			OnPropertyChanged(nameof(isSelected));
		}

		public ExamEditItem selectedItem
		{
			get => selectedIndex >= 0 ? items[selectedIndex] : placeHolder;
		}

		private void saveChange(object sender, RoutedEventArgs e)
		{
			qsList.SelectedIndex = -1;
			ReorderQuestions();
		}

		private void SendToEditor(object sender, RoutedEventArgs e)
		{
			NoteModel send = new NoteModel()
			{
				Title = "New question",
				Content = ExamEditItem.ToString(items, answerPosition.Bottom, QuestionSeparator.Dot, SpaceAfterSeparator.Yes)
			};
			/*string answers = "A=";
			foreach (var q in items)
			{
				if (!answerStoreMode)
				{
					send.Content += $"{q.Question} A={ansSymbol[q.Corrected]}\r\n";
				}
				else
				{
					send.Content += $"{q.Question}\r\n";
					answers += ansSymbol[q.Corrected];
				}
				int c = 1;
				foreach (string s in q.Answer)
				{
					send.Content += $"{ansSymbol[c - 1]}. {q.Answer[c - 1]}\r\n";
					c += 1;
				}
				for (int i = 0;i < q.Answer.Count - 1;i++)
				{
					send.Content += $"{ansSymbol[i]}. {q.Answer[i]}\r\n";
				}
			}
			if (answerStoreMode)
			{
				send.Content += answers;
			}*/
			NavigationService.Navigate<NoteEditorView>(send);
		}

		public const string ansSymbol = "ABCDEF";

		private void LeaveToMM(object sender, RoutedEventArgs e)
		{
			NavigationService.GoBack();
		}
	}

	public class ExamsEdit : ObservableCollection<ExamEditItem>
	{
		public void AddRange(ObservableCollection<ExamEditItem> items)
		{
			foreach (var item in items)
			{
				base.Add(item);
				item.Index = IndexOf(item);
			}			
		}

		public new void Add(ExamEditItem item)
		{
			base.Add(item);
			item.Index = IndexOf(item) + 1;
		}

		public new void Remove(ExamEditItem item)
		{
			item.Dispose();
			base.Remove(item);
			item.Index = -1;
			ReIndex();
		}

		public new void RemoveAt(int index)
		{
			var item = this[index];
			Remove(item);
		}

		public new ExamEditItem this[int index]
		{
			get { return base[index]; }
			set { base[index] = value; ReIndex(); }
		}
		
		public new void Move(int oldindx,int newindx)
		{
			base.Move(oldindx, newindx);
			ReIndex();
		}

		private void ReIndex()
		{
			foreach (var x in this)
			{
				x.Index = IndexOf(x) + 1;
			}
		}
	}

	public class ExamEditItem : Observable, IDisposable
	{
		[JsonIgnore]
		private static List<bool> UsedCounter = new List<bool>();
		[JsonIgnore]
		private static object Lock = new object();
		
		[JsonIgnore]
		public static int maxInd { get => UsedCounter.Count - 1; }

		int _i, _c;
		string qt;
		ObservableCollection<string> aw;

		public int Index
		{
			get => _i;
			set
			{
				Set(ref _i, value);
				OnPropertyChanged(nameof(isFirst));
				OnPropertyChanged(nameof(isSecond));
				OnPropertyChanged(nameof(isThird));
				OnPropertyChanged(nameof(isForth));
				OnPropertyChanged(nameof(isFifth));
			}
		}
		public string Question
		{
			get => qt;
			set => Set(ref qt, value);
		}
		public ObservableCollection<string> Answer
		{
			get => aw;
			set
			{
				Set(ref aw, value);
				OnPropertyChanged(nameof(First));
				OnPropertyChanged(nameof(Second));
				OnPropertyChanged(nameof(Third));
				OnPropertyChanged(nameof(Forth));
				OnPropertyChanged(nameof(Fifth));
			}
		}
		public int Corrected
		{
			get => _c;
			set => Set(ref _c, value);
		}

		public ExamEditItem()
		{
			int indx = -1;
			lock (Lock)
			{
				indx = GetAvailableIndex();
				if (indx == -1)
				{
					indx = UsedCounter.Count;
					UsedCounter.Add(true);
				}
			}
			Index = indx;
			Answer = new ObservableCollection<string>();
			Question = "";
			Answer.Add("");
#if DEBUG
			//Question = GetRandomWord();
			//Answer.Add(GetRandomWord());
#else
#endif
		}

		public void Dispose()
		{
			lock (Lock)
			{
				try { UsedCounter[Index] = false; }
				catch (Exception ex) { Debug.Write(ex.Message); }
			}
		}

		int GetAvailableIndex()
		{
			for (int i = 0; i < UsedCounter.Count; i++)
			{
				if (UsedCounter[i] == false)
					return i;
			}
			return -1;
		}

		//Binding
		[JsonIgnore]
		public Visibility First { get => Visibility.Visible; }
		[JsonIgnore]
		public Visibility Second { get => Answer.Count >= 1 ? (Answer[0].Length > 1 ? Visibility.Visible : Visibility.Collapsed) : Visibility.Collapsed; }
		[JsonIgnore]
		public Visibility Third { get => Answer.Count >= 2 ? (Answer[1].Length > 1 ? Visibility.Visible : Visibility.Collapsed) : Visibility.Collapsed; }
		[JsonIgnore]
		public Visibility Forth { get => Answer.Count >= 3 ? (Answer[2].Length > 1 ? Visibility.Visible : Visibility.Collapsed) : Visibility.Collapsed; }
		[JsonIgnore]
		public Visibility Fifth { get => Answer.Count >= 4 ? (Answer[3].Length > 1 ? Visibility.Visible : Visibility.Collapsed) : Visibility.Collapsed; }

		//TODO:Know issue: when answers added if it was only 1 char other answers will get remove
		[JsonIgnore]
		public string first
		{
			get => Answer.Count >= 1 ? Answer[0] : "";
			set
			{
				if (Answer.Count >= 1)
					Answer[0] = value;
				else
					nothing = value;
				if (value.Length > 1 && Answer.Count == 1)
					Answer.Add("");
				OnPropertyChanged(nameof(Second));
			}
		}
		[JsonIgnore]
		public string second
		{
			get => Answer.Count >= 2 ? Answer[1] : "";
			set
			{
				if (Answer.Count >= 2)
					Answer[1] = value;
				else
					nothing = value;
				if (value.Length > 1 && Answer.Count == 2)
					Answer.Add("");
				OnPropertyChanged(nameof(Second));
				OnPropertyChanged(nameof(Third));
			}
		}
		[JsonIgnore]
		public string third
		{
			get => Answer.Count >= 3 ? Answer[2] : "";
			set
			{
				if (Answer.Count >= 3)
					Answer[2] = value;
				else
					nothing = value;
				if (value.Length > 1 && Answer.Count == 3)
					Answer.Add("");
				OnPropertyChanged(nameof(Third));
				OnPropertyChanged(nameof(Forth));
			}
		}
		[JsonIgnore]
		public string forth
		{
			get => Answer.Count >= 4 ? Answer[3] : "";
			set
			{
				if (Answer.Count >= 4)
					Answer[3] = value;
				else
					nothing = value;
				if (value.Length > 1 && Answer.Count == 4)
					Answer.Add("");
				OnPropertyChanged(nameof(Forth));
				OnPropertyChanged(nameof(Fifth));
			}
		}
		[JsonIgnore]
		public string fifth
		{
			get => Answer.Count >= 5 ? Answer[4] : "";
			set
			{
				if (Answer.Count >= 5)
					Answer[4] = value;
				else
					nothing = value;
			}
		}

		[JsonIgnore]
		private string nothing { get; set; } = "";

		[JsonIgnore]
		public bool isFirst { get => Corrected == 0; set { if (value) Corrected = 0; ; OnPropertyChanged(nameof(isFirst)); OnPropertyChanged(nameof(isSecond)); OnPropertyChanged(nameof(isThird)); OnPropertyChanged(nameof(isForth)); OnPropertyChanged(nameof(isFifth)); } }
		[JsonIgnore]
		public bool isSecond { get => Corrected == 1; set { if (value) Corrected = 1; OnPropertyChanged(nameof(isFirst)); OnPropertyChanged(nameof(isSecond)); OnPropertyChanged(nameof(isThird)); OnPropertyChanged(nameof(isForth)); OnPropertyChanged(nameof(isFifth)); } }
		[JsonIgnore]
		public bool isThird { get => Corrected == 2; set { if (value) Corrected = 2; OnPropertyChanged(nameof(isFirst)); OnPropertyChanged(nameof(isSecond)); OnPropertyChanged(nameof(isThird)); OnPropertyChanged(nameof(isForth)); OnPropertyChanged(nameof(isFifth)); } }
		[JsonIgnore]
		public bool isForth { get => Corrected == 3; set { if (value) Corrected = 3; OnPropertyChanged(nameof(isFirst)); OnPropertyChanged(nameof(isSecond)); OnPropertyChanged(nameof(isThird)); OnPropertyChanged(nameof(isForth)); OnPropertyChanged(nameof(isFifth)); } }
		[JsonIgnore]
		public bool isFifth { get => Corrected == 4; set { if (value) Corrected = 4; OnPropertyChanged(nameof(isFirst)); OnPropertyChanged(nameof(isSecond)); OnPropertyChanged(nameof(isThird)); OnPropertyChanged(nameof(isForth)); OnPropertyChanged(nameof(isFifth)); } }

		[JsonIgnore]
		public Visibility isCanUp { get => Index == 1 ? Visibility.Collapsed : Visibility.Visible; }
		[JsonIgnore]
		public Visibility isCanDown { get => Index >= maxInd ? Visibility.Collapsed : Visibility.Visible; }

		public const string answerpos = "AnswerPosition";
		public const string separator = "Separate";
		public const string usesp8spr = "SpaceAfterSeparate";

		public static string ToString(IList<ExamEditItem> items, answerPosition answerPos, QuestionSeparator Separate, SpaceAfterSeparator UseSpace)
		{
			//Config
			string output = $"MODE#EXAM " +
				$"{answerpos}={answerPos.ToString()} " +
				$"{separator}={Separate.ToString()} " +
				$"{usesp8spr}={UseSpace.ToString()}\r\n";
			//Questions and Answers
			string separ8 = Separate == QuestionSeparator.Dot ? "." : ")";
			string space = UseSpace == SpaceAfterSeparator.Yes ? " " : "";
			string answerLine = ">";
			foreach (var item in items)
			{
				string index = $"{item.Index}";
				string after = answerPos == answerPosition.BehindAnswer ? $" A={ExamEdit.ansSymbol[item.Corrected]}" : "";
				answerLine += answerPos == answerPosition.Bottom ? $"{ExamEdit.ansSymbol[item.Corrected]}" : "";
				output += $"{index}{separ8}{space}{item.Question}{after}\r\n";
				int i = 1;
				foreach (var an in item.Answer)
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
		
		public static ObservableCollection<ExamEditItem> FromString(string content)
		{
			var items = new ObservableCollection<ExamEditItem>();
			List<string> lines = content.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
			answerPosition answerPos = answerPosition.Bottom;
			QuestionSeparator Separate = QuestionSeparator.Dot;
			SpaceAfterSeparator UseSpace = SpaceAfterSeparator.Yes;
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
			}
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
			ExamEditItem item = new ExamEditItem();
			foreach (var line in lines)
			{
				bool isquestion = char.IsNumber(line[0]);
				if (isquestion)
				{
					char check = Separate == QuestionSeparator.Dot ? '.' : ')';
					item.Index = int.Parse(line.Substring(0, line.IndexOf(check) - 1));
					item.Question = line.Substring(line.IndexOf(check));
					if (answerPos == answerPosition.BehindAnswer)
					{
						item.Corrected = item.Question[item.Question.Length - 1];
						item.Question = item.Question.Substring(0, item.Question.LastIndexOf('=') - 1);
					}
					else
					{
						item.Corrected = answersLine[item.Index];
					}
				}
				else
				{
					//TODO:Finish adding answer logic
					int predict = index;
					while (!char.IsNumber(lines[predict][0]))
					{
						predict += 1;
					}
				}
				index++;
			}
			return items;
		}

		public static T ParseEnum<T>(string value)
		{
			return (T)Enum.Parse(typeof(T), value, true);
		}

#if DEBUG
		private const string words = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
		private static List<string> rands = new List<string>();
		public static string GetRandomWord()
		{
			Random r = new Random();
			if (rands.Count < 5)
			{
				InitializeRandomness();
			}
			int len = r.Next(0, rands.Count - 1);
			string s = rands[len];
			rands.RemoveAt(len);
			return s;
		}

		public static void InitializeRandomness()
		{
			Random r = new Random();
			string s = "";
			int c = 0;
			int length = 0;
			for (c = r.Next(10, 100); c > 0; c--)
			{
				s = "";
				for (length = r.Next(10, 150); length > 0; length--)
				{
					s += words[r.Next(0, words.Length - 1)];
				}
				rands.Add(s);
			}
		}
#endif
	}

	public enum answerPosition
	{
		BehindAnswer,
		Bottom
	}

	public enum numberStyle
	{
		Number,
		Alphabet
	}
	
	public enum QuestionSeparator
	{
		Dot,
		Bracket
	}

	public enum SpaceAfterSeparator
	{
		Yes,
		No
	}
}