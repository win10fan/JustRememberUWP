using JustRemember.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace JustRemember.Models
{
	public class PrenoteModel
	{
		public string Fullpath { get; set; } = "";
		public string Name { get; set; } = "";
		public bool isFile { get; set; } = false;
		public bool hasDesc { get; set; } = false;
		public AudioDescriptor Descriptor;
		public StorageFile desFile;
		public StorageFile desAudiFile;

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
		[JsonIgnore]
		public Visibility hasDescView { get => hasDesc ? Visibility.Visible : Visibility.Collapsed; }

		private PrenoteModel() { }

		public static async Task<PrenoteModel> GetPrenote(string path,bool isFolder)
		{
			StorageFile file = isFolder ? null : await StorageFile.GetFileFromPathAsync(path);
			AudioDescriptor desor = new AudioDescriptor();
			StorageFile audi = null;
			bool foundDesc = false;
			if (!isFolder)
			{
				var pnt = await file.GetParentAsync();
				if (await pnt.TryGetItemAsync($"{file.DisplayName}.mde") is StorageFile descAtmp)
				{
					foundDesc = true;
					desor = await Json.ToObjectAsync<AudioDescriptor>(await FileIO.ReadTextAsync(descAtmp));
					audi = await pnt.GetFileAsync(desor.audioName);
				}
			}
			PrenoteModel pn = new PrenoteModel()
			{
				Fullpath = path,
				Name = Path.GetFileNameWithoutExtension(path),
				isFile = path.EndsWith(".txt"),
				hasDesc = foundDesc,
				Descriptor = foundDesc ? desor : null,
				desAudiFile = foundDesc ? audi : null
			};
			return pn;
		}

		public static async Task<PrenoteModel> GetPrenote(StorageFile path)
		{
			return await GetPrenote(path.Path, false);
		}

		public static async Task<PrenoteModel> GetPrenote(StorageFolder path)
		{
			return await GetPrenote(path.Path, true);
		}

		public static async Task<ObservableCollection<PrenoteModel>> GetChild(StorageFolder path)
		{
			var sub = await path.GetFoldersAsync();
			var nts = await path.GetFilesAsync();
			ObservableCollection<PrenoteModel> notes = new ObservableCollection<PrenoteModel>();
			foreach (var s in sub)
			{
				notes.Add(await GetPrenote(s));
			}
			foreach (var s in nts)
			{
				if (s.FileType.Contains("dep"))
				{
					continue;
				}
				notes.Add(await GetPrenote(s));
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
