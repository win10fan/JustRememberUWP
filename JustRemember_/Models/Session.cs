using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JustRemember_.Models
{
    public class Session
    {
        public Stat current;
        public int currentAt;
        public int lastCorrect;
        public List<Textlist> lastTextList = new List<Textlist>();
        public List<string> choiceList = new List<string>();
        public double progressFillAmount;
        public List<string> lastChoices = new List<string>();
        public List<bool> lastChoicesHide = new List<bool>();
        public selectMode choiceType;
        //public string displayText;
        public Note nowPlayed;
        //TODO:Consider get a better choice loader
        public Matchmode lastMode;
    }

    public enum Matchmode { begin, normal, end }

    public enum selectMode
    {
        styleA,
        styleB,
        styleC
    }
}
