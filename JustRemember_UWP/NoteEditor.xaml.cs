﻿using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace JustRemember_UWP
{
	public sealed partial class NoteEditor : Page
	{
		public NoteEditor()
		{
			InitializeComponent();
            opennedFile = null;
			abt = new MessageDialog("Note editor 1.3", "About");
			abt.Commands.Add(new UICommand("OK") { Id = 0 });
			abt.CancelCommandIndex = 0;
			newfilewarn = new MessageDialog("This will clear all your text", "Are you sure?");
			newfilewarn.Commands.Add(new UICommand("OK") { Invoked = delegate { textBox.Text = ""; opennedFile = null; } });
			newfilewarn.Commands.Add(new UICommand("Cancel") { Id = 0 });
			newfilewarn.CancelCommandIndex = 0;
			fileNotSaved = new MessageDialog("Do you want to save this change?", "File note saved");
			fileNotSaved.Commands.Add(new UICommand("Yes") { Invoked = delegate { /*Save progress*/ } });
			fileNotSaved.Commands.Add(new UICommand("No") { Invoked = delegate { Frame.Navigate(typeof(Selector)); } });
			appCommandActiveGroup = menuPage.Home;
		}
		MessageDialog abt;
		MessageDialog newfilewarn;
		MessageDialog fileNotSaved;

        StorageFile _file = null;
		public StorageFile opennedFile
		{
			get
			{
				return _file;
			}
			set
			{
				if (value == null)
				{
					editorTitle.Text = "Untitled - Note Editor";
				}
				else
				{
					editorTitle.Text = $"{value.Name} - Note Editor";
				}
				_file = value;
			}
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			textBox.Text = "";
		}

		private async void quit_Click(object sender, RoutedEventArgs e)
		{
            if (opennedFile != null)
            {
                await fileNotSaved.ShowAsync();
                return;
            }
            textBox.Text = "";
			Frame.Navigate(typeof(Selector));
		}

		private async void about_Click(object sender, RoutedEventArgs e)
		{
			await abt.ShowAsync();
		}

		private async void AppBarButton_Click(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrEmpty(textBox.Text))
			{
				await newfilewarn.ShowAsync();
			}
			else
			{
				textBox.Text = string.Empty;
                opennedFile = null;
			}
		}

		private async void clipboard_Click(object sender, RoutedEventArgs e)
		{
			string value = await Clipboard.GetContent().GetTextAsync();
			if (!string.IsNullOrEmpty(value))
			{
				textBox.Text += value;
			}
		}

		private void tab_Click(object sender, RoutedEventArgs e)
		{
			textBox.Text += "\t";
		}

		private void fileRoot_Click(object sender, RoutedEventArgs e)
		{
			appCommandActiveGroup = menuPage.File;
		}

		public enum menuPage
		{
			Home,
			File,
			Insert,
			Set
		}
		menuPage _nowPage;
		public menuPage appCommandActiveGroup
		{
			get
			{
				return _nowPage;
			}
			set
			{
				fileRoot.Visibility = value == menuPage.Home ? Visibility.Visible : Visibility.Collapsed;
				insertRoot.Visibility = value == menuPage.Home ? Visibility.Visible : Visibility.Collapsed;
				settingRoot.Visibility = value == menuPage.Home ? Visibility.Visible : Visibility.Collapsed;
				newFile.Visibility = value == menuPage.File ? Visibility.Visible : Visibility.Collapsed;
				openFile.Visibility = value == menuPage.File ? Visibility.Visible : Visibility.Collapsed;
				saveFile.Visibility = value == menuPage.File ? Visibility.Visible : Visibility.Collapsed;
				clipboardInsert.Visibility = value == menuPage.Insert ? Visibility.Visible : Visibility.Collapsed;
				newlineInsert.Visibility = value == menuPage.Insert ? Visibility.Visible : Visibility.Collapsed;
				tabInsert.Visibility = value == menuPage.Insert ? Visibility.Visible : Visibility.Collapsed;
				fontinc.Visibility = value == menuPage.Set ? Visibility.Visible : Visibility.Collapsed;
				fontdec.Visibility = value == menuPage.Set ? Visibility.Visible : Visibility.Collapsed;
				setSet.Visibility = value == menuPage.Set ? Visibility.Visible : Visibility.Collapsed;
				menuback.Visibility = value != menuPage.Home ? Visibility.Visible : Visibility.Collapsed;
				_nowPage = value;
			}
		}

		private void insertRoot_Click(object sender, RoutedEventArgs e)
		{
			appCommandActiveGroup = menuPage.Insert;
		}

		private void menuback_Click(object sender, RoutedEventArgs e)
		{
			appCommandActiveGroup = menuPage.Home;
		}

		private void settingRoot_Click(object sender, RoutedEventArgs e)
		{
			appCommandActiveGroup = menuPage.Set;
		}

		private void newFile_Click(object sender, RoutedEventArgs e)
		{
			opennedFile = null;
            textBox.Text = "";
		}

		private void newlineInsert_Click(object sender, RoutedEventArgs e)
		{
			textBox.Text += Environment.NewLine;
		}

		public static int[] fontSizes = new int[] { 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 36, 48, 72, 94, 108 };
		public int selected
		{
			get
			{
				return _sel;
			}
			set
			{
				if (value < 0) { value = 0; }
				if (value > fontSizes.Length - 1) { value = 0; }
				textBox.FontSize = fontSizes[value];
				_sel = value;
			}
		}
		int _sel;

		private void fontdec_Click(object sender, RoutedEventArgs e)
		{
			selected -= 1;
		}

		private void fontinc_Click(object sender, RoutedEventArgs e)
		{
			selected += 1;
		}

        private async void openFile_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(".txt");

            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                //Got a file
                opennedFile = file;
                textBox.Text = await FileIO.ReadTextAsync(file);
            }
        }

        private void saveFile_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
