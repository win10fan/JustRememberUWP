using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace JustRemember.Models
{
	public class PrenoteModel
	{
		public string Fullpath { get; set; } = "";
		public string Name { get; set; } = "";
		public bool isFile { get; set; } = false;

		[JsonIgnore]
		public SolidColorBrush iconColor
		{
			get => isFile ? new SolidColorBrush(Colors.WhiteSmoke) : new SolidColorBrush(Colors.Yellow);
		}
		[JsonIgnore]
		public SolidColorBrush iconColor2
		{
			get => isFile ? new SolidColorBrush(Colors.Gray) : new SolidColorBrush(Colors.Yellow);
		}
		[JsonIgnore]
		public string Icon { get => isFile ? "" : ""; }
		[JsonIgnore]
		public string Icon2 { get => isFile ? "" : ""; }

		private PrenoteModel() { }

		public static PrenoteModel GetPrenote(string path)
		{
			PrenoteModel pn = new PrenoteModel()
			{
				Fullpath = path,
				Name = Path.GetFileNameWithoutExtension(path),
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
				if (s.FileType.Contains("dep"))
				{
					continue;
				}
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
