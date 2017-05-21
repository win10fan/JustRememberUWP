﻿using JustRemember_.Helpers;
using JustRemember_.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using System.ComponentModel;
using Windows.UI.Xaml;
using System.Runtime.CompilerServices;
using Windows.Storage.Pickers;
using JustRemember_.Views;
using Windows.UI.Xaml.Input;
using JustRemember_.Services;
using System.Collections.Generic;

namespace JustRemember_.ViewModels
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
        }

        private void NavigateToMatchWithNote(RoutedEventArgs obj)
        {
            InitializeAndGoToMatch();
        }

        void InitializeAndGoToMatch()
        {
            //Initialize things before throw it to main page
            SessionModel current = new SessionModel();
            AppConfigModel Config = config;
            //Load anything else
            //Initialize class
            current = new SessionModel();
            //Initialize Session
            current.maxChoice = Config.totalChoice;
            current.SelectedNote = Notes[wr.workAround.SelectedIndex];
            current.currentChoice = 0;
            bool? nW = false;
            current.texts = TextList.Extract(current.SelectedNote.Content, out nW);
            current.noteWhiteSpaceMode = nW == true;
            HashSet<string> hashed = new HashSet<string>();
            List<string> chooseAble = new List<string>();
            foreach (TextList t in current.texts)
            {
                hashed.Add(t.actualText);
            }
            chooseAble = new List<string>(hashed);
            var choices = new List<ChoiceSet>();
            //--Generate choice
            Random validChoice = new Random();
            Random choiceTexts = new Random();
            for (int i = 0; i < current.texts.Count; i++)
            {
                ChoiceSet c = new ChoiceSet();
                c.choices = new List<string>();
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
            current.StatInfo = new StatModel();
            current.StatInfo.beginTime = DateTime.Now;
            current.StatInfo.NoteWordCount = current.texts.Count - 1;
            current.StatInfo.configChoice = Config.totalChoice;
            current.StatInfo.choiceInfo = new Dictionary<int, List<bool>>();
            for (int i = 0; i < current.texts.Count; i++)
            {
                List<bool> wrongIfo = new List<bool>();
                for (int c = 0;c != current.maxChoice;c++)
                {
                    wrongIfo.Add(false);
                }
                current.StatInfo.choiceInfo.Add(i, wrongIfo);
            }
            current.StatInfo.isTimeLimited = Config.isLimitTime;
            if (Config.isLimitTime)
            {
                current.StatInfo.totalLimitTime = Config.limitTime;
            }
            current.StatInfo.totalTimespend = TimeSpan.MinValue;
            current.StatInfo.totalLimitTime = Config.limitTime;
            current.StatInfo.setMode = Config.defaultMode;
            current.StatInfo.noteTitle = current.SelectedNote.Title;
            //Initialize selectedChoices
            current.selectedChoices = new List<SelectedChoices>();
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
            //TODO:Kick user to Editor page
        }

        async void ImportTextFile(RoutedEventArgs obj)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
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