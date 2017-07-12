using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppExtensions;  // App Extensions!!
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Media.Imaging;

namespace JustRemember.Models
{
	public class Extension
	{
		public BitmapImage Logo { get; set; }
		public AppExtension Core { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public PropertySet ExtensionInfo { get; set; }
		public ExtensionType extensionType { get; set; }

		public Extension(AppExtension _ext, PropertySet _pro, BitmapImage _logo, ExtensionType _type)
		{
			Logo = _logo;
			Core = _ext;
			ExtensionInfo = _pro;
			Name = _ext.DisplayName;
			Description = _ext.Description;
			extensionType = _type;
		}

		public Extension()
		{
			Logo = new BitmapImage();
			Core = null;
			extensionType = ExtensionType.Other;
			ExtensionInfo = null;
			Name = "unk";
			Description = "...";
		}
	}

	public enum ExtensionType
	{
		Notes,
		Script,
		Other
	}

	public static class ExtensionService
	{
		public static async Task<ObservableCollection<Extension>> GetExtension(IReadOnlyList<AppExtension> allext, ExtensionType _type)
		{
			ObservableCollection<Extension> res = new ObservableCollection<Extension>();
			foreach (var ext in allext)
			{
				//Properties
				var properties = await ext.GetExtensionPropertiesAsync() as PropertySet;
				//Logo
				var filestream = await (ext.AppInfo.DisplayInfo.GetLogo(new Windows.Foundation.Size(1, 1))).OpenReadAsync();
				BitmapImage logo = new BitmapImage();
				logo.SetSource(filestream);

				res.Add(new Extension(ext, properties, logo, _type));
			}
			return res;
		}
	}
}