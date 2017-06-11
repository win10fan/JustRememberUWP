using JustRemember.Models;
using JustRemember.ViewModels;
using System;
using System.Collections.Generic;
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