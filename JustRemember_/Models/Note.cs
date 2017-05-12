using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JustRemember_.Models
{
    public class Note
    {
        public string Title { get; set; }
        public string Content { get; set; }
        /*	public struct Note
	{
		public string Title;
		public string Content;
        
		public Note(string content, string title)
		{
			Title = title;
			Content = content;
		}

		public static implicit operator Note(SelectorItem item)
		{
			return item.content_note;
		}

		public static async void Save(Note saved)
		{
			var path = KnownFolders.DocumentsLibrary;
			var file = await path.CreateFileAsync(saved.Title + ".txt", CreationCollisionOption.ReplaceExisting);
			await FileIO.WriteTextAsync(file, saved.Content);
		}

		public static async void Delete(string filename)
		{
			var path = KnownFolders.DocumentsLibrary;
			var file = await path.TryGetItemAsync(filename);
			if (file != null)
			{
				await file.DeleteAsync();
			}
		}

		public static Note Load(string filename)
		{
			Note info = new Note();
			info.Title = filename;
			string path = Path.Combine(ApplicationData.Current.RoamingFolder.Path, filename);
			if (!File.Exists(path))
			{
				return info;
			}
			using (StreamReader reader = File.OpenText(path))
			{
				info.Content = reader.ReadToEnd();
				return info;
			}
		}

		public static async Task<Note> LoadAsync(string filename)
		{
			Note result = new Note();
			if (filename.Contains(".txt"))
			{
				result.Title = filename.Replace(".txt", "");
			}
			var path = await ApplicationData.Current.RoamingFolder.GetFolderAsync("Note");
			var file = await path.GetFileAsync(filename);
			if (file == null)
			{
				return result;
			}
			else
			{
				result.Content = await FileIO.ReadTextAsync(file);
				return result;
			}
		}

		public static async Task GetNotesList()
		{
			if (!Directory.Exists(ApplicationData.Current.RoamingFolder.Path + "\\Note\\"))
			{
				StorageFolder fol = ApplicationData.Current.RoamingFolder;
				await fol.CreateFolderAsync("Note");
			}
			var files = Directory.GetFiles(ApplicationData.Current.RoamingFolder.Path + "\\Note\\");
			if (files.Length == 0)
			{
                Utilities.notes?.Clear();
				return;
			}
			List<Note> notes = new List<Note>();
			foreach (var f in files)
			{
				FileInfo info = new FileInfo(f);
				if (info.Extension == ".txt")
				{
					Note newone = await LoadAsync(info.Name);
					notes.Add(newone);
				}
			}
			Utilities.notes = notes;
		}

		//public static async Task<List<Note>> GetDocList()
		//{
		//	//Same as get note.. But get in Document path
		//	var files = Directory.GetFiles(KnownFolders.DocumentsLibrary.Path);
		//	List<Note> notes = new List<Note>();
		//	foreach (var f in files)
		//	{

		//	}
		//}
	}
*/
    }
}
