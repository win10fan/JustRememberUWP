using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace JustRemember_UWP
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class End : Page
	{
		public End()
		{
			endingDesc = new List<string>();
			endingDesc.Add(totalWord);
			endingDesc.Add(totalWrong);
			endingDesc.Add("");
			endingDesc.Add(userTime);
			endingDesc.Add(userLimitTime);
			InitializeComponent();
		}

		public List<string> endingDesc { get; set; }

		public List<int> chartInfo
		{
			get
			{
				if (Utilities.currentSettings.stat[Utilities.currentSettings.stat.Count - 1] != null)
				{
					return Utilities.currentSettings.stat[Utilities.currentSettings.stat.Count - 1].wrongPerchoice;
				}
				return new int[]{ 10,9,8,7,6,5,4,3,2,1,0 }.ToList();
			}
			set
			{
				Utilities.currentSettings.stat[Utilities.currentSettings.stat.Count - 1].wrongPerchoice = value;
			}
		}

		public string totalWord
		{
			get
			{
				if (Utilities.currentSettings.stat[Utilities.currentSettings.stat.Count - 1] != null)
				{
					return $"Total word: {Utilities.currentSettings.stat[Utilities.currentSettings.stat.Count - 1].totalWords}";
				}
				return "Total word: N/A";
			}
		}

		public string totalWrong
		{
			get
			{
				if (Utilities.currentSettings.stat[Utilities.currentSettings.stat.Count - 1] != null)
				{
					var first = Utilities.currentSettings.stat[Utilities.currentSettings.stat.Count - 1].totalWrong >= 2 ? "s" : "";
					return $"Total wrong{first}: {Utilities.currentSettings.stat[Utilities.currentSettings.stat.Count - 1].totalWrong}";
				}
				return "Total wrong: N/A";
			}
		}

		public string userTime
		{
			get
			{
				if (Utilities.currentSettings.stat[Utilities.currentSettings.stat.Count - 1] != null)
				{
					if (Utilities.currentSettings.stat[Utilities.currentSettings.stat.Count - 1].useTimeLimit)
					{
						return $"Time: {Utilities.currentSettings.stat[Utilities.currentSettings.stat.Count - 1].totalTime.ToStringAsTime()}";
					}
				}
				return "Time: N/A";
			}
		}

		public string userLimitTime
		{
			get
			{
				if (Utilities.currentSettings.stat[Utilities.currentSettings.stat.Count - 1] != null)
				{
					if (Utilities.currentSettings.stat[Utilities.currentSettings.stat.Count - 1].useTimeLimit)
					{
						return $"Limit: {Utilities.currentSettings.stat[Utilities.currentSettings.stat.Count - 1].totalLimitTime.ToStringAsTime()}";
					}
				}
				return "Limit: N/A";
			}
		}

		private void button_Click(object sender, RoutedEventArgs e)
		{
			Frame.Navigate(typeof(Match));
		}
	}
}