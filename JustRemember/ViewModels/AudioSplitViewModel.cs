using JustRemember.Helpers;
using JustRemember.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace JustRemember.ViewModels
{
	public class AudioSplitViewModel : Observable
	{
		ObservableCollection<AudioSplitItem> splits;
		public ObservableCollection<AudioSplitItem> Splits { get => splits; set => Set(ref splits, value); }

		NoteModel workwith;
		public NoteModel NoteBase { get => workwith; set { Set(ref workwith, value); OnPropertyChanged(nameof(needNote)); }  }

		//Initialize as async if needed
		public void Initialize(object parameter)
		{
			if (parameter is NoteModel)
			{
				NoteBase = parameter as NoteModel;
			}
			else
			{
				NoteBase = null;
			}
		}

		//Commands
		public ICommand openNote;

		public MessageDialog noExam;
		public MessageDialog notLongEnough;
		public AudioSplitViewModel()
		{
			openNote = new RelayCommand<RoutedEventArgs>(OPENNOTE);

			notLongEnough = new MessageDialog(App.language.GetString("Dialog_Not_long_enough_main"));
			notLongEnough.Commands.Add(new UICommand(App.language.GetString("Match_dialog_ok")));

			noExam = new MessageDialog("Text file with custom mode is not allowed");
			noExam.Commands.Add(new UICommand(App.language.GetString("Match_dialog_ok")));
		}

		private async void OPENNOTE(RoutedEventArgs obj)
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
				IBuffer buffer = await FileIO.ReadBufferAsync(file);
				DataReader reader = DataReader.FromBuffer(buffer);
				byte[] fileContent = new byte[reader.UnconsumedBufferLength];
				reader.ReadBytes(fileContent);
				string content = Encoding.UTF8.GetString(fileContent, 0, fileContent.Length);
				SessionModel test = SessionModel.generate(new NoteModel { Title = "Test", Content = content });
				if (test == null)
				{
					await notLongEnough.ShowAsync();
					return;
				}
				else
				{
					foreach (var it in test.texts)
					{
						Splits.Add(new AudioSplitItem(it.actualText));
					}
				}
				StorageFolder folder = ApplicationData.Current.RoamingFolder;
				var noteFolder = await folder.GetFolderAsync("Notes");
				await file.CopyAsync(noteFolder);
				NoteBase = new NoteModel() { Title = file.DisplayName, Content = content };
			}
		}

		//Binding properties
		public Visibility needNote
		{
			get => NoteBase == null ? Visibility.Visible : Visibility.Collapsed;
		}
	}
}
