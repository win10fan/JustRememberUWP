using JustRemember.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace JustRemember.Services
{
	public static class PrenoteService
	{
		public static bool isDeployed
		{
			get
			{
				if (Directory.Exists(ApplicationData.Current.LocalFolder.Path + "\\Bundled memos"))
				{
					return true;
				}
				return false;
			}
		}

		public static void DeployPrenote()
		{
			string prenotepath = Windows.ApplicationModel.Package.Current.InstalledLocation.Path + "\\Bundled memos";
			var files = Directory.GetFiles(prenotepath);
			string deployPath = ApplicationData.Current.LocalFolder.Path + "\\Bundled memos";
			Directory.CreateDirectory(deployPath);
			for (int i = 0; i < files.Length - 1; i++)
			{
				string[] path = Path.GetFileName(files[i]).Split('-');
				string cachePath = $"{deployPath}\\{string.Join("\\", path)}";
				FileInfo f = new FileInfo(cachePath);
				if (!Directory.Exists(f.DirectoryName))
				{
					Directory.CreateDirectory(f.DirectoryName);
				}
				File.Copy(files[i], cachePath);
			}
		}

		public static async Task DeployMemoFromExtension(StorageFolder extFolder, string extensionName)
		{			
			App.isDeploying = true;
			tasks.Add(new TaskInfo());
			int ind = tasks.Count - 1;
			tasks[ind].progress = 0;
			//First get prenote folder
			StorageFolder prenote = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("Bundled memos");
			//Create confirm file
			var confirm = await prenote.CreateFileAsync($"{extensionName}.dep");
			//Create root directory
			StorageFolder root = await prenote.CreateFolderAsync(extensionName);
			//Loop through all file in extension public folder
			//Get file first
			var notes = await extFolder.GetFilesAsync();
			tasks[ind].progress = 10;
			float c = notes.Count;
			await Task.Run(async () => {
				foreach (var note in notes)
				{
					//Create folders according to the note file name
					string[] paths = note.Name.Split('-');
					//Loop through paths to create directories
					//But first get root folder start from \\Prenote\\[extension name]
					StorageFolder root2 = root;
					for (int i = 0; i < paths.Length - 1; i++)
					{
						if (paths[i].Contains(".txt")) { continue; }
						try
						{
							root2 = await root2.CreateFolderAsync(paths[i]);
						}
						catch (Exception ex)
						{
							Debug.Write(ex.Message);
							root2 = await root2.GetFolderAsync(paths[i]);
						}
					}
					//Then copy that note from extension folder
					await note.CopyAsync(root2, paths[paths.Length - 1]);
					c -= 1;
					float count = (float)(notes.Count - c) / notes.Count;
					tasks[ind].progress = 10 + (int)(count * 90);
				}
			});
			tasks[ind].isDone = true;
			App.isDeploying = false;
		}

		public static List<TaskInfo> tasks = new List<TaskInfo>();
		public class TaskInfo
		{
			public int progress;
			public bool isDone;
		}
		public static int progress
		{
			get
			{
				float total = 0;
				int done = 0;
				foreach (var i in tasks)
				{
					if (i.isDone) { done += 1; continue; }
					total += i.progress;
				}
				if (tasks.Count == done)
				{
					tasks.Clear();
					return 100;
				}
				total = total / tasks.Count;
				if (total > 100)
				{
					total = 100;
				}
				return (int)total;
			}
		}

		public static async void RequestRemovePrenoteExtension(string extName)
		{
			StorageFolder prenote = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("Bundled memos");
			
			if (File.Exists($"{prenote.Path}\\{extName}.dep"))
			{
				//Delete folder and prenote
				StorageFile confirm = await prenote.GetFileAsync($"{extName}.dep");
				StorageFolder notes = await prenote.GetFolderAsync(extName);
				await confirm.DeleteAsync();
				await notes.DeleteAsync();
			}
		}
		
		public static IEnumerable<PathDir> GetBreadcrumbPath(this string path)
		{
			if (path == null) { yield break; }
			var ret = new DirectoryInfo(path);
			yield return new PathDir(ret.FullName);
			for (int i = 0;i < 10;i++)
			{
				if (ret.Name != "Bundled memos")
				{
					ret = ret.Parent;
				}
				else { yield break; }
				yield return new PathDir(ret.FullName);
			}
			yield break;
		}
	}
}