using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JustRemember_.Models
{
    public enum textMode { Char, WhiteSpace }
    public struct Textlist
    {
        public string Text { get; set; }
		public string Commands { get; set; }
		public textMode Mode { get; set; }
        /*

	public struct textlist
	{
		public static readonly textlist Empty = new textlist("", "\0", textMode.Char);
		
		public textlist(string text)
		{
			Text = text;
			Commands = "\0";
			Mode = textMode.Char;
		}

		public textlist(string text, char command)
		{
			Text = text;
			Commands = "";
			Commands += command;
			Mode = textMode.Char;
		}

		public textlist(string text, string commands)
		{
			Text = text;
			Commands = commands;
			Mode = textMode.Char;
		}

		public textlist(string text, string commands,textMode mode)
		{
			Text = text;
			Commands = commands;
			Mode = mode;
		}

		public override string ToString()
		{
			return Text;
		}
	}*/
    }
}
