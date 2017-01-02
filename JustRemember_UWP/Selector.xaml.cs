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
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Visible;
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += Selector_BackRequested;
        }

        private void Selector_BackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested -= Selector_BackRequested;
            Frame.Navigate(typeof(MainPage));
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
            if (deleteMode.IsChecked == true)
            {
                itemCount.Text = App.language.GetString("delMode");
            }
            else if (deleteMode.IsChecked == false)
            {
                itemCount.Text = selectors.Count == 0 ? App.language.GetString("fileNo") : $"{App.language.GetString("fileYes")}: {selectors.Count}";
            }
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e)
		{
			await Note.GetNotesList();
			RefreshList();
		}

		private void button_Click(object sender, RoutedEventArgs e)
        {
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested -= Selector_BackRequested;
            Frame.Navigate(typeof(MainPage));
		}

		private void AppBarButton_Click_1(object sender, RoutedEventArgs e)
		{
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested -= Selector_BackRequested;
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
                    Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested -= Selector_BackRequested;
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
                    Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested -= Selector_BackRequested;
                    Frame.Navigate(typeof(Match));
                }
            }
        }

        private void deleteMode_Checked(object sender, RoutedEventArgs e)
        {
            itemCount.Text = App.language.GetString("delMode");
        }

        private void deleteMode_Unchecked(object sender, RoutedEventArgs e)
        {
            itemCount.Text = Utilities.notes.Count == 0 ? App.language.GetString("fileNo") : $"{App.language.GetString("fileYes")}: {Utilities.notes.Count}";
        }
    }
}
