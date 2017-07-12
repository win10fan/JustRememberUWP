using JustRemember.ViewModels;
using JustRemember.Models;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace JustRemember.Views
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class QuestionDesignView : Page
	{
		public QuestionDesignViewModel vm { get; } = new QuestionDesignViewModel();
		public QuestionDesignView()
		{
			this.InitializeComponent();
		}

		public ListView questions
		{
			get => questionList;
		}
		public TextBox filenameInput { get => fileNameEdit; }
		public TextBlock selectedTBIndex { get => indexSelected; }

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			vm.Initialize(e.Parameter);
			DirectoryInfo dir = new DirectoryInfo($"{ApplicationData.Current.RoamingFolder.Path}\\Notes\\");
			var files = dir.GetFiles();
			foreach (var f in files)
			{
				vm.memos.Add(Path.GetFileNameWithoutExtension(f.FullName));
			}
		}

		private void MoveUp(object sender, RoutedEventArgs e)
		{
			string uid = (sender as Button).Tag.ToString();
			int index = vm.indexAt(uid) - 1;
			var item = vm.Questions[index];
			vm.Questions.RemoveAt(index);
			vm.Questions.Insert(index - 1, item);
			vm.ReIndex();
		}

		private void MoveDown(object sender, RoutedEventArgs e)
		{
			string uid = (sender as Button).Tag.ToString();
			int index = vm.indexAt(uid) - 1;
			var item = vm.Questions[index];
			vm.Questions.RemoveAt(index);
			vm.Questions.Insert(index + 1, item);
			vm.ReIndex();
		}

		private void DeleteQuestion(object sender, RoutedEventArgs e)
		{
			string uid = (sender as Button).Tag.ToString();
			int index = vm.indexAt(uid) - 1;
			vm.Questions.RemoveAt(index);
			QuestionDesignHelper.ids.Remove(uid);
			vm.ReIndex();
		}

		private void SaveContent(object sender, RoutedEventArgs e)
		{
			vm.ReIndex();
		}
	}
}
