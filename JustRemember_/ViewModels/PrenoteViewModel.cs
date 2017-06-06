using JustRemember.Helpers;
using JustRemember.Models;
using JustRemember.Services;
using JustRemember.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Xaml;
using static JustRemember.Services.PrenoteService;

namespace JustRemember.ViewModels
{
	public class PrenoteViewModel : INotifyPropertyChanged
	{
		public PrenoteView v;
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
		}

		public StorageFolder basePath = ApplicationData.Current.LocalFolder;
		public AppConfigModel config;

		string _pt;
		public string Path
		{
			get => _pt;
			set
			{
				Set(ref _pt, value);
				OnPropertyChanged("PathsSplit");
			}
		}

		public ObservableCollection<PathDir> PathsSplit
		{
			get
			{
				var pth = Path.GetBreadcrumbPath().Reverse();
				var val = new ObservableCollection<PathDir>(pth);
				return val;
			}
		}

		public async void Navigate(string path)
		{
			basePath = await StorageFolder.GetFolderFromPathAsync(path);
			Path = basePath.Path;
			notes = await PrenoteModel.GetChild(basePath);
			isMoreUp = new DirectoryInfo(basePath.Path).Name != "Prenote";
			OnPropertyChanged(nameof(notes));
		}

		public ObservableCollection<PrenoteModel> notes;

		public ICommand navTo;
		public ICommand navUp;

		public async void Initialize()
		{
			basePath = (StorageFolder)await basePath.TryGetItemAsync("Prenote");
			if (basePath == null)
			{
				basePath = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Prenote");
			}
			Navigate(basePath.Path);
			config = await AppConfigModel.Load2();
			//Command
			navTo = new RelayCommand<RoutedEventArgs>(NAVTO);
			navUp = new RelayCommand<RoutedEventArgs>(NAVUP);
		}

		bool _canUp;
		public bool isMoreUp
		{
			get => _canUp;
			set => Set(ref _canUp, value);
		}

		public void NavToAccordingToWhatYouBeenClickOnPathList(int selected)
		{
			Navigate(PathsSplit[selected].FullPath);
		}

		private void NAVUP(RoutedEventArgs obj)
		{
			DirectoryInfo dir = new DirectoryInfo(basePath.Path);
			if (dir.Name == "Prenote")
			{
				NavigationService.GoBack();
				return;
			}
			Navigate(dir.Parent.FullName);
		}

		private async void NAVTO(RoutedEventArgs obj)
		{
			if (v.FileList.SelectedIndex > -1)
			{
				if (!notes[v.FileList.SelectedIndex].isFile)
					Navigate(notes[v.FileList.SelectedIndex].Fullpath);
				else
					InitializeAndGoToMatch(await NoteModel.GetOneNoteButNotMicrosoftOneNoteButNoteWithParticularPath(notes[v.FileList.SelectedIndex].Fullpath));
			}
		}

		void InitializeAndGoToMatch(NoteModel datNote)
		{
			//Initialize things before throw it to main page 
			//Copy config
			AppConfigModel Config = config;
			//Initialize session
			SessionModel current = new SessionModel()
			{
				selectedChoices = new ObservableCollection<SelectedChoices>(),
				isNew = true,
				maxChoice = Config.totalChoice,
				currentChoice = 0
			};
			//Load anything else
			//Initialize Session
			current.SelectedNote = datNote;
			current.texts = TextList.Extract(current.SelectedNote.Content, out bool? nW);
			current.noteWhiteSpaceMode = nW == true;
			HashSet<string> hashed = new HashSet<string>();
			ObservableCollection<string> chooseAble = new ObservableCollection<string>();
			foreach (TextList t in current.texts)
			{
				hashed.Add(t.actualText);
			}
			chooseAble = new ObservableCollection<string>(hashed);
			var choices = new ObservableCollection<ChoiceSet>();
			//--Generate choice
			Random validChoice = new Random();
			Random choiceTexts = new Random();
			for (int i = 0; i < current.texts.Count; i++)
			{
				ChoiceSet c = new ChoiceSet()
				{
					choices = new List<string>(),

				};
				//Get valid number first
				int valid = validChoice.Next(0, (current.maxChoice) * 100);
				valid = valid / 100;
				c.corrected = valid.Clamp(0, current.maxChoice - 1);
				//Generate choice text depend on difficult
				for (int cnt = 0; cnt < current.maxChoice; cnt++)
				{
					if (cnt == c.corrected)
					{
						c.choices.Add(current.texts[i].actualText);
					}
					c.choices.Add("");
				}
				switch (Config.defaultMode)
				{
					case matchMode.Easy:
						//Generate random text in a whole array
						for (int num = current.maxChoice; num > -1; num--)
						{
							if (num == c.corrected)
							{
								continue;
							}
							int range = choiceTexts.Next(0, chooseAble.Count - 2);
							List<string> cache = new List<string>(chooseAble);
							cache.Remove(current.texts[i].actualText);
							c.choices[num] = cache[range];
						}
						break;
					case matchMode.Normal:
						//Generate chocie that near current choice within range of 1/4
						for (int num = current.maxChoice; num > -1; num--)
						{
							if (num == c.corrected)
							{
								continue;
							}
							if ((chooseAble.Count - 1) < 20)
							{
								int range = choiceTexts.Next(0, chooseAble.Count - 1);
								c.choices[num] = chooseAble[range];
							}
							else
							{
								int quater = chooseAble.Count - 1;
								int half = chooseAble.Count - 1;
								int quaterPastHalf = half + quater;
								if (i < quater)
								{
									int range = choiceTexts.Next(0, half);
									List<string> cache = new List<string>(chooseAble);
									cache.Remove(current.texts[i].actualText);
									c.choices[num] = cache[range];
								}
								else if (i > quater && i < quaterPastHalf)
								{
									int range = choiceTexts.Next(quater, quaterPastHalf);
									List<string> cache = new List<string>(chooseAble);
									cache.Remove(current.texts[i].actualText);
									c.choices[num] = cache[range];
								}
								else if (i > quaterPastHalf)
								{
									int range = choiceTexts.Next(half, chooseAble.Count - 1);
									List<string> cache = new List<string>(chooseAble);
									cache.Remove(current.texts[i].actualText);
									c.choices[num] = cache[range];
								}
							}
						}
						break;
					case matchMode.Hard:
						//Generate in range near current with in range 10
						for (int num = current.maxChoice; num > -1; num--)
						{
							if (num == c.corrected)
							{
								continue;
							}
							int min = i - 10;
							int max = i + 10;
							int range = choiceTexts.Next(
								min.Clamp(0, chooseAble.Count - 1),
								max.Clamp(0, chooseAble.Count - 1));
							List<string> cache = new List<string>(chooseAble);
							cache.Remove(current.texts[i].actualText);
							c.choices[num] = cache[range];
						}
						break;
				}
				//Put choice in choice list
				choices.Add(c);
			}
			current.choices = choices;
			//Initialize stat
			current.StatInfo = new StatModel()
			{
				beginTime = DateTime.Now,
				NoteWordCount = current.texts.Count - 1,
				configChoice = Config.totalChoice,
				choiceInfo = new Dictionary<int, List<bool>>(),
				setMode = Config.defaultMode,
				noteTitle = current.SelectedNote.Title,
				totalTimespend = TimeSpan.MinValue,
				isTimeLimited = Config.isLimitTime,
				totalLimitTime = Config.isLimitTime ? Config.limitTime : TimeSpan.MinValue
			};
			for (int i = 0; i < current.texts.Count; i++)
			{
				List<bool> wrongIfo = new List<bool>();
				for (int c = 0; c != current.maxChoice; c++)
				{
					wrongIfo.Add(false);
				}
				current.StatInfo.choiceInfo.Add(i, wrongIfo);
			}
			//Goto Match page
			NavigationService.Navigate<Match>(current);
		}
	}
}
