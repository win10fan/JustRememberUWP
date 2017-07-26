using JustRemember.Helpers;
using JustRemember.Models;
using JustRemember.Services;
using JustRemember.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace JustRemember.ViewModels
{
	public class QuestionDesignViewModel : Observable
	{
		ObservableCollection<QuestionDesignModel> _mod = new ObservableCollection<QuestionDesignModel>();
		public ObservableCollection<QuestionDesignModel> Questions { get => _mod; set { Set(ref _mod, value); QuestionDesignModel.maxIndex = Questions.Count; OnPropertyChanged(nameof(questionCount)); OnPropertyChanged(nameof(questionCountSTR)); } }
		
		public int questionCount { get => Questions.Count; }
		public QuestionDesignView view { get => NavigationService.Frame.Content as QuestionDesignView; }

		public ICommand showSetting;
		public ICommand sendToEditor;
		public ICommand addQuestion;
		public ICommand refreshQuestions;
		public ICommand saveFile;
		public ICommand saveFile4R;
		public ICommand savenameCHK;
		public NoteModel sender;
		MessageDialog doneSave;
		public void Initialize(object parameter)
		{
			Questions = new ObservableCollection<QuestionDesignModel>();
			if (parameter is NoteModel)
			{
				//Try to open it
				sender = parameter as NoteModel;
				Questions = QuestionDesignHelper.FromString((parameter as NoteModel).Content);
				qsName = sender.Title;
			}
			//Commands
			sendToEditor = new RelayCommand<RoutedEventArgs>(SENDTOEDITOR);
			showSetting = new RelayCommand<RoutedEventArgs>(OPENSETTING);
			addQuestion = new RelayCommand<RoutedEventArgs>(ADDQUESTION);
			refreshQuestions = new RelayCommand<RoutedEventArgs>(REFRESHQUESTIONS);
			saveFile = new RelayCommand<RoutedEventArgs>(SAVEFILE);
			saveFile4R = new RelayCommand<RoutedEventArgs>(SAVEFILEFORREAL);
			savenameCHK = new RelayCommand<TextChangedEventArgs>(SAVEFILENAME);
			//
			memos = new List<string>();
			//
			doneSave = new MessageDialog("File saved");
			doneSave.Commands.Add(new UICommand("OK"));
		}

		public List<string> memos;
		async void SAVEFILEFORREAL(RoutedEventArgs obj)
		{
			bool ok1 = isExistAR == Visibility.Collapsed;
			bool ok2 = isEmpty == Visibility.Collapsed;
			bool ok3 = isIllegal == Visibility.Collapsed;
			if (ok1 && ok2 && ok3)
			{
				//Save
				NoteModel sve = new NoteModel() { Title = qsName, Content = QuestionDesignHelper.ToString(Questions) };
				await NoteModel.SaveNote(sve);
				await doneSave.ShowAsync();
			}
		}

		Visibility _a, _b, _c;
		public Visibility isExistAR { get => _a; set => Set(ref _a, value); }

		public Visibility isEmpty { get => _b; set => Set(ref _b, value); }

		public Visibility isIllegal { get => _c; set => Set(ref _c, value); }

		string _namee;
		public string qsName
		{
			get => _namee;
			set { Set(ref _namee, value); UpdateName(value); }
		}

		async void UpdateName(string name)
		{
			await MobileTitlebarService.Refresh(name);
		}

		bool _sav;
		public bool showingSave { get => _sav; set => Set(ref _sav, value); }
		private void SAVEFILENAME(TextChangedEventArgs obj)
		{
			qsName = view.filenameInput.Text;
			//Check existance
			if (memos.Count > 0)
			{
				foreach (var s in memos)
				{
					isExistAR = s == qsName ? Visibility.Visible : Visibility.Collapsed;
				}
			}
			else
			{
				isExistAR = Visibility.Collapsed;
			}
			isEmpty = string.IsNullOrEmpty(qsName.Trim()) ? Visibility.Visible : Visibility.Collapsed;
			isIllegal = !Extend.IsFileValid(qsName) ? Visibility.Collapsed : Visibility.Visible;
			OnPropertyChanged(nameof(qsName));
		}
		
		private void SAVEFILE(RoutedEventArgs obj)
		{
			showingSave = !showingSave;
			saveIconPopup = new SymbolIcon(showingSave ? Symbol.Cancel : Symbol.Save);
			saveLabelPopup = showingSave ? App.language.GetString("QuestionDesign_Close") : App.language.GetString("Edit_save");
		}

		SymbolIcon _sic = new SymbolIcon(Symbol.Save);
		public SymbolIcon saveIconPopup { get => _sic; set => Set(ref _sic, value); }

		string _slc = "Save";
		public string saveLabelPopup { get => _slc; set => Set(ref _slc, value); }

		private void REFRESHQUESTIONS(RoutedEventArgs obj)
		{
			var qar = new List<QuestionDesignModel>();
			qar.AddRange(Questions);
			Questions.Clear();
			foreach (var it in qar)
			{
				Questions.Add(it);
			}
		}

		private void ADDQUESTION(RoutedEventArgs obj)
		{
			Questions.Add(new QuestionDesignModel());
			ReIndex();
			var view = NavigationService.Frame.Content as QuestionDesignView;
			view.questions.SelectedIndex = Questions.Count - 1;
			view.questions.ScrollIntoView(view.questions.SelectedItem);
		}

		public void ReIndex()
		{
			int inde = 1;
			foreach (var q in Questions)
			{
				q.Index = inde;
				inde += 1;
			}
			REFRESHQUESTIONS(null);
			OnPropertyChanged(nameof(Questions));
		}

		public int indexAt(string uid)
		{
			foreach (QuestionDesignModel item in Questions)
			{
				if (item.UID == uid)
				{
					return item.Index;
				}
			}
			return 0;
		}
		
		public static int questionFileCount = 0;
		private void SENDTOEDITOR(RoutedEventArgs obj)
		{
			questionFileCount += 1;
			string content = QuestionDesignHelper.ToString(Questions);
			if (!string.IsNullOrEmpty(content))
			{
				NavigationService.Navigate<NoteEditorView>(new NoteModel() { Title = $"Questions{questionFileCount}", Content = content });
				return;
			}
			NavigationService.Navigate<NoteEditorView>();
		}

		bool _set;
		public bool showSettingPopup { get => _set; set => Set(ref _set, value); }
		private void OPENSETTING(RoutedEventArgs obj)
		{
			showSettingPopup = !showSettingPopup;
		}

		int _sel = -1;
		public int selectedQuestion { get => _sel; set { Set(ref _sel, value); OnPropertyChanged(nameof(selected)); OnPropertyChanged(nameof(isSelected)); if (value > -1) { view.selectedTBIndex.Text = Questions[selectedQuestion].Index.ToString(); } } }

		public QuestionDesignModel selected
		{
			get { return selectedQuestion > -1 ? Questions[selectedQuestion] : new QuestionDesignModel();  }
			set { if (selectedQuestion < 0) { return; } Questions[selectedQuestion] = value; OnPropertyChanged(nameof(selected)); }
		}

		public Visibility isSelected
		{
			get => selectedQuestion > -1 ? Visibility.Visible : Visibility.Collapsed;
		}

		public string questionCountSTR { get => questionCount <= 1 ? App.language.GetString("QE_Question") : App.language.GetString("QE_Questions"); }

		#region Setting and example
		public int AnswerPosition
		{
			get => (int)App.Config.AnswerPosition;
			set { App.Config.AnswerPosition = (answerPosition)value;
				OnPropertyChanged(nameof(answerExampleA1));
				OnPropertyChanged(nameof(answerExampleA2));
				OnPropertyChanged(nameof(answerExampleB));
			}
		}

		public int questionSeparator
		{
			get => (int)App.Config.questionSeparator;
			set { App.Config.questionSeparator = (QuestionSeparator)value;
				OnPropertyChanged(nameof(whatAfterQAList));
			}
		}
		
		public string customAnswerHeader
		{
			get => App.Config.customChoiceHeader;
			set
			{
				App.Config.customChoiceHeader = value;
				OnPropertyChanged(nameof(customAnswerHeaderA));
				OnPropertyChanged(nameof(customAnswerHeaderB));
				OnPropertyChanged(nameof(customAnswerHeaderC));
				OnPropertyChanged(nameof(customAnswerHeaderD));
				OnPropertyChanged(nameof(customAnswerHeaderE));
			}
		}

		public string customAnswerHeaderA { get { if (App.Config.customChoiceHeader.Length < 1) { return ""; } return App.Config.customChoiceHeader[0].ToString(); } }
		public string customAnswerHeaderB { get { if (App.Config.customChoiceHeader.Length < 2) { return ""; } return App.Config.customChoiceHeader[1].ToString(); } }
		public string customAnswerHeaderC { get { if (App.Config.customChoiceHeader.Length < 3) { return ""; } return App.Config.customChoiceHeader[2].ToString(); } }
		public string customAnswerHeaderD { get { if (App.Config.customChoiceHeader.Length < 4) { return ""; } return App.Config.customChoiceHeader[3].ToString(); } }
		public string customAnswerHeaderE { get { if (App.Config.customChoiceHeader.Length < 5) { return ""; } return App.Config.customChoiceHeader[4].ToString(); } }

		public bool spaceAfterSeparator
		{
			get => App.Config.spaceAfterSeparator == SpaceAfterSeparator.Yes;
			set
			{
				App.Config.spaceAfterSeparator = value ? SpaceAfterSeparator.Yes : SpaceAfterSeparator.No;
				OnPropertyChanged(nameof(useSpaceAfterSep));
			}
		}

		public string answerExampleA1
		{
			get => App.Config.AnswerPosition == answerPosition.BehindAnswer ? "A=C" : "";
		}

		public string answerExampleA2
		{
			get => App.Config.AnswerPosition == answerPosition.BehindAnswer ? "A=D" : "";
		}
		
		public string answerExampleB
		{
			get => App.Config.AnswerPosition == answerPosition.Bottom ? "\r\nA=CDB" : "";
		}

		public string whatAfterQAList
		{
			get => App.Config.questionSeparator == QuestionSeparator.Dot ? "." : ")";
		}

		public string useSpaceAfterSep
		{
			get => App.Config.spaceAfterSeparator == SpaceAfterSeparator.Yes ? " " : "";
		}
		#endregion		
	}
}