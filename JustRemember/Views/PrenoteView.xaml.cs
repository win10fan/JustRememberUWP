using JustRemember.Services;
using JustRemember.ViewModels;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppExtensions;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System;
using Windows.Storage;
using System.IO;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace JustRemember.Views
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class PrenoteView : Page
	{
		public PrenoteViewModel vm { get; } = new PrenoteViewModel();
		public PrenoteView()
		{
			this.InitializeComponent();
		}
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			if (!PrenoteService.isDeployed)
			{
				PrenoteService.DeployPrenote();
			}
			//Check prenote from extensions
			GetNotesFromExtension();
			vm.Initialize();
			vm.v = this;
			MobileTitlebarService.Refresh();
			base.OnNavigatedTo(e);
		}
		
		public async void GetNotesFromExtension()
		{
			//Get extensions
			var moarExt = AppExtensionCatalog.Open("rememberit.notes");
			var allExts = await moarExt.FindAllAsync();
			//Try to get notes from extension
			if (allExts.Count < 1) { return; }
			foreach (var ext in allExts)
			{
				//Verify if this extension note is already deploy
				var depChk = (StorageFolder)await ApplicationData.Current.LocalFolder.TryGetItemAsync("Bundled memos");
				if (File.Exists($"{depChk.Path}\\{ext.DisplayName}.dep"))
				{
					//Extension is already exist
					continue;
				}
				else
				{
					//Deploy note
					await PrenoteService.DeployMemoFromExtension(await ext.GetPublicFolderAsync(), ext.DisplayName);
				}
			}
		}

		public ProgressBar extracting { get => progressInfo; }

		public ListView FileList
		{
			get => fl;
			set => fl = value;
		}

		private void NavTo(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
		{
			vm.navTo.Execute(e);
		}

		private void NavUp(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			vm.navUp.Execute(e);
		}

		private async void navigateToDat(object sender, SelectionChangedEventArgs e)
		{
			if (pathItem.SelectedIndex > -1)
			{
				vm.NavToAccordingToWhatYouBeenClickOnPathList(pathItem.SelectedIndex);
				await Task.Delay(500);
				scItem.ChangeView(0, scItem.HorizontalOffset, scItem.ZoomFactor);
			}
		}

		private void RefreshList(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			vm.Refresh();
		}

		private void CopyNEdit(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			vm.Edit();
		}

		private void Open(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			vm.navTo.Execute(null);
		}
	}
}
