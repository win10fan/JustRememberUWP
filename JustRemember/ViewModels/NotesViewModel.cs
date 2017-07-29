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
using System.Text;
using Windows.Storage.Streams;
using System.IO.Compression;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

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
		public ICommand SendToAudioSplit { get; private set; }
		public ICommand ExportFiles { get; private set; }

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
			SendToAudioSplit = new RelayCommand<RoutedEventArgs>(SENDTOAUDIOSPLIT);
			ExportFiles = new RelayCommand<RoutedEventArgs>(EXPORTFILES);
			//Dialog
			notLongEnough = new MessageDialog(App.language.GetString("Dialog_Not_long_enough_main"));
			notLongEnough.Commands.Add(new UICommand(App.language.GetString("Match_dialog_ok")));
		}

		private async void EXPORTFILES(RoutedEventArgs obj)
		{
			FileSavePicker savePicker = new FileSavePicker()
			{
				SuggestedStartLocation = PickerLocationId.DocumentsLibrary
			};
			if (Notes[selectedIndex].hasDesc)
			{				
				savePicker.FileTypeChoices.Add("Memo archieve", new List<string>() { ".mar" });
				savePicker.SuggestedFileName = $"{Notes[selectedIndex].Title}.zip";

				var file = await savePicker.PickSaveFileAsync();
				if (file != null)
				{
					var fol = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(Notes[selectedIndex].Title);
					var nf = await fol.CreateFileAsync($"{Notes[selectedIndex].Title}.txt");
					await FileIO.WriteTextAsync(nf, Notes[selectedIndex].Content);
					//TODO:Finish import zip function
					var descMain = await ApplicationData.Current.RoamingFolder.TryGetItemAsync("Description") as StorageFolder;
					var desc = await descMain.GetFileAsync($"{Notes[selectedIndex].Title}.mde");
					var descor = await Json.ToObjectAsync<AudioDescriptor>(await FileIO.ReadTextAsync(desc));
					var descAudi = await descMain.GetFileAsync(descor.audioName);
					await desc.CopyAsync(fol);
					await descAudi.CopyAsync(fol);
					await Task.Run(() =>
					{
						ZipFile.CreateFromDirectory(fol.Path, $"{ApplicationData.Current.LocalFolder.Path}\\{file.Name}");
					});
					await fol.DeleteAsync();
					var arc = await ApplicationData.Current.LocalFolder.GetFileAsync(file.Name);
					await arc.CopyAndReplaceAsync(file);
					await arc.DeleteAsync();
				}
			}
			else
			{
				savePicker.FileTypeChoices.Add("Text file", new List<string>() { ".txt" });
				savePicker.SuggestedFileName = $"{Notes[selectedIndex].Title}.txt";
				var file = await savePicker.PickSaveFileAsync();
				if (file != null)
				{
					var fol = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(Notes[selectedIndex].Title);
					await FileIO.WriteTextAsync(file, Notes[selectedIndex].Content);
				}
			}
		}

		private void SENDTOAUDIOSPLIT(RoutedEventArgs obj)
		{
			if (selectedIndex > -1)
			{
				NavigationService.Navigate<AudioDescription>(Notes[selectedIndex]);
				return;
			}
			NavigationService.Navigate<AudioDescription>();
		}

		public bool isItQA
		{
			get
			{
				if (Notes == null) { return false; }
				if (Notes?.Count < 1) { return false; }
				return Notes?[selectedIndex > -1 ? selectedIndex : 0].Mode == noteMode.Question;
			}
		}

		public Visibility isThisQA
		{
			get => isItQA ? Visibility.Visible : Visibility.Collapsed;
		}

		public Visibility isThisNotQA
		{
			get => !isItQA ? Visibility.Visible : Visibility.Collapsed;
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
			set
			{
				Set(ref editWithDiag, value);
				OnPropertyChanged(nameof(isThisQA));
				OnPropertyChanged(nameof(isThisNotQA));
			}
		}

		private void EDITSELECTOR(RoutedEventArgs obj)
		{
			EditWithPopup = true;
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
			openPicker.FileTypeFilter.Add(".mar");

			StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
				if (file.Name.EndsWith(".txt"))
				{
					IBuffer buffer = await FileIO.ReadBufferAsync(file);
					DataReader reader = DataReader.FromBuffer(buffer);
					byte[] fileContent = new byte[reader.UnconsumedBufferLength];
					reader.ReadBytes(fileContent);
					string content = Encoding.UTF8.GetString(fileContent, 0, fileContent.Length);
					NoteModel datNote = new NoteModel(file.DisplayName, content);
					SessionModel test = SessionModel.generate(datNote);
					if (test == null)
					{
						await notLongEnough.ShowAsync();
						return;
					}
					await NoteModel.SaveNote(datNote);
					Notes = await NoteModel.GetNotesAsync();
				}
				else if (file.Name.EndsWith(".mar"))
				{
					//Memo archive
					StorageFolder target = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(file.DisplayName);
					Stream s = await file.OpenStreamForReadAsync();
					ZipArchive arc = new ZipArchive(s);
					arc.ExtractToDirectory(target.Path);
					//Gather files
					var nfd = await ApplicationData.Current.RoamingFolder.TryGetItemAsync("Notes") as StorageFolder;
					var dfd = await ApplicationData.Current.RoamingFolder.TryGetItemAsync("Description") as StorageFolder;
					foreach (var content in await target.GetFilesAsync())
					{
						if (content.Name.EndsWith(".txt"))
						{
							if (nfd == null)
							{
								nfd = await ApplicationData.Current.RoamingFolder.CreateFolderAsync("Notes");
							}
							await content.MoveAsync(nfd);
						}
						else
						{
							if (dfd == null)
							{
								dfd = await ApplicationData.Current.RoamingFolder.CreateFolderAsync("Description");
							}
							await content.MoveAsync(dfd);
						}
					}
					await target.DeleteAsync();
					ReloadingListAsync(null);
				}
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