using JustRemember.Models;
using JustRemember.Services;
using JustRemember.ViewModels;
using System;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace JustRemember.Views
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class NoteEditorView : Page
	{
		public NoteEditorViewModel editor { get; } = new NoteEditorViewModel();
		public NoteEditorView()
		{
			this.InitializeComponent();
			menuPane.Height = new GridLength(0);
		}

		protected async override void OnNavigatedTo(NavigationEventArgs e)
		{
			if (e.Parameter is NoteModel note)
			{
				editor.editedNote = note;
				editor.NoteName = note.Title;
				editor.NoteContent = note.Content;
			}
			else
			{
				if (App.attentionReq)
				{
					NoteModel note2 = App.selectedNote;
					editor.editedNote = note2;
					editor.NoteName = note2.Title;
					editor.NoteContent = note2.Content;
					App.attentionReq = false;
				}
				else
				{
					editor.editedNote = new NoteModel();
					editor.NoteName = "Untitled";
					editor.NoteContent = "";
				}
			}
			editor.ReNew();
			editor.view = this;
			mainEdit.Document.SetText(Windows.UI.Text.TextSetOptions.UnicodeBidi, editor.NoteContent);
			mainEdit.Document.ApplyDisplayUpdates();
			mainEdit.Document.BeginUndoGroup();
			editor.isNew = editor.NoteName == "Untitled";
			if (editor.isNew)
			{
				var notes = await NoteModel.GetNotesAsync();
				editor.fileList = new List<string>();
				foreach (var n in notes)
				{
					editor.fileList.Add(n.Title);
				}
			}
			await MobileTitlebarService.Refresh(editor.NoteName);
			base.OnNavigatedTo(e);
		}

		bool _menu;
		public bool ShowMenu
		{
			get => _menu;
			set => _menu = value;
		}
		private void AppBarButton_Click(object sender, RoutedEventArgs e)
		{
			ShowMenu = !ShowMenu;
			if (!ShowMenu)
			{
				hideMenu.Stop();
				showMenu.Begin();
				menuPane.Height = new GridLength(0);
				return;
			}
			else
			{
				showMenu.Stop();
				hideMenu.Begin();
				menuPane.Height = new GridLength(1, GridUnitType.Star);
				return;
			}
		}

		public RichEditBox MainEditBox
		{
			get => mainEdit;
			set => mainEdit = value;
		}

		public TextBox fileInput
		{
			get => fileDefine;
		}
	}
}
