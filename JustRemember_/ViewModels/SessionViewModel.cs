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
        
        public async void InitializeNew(NoteModel selected)
        {
            Config = await SettingsStorageExtensions.ReadAsync<AppConfigModel>(ApplicationData.Current.LocalFolder, "appconfig");
            //Load anything else
            current = new SessionModel();
            current.StatInfo = new StatModel();
            current.StatInfo.beginTime = DateTime.Now;
            current.StatInfo.noteTitle = selected.Title;
            current.StatInfo.setMode = Config.defaultMode;
            current.StatInfo.isTimeLimited = Config.isLimitTime;
            if (Config.isLimitTime)
            {
                current.StatInfo.totalLimitTime = Config.limitTime;
            }
            current.maxChoice = Config.totalChoice;
            current.SelectedNote = selected;
            current.currentChoice = -1;
            current.texts = TextList.Extract(selected.Content);
        }
        
        public async void RestoreSession(SessionModel parameter)
        {
            Config = await SettingsStorageExtensions.ReadAsync<AppConfigModel>(ApplicationData.Current.LocalFolder, "appconfig");
            current = parameter;
            //Restore session
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
                return current.StatInfo.wrongPerChoice.Sum();
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
