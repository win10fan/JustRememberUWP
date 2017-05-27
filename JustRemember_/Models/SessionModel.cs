using JustRemember_.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace JustRemember_.Models
{
    public class SessionModel
    {
        public NoteModel SelectedNote { get; set; }
        public bool noteWhiteSpaceMode { get; set; } //True = Begin with white space on every items | false = Begin with text on all items
        public StatModel StatInfo { get; set; }
        public int currentChoice { get; set; }
        public ObservableCollection<TextList> texts { get; set; } = new ObservableCollection<TextList>();
        public ObservableCollection<ChoiceSet> choices { get; set; } = new ObservableCollection<ChoiceSet>();
        public int maxChoice { get; set; }
        public ObservableCollection<SelectedChoices> selectedChoices { get; set; } = new ObservableCollection<SelectedChoices>();
        public bool isNew { get; set; }
        public string GeneratedName
        {
            get
            {
                var time = StatInfo.beginTime;
                return $"{time.Year}{time.Month}{time.Date}-{time.Hour}{time.Minute}{time.Second}-{StatInfo.noteTitle}";
            }
        }

        public int totalChoices
        {
            get
            {
                return texts.Count;
            }
        }

        public int choiceProgress
        {
            get
            {
                float a = (float)currentChoice / (float)totalChoices;
                return (int)(a * 100);
            }
        }

        public SessionModel()
        {
            SelectedNote = new NoteModel();
            noteWhiteSpaceMode = false;
            StatInfo = new StatModel();
            currentChoice = 0;
            texts = new ObservableCollection<TextList>();
            choices = new ObservableCollection<ChoiceSet>();
            maxChoice = 3;
            selectedChoices = new ObservableCollection<SelectedChoices>();
        }
    }

    /// <summary>
    /// Contain snip of text from note content | whice later use to generate choice
    /// </summary>
    public class TextList
    {
        public string actualText { get; set; }//This one has no white space
        public string text { get; set; }
        public string whitespace { get; set; }
        public bool isWhitespace { get; set; }
        
        public TextList()
        {
            actualText = "";
            text = "";
            whitespace = "";
            isWhitespace = false;
        }

        public static ObservableCollection<PreDeterminiteText> ExtractContent(string content)
        {
            ObservableCollection<PreDeterminiteText> item = new ObservableCollection<PreDeterminiteText>();
            if (content.Length < 1) { return item; }
            foreach (char c in content)
            {
                PreDeterminiteText pd = new PreDeterminiteText();
                if (char.IsWhiteSpace(c))
                {
                    pd.piece = c.ToString();
                    pd.isWhiteSpace = true;
                    item.Add(pd);
                }
                else
                {
                    pd.piece = c.ToString();
                    pd.isWhiteSpace = false;
                    item.Add(pd);
                }
            }
            return item;
        }

        public static ObservableCollection<TextList> Extract(string content, out bool? mode)
        {
            ObservableCollection<PreDeterminiteText> basicSort = ExtractContent(content);
            PreDeterminiteText prev = null;
            mode = null; //True = Begin with white space on every items | false = Begin with text on all items
            int lastID = 0;
            foreach (var pd in basicSort)
            {
                if (mode == null)
                {
                    mode = pd.isWhiteSpace;
                }
                if (prev == null)
                {
                    //First item
                    pd.groupID = lastID;
                    prev = pd;
                    continue;
                }
                //Every items
                if (mode == true)
                {
                    if (prev.isWhiteSpace && pd.isWhiteSpace)
                    {
                        pd.groupID = lastID;
                        prev = pd;
                        continue;
                    }
                    if (prev.isWhiteSpace && !pd.isWhiteSpace)
                    {
                        pd.groupID = lastID;
                        prev = pd;
                        continue;
                    }
                    if (!prev.isWhiteSpace && !pd.isWhiteSpace)
                    {
                        pd.groupID = lastID;
                        prev = pd;
                        continue;
                    }
                    if (!prev.isWhiteSpace && pd.isWhiteSpace)
                    {
                        lastID += 1;
                        pd.groupID = lastID;
                        prev = pd;
                        continue;
                    }
                }
                if (mode == false)
                {
                    if (!prev.isWhiteSpace && !pd.isWhiteSpace)
                    {
                        pd.groupID = lastID;
                        prev = pd;
                        continue;
                    }
                    if (!prev.isWhiteSpace && pd.isWhiteSpace)
                    {
                        //Get some white space after
                        pd.groupID = lastID;
                        prev = pd;
                        continue;
                    }
                    if (prev.isWhiteSpace && pd.isWhiteSpace)
                    {
                        //Still white space | keep adding
                        pd.groupID = lastID;
                        prev = pd;
                        continue;
                    }
                    if (prev.isWhiteSpace && !pd.isWhiteSpace)
                    {
                        //Start new character | change group ID
                        lastID += 1;
                        pd.groupID = lastID;
                        prev = pd;
                        continue;
                    }
                }
            }
            ObservableCollection<TextList> list = new ObservableCollection<TextList>();
            for (int i= 0;i < lastID + 1; i++) { list.Add(new TextList()); }
            foreach (var pd in basicSort)
            {
                list[pd.groupID].text += pd.piece;
                if (!pd.isWhiteSpace)
                {
                    list[pd.groupID].actualText += pd.piece;
                }
            }
            return list;
        }
    }

    public class PreDeterminiteText
    {
        public string piece;
        public bool isWhiteSpace;
        public int groupID;

        public PreDeterminiteText()
        {
            piece = "";
            isWhiteSpace = false;
            groupID = 0;
        }
    }

    /// <summary>
    /// Choice that generate on this session
    /// </summary>
    public class ChoiceSet
    {
        public List<string> choices;
        public int corrected;

        public ChoiceSet()
        {
            choices = new List<string>();
            corrected = 0;
        }
    }

    public class SelectedChoices
    {
        public bool isItWrong { get; set; }
        public string finalText { get; set; } //This already have white space combine

        public SelectedChoices()
        {
            isItWrong = false;
            finalText = "";
        }

        public SolidColorBrush mark
        {
            get
            {
                if (isItWrong)
                {
                    return new SolidColorBrush(Colors.Red);
                }
                if (Application.Current.RequestedTheme == ApplicationTheme.Light)
                {
                    return new SolidColorBrush(Colors.Black);
                }
                return new SolidColorBrush(Colors.White);
            }
        }
    }
}
