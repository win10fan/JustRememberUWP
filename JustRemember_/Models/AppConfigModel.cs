using JustRemember_.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace JustRemember_.Models
{
   public  class AppConfigModel
    {
        public int language { get; set; }
        public bool isItLightTheme { get; set; }
        public bool isLimitTime { get; set; }
        public bool obfuscateWrongText { get; set; }
        public matchMode defaultMode { get; set; }
        public TimeSpan limitTime { get; set; }
        public int totalChoice { get; set; }
        public int displayTextSize { get; set; }
        public int defaultSeed { get; set; }
        public bool autoScrollContent { get; set; }
        public whenFinalChoice AfterFinalChoice { get; set; }
        /// <summary>
        /// If "AfterFinalChoice" is not "EndPage" always save stat or discard
        /// </summary>
        public bool saveStatAfterEnd { get; set; }
        public bool hintAtFirstchoice { get; set; }
        public choiceDisplayMode choiceStyle { get; set; }
        
        public async Task<AppConfigModel> Load()
        {
            return await SettingsStorageExtensions.ReadAsync<AppConfigModel>(ApplicationData.Current.LocalFolder, "appconfig");
        }

        public async void Save()
        {
            await SettingsStorageExtensions.SaveAsync(ApplicationData.Current.LocalFolder, "appconfig", this);
        }

        public AppConfigModel()
        {
            //Provide default setting
            language = 0;
            isItLightTheme = false;
            isLimitTime = false;
            obfuscateWrongText = false;
            defaultMode = matchMode.Easy;
            limitTime = TimeSpan.FromMinutes(5);
            totalChoice = 3;
            displayTextSize = 18;
            defaultSeed = -1;
            autoScrollContent = true;
            AfterFinalChoice = whenFinalChoice.EndPage;
            saveStatAfterEnd = true;
            hintAtFirstchoice = true;
        }
    }

    public enum whenFinalChoice
    {
        EndPage,
        Restart,
        BackHome
    }

    public enum choiceDisplayMode
    {
        Center,
        Bottom,
        Write
    }
}
