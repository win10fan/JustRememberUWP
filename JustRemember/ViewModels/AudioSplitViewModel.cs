using JustRemember.Helpers;
using JustRemember.Models;
using JustRemember.Services;
using JustRemember.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace JustRemember.ViewModels
{
	public class AudioSplitViewModel : Observable
	{
		public AudioDescription view;

		ObservableCollection<AudioSplitItem> splits;
		public ObservableCollection<AudioSplitItem> Splits { get => splits; set => Set(ref splits, value); }

		NoteModel workwith;
		public NoteModel NoteBase { get => workwith; set { Set(ref workwith, value); OnPropertyChanged(nameof(needNote)); }  }

		//Initialize as async if needed
		public async void Initialize(object parameter)
		{
			view = NavigationService.Frame.Content as AudioDescription;

			if (parameter is NoteModel)
			{
				NoteBase = parameter as NoteModel;
				if (NoteBase.hasDesc)
				{
					AudioDescriptor ador = await Json.ToObjectAsync<AudioDescriptor>(await FileIO.ReadTextAsync(await NoteBase.GetDescription()));
					Splits = ador.Splits;
					audio = await (await ApplicationData.Current.RoamingFolder.GetFolderAsync("Description")).GetFileAsync(ador.audioName);
				}
			}
			else
			{
				NoteBase = null;
			}
			if (NoteBase != null && !NoteBase.hasDesc)
			{
				Splits = new ObservableCollection<AudioSplitItem>();
				SessionModel test = SessionModel.generate(NoteBase);
				if (test == null)
				{
					await notLongEnough.ShowAsync();
					return;
				}
				else
				{
					OnPropertyChanged(nameof(needNote));
					foreach (var it in test.texts)
					{
						Splits.Add(new AudioSplitItem(it.actualText));
						await Task.Delay(50);
					}
				}
			}
		}

		//Commands
		public ICommand openNote{ get; private set; }
		public ICommand openAudio{ get; private set; }
		public ICommand goBack{ get; private set; }
		public ICommand saveChange{ get; private set; }

		public MessageDialog noExam;
		public MessageDialog notLongEnough;
		public MessageDialog saveComplete;
		public AudioSplitViewModel()
		{
			openNote = new RelayCommand<RoutedEventArgs>(OPENNOTE);
			openAudio = new RelayCommand<RoutedEventArgs>(OPENAUDIO);
			goBack = new RelayCommand<RoutedEventArgs>(GOBACK);
			saveChange = new RelayCommand<RoutedEventArgs>(SAVECHANGE);

			notLongEnough = new MessageDialog(App.language.GetString("Dialog_Not_long_enough_main"));
			notLongEnough.Commands.Add(new UICommand(App.language.GetString("Match_dialog_ok")));

			noExam = new MessageDialog(App.language.GetString("QA_Unsupport"));
			noExam.Commands.Add(new UICommand(App.language.GetString("Match_dialog_ok")));

			saveComplete = new MessageDialog(App.language.GetString("Audio_Copy_Complete"));
			saveComplete.Commands.Add(new UICommand(App.language.GetString("Match_dialog_ok")));
		}

		bool iss = false;
		public bool isSaving { get => iss; set { Set(ref iss, value); OnPropertyChanged(nameof(isSavingIcon)); } }

		public Symbol isSavingIcon
		{
			get => isSaving ? Symbol.Sync : Symbol.Save;
		}
		private async void SAVECHANGE(RoutedEventArgs obj)
		{
			if (isSaving) { return; }
			isSaving = true;
			view.RotateIcon.Begin();
			string filename = NoteBase.Title;
			var notefolder = await ApplicationData.Current.RoamingFolder.TryGetItemAsync("Description") as StorageFolder;
			if (notefolder == null)
			{
				notefolder = await ApplicationData.Current.RoamingFolder.CreateFolderAsync("Description");
			}
			//Copy audio file
			var audioFile = await notefolder.TryGetItemAsync($"{filename}{audioExt}") as StorageFile;
			if (audioFile == null && audio != null)
			{
				await audio.CopyAsync(notefolder, $"{filename}{audioExt}");
			}
			//Save split config
			//Check file status
			var spf = await notefolder.TryGetItemAsync($"{filename}.mde") as StorageFile;
			if (spf == null)
			{
				spf = await notefolder.CreateFileAsync($"{filename}.mde");
			}
			else
			{
				await spf.DeleteAsync();
				spf = await notefolder.CreateFileAsync($"{filename}.mde");
			}
			AudioDescriptor desc = new AudioDescriptor(Splits, $"{filename}{audioExt}");
			string ifo = await Json.StringifyAsync(desc);
			await FileIO.WriteTextAsync(spf, ifo);
			view.RotateIcon.Stop();
			view.AngleIcon.Rotation = 0;
			isSaving = false;
			await saveComplete.ShowAsync();
		}
		StorageFile audio;
		string audioExt;

		private void GOBACK(RoutedEventArgs obj)
		{
			NavigationService.GoBack();
		}

		private async void OPENAUDIO(RoutedEventArgs obj)
		{
			FileOpenPicker openPicker = new FileOpenPicker()
			{
				ViewMode = PickerViewMode.List,
				SuggestedStartLocation = PickerLocationId.MusicLibrary
			};
			openPicker.FileTypeFilter.Add(".mp3");
			openPicker.FileTypeFilter.Add(".wav");

			audio = await openPicker.PickSingleFileAsync();
			if (audio != null)
			{
				var stream = await audio.OpenAsync(FileAccessMode.Read);
				view.mdControl.SetSource(stream, audio.ContentType);
				audioExt = Path.GetExtension(audio.Path);
			}
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
				NoteModel datNote = new NoteModel() { Title = file.DisplayName, Content = content };
				SessionModel test = SessionModel.generate(datNote);
				await NoteModel.SaveNote(datNote);
				NoteBase = new NoteModel() { Title = file.DisplayName, Content = content };
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
						await Task.Delay(50);
					}
				}
			}
		}

		//Binding properties
		public Visibility needNote
		{
			get => NoteBase == null ? Visibility.Visible : Visibility.Collapsed;
		}
	}
}