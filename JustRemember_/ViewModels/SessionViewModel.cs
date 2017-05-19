using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JustRemember_.Models;
using Windows.System.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using JustRemember_.Helpers;
using Windows.Storage;
using Windows.UI.Xaml;
using System.Windows.Input;
using JustRemember_.Views;

namespace JustRemember_.ViewModels
{
    public class SessionViewModel : INotifyPropertyChanged
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(isPausing)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(totalWrong)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(spendedTime)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(totalLimitTime)));
            //Update choice buttons
            UpdateChoice();
        }

        void UpdateChoice()
        {
            //Update choice visibility
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Choice0Display)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Choice1Display)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Choice2Display)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Choice3Display)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Choice4Display)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(choiceImDisplay)));
            //Update choice text
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Choice0Content)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Choice1Content)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Choice2Content)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Choice3Content)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Choice4Content)));
            if (Config.autoScrollContent)
            {
                view.displayTexts.ChangeView(view.displayTexts.HorizontalOffset, view.displayTexts.ExtentHeight, view.displayTexts.ZoomFactor);
            }
        }

        SessionModel _session;
        public SessionModel current
        {
            get => _session;
            set => Set(ref _session, value);
        }

        AppConfigModel _c = new AppConfigModel();
        public AppConfigModel Config
        {
            get => _c;
            set => Set(ref _c, value);
        }

        public Match view;

        ThreadPoolTimer timer;
        void SetTimer()
        {
            timer = ThreadPoolTimer.CreatePeriodicTimer((t) =>
            {
                //Count timer
                current.StatInfo.totalTimespend.Add(TimeSpan.FromSeconds(1));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(spendedTime)));
            }, TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// This choice use for binding on display which can't be -1
        /// </summary>
        public int currentChoice
        {
            get
            {
                if (current == null)
                {
                    return 0;
                }
                if (current.currentChoice < 0)
                {
                    return 0;
                }
                return current.currentChoice;
            }
            set
            {
                if (current == null) { return; }
                current.currentChoice = value;
            }
        }

        public ICommand chooseChoice0;
        public ICommand chooseChoice1;
        public ICommand chooseChoice2;
        public ICommand chooseChoice3;
        public ICommand chooseChoice4;
        public ICommand chooseCommandChoice;

        public void InitializeCommands()
        {
            chooseChoice0 = new RelayCommand<RoutedEventArgs>(Choose0);
        }

        private void ChooseCommand(RoutedEventArgs obj)
        {
            Choose(-1);
        }

        private void Choose0(RoutedEventArgs obj)
        {
            Choose(0);
        }

        private void Choose1(RoutedEventArgs obj)
        {
            Choose(1);
        }

        private void Choose2(RoutedEventArgs obj)
        {
            Choose(2);
        }

        private void Choose3(RoutedEventArgs obj)
        {
            Choose(3);
        }

        private void Choose4(RoutedEventArgs obj)
        {
            Choose(4);
        }
        
        public void Choose(int choice)
        {
            if (choice < 0)
            {
                //Command choice
                if (current.currentChoice < 0)
                {
                    current.currentChoice = 0;
                }
                else if (current.currentChoice > current.texts.Count - 1)
                {
                    //TODO:Do what depend on setting
                    switch (Config.AfterFinalChoice)
                    {
                        case whenFinalChoice.EndPage:
                            //TODO:Kick user to end page
                            break;
                        case whenFinalChoice.Restart:
                            //TODO:Restart match
                            break;
                        case whenFinalChoice.BackHome:
                            //TODO:Return to main page
                            break;
                    }
                }
                UpdateChoice();
            }
            else
            {
                //Normal choice
                if (choice != current.choices[currentChoice].corrected)
                {
                    //Wrong choice
                    switch (current.StatInfo.setMode)
                    {
                        case matchMode.Easy:
                            //Mark this choice as wrong
                            current.StatInfo.choiceInfo[currentChoice][choice] = true;
                            //Advance time up little bit as wrong choice has been selected
                            current.StatInfo.totalTimespend.Add(TimeSpan.FromSeconds(2));
                            //Update choice text
                            //TODO:As ^ said
                            //Update progress
                            if (current.currentChoice + 1 == current.texts.Count)
                            {
                                //Let UI show final button
                                break;
                            }
                            //Move to next choice
                            current.currentChoice += 1;
                            break;
                        case matchMode.Normal:
                            //Mark selected choice as wrong
                            current.StatInfo.choiceInfo[currentChoice][choice] = true;
                            //Advance time up little bit as wrong choice has been selected
                            current.StatInfo.totalTimespend.Add(TimeSpan.FromSeconds(5));
                            break;
                        case matchMode.Hard:
                            //Reset round
                            //TODO:Reset match
                            break;
                    }
                }
                else if (choice == current.choices[currentChoice].corrected)
                {
                    //Corrent choice
                    //Update display text
                    //TODO:Update display text in MVVM method
                    //Check that choice that has been selected is final
                    if (currentChoice + 1 > current.texts.Count)
                    {
                        //This is the end
                        //Let UI update
                        UpdateChoice();
                        return;
                    }
                    else
                    {
                        //Keep on struggle to the next choice
                        currentChoice += 1;
                        UpdateChoice();
                    }
                }
            }
        }

        public async void InitializeNew(NoteModel selected)
        {
            Config = await SettingsStorageExtensions.ReadAsync<AppConfigModel>(ApplicationData.Current.LocalFolder, "appconfig");
            //Load anything else
            //Initialize class
            current = new SessionModel();
            //Initialize Session
            current.maxChoice = Config.totalChoice;
            current.SelectedNote = selected;
            current.currentChoice = -1;
            bool? nW = false;
            current.texts = TextList.Extract(selected.Content, out nW);
            current.noteWhiteSpaceMode = nW == true;
            HashSet<string> chooseAble = new HashSet<string>();
            foreach (TextList t in current.texts)
            {
                chooseAble.Add(t.actualText);
            }
            List<ChoiceSet> choices = new List<ChoiceSet>();
            //--Generate choice
            Random validChoice = new Random();
            Random choiceTexts = new Random();
            for (int i = 0; i < current.texts.Count - 1; i++)
            {
                ChoiceSet c = new ChoiceSet();
                //Get valid number first
                int valid = validChoice.Next(-100, (current.maxChoice + 1) * 100);
                valid = valid / 100;
                c.corrected = Clamp(valid, 0, current.maxChoice);
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
                        for (int num = current.maxChoice; num > 0; num--)
                        {
                            if (num == c.corrected)
                            {
                                continue;
                            }
                            int range = choiceTexts.Next(0, current.texts.Count - 1);
                            c.choices[num] = current.texts[range].actualText;
                        }
                        break;
                    case matchMode.Normal:
                        //Generate chocie that near current choice within range of 1/4
                        for (int num = current.maxChoice; num > 0; num--)
                        {
                            if (num == c.corrected)
                            {
                                continue;
                            }
                            if ((current.texts.Count - 1) < 20)
                            {
                                int range = choiceTexts.Next(0, current.texts.Count - 1);
                                c.choices[num] = current.texts[range].actualText;
                            }
                            else
                            {
                                int quater = current.texts.Count - 1;
                                int half = current.texts.Count - 1;
                                int quaterPastHalf = half + quater;
                                if (i < quater)
                                {
                                    int range = choiceTexts.Next(0, half);
                                    c.choices[num] = current.texts[range].actualText;
                                }
                                else if (i > quater && i < quaterPastHalf)
                                {
                                    int range = choiceTexts.Next(quater, quaterPastHalf);
                                    c.choices[num] = current.texts[range].actualText;
                                }
                                else if (i > quaterPastHalf)
                                {
                                    int range = choiceTexts.Next(half, current.texts.Count - 1);
                                    c.choices[num] = current.texts[range].actualText;
                                }
                            }
                        }
                        break;
                    case matchMode.Hard:
                        //Generate in range near current with in range 10
                        for (int num = current.maxChoice; num > 0; num--)
                        {
                            if (num == c.corrected)
                            {
                                continue;
                            }
                            int min = i - 10;
                            int max = i + 10;
                            int range = choiceTexts.Next(
                                Clamp(min, 0, current.texts.Count - 1),
                                Clamp(max, 0, current.texts.Count - 1));
                            c.choices[num] = current.texts[range].actualText;
                        }
                        break;
                }
                //Put choice in choice list
                current.choices.Add(c);
            }
            //Initialize stat
            current.StatInfo = new StatModel();
            current.StatInfo.beginTime = DateTime.Now;
            current.StatInfo.NoteWordCount = current.texts.Count - 1;
            current.StatInfo.configChoice = Config.totalChoice;
            current.StatInfo.choiceInfo = new Dictionary<int, List<bool>>();
            List<bool> choiceWrongInfo = new List<bool>();
            for (int i = 0;i < current.maxChoice; i++) { choiceWrongInfo.Add(false); }
            for (int i = 0; i < current.texts.Count - 1; i++)
            {
                current.StatInfo.choiceInfo.Add(i, choiceWrongInfo);
            }
            current.StatInfo.isTimeLimited = Config.isLimitTime;
            if (Config.isLimitTime)
            {
                current.StatInfo.totalLimitTime = Config.limitTime;
            }
            current.StatInfo.totalTimespend = TimeSpan.MinValue;
            current.StatInfo.totalLimitTime = Config.limitTime;
            current.StatInfo.setMode = Config.defaultMode;
            current.StatInfo.noteTitle = selected.Title;
            //UI Initialize
            InitializeCommands();
        }

        public static int Clamp(int value,int min,int max)
        {
            if (value < min) { return min; }
            if (value > max) { return max; }
            return value;
        }
    
        //Choice visibility
        public Visibility choiceImDisplay
        {
            get
            {
                if (current == null || Config == null)
                {
                    return Visibility.Visible;
                }
                else
                {
                    if (current.currentChoice < 0)
                    {
                        return Visibility.Visible;
                    }
                    else if (current.currentChoice > current.choices.Count - 1)
                    {
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Visible;
                    }
                }
            }
        }

        public Visibility Choice0Display
        {
            get
            {
                //Early
                if (current == null || Config == null)
                {
                    return Visibility.Collapsed;
                }
                //Already initialize
                else
                {
                    //Not even first choice
                    if (current.currentChoice < 0)
                    {
                        return Visibility.Collapsed;
                    }
                    //Pass the last choice
                    else if (current.currentChoice > current.choices.Count - 1)
                    {
                        return Visibility.Collapsed;
                    }
                    //Current choice is already choose and it wrong
                    else if (current.StatInfo.choiceInfo[currentChoice][0])//If it true already answer | Which mean hide it
                    {
                        return Visibility.Collapsed;
                    }
                    else
                    {
                        return Visibility.Visible;
                    }
                }
            }
        }

        public Visibility Choice1Display
        {
            get
            {
                //Early
                if (current == null || Config == null)
                {
                    return Visibility.Collapsed;
                }
                //Already initialize
                else
                {
                    //Not even first choice
                    if (current.currentChoice < 0)
                    {
                        return Visibility.Collapsed;
                    }
                    //Pass the last choice
                    else if (current.currentChoice > current.choices.Count - 1)
                    {
                        return Visibility.Collapsed;
                    }
                    //Current choice is already choose and it wrong
                    else if (current.StatInfo.choiceInfo[currentChoice][1])//If it true already answer | Which mean hide it
                    {
                        return Visibility.Collapsed;
                    }
                    else
                    {
                        return Visibility.Visible;
                    }
                }
            }
        }

        public Visibility Choice2Display
        {
            get
            {
                //Early
                if (current == null || Config == null)
                {
                    return Visibility.Collapsed;
                }
                //Already initialize
                else
                {
                    //Not even first choice
                    if (current.currentChoice < 0)
                    {
                        return Visibility.Collapsed;
                    }
                    //Pass the last choice
                    else if (current.currentChoice > current.choices.Count - 1)
                    {
                        return Visibility.Collapsed;
                    }
                    //Current choice is already choose and it wrong
                    else if (current.StatInfo.choiceInfo[currentChoice][2])//If it true already answer | Which mean hide it
                    {
                        return Visibility.Collapsed;
                    }
                    else
                    {
                        return Visibility.Visible;
                    }
                }
            }
        }

        public Visibility Choice3Display
        {
            get
            {
                if (Config.totalChoice < 3)
                {
                    //User need 3 choice
                    return Visibility.Collapsed;
                }
                //Early
                if (current == null || Config == null)
                {
                    return Visibility.Collapsed;
                }
                //Already initialize
                else
                {
                    //Not even first choice
                    if (current.currentChoice < 0)
                    {
                        return Visibility.Collapsed;
                    }
                    //Pass the last choice
                    else if (current.currentChoice > current.choices.Count - 1)
                    {
                        return Visibility.Collapsed;
                    }
                    //Current choice is already choose and it wrong
                    else if (current.StatInfo.choiceInfo[currentChoice][3])//If it true already answer | Which mean hide it
                    {
                        return Visibility.Collapsed;
                    }
                    else
                    {
                        return Visibility.Visible;
                    }
                }
            }
        }

        public Visibility Choice4Display
        {
            get
            {
                //Early
                if (current == null || Config == null)
                {
                    return Visibility.Collapsed;
                }
                //Already initialize
                else
                {
                    if (Config.totalChoice < 5)
                    {
                        //User need 4 choice
                        return Visibility.Collapsed;
                    }
                    //Not even first choice
                    if (current.currentChoice < 0)
                    {
                        return Visibility.Collapsed;
                    }
                    //Pass the last choice
                    else if (current.currentChoice > current.choices.Count - 1)
                    {
                        return Visibility.Collapsed;
                    }
                    //Current choice is already choose and it wrong
                    else if (current.StatInfo.choiceInfo[currentChoice][4])//If it true already answer | Which mean hide it
                    {
                        return Visibility.Collapsed;
                    }
                    else
                    {
                        return Visibility.Visible;
                    }
                }
            }
        }

        //Choice text
        public string Choice0Content
        {
            get
            {
                //Too early
                if (current == null || Config == null)
                {
                    return "";
                }
                else
                {
                    return current.choices[currentChoice].choices[0];
                }
            }
        }

        public string Choice1Content
        {
            get
            {
                //Too early
                if (current == null || Config == null)
                {
                    return "";
                }
                else
                {
                    return current.choices[currentChoice].choices[1];
                }
            }
        }

        public string Choice2Content
        {
            get
            {
                //Too early
                if (current == null || Config == null)
                {
                    return "";
                }
                else
                {
                    return current.choices[currentChoice].choices[2];
                }
            }
        }

        public string Choice3Content
        {
            get
            {
                //Too early
                if (current == null || Config == null)
                {
                    return "";
                }
                else
                {
                    if (Config.totalChoice < 4)
                    {
                        return "";
                    }
                    return current.choices[currentChoice].choices[3];
                }
            }
        }

        public string Choice4Content
        {
            get
            {
                //Too early
                if (current == null || Config == null)
                {
                    return "";
                }
                else
                {
                    if (Config.totalChoice < 5)
                    {
                        return "";
                    }
                    return current.choices[currentChoice].choices[4];
                }
            }
        }

        public async void RestoreSession(SessionModel parameter)
        {
            Config = await SettingsStorageExtensions.ReadAsync<AppConfigModel>(ApplicationData.Current.LocalFolder, "appconfig");
            current = parameter;
            //TODO:Restore session

            //After everything restore
            InitializeCommands();
            return;
        }

        bool _p;
        public bool isPausing
        {
            get
            {
                return _p;
            }
            set
            {
                if (value)
                {
                    timer.Cancel();
                }
                else
                {
                    SetTimer();
                }
                Set(ref _p, value);
            }
        }

        public int totalWrong
        {
            get
            {
                if (current == null)
                {
                    return 0;
                }
                return current.StatInfo.GetTotalWrong();
            }
        }

        public string spendedTime
        {
            get
            {
                if (current == null) { return "--:--"; }
                if (current?.StatInfo?.isTimeLimited == false) { return "--:--"; }
                return $"{current?.StatInfo?.totalTimespend.Minutes:00}:{current.StatInfo?.totalTimespend.Seconds}";
            }
        }

        public string totalLimitTime
        {
            get
            {
                if (current == null) { return "--:--"; }
                if (current?.StatInfo?.isTimeLimited == false) { return "--:--"; }
                return $"{current?.StatInfo?.totalLimitTime.Minutes:00}:{current.StatInfo?.totalLimitTime.Seconds}";
            }
        }
    }
}