using JustRemember.Helpers;
using JustRemember.Models;
using JustRemember.Services;
using JustRemember.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace JustRemember.ViewModels
{
	public class NoteEditorViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
		{
			if (Equals(storage, value))
			{
				return;
			}
			storage = value;
			OnPropertyChanged(propertyName);
		}

		protected async void OnPropertyChanged(string property)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
			if (property == nameof(NoteName))
			{
				await MobileTitlebarService.Refresh(NoteName);
			}
		}

		public NoteEditorView view;

		NoteModel _note = new NoteModel();
		public NoteModel editedNote
		{
			get
			{
				return _note;
			}
			set
			{
				Set(ref _note, value);
			}
		}

		public string NoteName
		{
			get
			{
				return editedNote.Title;
			}
			set
			{
				_note.Title = value;
				OnPropertyChanged(nameof(editedNote));
				OnPropertyChanged(nameof(saveableSymbol));
				OnPropertyChanged(nameof(saveableSymbolColor));
				TitleUpdate(value);
			}
		}

		async void TitleUpdate(string val)
		{
			await MobileTitlebarService.Refresh(val);
		}

		public string NoteContent
		{
			get
			{
				return editedNote.Content;
			}
			set
			{
				_note.Content = value;
				OnPropertyChanged(nameof(editedNote));
			}
		}

		int _szz = 18;
		public int FontSize
		{
			get
			{
				return _szz;
			}
			set
			{
				Set(ref _szz, value);
				view.MainEditBox.FontSize = value;
				view.MainEditBox.Document.ApplyDisplayUpdates();
			}
		}

		int[] sizeList = new int[] { -1, 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
		public int fontSizeCBB
		{
			get
			{
				if (sizeList.Contains(FontSize))
				{
					return sizeList.ToList().IndexOf(FontSize);
				}
				return 0;
			}
			set
			{
				FontSize = sizeList[value];
			}
		}

		bool _bol, _ita = false;
		public bool isBold
		{
			get => _bol;
			set
			{
				Set(ref _bol, value);
				OnPropertyChanged(nameof(FontName));
			}
		}

		public bool isItalic
		{
			get => _ita;
			set
			{
				Set(ref _ita, value);
				OnPropertyChanged(nameof(FontName));
			}
		}
		
		public string FontName
		{
			get
			{
				string isb = isBold ? "Bold" : "";
				string isi = isItalic ? "Italic" : "";
				return $"ms-appx:/Assets/Fonts/THSarabunNew{isb}{isi}.ttf#TH Sarabun New";
			}
		}

		bool _undo = false;
		bool _redo = false;
		public bool canUndo
		{
			get => _undo;
			set
			{
				Set(ref _undo, value);
			}
		}

		public bool canRedo
		{
			get => _redo;
			set
			{
				Set(ref _redo, value);
			}
		}

		public ICommand Undo;
		public ICommand Redo;

		public ICommand clearInput;
		public ICommand saveInput;
		public ICommand loadInput;

		public ICommand splitUndo;

		public ICommand saveFilename;
		public ICommand sendToQE;

		public MessageDialog doneSave;
		public void ReNew()
		{
			Undo = new RelayCommand<RoutedEventArgs>(UNDO);
			Redo = new RelayCommand<RoutedEventArgs>(REDO);
			//
			clearInput = new RelayCommand<RoutedEventArgs>(CLEARINPUT);
			saveInput = new RelayCommand<RoutedEventArgs>(SAVEINPUT);
			loadInput = new RelayCommand<RoutedEventArgs>(LOADINPUT);
			//Load
			//
			splitUndo = new RelayCommand<KeyRoutedEventArgs>(SPLITUNDO);
			//
			saveFilename = new RelayCommand<TextChangedEventArgs>(SAVEFILENAME);
			//
			sendToQE = new RelayCommand<RoutedEventArgs>(SENDTOQE);
			//Dialog
			notLongEnough = new MessageDialog(App.language.GetString("Dialog_Not_long_enough_editor"));
			notLongEnough.Commands.Add(new UICommand(App.language.GetString("Match_dialog_ok")));
			doneSave = new MessageDialog("File saved");
			doneSave.Commands.Add(new UICommand("OK"));
		}

		private void SENDTOQE(RoutedEventArgs obj)
		{
			if (editedNote?.Mode == noteMode.Question)
			{
				NavigationService.Navigate<QuestionDesignView>(editedNote);
				return;
			}
			NavigationService.Navigate<QuestionDesignView>();
		}

		public MessageDialog notLongEnough;
		
		string mode;
		public string modeDetection
		{
			get => mode;
			set
			{
				pcItem = value == "P" ? Visibility.Visible : Visibility.Collapsed;
				mbItem = value == "M" ? Visibility.Visible : Visibility.Collapsed;
				mode = value;
			}
		}

		Visibility _pc = Visibility.Collapsed;
		Visibility _mb = Visibility.Visible;
		public Visibility pcItem { get => _pc; set => Set(ref _pc, value); }
		public Visibility mbItem { get => _mb; set => Set(ref _mb, value); }

		Visibility _emp, _con, _al = Visibility.Collapsed;
		public Visibility isEmpty
		{
			get => _emp;
			set => Set(ref _emp, value);
		}
		public Visibility isContainIlegalName
		{
			get => _con;
			set => Set(ref _con, value);
		}
		public bool isNew;
		public Visibility isAlreadyExist
		{
			get
			{
				if (!isNew)
				{
					return Visibility.Collapsed;
				}
				return _al;
			}
			set => Set(ref _al, value);
		}
		public List<string> fileList;

		public Symbol saveableSymbol
		{
			get
			{
				if (isEmpty == Visibility.Collapsed && isContainIlegalName == Visibility.Collapsed && isAlreadyExist == Visibility.Collapsed)
				{
					return Symbol.Accept;
				}
				return Symbol.Important;
			}
		}

		public SolidColorBrush saveableSymbolColor
		{
			get
			{
				if (isEmpty == Visibility.Collapsed && isContainIlegalName == Visibility.Collapsed && isAlreadyExist == Visibility.Collapsed)
				{
					return new SolidColorBrush(Colors.Green);
				}
				return new SolidColorBrush(Colors.Red);
			}
		}
		
		private void SAVEFILENAME(TextChangedEventArgs obj)
		{
			string chk = NoteName;
			//Check existance
			if (isNew)
			{
				isAlreadyExist = fileList.Contains(chk) ? Visibility.Visible : Visibility.Collapsed;
			}
			isEmpty = string.IsNullOrWhiteSpace(chk.Trim()) ? Visibility.Visible : Visibility.Collapsed;
			isContainIlegalName = chk.IsFileValid() ? Visibility.Visible : Visibility.Collapsed;
		}

		private void REDO(RoutedEventArgs obj)
		{
			view.MainEditBox.Document.Redo();
			canUndo = view.MainEditBox.Document.CanUndo();
			canRedo = view.MainEditBox.Document.CanRedo();
		}

		private void UNDO(RoutedEventArgs obj)
		{
			view.MainEditBox.Document.Undo();
			canUndo = view.MainEditBox.Document.CanUndo();
			canRedo = view.MainEditBox.Document.CanRedo();
		}

		private void SPLITUNDO(Windows.UI.Xaml.Input.KeyRoutedEventArgs obj)
		{
			if (obj.Key == Windows.System.VirtualKey.Space)
			{
				view.MainEditBox.Document.EndUndoGroup();
				view.MainEditBox.Document.BeginUndoGroup();
			}
			canUndo = view.MainEditBox.Document.CanUndo();
			canRedo = view.MainEditBox.Document.CanRedo();
		}

		private async void LOADINPUT(RoutedEventArgs obj)
		{
			FileOpenPicker openPicker = new FileOpenPicker()
			{
				ViewMode = PickerViewMode.List,
				SuggestedStartLocation = PickerLocationId.DocumentsLibrary
			};
			openPicker.FileTypeFilter.Add(".txt");

			StorageFile file = await openPicker.PickSingleFileAsync();
			if (file != null)
			{
				string content = await FileIO.ReadTextAsync(file);
				view.MainEditBox.Document.SetText(Windows.UI.Text.TextSetOptions.UnicodeBidi, content);
			}
		}

		private async void SAVEINPUT(RoutedEventArgs obj)
		{
			view.MainEditBox.Document.GetText(Windows.UI.Text.TextGetOptions.UseCrlf, out string cont);
			NoteContent = cont;
			//Test it first
			SessionModel t = SessionModel.generate(editedNote);
			if (t == null)
			{
				await notLongEnough.ShowAsync();
				return;
			}
			await NoteModel.SaveNote(editedNote);
			await doneSave.ShowAsync();
		}

		private void CLEARINPUT(RoutedEventArgs obj)
		{
			view.MainEditBox.Document.SetText(Windows.UI.Text.TextSetOptions.None, "\0");
		}
	}

	public static class Extend
	{
		public static bool Between(this int value, double min, double max)
		{
			if (value > min && value < max)
			{
				return true;
			}
			return false;
		}

		public static bool IsFileValid(this string tocp)
		{
			foreach (char c in Path.GetInvalidFileNameChars())
			{
				if (tocp.Contains(c))
				{
					return true;
				}
			}
			return false;
		}
	}
}
