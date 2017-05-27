using JustRemember_.Helpers;
using JustRemember_.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JustRemember_.ViewModels
{
    public class SavedSessionViewModel : INotifyPropertyChanged
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
        }

        SavedSessionModel _md;
        public SavedSessionModel model
        {
            get { return _md; }
            set { _md = value; }
        }
        public async void Initialize()
        {
            model = new SavedSessionModel();
            model.save = await model.Load();
        }

        public string SessionCount
        {
            get
            {
                if (model?.save?.Count < 1)
                {
                    return "No session saved";
                }
                string s = model?.save?.Count > 1 ? "s" : "";
                return $"{model?.save?.Count} saved session{s}";
            }
        }
    }
}
