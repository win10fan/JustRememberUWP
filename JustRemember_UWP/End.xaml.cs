﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

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
			InitializeComponent();
			endList.Items.Add(totalWord);
			endList.Items.Add(totalWrong);
			endList.Items.Add("");
			endList.Items.Add(userTime);
			endList.Items.Add(userLimitTime);
		}

		public List<KeyValuePair<string, int>> wrongperChoice = new List<KeyValuePair<string, int>>();

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

		private async void lineChart_Loaded(object sender, RoutedEventArgs e)
		{
			wrongperChoice = new List<KeyValuePair<string, int>>();
			var lst = Utilities.currentSettings.stat.lastItem();
			for (int i = 0;i < lst.wrongPerchoice.Count - 1;i++)
			{
				wrongperChoice.Add(new KeyValuePair<string, int>(i.ToString(), lst.wrongPerchoice[i]));
			}
			(lineChart.Series[0] as LineSeries).ItemsSource = wrongperChoice;
		}
	}
}