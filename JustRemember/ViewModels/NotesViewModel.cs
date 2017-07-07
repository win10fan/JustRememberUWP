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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JustRemember.ViewModels
{
    public class NotesViewModel : INotifyPropertyChanged
    {
        public MainPage wr;
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NoteCount)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(noNoteSuggestion)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsNotSelected)));
        }
		
        ObservableCollection<NoteModel> _notes;

        public ObservableCollection<NoteModel> Notes
        {
            get { return _notes; }
            set
            {
                Set(ref _notes, value);
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
			//Other
			saver = new DispatcherTimer()
			{
				Interval = TimeSpan.FromSeconds(5)
			};
			saver.Tick += Saver_Tick;
			saver.Start();
		}

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

        void InitializeAndGoToMatch()
        {
			SessionModel current = SessionModel.generate(Notes[wr.WAR.SelectedIndex]);
            //Goto Match page
            NavigationService.Navigate<Match>(current);
        }

        private void DeSelectNote(RoutedEventArgs obj)
        {
            wr.workAround.SelectedIndex = -1;
        }

        async void DeleteSelectedItem(RoutedEventArgs obj)
        {
            if (wr.WAR.SelectedItem == null)
            {
                return;
            }
            await NoteModel.DeleteNote(Notes[wr.WAR.SelectedIndex].Title);
            Notes = await NoteModel.GetNotesAsync();
        }

        private void EditSelectedItem(RoutedEventArgs obj)
        {
			if (wr.WAR.SelectedIndex < 0)
			{
				return;
			}
			NavigationService.Navigate<NoteEditorView>(Notes[wr.WAR.SelectedIndex]);
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
            if (wr.workAround.SelectedIndex == -1)
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
					Notes?.Count == 1 ? App.language.GetString("Home_note_single") : App.language.GetString("Home_note_prupal"));
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
                if (wr.WAR.SelectedIndex < 0)
                {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }

        public Visibility IsNotSelected
        {
            get
            {
                if (wr.WAR.SelectedIndex > -1)
                {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }
    }
}