using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace JustRemember.Services
{
	public static class AnnoyPlayer
	{
		private static MediaElement player = null;
		private static Random randomCore = null;

		public static void Initialize()
		{
			player = new MediaElement();
			randomCore = new Random();
			noDup = new List<int>();
		}

		public static async void Play(int that)
		{
			switch (player.CurrentState)
			{
				case Windows.UI.Xaml.Media.MediaElementState.Stopped:
				case Windows.UI.Xaml.Media.MediaElementState.Closed:
				case Windows.UI.Xaml.Media.MediaElementState.Paused:
					break;
				case Windows.UI.Xaml.Media.MediaElementState.Playing:
				case Windows.UI.Xaml.Media.MediaElementState.Opening:
					return;
				default:
					break;
			}
			if (isPlaying) { return; }
			Windows.Storage.StorageFolder folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets\\Beep\\");
			Windows.Storage.StorageFile file = await folder.GetFileAsync($"annoy{that}.mp3");
			var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
			player.SetSource(stream, file.ContentType);
			player.Play();
		}

		public static void Play()
		{
			noDup.Add(randomCore.Next(1, 7));
			if (noDup.Count > 2)
			{
				while (noDup[noDup.Count - 1] == noDup[noDup.Count - 2])
				{
					noDup.Add(randomCore.Next(1, 7));
				}
			}
			Play(noDup[noDup.Count - 1]);
		}

		static List<int> noDup;

		public static bool isPlaying
		{
			get
			{
				switch (player.CurrentState)
				{
					case Windows.UI.Xaml.Media.MediaElementState.Stopped:
					case Windows.UI.Xaml.Media.MediaElementState.Closed:
					case Windows.UI.Xaml.Media.MediaElementState.Paused:
						return false;
					case Windows.UI.Xaml.Media.MediaElementState.Playing:
					case Windows.UI.Xaml.Media.MediaElementState.Opening:
						return true;
					default:
						return true;
				}
			}
		}
	}
}