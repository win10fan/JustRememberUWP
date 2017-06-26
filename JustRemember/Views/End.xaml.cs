using JustRemember.Helpers;
using JustRemember.Models;
using JustRemember.Services;
using JustRemember.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace JustRemember.Views
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class End : Page, INotifyPropertyChanged
	{
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
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(width)));
		}

		public End()
		{
			this.InitializeComponent();
		}
		public StatModel current;

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			current = new StatModel();
			if (e.Parameter is StatModel)
			{
				current = (StatModel)e.Parameter;
				InitializeData();
			}
			base.OnNavigatedTo(e);
		}

		ObservableCollection<ChoicesCorrected> _c, _co;
		public ObservableCollection<ChoicesCorrected> choices { get => _c; set => Set(ref _c, value); }

		public ObservableCollection<ChoicesCorrected> corrected { get => _co; set => Set(ref _co, value); }

		public int width { get { if (choices.Count < 1) { return 40; } return choices.Count * 40; } }

		private async void InitializeData()
		{
			int totalWork = current.choiceInfo2.Count;
			if (totalWork > 100)
			{
				totalWork = 100;
			}
			choices = new ObservableCollection<ChoicesCorrected>();
			corrected = new ObservableCollection<ChoicesCorrected>();
			for (int i = 0; i < totalWork; i++)
			{
				choices.Add(new ChoicesCorrected(current.choiceInfo2[i], current.correctedChoice[i]));
				corrected.Add(new ChoicesCorrected(i + 1, current.correctedChoice[i]));
				await Task.Delay(50);
				RefreshGraph(null, null);
			}
		}

		private void Save(object sender, RoutedEventArgs e)
		{
			StatModel.Set(current);
			NavigationService.GoBack();
		}

		private void Delete(object sender, RoutedEventArgs e)
		{
			NavigationService.GoBack();
		}
		
		private void RefreshGraph(object sender, RoutedEventArgs e)
		{
			OnPropertyChanged(nameof(choices));
			OnPropertyChanged(nameof(corrected));
			OnPropertyChanged(nameof(width));
		}

		public bool saveable
		{
			get => App.Config.defaultSeed != -1;
		}
	}
	public class ChoicesCorrected
	{
		public string catagory { get; set; } 
		public double corrected { get; set; }

		public ChoicesCorrected()
		{
			catagory = "item";
			corrected = 0;
		}

		public ChoicesCorrected(choiceInfoUnDic input,int cor)
		{
			catagory = (input.choice + 1).ToString();
			for (int i = 0; i < input.origin.Count - 1; i++)
			{
				if (input.origin[i])
				{
					corrected = i;
					return;
				}
			}
			corrected = cor;
		}

		public ChoicesCorrected(int ch,int cor)
		{
			catagory = ch.ToString();
			corrected = cor;
		}
	}
}
