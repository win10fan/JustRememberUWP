using JustRemember.Models;
using JustRemember.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace JustRemember.Views
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class End : Page, INotifyPropertyChanged
	{
		public double halfRes
		{
			get => App.Config.halfResolution;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
		{
			if (Equals(storage, value))
			{
				return;
			}
			storage = value;
			OnPropertyChanged(propertyName);
		}

		void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public End()
		{
			this.InitializeComponent();
		}
		public StatModel current;
		public static int maxChoice;

		public GridLength showAds
		{
			get => App.Config.useAd ? new GridLength(120, GridUnitType.Pixel) : new GridLength(0);
		}

		public bool miniview;
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			current = new StatModel();
			if (e.Parameter is StatModel)
			{
				current = (StatModel)e.Parameter;
				maxChoice = current.configChoice;
				InitializeData();
			}
			if (e.Parameter is List<object>)
			{
				List<object> param = e.Parameter as List<object>;
				current = (StatModel)param[0];
				maxChoice = current.configChoice;
				if ((bool)param[1])
				{
					//Hide save button
					//Hide notice on bottom of page
					//etc.
					end_notice.Visibility = Visibility.Collapsed;
					saveBTN.Visibility = Visibility.Collapsed;
					emptyBTN.Visibility = Visibility.Visible;
					deleteBTN.Visibility = Visibility.Collapsed;
					miniview = true;
					InitializeData();
				}
			}
			base.OnNavigatedTo(e);
		}

		ObservableCollection<ChoiceTable> _choices = new ObservableCollection<ChoiceTable>();
		public ObservableCollection<ChoiceTable> Choices { get => _choices; set => Set(ref _choices, value); }
		private async void InitializeData()
		{
			int totalWork = current.choiceInfo.Count;
			if (totalWork > 100)
			{
				totalWork = 100;
			}
			for (int i = 0; i < totalWork; i++)
			{
				Choices.Add(new ChoiceTable(i + 1, current.correctedChoice[i] + 1, current.choiceInfo[i]));
				bool all = false;
				foreach (var item in current.choiceInfo[i])
				{
					if (item)
					{
						all = true;
					}
				}
				if (!all)
				{
					Choices[i].SelectedChoice[Choices[i].CorrectChoice - 1] = true;
				}
				OnPropertyChanged(nameof(Choices));
				await Task.Delay(100);
			}
		}

		private void Save(object sender, RoutedEventArgs e)
		{
			StatModel.Set(current);
			NavigationService.Navigate<Match>(App.cachedSession);
		}

		private void Delete(object sender, RoutedEventArgs e)
		{
			NavigationService.Navigate<Match>(App.cachedSession);
		}
		
		public bool saveable
		{
			get => !App.Config.useSeed;
		}
		
		public GridLength showChoice4Grid
		{
			get
			{
				if (maxChoice >= 4)
				{
					return new GridLength(1, GridUnitType.Star);
				}
				return new GridLength(0, GridUnitType.Pixel);
			}
		}

		public GridLength showChoice5Grid
		{
			get
			{
				if (maxChoice >= 5)
				{
					return new GridLength(1, GridUnitType.Star);
				}
				return new GridLength(0, GridUnitType.Pixel);
			}
		}

	}

	public class ChoiceTable
	{
		public int Order { get; set; }
		public int CorrectChoice { get; set; }
		public List<bool> SelectedChoice { get; set; }

		public ChoiceTable()
		{
			Order = 0;
			CorrectChoice = 0;
			SelectedChoice = new List<bool>() { false, false, false, false, false };
		}

		public ChoiceTable(int order,int correct, List<bool> selected)
		{
			Order = order;
			CorrectChoice = correct;
			SelectedChoice = selected;
		}
		
		public GridLength showChoice4Grid
		{
			get
			{
				if (End.maxChoice >= 4)
				{
					return new GridLength(1, GridUnitType.Star);
				}
				return new GridLength(0, GridUnitType.Pixel);
			}
		}
		
		public GridLength showChoice5Grid
		{
			get
			{
				if (End.maxChoice >= 5)
				{
					return new GridLength(1, GridUnitType.Star);
				}
				return new GridLength(0, GridUnitType.Pixel);
			}
		}
		
		public Visibility isCorrect
		{
			get
			{
				return SelectedChoice[CorrectChoice - 1] ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		public Visibility isInCorrect
		{
			get
			{
				return !SelectedChoice[CorrectChoice - 1] ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		public Visibility choosing1
		{
			get
			{
				return SelectedChoice.Count >= 1 ? (SelectedChoice[0] ? Visibility.Visible : Visibility.Collapsed) : Visibility.Collapsed;
			}
		}

		public Visibility choosing2
		{
			get
			{
				return SelectedChoice.Count >= 2 ? (SelectedChoice[1] ? Visibility.Visible : Visibility.Collapsed) : Visibility.Collapsed;
			}
		}

		public Visibility choosing3
		{
			get
			{
				return SelectedChoice.Count >= 3 ? (SelectedChoice[2] ? Visibility.Visible : Visibility.Collapsed) : Visibility.Collapsed;
			}
		}

		public Visibility choosing4
		{
			get
			{
				return SelectedChoice.Count >= 4 ? (SelectedChoice[3] ? Visibility.Visible : Visibility.Collapsed) : Visibility.Collapsed;
			}
		}

		public Visibility choosing5
		{
			get
			{
				return SelectedChoice.Count >= 5 ? (SelectedChoice[4] ? Visibility.Visible : Visibility.Collapsed) : Visibility.Collapsed;
			}
		}

		public Visibility is1Correct { get => CorrectChoice == 1 ? Visibility.Visible : Visibility.Collapsed; }
		public Visibility is2Correct { get => CorrectChoice == 2 ? Visibility.Visible : Visibility.Collapsed; }
		public Visibility is3Correct { get => CorrectChoice == 3 ? Visibility.Visible : Visibility.Collapsed; }
		public Visibility is4Correct { get => CorrectChoice == 4 ? Visibility.Visible : Visibility.Collapsed; }
		public Visibility is5Correct { get => CorrectChoice == 5 ? Visibility.Visible : Visibility.Collapsed; }

		public SolidColorBrush BG
		{
			get => Order % 2 == 0 ? (SolidColorBrush)App.Current.Resources["AccentButtonBackgroundDisabled"] : (SolidColorBrush)App.Current.Resources["AccentButtonBackgroundPressed"];
		}
	}
}