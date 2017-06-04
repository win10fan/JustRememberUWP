using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace JustRemember_.Models
{
	public class PrenoteModel
	{
		public string Fullpath { get; set; } = "";
		public string Name { get; set; } = "";
		public string Icon { get; set; } = "?";
		public bool isFile { get; set; } = false;

		private PrenoteModel() { }

		public static PrenoteModel GetPrenote(string path)
		{
			PrenoteModel pn = new PrenoteModel()
			{
				Fullpath = path,
				Name = Path.GetFileNameWithoutExtension(path),
				Icon = path.EndsWith(".txt") ? "" : "",
				isFile = path.EndsWith(".txt")
			};
			return pn;
		}

		public static PrenoteModel GetPrenote(StorageFile path)
		{
			return GetPrenote(path.Path);
		}

		public static PrenoteModel GetPrenote(StorageFolder path)
		{
			return GetPrenote(path.Path);
		}

		public static ObservableCollection<PrenoteModel> GetChild(StorageFolder path)
		{
			var sub = Directory.GetFiles(path.Path);
			ObservableCollection<PrenoteModel> notes = new ObservableCollection<PrenoteModel>();
			foreach (var s in sub)
			{
				notes.Add(GetPrenote(s));
			}
			return notes;
		}
	}
}
