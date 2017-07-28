using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace JustRemember.Services
{
    public static class DescriptionService
    {
		public static bool hasDescription(string file)
		{
			string path = $"{ApplicationData.Current.RoamingFolder.Path}\\Description\\{file}.mde";
			if (File.Exists(path))
			{
				return true;
			}
			return false;
		}

		public static async Task<StorageFile> GetDescription(string file)
		{
			if (!hasDescription(file))
				return null;
			StorageFolder folder = await ApplicationData.Current.RoamingFolder.TryGetItemAsync("Description") as StorageFolder;
			var infoFile = await folder.TryGetItemAsync($"{file}.mde") as StorageFile;
			if (infoFile == null)
				return null;
			return infoFile;
		}
    }
}
