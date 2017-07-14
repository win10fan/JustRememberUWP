using JustRemember.Helpers;
using JustRemember.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using System.ComponentModel;
using Windows.UI.Xaml;
using System.Runtime.CompilerServices;
using Windows.Storage.Pickers;
using JustRemember.Views;
using Windows.UI.Xaml.Input;
using JustRemember.Services;
using Windows.UI.Popups;
using System.Diagnostics;

namespace JustRemember.ViewModels
{
	public class NotesViewModel : INotifyPropertyChanged
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

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(noNoteSuggestion)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsNotSelected)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(isReady)));
        }

		public Visibility isReady
		{
			get
			{
				if (config == null)
				{
					return Visibility.Visible;
				}
				return Visibility.Collapsed;
			}
		}

        ObservableCollection<NoteModel> _notes;

        public ObservableCollection<NoteModel> Notes
        {
            get { return _notes; }
            set
            {
                Set(ref _notes, value);
				OnPropertyChanged(nameof(NoteCount));
            }
        }

        public ICommand OpenNote { get; private set; }
        public ICommand SelectNote { get; private set; }
        public ICommand ReloadList { get; private set; }
        public ICommand ImportNote { get; private set; }
        public ICommand EditSelected { get; private set; }
        public ICommand DeleteSelected { get; private set; }
        public ICommand DeSelect { get; private set; }
        public ICommand SendToMatch { get; private set; }
		public ICommand OpenAddFlyout { get; private set; }
		public ICommand GoToNoteEditorWithNote { get; private set; }
		public ICommand GoToQuesionDesignerWithNote { get; private set; }
		public ICommand EditSelector { get; private set; }
		public ICommand SendToNoteEdior { get; private set; }
		public ICommand SendToQuestionDesigner { get; private set; }
		public ICommand CloseOpenWithDialog { get; private set; }

		public NotesViewModel()
        {
            OpenNote = new RelayCommand<DoubleTappedRoutedEventArgs>(OnItemDoubleClick);
            SelectNote = new RelayCommand<SelectionChangedEventArgs>(OnSelectNote);
            ReloadList = new RelayCommand<RoutedEventArgs>(ReloadingListAsync);
            ImportNote = new RelayCommand<RoutedEventArgs>(ImportTextFile);
            EditSelected = new RelayCommand<RoutedEventArgs>(EditSelectedItem);
            DeleteSelected = new RelayCommand<RoutedEventArgs>(DeleteSelectedItem);
            DeSelect = new RelayCommand<RoutedEventArgs>(DeSelectNote);
            SendToMatch = new RelayCommand<RoutedEventArgs>(NavigateToMatchWithNote);
			OpenAddFlyout = new RelayCommand<RoutedEventArgs>(OPENADDFLYOUT);
			GoToNoteEditorWithNote = new RelayCommand<RoutedEventArgs>(GOTONOTEEDITORWITHNOTE);
			GoToQuesionDesignerWithNote = new RelayCommand<RoutedEventArgs>(GOTOQUESTIONDESIGNERWITHNOTE);
			EditSelector = new RelayCommand<RoutedEventArgs>(EDITSELECTOR);
			SendToNoteEdior = new RelayCommand<RoutedEventArgs>(SENDTONOTEEDITOR);
			SendToQuestionDesigner = new RelayCommand<RoutedEventArgs>(SENDTOQUESTIONDESIGNER);
			CloseOpenWithDialog = new RelayCommand<RoutedEventArgs>(CLOSEOPENWITHDIALOG);
			//Dialog
			notLongEnough = new MessageDialog(App.language.GetString("Dialog_Not_long_enough_main"));
			notLongEnough.Commands.Add(new UICommand(App.language.GetString("Match_dialog_ok")));
			//Other
			saver = new DispatcherTimer()
			{
				Interval = TimeSpan.FromSeconds(5)
			};
			saver.Tick += Saver_Tick;
			saver.Start();
		}
		
		private void CLOSEOPENWITHDIALOG(RoutedEventArgs obj)
		{
			EditWithPopup = false;
		}

		private void SENDTOQUESTIONDESIGNER(RoutedEventArgs obj)
		{
			NavigationService.Navigate<QuestionDesignView>(Notes[selectedIndex]);
			EditWithPopup = false;
		}

		private void SENDTONOTEEDITOR(RoutedEventArgs obj)
		{
			NavigationService.Navigate<NoteEditorView>(selectedIndex > -1 ? Notes[selectedIndex] : null);
			EditWithPopup = false;
		}

		bool editWithDiag;
		public bool EditWithPopup
		{
			get => editWithDiag;
			set => Set(ref editWithDiag, value);
		}

		private void EDITSELECTOR(RoutedEventArgs obj)
		{
			if (Notes[selectedIndex].Mode == Models.noteMode.Question)
			{
				EditWithPopup = true;
			}
			else
			{
				EditSelected.Execute(null);
			}
		}

		private void GOTOQUESTIONDESIGNERWITHNOTE(RoutedEventArgs obj)
		{
			NavigationService.Navigate<QuestionDesignView>(selectedIndex > -1 ? Notes[selectedIndex] : null);
		}

		private void GOTONOTEEDITORWITHNOTE(RoutedEventArgs obj)
		{
			NavigationService.Navigate<NoteEditorView>(selectedIndex > -1 ? Notes[selectedIndex] : null);
		}

		private void OPENADDFLYOUT(RoutedEventArgs obj)
		{
			(obj.OriginalSource as AppBarButton).Flyout.ShowAt(obj.OriginalSource as FrameworkElement);
		}

		public MessageDialog notLongEnough;

		private async void Saver_Tick(object sender, object e)
		{
			if (AppConfigModel.isDirty)
				await config.Save();
		}
		DispatcherTimer saver;

		private void NavigateToMatchWithNote(RoutedEventArgs obj)
        {
            InitializeAndGoToMatch();
        }

		async void InitializeAndGoToMatch()
        {
			SessionModel current = SessionModel.generate(Notes[selectedIndex]);
			if (current == null)
			{
				await notLongEnough.ShowAsync();
				return;
			}
            //Goto Match page
            NavigationService.Navigate<Match>(current);
        }

        private void DeSelectNote(RoutedEventArgs obj)
        {
            selectedIndex = -1;
        }

        async void DeleteSelectedItem(RoutedEventArgs obj)
        {
            if (selectedIndex < 0)
            {
                return;
            }
            await NoteModel.DeleteNote(Notes[selectedIndex].Title);
			Notes.RemoveAt(selectedIndex);
        }

        private void EditSelectedItem(RoutedEventArgs obj)
        {
			NavigationService.Navigate<NoteEditorView>(selectedIndex > -1 ? Notes[selectedIndex] : null);
        }

        async void ImportTextFile(RoutedEventArgs obj)
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
				//Try to read it first:
				string content = await FileIO.ReadTextAsync(file);
				SessionModel test = SessionModel.generate(new NoteModel { Title = "Test", Content = content });
				if (test == null)
				{
					await notLongEnough.ShowAsync();
					return;
				}
                StorageFolder folder = ApplicationData.Current.RoamingFolder;
                var noteFolder = await folder.GetFolderAsync("Notes");
                await file.CopyAsync(noteFolder);
                Notes = await NoteModel.GetNotesAsync();
            }
        }
		
        async void ReloadingListAsync(RoutedEventArgs obj)
        {
            Notes = await NoteModel.GetNotesAsync();
        }

        private void OnSelectNote(SelectionChangedEventArgs obj)
        {
            OnPropertyChanged(nameof(IsSelected));
        }

        public void OnItemDoubleClick(DoubleTappedRoutedEventArgs obj)
        {
            if (selectedIndex == -1)
            {
                return;
            }
            else
            {
                InitializeAndGoToMatch();
            }
        }

        AppConfigModel config
		{
			get => App.Config;
		}

        public async void Initialize()
        {
            Notes = await NoteModel.GetNotesAsync();
        }

        public string NoteCount
        {
            get
            {
                if (Notes?.Count < 1)
                {
					return App.language.GetString("Home_NoNote1");
				}
				return string.Format(
					App.language.GetString("Home_note_count_format"), 
					Notes?.Count, 
					Notes?.Count == 1 ? App.language.GetString("Home_note_single") : App.language.GetString("Home_note_prural"));
            }
        }

        public Visibility noNoteSuggestion
        {
            get
            {
                return Notes?.Count < 1 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility IsSelected
        {
            get
            {
                if (selectedIndex < 0)
                {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }

		int sel = -1;
		public int selectedIndex
		{
			get => sel;
			set => Set(ref sel, value);
		}

        public Visibility IsNotSelected
        {
            get
            {
				return selectedIndex > -1 ? Visibility.Collapsed : Visibility.Visible;
            }
        }

		public GridLength showAds
		{
			get => App.Config.useAd ? new GridLength(120,GridUnitType.Pixel) : new GridLength(0);
		}
    }
}