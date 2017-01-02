using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

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
			var selectors = new List<SelectorItem>();
			if (Utilities.notes != null)
			{
				foreach (var n in Utilities.notes)
				{
					SelectorItem si = new SelectorItem(n);
					selectors.Add(si);
				}
			}
            if (Utilities.sessions != null)
            {
                foreach (var n in Utilities.sessions)
                {
                    SelectorItem si = new SelectorItem(n);
                    selectors.Add(si);
                }
            }
            listView.ItemsSource = selectors;
            //
            if (deleteMode.IsChecked == true)
            {
                itemCount.Text = App.language.GetString("delMode");
            }
            else if (deleteMode.IsChecked == false)
            {
                itemCount.Text = $"{App.language.GetString("fileYes")}: {Utilities.notes.Count} | {App.language.GetString("note1")}: {Utilities.sessions.Count}";
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
            var data = (SelectorItem)listView.SelectedItem;
            if (data == null) { return; }
            Utilities.selected = data;
            if (!Utilities.isSmallLoaderMode)
            {
                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested -= Selector_BackRequested;
                Frame.Navigate(typeof(Match));
            }
        }

        private void listView_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            var data = (SelectorItem)listView.SelectedItem;
            if (data == null) { return; }
            Utilities.selected = data;
            if (!Utilities.isSmallLoaderMode)
            {
                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested -= Selector_BackRequested;
                Frame.Navigate(typeof(Match));
            }
        }

        private void deleteMode_Checked(object sender, RoutedEventArgs e)
        {
            itemCount.Text = App.language.GetString("delMode");
        }

        private void deleteMode_Unchecked(object sender, RoutedEventArgs e)
        {
            itemCount.Text = $"{App.language.GetString("fileYes")}: {Utilities.notes.Count} | {App.language.GetString("note1")}: {Utilities.sessions.Count}";
        }
    }
}
