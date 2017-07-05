using JustRemember.Models;
using JustRemember.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
				if (Directory.Exists(ApplicationData.Current.LocalFolder.Path + "\\Prenote"))
				{
					return true;
				}
				return false;
			}
		}

		public static void DeployPrenote()
		{
			string prenotepath = Windows.ApplicationModel.Package.Current.InstalledLocation.Path + "\\Prenote";
			var files = Directory.GetFiles(prenotepath);
			string deployPath = ApplicationData.Current.LocalFolder.Path + "\\Prenote";
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

		public static async void DeployPrenotesExtension(StorageFolder extFolder,string extensionName)
		{
			App.isDeploying = true;
			//First get prenote folder
			StorageFolder prenote = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("Prenote");
			//Create confirm file
			var confirm = await prenote.CreateFileAsync($"{extensionName}.dep");
			//Create root directory
			StorageFolder root = await prenote.CreateFolderAsync(extensionName);
			//Loop through all file in extension public folder
			//Get file first
			var notes = await extFolder.GetFilesAsync();
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
				await note.CopyAsync(root2,paths[paths.Length - 1]);
			}
			App.isDeploying = false;
		}
		
		public static IEnumerable<PathDir> GetBreadcrumbPath(this string path)
		{
			if (path == null) { yield break; }
			var ret = new DirectoryInfo(path);
			yield return new PathDir(ret.FullName);
			for (int i = 0;i < 10;i++)
			{
				if (ret.Name != "Prenote")
				{
					ret = ret.Parent;
				}
				else { yield break; }
				yield return new PathDir(ret.FullName);
			}
			yield break;
			//if (path.Parent != null) ret.AddRange(Split(path.Parent));
			//ret.Add(path);

			//foreach (var idx in null)
			//{
			//	if (ret.Parent.Name != "Prenote")
			//	{ ret = ret.Parent; }
				
			//	yield return new PathDir(path.Substring(0, idx - 1), ret.Parent.FullName);
			//}
		}
	}
}