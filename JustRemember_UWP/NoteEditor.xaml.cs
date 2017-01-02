using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
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
			abt.Commands.Add(new UICommand(App.language.GetString("cmdOK")) { Id = 0 });
			abt.CancelCommandIndex = 0;
			newfilewarn = new MessageDialog(App.language.GetString("noteClear2"), App.language.GetString("noteClear1"));
			newfilewarn.Commands.Add(new UICommand(App.language.GetString("cmdOK")) { Invoked = delegate { textBox.Text = ""; opennedFile = null; } });
			newfilewarn.Commands.Add(new UICommand(App.language.GetString("cmdCancel")) { Id = 0 });
			newfilewarn.CancelCommandIndex = 0;
			fileNotSaved = new MessageDialog(App.language.GetString("noteSave1"), App.language.GetString("noteSave1"));
			fileNotSaved.Commands.Add(new UICommand(App.language.GetString("cmdYes")) { Invoked = delegate { SaveCurrentFile(); Frame.Navigate(typeof(Selector)); } });
			fileNotSaved.Commands.Add(new UICommand(App.language.GetString("cmdNo")) { Invoked = delegate { Frame.Navigate(typeof(Selector)); } });
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

		private async void newFile_Click(object sender, RoutedEventArgs e)
		{
            if (opennedFile != null || !string.IsNullOrEmpty(textBox.Text))
            {
                await newfilewarn.ShowAsync();
                return;
            }
            textBox.Text = "";
            opennedFile = null;
        }

		private void newlineInsert_Click(object sender, RoutedEventArgs e)
		{
			textBox.Text += Environment.NewLine;
            if (textBox.Text.Length < 1) { return; }
            textBox.SelectionStart = textBox.Text.Length;
            textBox.SelectionLength = 0;

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

        private async void saveFile_Click(object sender, RoutedEventArgs e)
        {
            if (opennedFile != null)
            {
                await FileIO.WriteTextAsync(opennedFile, textBox.Text);
            }
            else
            {
                //Ask for save location
                addNew_List_Parent.Visibility = Visibility.Visible;
            }
        }
        
        private void addNewListInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            string tocp = ((TextBox)sender).Text.ToLower();
            no_nameAlreadyExist.Visibility = GetSameText(tocp) ? Visibility.Visible : Visibility.Collapsed;
            no_emptyName.Visibility = string.IsNullOrEmpty(tocp.Trim()) ? Visibility.Visible : Visibility.Collapsed;
            no_invalidName.Visibility = IsFileValid(tocp) ? Visibility.Visible : Visibility.Collapsed;
            bool val = no_nameAlreadyExist.Visibility == Visibility.Visible || no_emptyName.Visibility == Visibility.Visible || no_invalidName.Visibility == Visibility.Visible;
            saveBTN.IsEnabled = !val;
        }

        private bool IsFileValid(string tocp)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                if (tocp.Contains(c))
                {
                    return true;
                }
            }
            return false;
        }

        private bool GetSameText(string tocp)
        {
            var dir = $"{ApplicationData.Current.RoamingFolder.Path}/Note/";
            foreach (var itm in Directory.GetFiles(dir))
            {
                if (Path.GetFileNameWithoutExtension(itm) == tocp)
                {
                    return true;
                }
            }
            return false;
        }

        async void SaveCurrentFile()
        {
            var folder = await ApplicationData.Current.RoamingFolder.GetFolderAsync("Note");
            var file = await folder.CreateFileAsync(addNewListInput.Text + ".txt", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, textBox.Text);
        }

        private async void saveBTN_Click(object sender, RoutedEventArgs e)
        {
            var folder = await ApplicationData.Current.RoamingFolder.GetFolderAsync("Note");
            var file = await folder.CreateFileAsync(addNewListInput.Text + ".txt",CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, textBox.Text);
            opennedFile = file;
            addNew_List_Parent.Visibility = Visibility.Collapsed;
            addNewListInput.Text = "";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            addNew_List_Parent.Visibility = Visibility.Collapsed;
            addNewListInput.Text = "";
        }
    }
}
