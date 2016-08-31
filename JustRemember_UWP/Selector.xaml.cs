using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace JustRemember_UWP
{
	public sealed partial class Selector : Page
	{
		public Selector()
		{
			InitializeComponent();
		}

		private async void AppBarButton_Click(object sender, RoutedEventArgs e)
		{
			await Note.GetNotesList();
			RefreshList();
		}

		public void RefreshList()
		{
			List<SelectorItem> items = new List<SelectorItem>();
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
            itemCount.Text = selectors.Count == 0 ? $"No user note.{Environment.NewLine}But you can add from editor" : $"Total user note: {selectors.Count}";
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

        private async void listView_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            if (listView.SelectedIndex >= 0)
            {
                if (deleteMode.IsChecked == true)
                {
                    var selec = Utilities.notes[listView.SelectedIndex];
                    var folder = await Windows.Storage.ApplicationData.Current.RoamingFolder.GetFolderAsync("Note");
                    var file = await folder.GetFileAsync(selec.Title + ".txt");
                    await file.DeleteAsync();
                    await Note.GetNotesList();
                    RefreshList();
                    return;
                }
                Utilities.selected = Utilities.notes[listView.SelectedIndex];
                if (!Utilities.isSmallLoaderMode)
                {
                    Frame.Navigate(typeof(Match));
                }
            }
        }
    }
}
