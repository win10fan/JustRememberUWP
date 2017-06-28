using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace JustRemember.Models
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

		public static async Task<ObservableCollection<PrenoteModel>> GetChild(StorageFolder path)
		{
			var sub = await path.GetFoldersAsync();
			var nts = await path.GetFilesAsync();
			ObservableCollection<PrenoteModel> notes = new ObservableCollection<PrenoteModel>();
			foreach (var s in sub)
			{
				notes.Add(GetPrenote(s));
			}
			foreach (var s in nts)
			{
				notes.Add(GetPrenote(s));
			}
			return notes;
		}
	}

	public class PathDir
	{
		public string Name { get; set; }
		public string FullPath { get; set; }

		public PathDir(string p)
		{
			DirectoryInfo dir = new DirectoryInfo(p);
			int order = 0;
			if (char.IsNumber(dir.Name[0]))
			{
				if (char.IsNumber(dir.Name[1]))
					order = int.Parse($"{dir.Name[0]}{dir.Name[1]}");
				else
					order = int.Parse($"{dir.Name[0]}");
			}
			Name = dir.Name.Replace(order.ToString(), "");
			FullPath = p;
		}
	}
}
