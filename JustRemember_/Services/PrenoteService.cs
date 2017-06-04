using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace JustRemember_.Services
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
				if (path.Length == 3)
				{
					string cachep = $"{deployPath}\\{path[0]}\\{path[1]}";
					if (!Directory.Exists(cachep))
					{
						Directory.CreateDirectory(cachep);
					}
					cachep += "\\" + path[2];
					File.Copy(files[i], cachep);
				}
				//else //Deeper prenote
				//{                    
				//}
			}
		}

		public static IEnumerable<string> GetBreadcrumbPath(this string path)
		{
			string tmp = "";
			if (!path.StartsWith("/"))
			{
				tmp = "/" + path;
			}
			var index = tmp.IndexOf("/");
			var indices = tmp.Select((x, idx) => new { x, idx }).Where(p => p.x == '/' && p.idx > index + 1).Select(p => p.idx);

			foreach (var idx in indices)
			{
				yield return path.Substring(0, idx - 1);
			}
			yield return path;
		}
	}
}