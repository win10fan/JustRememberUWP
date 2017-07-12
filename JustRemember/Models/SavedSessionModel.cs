using JustRemember.Helpers;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace JustRemember.Models
{
	public class SavedSessionModel
	{
		public ObservableCollection<SessionModel> save { get; set; }

		public SavedSessionModel()
		{
			save = new ObservableCollection<SessionModel>();
		}

		public static async Task<ObservableCollection<SessionModel>> Load()
		{
			StorageFolder folder = ApplicationData.Current.RoamingFolder;
			if (!Directory.Exists(ApplicationData.Current.RoamingFolder.Path + "\\Sessions\\"))
			{
				await folder.CreateFolderAsync("Sessions");
			}
			if (Directory.GetFiles(ApplicationData.Current.RoamingFolder.Path + "\\Sessions\\").Length < 1)
			{
				return new ObservableCollection<SessionModel>();
			}
			var sfol = await folder.GetFolderAsync("Sessions");
			var sessions = await sfol.GetFilesAsync();
			ObservableCollection<SessionModel> allsession = new ObservableCollection<SessionModel>();
			if (sessions != null)
			{
				foreach (var file in sessions)
				{
					if (file.FileType == ".info")
					{
						string notePath = $"{file.Path}{noteExtension}";
						string statPath = $"{file.Path}{statExtension}";
						string chosPath = $"{file.Path}{choicesExtension}";
						string noteName = Path.GetFileName(notePath);
						string statName = Path.GetFileName(statPath);
						string chosName = Path.GetFileName(chosPath);
						StorageFile note = await sfol.GetFileAsync(noteName);
						StorageFile stat = await sfol.GetFileAsync(statName);
						StorageFile chos = await sfol.GetFileAsync(chosName);
						SessionModel ss = new SessionModel();
						ss = await Json.ToObjectAsync<SessionModel>(await FileIO.ReadTextAsync(file));
						ss.SelectedNote = await Json.ToObjectAsync<NoteModel>(await FileIO.ReadTextAsync(note));
						ss.StatInfo = await Json.ToObjectAsync<StatModel>(await FileIO.ReadTextAsync(stat));
						ss.choices = await Json.ToObjectAsync<ObservableCollection<ChoiceSet>>(await FileIO.ReadTextAsync(chos));
						ss.texts = TextList.Extract(ss.SelectedNote.Content);
						allsession.Add(ss);
					}
				}
			}
			return allsession;
		}

		public static async Task Save(ObservableCollection<SessionModel> save)
		{
			if (save.Count < 1)
			{
				return;
			}
			StorageFolder folder = ApplicationData.Current.RoamingFolder;
			if (!Directory.Exists(ApplicationData.Current.RoamingFolder.Path + "\\Sessions\\"))
			{
				await folder.CreateFolderAsync("Sessions");
			}
			var sessionFolder = await folder.GetFolderAsync("Sessions");
			foreach (var ss in save)
			{
				await SaveOne(ss, sessionFolder);
			}
		}

		public static async Task AddNew(SessionModel ss)
		{
			StorageFolder folder = ApplicationData.Current.RoamingFolder;
			if (!Directory.Exists(ApplicationData.Current.RoamingFolder.Path + "\\Sessions\\"))
			{
				await folder.CreateFolderAsync("Sessions");
			}
			var sessionFolder = await folder.GetFolderAsync("Sessions");
			await SaveOne(ss, sessionFolder);
		}
		
		public static async Task<bool> isExist(SessionModel ss)
		{
			var getFol = (StorageFolder)await ApplicationData.Current.RoamingFolder.TryGetItemAsync("Sessions");
			if (getFol != null)
			{
				StorageFile datFile =(StorageFile)await getFol.TryGetItemAsync(ss.GeneratedName);
				if (datFile != null)
				{
					return true;
				}
				return false;
			}
			return false;
		}

		private static async Task SaveOne(SessionModel info, StorageFolder at)
		{
			info.isNew = false; //It been a session | which is not new anymore
			string content = await Json.StringifyAsync(info);
			string noteContent = await Json.StringifyAsync(info.SelectedNote);
			string statContent = await Json.StringifyAsync(info.StatInfo);
			string geneContent = await Json.StringifyAsync(info.choices);
			NoteModel note = info.SelectedNote;
			StatModel stat = info.StatInfo;
			ObservableCollection<ChoiceSet> chos = info.choices;
			StorageFile file = await at.CreateFileAsync(info.GeneratedName);
			StorageFile fileN = await at.CreateFileAsync($"{info.GeneratedName}{noteExtension}");
			StorageFile fileS = await at.CreateFileAsync($"{info.GeneratedName}{statExtension}");
			StorageFile fileG = await at.CreateFileAsync($"{info.GeneratedName}{choicesExtension}");
			await FileIO.WriteTextAsync(file, content);
			await FileIO.WriteTextAsync(fileN, noteContent);
			await FileIO.WriteTextAsync(fileS, statContent);
			await FileIO.WriteTextAsync(fileG, geneContent);
		}

		public static async Task Delete(string filename)
		{
			StorageFolder folder = await ApplicationData.Current.RoamingFolder.GetFolderAsync("Sessions");
			StorageFile main = await folder.GetFileAsync(filename);
			StorageFile note = await folder.GetFileAsync($"{filename}{noteExtension}");
			StorageFile stat = await folder.GetFileAsync($"{filename}{statExtension}");
			StorageFile chos = await folder.GetFileAsync($"{filename}{choicesExtension}");
			await main.DeleteAsync();
			await note.DeleteAsync();
			await stat.DeleteAsync();
			await chos.DeleteAsync();
		}

		const string noteExtension = ".note";
		const string statExtension = ".stat";
		const string choicesExtension = ".gen";
	}
}