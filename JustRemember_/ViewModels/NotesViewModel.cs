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

		DispatcherTimer tck;
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
			tck = new DispatcherTimer()
			{
				Interval = TimeSpan.FromMilliseconds(500),
			};
			tck.Tick += Tck_TickAsync;
			tck.Start();
        }

		DateTime tilStop;
		bool getStop;
		private async void Tck_TickAsync(object sender, object e)
		{
			if (config == null)
			{
				App.Config = await SettingsStorageExtensions.ReadAsync<AppConfigModel>(ApplicationData.Current.LocalFolder, "appconfig");
				config = App.Config;
				return;
			}
			OnPropertyChanged(nameof(isReady));
			if (config != null)
			{
				if (!getStop)
				{
					tilStop = DateTime.Now + TimeSpan.FromSeconds(1);
					getStop = true;
					return;
				}
				if (DateTime.Now > tilStop)
				{
					OnPropertyChanged(nameof(isReady));
					tck.Stop();
				}
			}
		}

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

        AppConfigModel config;
        public async void Initialize()
        {
            Notes = await NoteModel.GetNotesAsync();
            config = await SettingsStorageExtensions.ReadAsync<AppConfigModel>(ApplicationData.Current.LocalFolder, "appconfig");
        }

        public string NoteCount
        {
            get
            {
                if (Notes?.Count < 1)
                {
                    return "Note empty";
                }
                string s = Notes?.Count > 1 ? "s" : "";
                return $"{Notes?.Count} note{s}";
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