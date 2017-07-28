using JustRemember.Helpers;
using JustRemember.Services;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;

namespace JustRemember.Models
{
	public class NoteModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
		public bool hasDesc { get; set; }

		public NoteModel()
		{
			Title = "Untitled";
			Content = "";
			hasDesc = false;
		}

		public NoteModel(string title,string content)
		{
			Title = title;
			Content = content;
			hasDesc = DescriptionService.hasDescription(title);
		}

		[JsonIgnore]
        public string FirstLine
        {
            get
            {
                if (Mode == noteMode.None)
				{
					return lines[0];
				}
				if (Mode == noteMode.Question && lines[0].Contains("AnswerPosition=BehindAnswer"))
				{
					return lines[1].Substring(0, lines[1].LastIndexOf('=') - 1);
				}
				return lines[1];
            }
        }

		[JsonIgnore]
		string[] lines
		{
			get
			{
				if (string.IsNullOrEmpty(Content))
					return new string[] { "" };
				var pack = Content.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
				return pack;
			}
		}

		[JsonIgnore]
		public noteMode Mode
		{
			get
			{
				if (lines[0].StartsWith("#MODE=EXAM"))
				{
					return noteMode.Question;
				}
				else if (lines[0].StartsWith("#MODE=VOLC"))
				{
					return noteMode.Volcabulary;
				}
				return noteMode.None;
			}
		}

		[JsonIgnore]
		public Visibility hasDescView
		{
			get => hasDesc ? Visibility.Visible : Visibility.Collapsed;
		}
		
		public async Task<StorageFile> GetDescription()
		{
			return await DescriptionService.GetDescription(Title);
		}

        public static async Task<ObservableCollection<NoteModel>> GetNotesAsync()
        {
            StorageFolder folder = ApplicationData.Current.RoamingFolder;
            if (!Directory.Exists(ApplicationData.Current.RoamingFolder.Path + "\\Notes\\"))
            {
                await folder.CreateFolderAsync("Notes");
            }
            if (Directory.GetFiles(ApplicationData.Current.RoamingFolder.Path + "\\Notes\\").Length < 1)
            {
                return new ObservableCollection<NoteModel>();
            }
            var noteFolder = await folder.GetFolderAsync("Notes");
            var noteFiles = await noteFolder.GetFilesAsync();
            ObservableCollection<NoteModel> allNote = new ObservableCollection<NoteModel>();
            if (noteFiles != null)
            {
                foreach (var file in noteFiles)
                {
					IBuffer buffer = await FileIO.ReadBufferAsync(file);
					DataReader reader = DataReader.FromBuffer(buffer);
					byte[] fileContent = new byte[reader.UnconsumedBufferLength];
					reader.ReadBytes(fileContent);
					string content = Encoding.UTF8.GetString(fileContent, 0, fileContent.Length);
					NoteModel n3 = new NoteModel(file.DisplayName, content);
					allNote.Add(n3);
                }
            }
            return allNote;
        }

        public static async Task DeleteNote(string name)
        {
            StorageFolder folder = ApplicationData.Current.RoamingFolder;
            var noteFolder = await folder.GetFolderAsync("Notes");
            var noteFiles = await noteFolder.GetFileAsync($"{name}.txt");
			//Check for description files
			var descFolder = await folder.TryGetItemAsync("Description") as StorageFolder;
			if (await descFolder.TryGetItemAsync($"{name}.mde") is StorageFile descFile)
			{
				AudioDescriptor ador = await Json.ToObjectAsync<AudioDescriptor>(await FileIO.ReadTextAsync(descFile));
				var audiF = await ador.GetAudio();
				await audiF.DeleteAsync();
				await descFile.DeleteAsync();
			}
			//Then delete note
			if (noteFiles != null)
            {
                await noteFiles.DeleteAsync();
            }
        }

		public static async Task SaveNote(NoteModel note)
		{
			string filename = note.Title;
			if (!filename.EndsWith(".txt"))
				filename = $"{filename}.txt";
			var folder = (StorageFolder) await ApplicationData.Current.RoamingFolder.TryGetItemAsync("Notes");
			if (folder != null)
			{
				//Folder exist
				var nf = (StorageFile)await folder.TryGetItemAsync(filename);
				if (nf == null)
				{
					//No file
					nf = await folder.CreateFileAsync(filename);
				}
				else
				{
					await nf.DeleteAsync();
					nf = await folder.CreateFileAsync(filename);
				}
				await FileIO.WriteTextAsync(nf, note.Content);
			}
		}

		public static async Task<NoteModel> GetOneNoteButNotMicrosoftOneNoteButOneOfANoteWithParticularPath(string path)
		{
			StorageFile fol = await StorageFile.GetFileFromPathAsync(path);
			string displayName = fol.DisplayName;
			if (displayName.EndsWith(".txt"))
			{
				displayName = displayName.Replace(".txt", "");
			}
			while (char.IsNumber(displayName[0]))
			{
				displayName = displayName.Remove(0, 1);
			}
			IBuffer buffer = await FileIO.ReadBufferAsync(fol);
			DataReader reader = DataReader.FromBuffer(buffer);
			byte[] fileContent = new byte[reader.UnconsumedBufferLength];
			reader.ReadBytes(fileContent);
			string content = Encoding.UTF8.GetString(fileContent, 0, fileContent.Length);
			NoteModel datNote = new NoteModel()
			{
				Title = displayName,
				Content = content
			};
			return datNote;
		}

		public static NoteModel empty
		{
			get
			{
				string title = "$Empty";
				string content = "$%#@^)_(@$^)_@(^@#^$#&*)_@#^@";
				NoteModel emp = new NoteModel()
				{
					Title = title,
					Content = content
				};
				return emp;
			}
		}
	}

	public enum noteMode
	{
		None,
		Question,
		Volcabulary
	}
}
