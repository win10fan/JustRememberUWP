using JustRemember.Services;
using JustRemember.ViewModels;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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
			vm.Initialize();
			vm.v = this;
			base.OnNavigatedTo(e);
		}

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
