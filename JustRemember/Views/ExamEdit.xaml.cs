using JustRemember.Helpers;
using JustRemember.Models;
using JustRemember.Services;
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

		ObservableCollection<ExamEditItem> _item;
		public ObservableCollection<ExamEditItem> items { get => _item; set => Set(ref _item, value); }
		bool _am;
		public bool answerStoreMode { get => _am; set => Set(ref _am, value); }
		
		public ExamEdit()
		{
			this.InitializeComponent();
			items = new ObservableCollection<ExamEditItem>();
			
#if DEBUG
			ExamEditItem.InitializeRandomness();
#endif
			InitializeDispatch();		
		}
		
		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			if (e.Parameter != null)
			{
				var note = (NoteModel)e.Parameter;
				List<string> lines = note.Content.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
				string answers = null;
				if (lines.Last().StartsWith("A="))
				{
					answers = lines.Last().Remove(0, 1);
					lines.RemoveAt(lines.Count - 1);
				}
				lines.RemoveAt(0);
				ExamEditItem item = new ExamEditItem(0);
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
								item = new ExamEditItem(items.Count);
								item.Answer.Clear();
							}
						}
					}
				}
				item.Answer.Add(lines.Last().Remove(0, lines.Last().IndexOf('.') + 1).Trim());
				items.Add(item);
				item = new ExamEditItem(items.Count);
				item.Answer.Clear();
			}
			await MobileTitlebarService.Refresh("Question desinger");
			base.OnNavigatedTo(e);
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
			if (items.Count == 0)
			{
				items.Add(new ExamEditItem(0));
				ExamEditItem.maxInd = items.Count;
				ReorderQuestions();
				return;
			}
			items.Add(new ExamEditItem(items[items.Count - 1].Index));
			ExamEditItem.maxInd = items.Count;
			ReorderQuestions();
		}

		private void DeleteQuestion(object sender, RoutedEventArgs e)
		{
			if (items.Count == 1)
			{
				items.Clear();
				ReorderQuestions();
				return;
			}
			int that = (int)(sender as Button).Tag;
			that -= 1;
			ExamEditItem.maxInd = items.Count - 1;
			items.RemoveAt(that);
			//Reorder
			ReorderQuestions();
		}

		public void ReorderQuestions()
		{
			ExamEditItem.maxInd = items.Count - 1;
			ReSort();
			ReIndex();
			OnPropertyChanged(nameof(items));
		}

		async void ReIndex()
		{
			await _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{
				int i = 1;
				foreach (var q in items)
				{
					if (q.Index == i)
					{
						i += 1;
						continue;
					}
					else
					{
						q.Index = i;
						i += 1;
						continue;
					}
				}
			});
		}

		async void ReSort()
		{
			await _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{
				List<ExamEditItem> qs = new List<ExamEditItem>(items);
				qs.Sort((x, y) => x.Index.CompareTo(y.Index));
				items = new ObservableCollection<ExamEditItem>(qs);
			});
		}

		private void MoveUp(object sender, RoutedEventArgs e)
		{
			int that = (int)(sender as Button).Tag;
			that -= 1;
			items.Move(that, that - 1);
			ReIndex();
			ReorderQuestions();
		}

		private void MoveDown(object sender, RoutedEventArgs e)
		{
			int that = (int)(sender as Button).Tag;
			that -= 1;
			items.Move(that, that + 1);
			ReIndex();
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
			get => selectedIndex >= 0 ? (ExamEditItem)qsList.SelectedItem : new ExamEditItem(0);
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
				Content = "#MODE=EXAM\r\n"
			};
			string answers = "A=";
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
			}
			NavigationService.Navigate<NoteEditorView>(send);
		}

		string ansSymbol = "ABCDEF";

		private void LeaveToMM(object sender, RoutedEventArgs e)
		{
			NavigationService.GoBack();
		}
	}

	public class ExamEditItem : Observable
	{
		public static int maxInd = 5;

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

		public ExamEditItem(int prev)
		{
			if (prev < 0) { prev = 0; }
			Index = prev + 1;
			Answer = new ObservableCollection<string>();
			Question = "";
			Answer.Add("");
#if DEBUG
			Question = GetRandomWord();
			Answer.Add(GetRandomWord());
#else
#endif
		}

		//Binding
		public Visibility First { get => Visibility.Visible; }
		public Visibility Second { get => Answer.Count >= 1 ? (Answer[0].Length > 1 ? Visibility.Visible : Visibility.Collapsed) : Visibility.Collapsed; }
		public Visibility Third { get => Answer.Count >= 2 ? (Answer[1].Length > 1 ? Visibility.Visible : Visibility.Collapsed) : Visibility.Collapsed; }
		public Visibility Forth { get => Answer.Count >= 3 ? (Answer[2].Length > 1 ? Visibility.Visible : Visibility.Collapsed) : Visibility.Collapsed; }
		public Visibility Fifth { get => Answer.Count >= 4 ? (Answer[3].Length > 1 ? Visibility.Visible : Visibility.Collapsed) : Visibility.Collapsed; }

		//TODO:Know issue: when answers added if it was only 1 char other answers will get remove
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
				if (string.IsNullOrWhiteSpace(value) && Answer.Count == 2)
					if (Answer[1] == "\0")
						Answer.RemoveAt(1);
				OnPropertyChanged(nameof(Second));
				OnPropertyChanged(nameof(Third));
				OnPropertyChanged(nameof(Forth));
				OnPropertyChanged(nameof(Fifth));
			}
		}
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
				if (string.IsNullOrWhiteSpace(value) && Answer.Count == 3)
					if (Answer[2] == "\0")
						Answer.RemoveAt(2);
				OnPropertyChanged(nameof(Second));
				OnPropertyChanged(nameof(Third));
				OnPropertyChanged(nameof(Forth));
				OnPropertyChanged(nameof(Fifth));
			}
		}
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
				if (string.IsNullOrWhiteSpace(value) && Answer.Count == 4)
					if (Answer[3] == "\0")
						Answer.RemoveAt(3);
				OnPropertyChanged(nameof(Second));
				OnPropertyChanged(nameof(Third));
				OnPropertyChanged(nameof(Forth));
				OnPropertyChanged(nameof(Fifth));
			}
		}
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
				if (string.IsNullOrWhiteSpace(value) && Answer.Count == 5)
					if (Answer[4] == "\0")
						Answer.RemoveAt(4);
				OnPropertyChanged(nameof(Second));
				OnPropertyChanged(nameof(Third));
				OnPropertyChanged(nameof(Forth));
				OnPropertyChanged(nameof(Fifth));
			}
		}
		public string fifth
		{
			get => Answer.Count >= 5 ? Answer[4] : "";
			set
			{
				if (Answer.Count >= 5)
					Answer[4] = value;
				else
					nothing = value;
				OnPropertyChanged(nameof(Second));
				OnPropertyChanged(nameof(Third));
				OnPropertyChanged(nameof(Forth));
				OnPropertyChanged(nameof(Fifth));
			}
		}

		private string nothing { get; set; } = "";

		public bool isFirst { get => Corrected == 0; set { if (value) Corrected = 0; ; OnPropertyChanged(nameof(isFirst)); OnPropertyChanged(nameof(isSecond)); OnPropertyChanged(nameof(isThird)); OnPropertyChanged(nameof(isForth)); OnPropertyChanged(nameof(isFifth)); } }
		public bool isSecond { get => Corrected == 1; set { if (value) Corrected = 1; OnPropertyChanged(nameof(isFirst)); OnPropertyChanged(nameof(isSecond)); OnPropertyChanged(nameof(isThird)); OnPropertyChanged(nameof(isForth)); OnPropertyChanged(nameof(isFifth)); } }
		public bool isThird { get => Corrected == 2; set { if (value) Corrected = 2; OnPropertyChanged(nameof(isFirst)); OnPropertyChanged(nameof(isSecond)); OnPropertyChanged(nameof(isThird)); OnPropertyChanged(nameof(isForth)); OnPropertyChanged(nameof(isFifth)); } }
		public bool isForth { get => Corrected == 3; set { if (value) Corrected = 3; OnPropertyChanged(nameof(isFirst)); OnPropertyChanged(nameof(isSecond)); OnPropertyChanged(nameof(isThird)); OnPropertyChanged(nameof(isForth)); OnPropertyChanged(nameof(isFifth)); } }
		public bool isFifth { get => Corrected == 4; set { if (value) Corrected = 4; OnPropertyChanged(nameof(isFirst)); OnPropertyChanged(nameof(isSecond)); OnPropertyChanged(nameof(isThird)); OnPropertyChanged(nameof(isForth)); OnPropertyChanged(nameof(isFifth)); } }

		public Visibility isCanUp { get => Index == 1 ? Visibility.Collapsed : Visibility.Visible; }
		public Visibility isCanDown { get => Index > maxInd ? Visibility.Collapsed : Visibility.Visible; }
		
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
}