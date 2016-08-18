using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

namespace JustRemember_UWP
{
    public sealed partial class End : Page
	{
		public End()
		{
            stats = new StatList();
            stats.Stats = StatList.Load();
			InitializeComponent();
			endList.Items.Add(totalWord);
			endList.Items.Add(totalWrong);
			endList.Items.Add("");
			endList.Items.Add(userTime);
			endList.Items.Add(userLimitTime);
		}

        public StatList stats;

		public List<KeyValuePair<string, int>> wrongperChoice = new List<KeyValuePair<string, int>>();

		public string totalWord
		{
			get
			{
				if (Utilities.newStat != null)
				{
					return $"Total word: {Utilities.newStat.totalWords}";
				}
				return "Total word: N/A";
			}
		}

		public string totalWrong
		{
			get
			{
				if (Utilities.newStat != null)
                {
					var first = Utilities.newStat.totalWrong >= 2 ? "s" : "";
					return $"Total wrong{first}: {Utilities.newStat.totalWrong}";
				}
				return "Total wrong: N/A";
			}
		}

		public string userTime
		{
			get
			{
				if (Utilities.newStat != null)
                {
					if (Utilities.newStat.useTimeLimit)
					{
						return $"Time: {Utilities.newStat.totalTime.ToStringAsTime()}";
					}
				}
				return "Time: N/A";
			}
		}

		public string userLimitTime
		{
			get
			{
				if (Utilities.newStat != null)
                {
					if (Utilities.newStat.useTimeLimit)
					{
						return $"Limit: {Utilities.newStat.totalLimitTime.ToStringAsTime()}";
					}
				}
				return "Limit: N/A";
			}
		}
        
		private void lineChart_Loaded(object sender, RoutedEventArgs e)
		{
			wrongperChoice = new List<KeyValuePair<string, int>>();
			var lst = stats.Stats.lastItem();
			for (int i = 0;i < lst.wrongPerchoice.Count - 1;i++)
			{
				wrongperChoice.Add(new KeyValuePair<string, int>(i.ToString(), lst.wrongPerchoice[i]));
			}
			(lineChart.Series[0] as LineSeries).ItemsSource = wrongperChoice;
		}

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (Utilities.currentSettings.defaultSeed != -1)
            {
                stats.Stats.Add(Utilities.newStat);
                StatList.SaveAll(stats.Stats);
            }
            Frame.GoBack();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Utilities.newStat = new statInfo();
            Frame.GoBack();
        }
    }
}