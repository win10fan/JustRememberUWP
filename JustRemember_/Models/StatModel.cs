using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace JustRemember_.Models
{
    public class StatModel
    {
        public DateTime beginTime;
        public int NoteWordCount;
        public int configChoice;
        public Dictionary<int, List<bool>> choiceInfo;
        public bool isTimeLimited;
        public TimeSpan totalTimespend;
        public TimeSpan totalLimitTime;
        public matchMode setMode;
        public string noteTitle;
  
        public int GetTotalWrong()
        {
            int wrongCount = 0;
            foreach (var item in choiceInfo)
            {
                foreach (bool vlu in item.Value)
                {
                    if (vlu)
                    {
                        wrongCount += 1;
                    }
                }
            }
            return wrongCount;
        }

  [JsonIgnore]
  public Visibility wasTimeLimited
        {
            get
            {
                if (isTimeLimited) { return Visibility.Visible; }
                return Visibility.Collapsed;
            }
        }

  [JsonIgnore]
  public int timeValue
        {
            get
            {
                if (!isTimeLimited) { return 0; }
                double a = totalTimespend.TotalSeconds;
                double b = totalLimitTime.TotalSeconds;
                double val = a / b;
                return (int)(val * 100);
            }
        }

        public StatModel()
        {
            beginTime = DateTime.Now;
            NoteWordCount = 10;
            configChoice = 3;
            choiceInfo = new Dictionary<int, List<bool>>();
            isTimeLimited = false;
            totalTimespend = TimeSpan.MinValue;
            totalLimitTime = TimeSpan.FromMinutes(5);
            setMode = matchMode.Easy;
            noteTitle = "Untitled";
        }
        /*public class statInfo
    {
        public string dateandTime;
        public int totalWords;
        public int totalChoice;
        public List<int> wrongPerchoice = new List<int>();
        public bool useTimeLimit;
        public float totalTime;
        public float totalLimitTime;
        public challageMode currentMode;
        public string noteTitle;

        public static string Serialize(statInfo info)
        {
            string content = "";
            content += $"{nameof(dateandTime)}={info.dateandTime}{Environment.NewLine}";
            content += $"{nameof(totalWords)}={info.totalWords}{Environment.NewLine}";
            content += $"{nameof(totalChoice)}={info.totalChoice}{Environment.NewLine}";
            content += $"{nameof(wrongPerchoice)}={StringSerializeHelper.ListOfIntToString(info.wrongPerchoice)}{Environment.NewLine}";
            content += $"{nameof(useTimeLimit)}={StringSerializeHelper.BoolToString(info.useTimeLimit)}{Environment.NewLine}";
            content += $"{nameof(totalTime)}={info.totalTime}{Environment.NewLine}";
            content += $"{nameof(totalLimitTime)}={info.totalLimitTime}{Environment.NewLine}";
            content += $"{nameof(currentMode)}={info.currentMode.ToString()}{Environment.NewLine}";
            content += $"{nameof(noteTitle)}={info.noteTitle}";
            return content;
        }

        public static statInfo DeSerialize(string info)
        {
            statInfo value = new statInfo();
            string line;

            StringReader file = new StringReader(info);
            while ((line = file.ReadLine()) != null)
            {
                if (line.StartsWith(nameof(dateandTime)))
                {
                    value.dateandTime = StringSerializeHelper.GetString(line, nameof(dateandTime));
                }
                else if (line.StartsWith(nameof(totalWords)))
                {
                    value.totalWords = StringSerializeHelper.GetInt(line, nameof(totalWords));
                }
                else if (line.StartsWith(nameof(totalChoice)))
                {
                    value.totalChoice = StringSerializeHelper.GetInt(line, nameof(totalChoice));
                }
                else if (line.StartsWith(nameof(wrongPerchoice)))
                {
                    value.wrongPerchoice = StringSerializeHelper.GetListOfInt(line, nameof(wrongPerchoice));
                }
                else if (line.StartsWith(nameof(useTimeLimit)))
                {
                    value.useTimeLimit = StringSerializeHelper.GetBool(line, nameof(useTimeLimit));
                }
                else if (line.StartsWith(nameof(totalTime)))
                {
                    value.totalTime = StringSerializeHelper.GetFloat(line, nameof(totalTime));
                }
                else if (line.StartsWith(nameof(totalLimitTime)))
                {
                    value.totalLimitTime = StringSerializeHelper.GetFloat(line, nameof(totalLimitTime));
                }
                else if (line.StartsWith(nameof(currentMode)))
                {
                    value.currentMode = StringSerializeHelper.GetEnum<challageMode>(line, nameof(currentMode));
                }
                else if (line.StartsWith(nameof(noteTitle)))
                {
                    value.noteTitle = StringSerializeHelper.GetString(line, nameof(noteTitle));
                }
            }
            return value;
        }

        //Read-only values
        public string timeProgress
        {
            get
            {
                if (useTimeLimit)
                {
                    return $"{Utilities.ToStringAsTime(totalTime)}/{Utilities.ToStringAsTime(totalLimitTime)}";
                }
                return "N/A";
            }
        }
        public int timeInPercent
        {
            get
            {
                if (useTimeLimit)
                {
                    float cache = (totalTime / totalLimitTime) * 100;
                    return Convert.ToInt32(cache);
                }
                return 0;
            }
        }
        public string choiceInfo
        {
            get
            {
                return $"{totalWrong} wrong choice out of {totalWords}. Average wrong {wrongPerchoice.Average().ToString("0.00")}";
            }
        }
        public string titleInfo
        {
            get
            {
                return $"{noteTitle} - {dateandTime}";
            }
        }
        public int totalWrong
        {
            get
            {
                int value = 0;
                foreach (int i in wrongPerchoice)
                {
                    value += i;
                }
                return value;
            }
        }
    }*/
    }

    public enum matchMode
    {
        Easy,
        Normal,
        Hard
    }
}
