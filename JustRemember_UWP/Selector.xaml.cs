using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	public sealed partial class Selector : Page
	{
		public Selector()
		{
			this.InitializeComponent();
			//DataContext = SelectorItem.GetItems();
		}

		private async void AppBarButton_Click(object sender, RoutedEventArgs e)
		{
			await Note.GetNotesList();
			RefreshList();
		}

		public void RefreshList()
		{
			List<SelectorItem> items = new List<SelectorItem>();
			listView.Items.Clear();
			var selectors = new List<SelectorItem>();
			if (Utilities.notes != null)
			{
				foreach (var n in Utilities.notes)
				{
					SelectorItem si = new SelectorItem(n);
					selectors.Add(si);
				}
			}
			listView.ItemsSource = selectors;
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e)
		{
			await Note.GetNotesList();
			RefreshList();
		}

		private void button_Click(object sender, RoutedEventArgs e)
		{
			Frame.Navigate(typeof(MainPage));
		}

		private void AppBarButton_Click_1(object sender, RoutedEventArgs e)
		{
			Frame.Navigate(typeof(NoteEditor));
		}

		private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			openFile.Visibility = listView.SelectedIndex != -1 ? Visibility.Visible : Visibility.Collapsed;
		}

		private void openFile_Click(object sender, RoutedEventArgs e)
		{
			if (listView.SelectedIndex >= 0)
			{
				Utilities.selected = Utilities.notes[listView.SelectedIndex];
				if (!Utilities.isSmallLoaderMode)
				{
					Frame.Navigate(typeof(Match));
				}
			}
		}

		private void titleBar_Loaded(object sender, RoutedEventArgs e)
		{
			if (Utilities.isSmallLoaderMode)
			{
				titleBar.Visibility = Visibility.Collapsed;
				listView.Margin = new Thickness(0);
				openEditor.Visibility = Visibility.Collapsed;
			}
			else
			{
				titleBar.Visibility = Visibility.Visible;
				listView.Margin = new Thickness(0, 50, 0, 0);
				openEditor.Visibility = Visibility.Visible;
			}
		}
	}
}
