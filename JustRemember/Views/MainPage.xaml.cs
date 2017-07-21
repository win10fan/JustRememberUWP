using JustRemember.Models;
using JustRemember.Services;
using JustRemember.ViewModels;
using System.Diagnostics;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace JustRemember.Views
{
	public sealed partial class MainPage : Page
	{
		public NotesViewModel ViewModel { get; } = new NotesViewModel();
		public SavedSessionViewModel ViewModel2 { get; } = new SavedSessionViewModel();
		public MainPage()
		{
			InitializeComponent();
			ApplicationView.GetForCurrentView().VisibleBoundsChanged += MainPage_VisibleBoundsChanged;
		}

		private void MainPage_VisibleBoundsChanged(ApplicationView sender, object args)
		{
			if (sender.VisibleBounds.Width >= 700)
				MobileTitlebarService.Refresh();
			else
				changePage(mainPivot, null);
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			if (!App.Config.showDebugging)
			{
				App.Config.antiSpamChoice = true;
			}
			ViewModel.Initialize();
			ViewModel2.Initialize();
			MobileTitlebarService.Refresh();
			NavigationService.Frame.BackStack.Clear();
			base.OnNavigatedTo(e);
		}

		private async void changePage(Pivot sender, PivotItemEventArgs args)
		{
			if (sender == null) { return; }
			if (sender.SelectedIndex == 0)
				await MobileTitlebarService.Refresh(App.language.GetString("Home_note"));
			else if (sender.SelectedIndex == 1)
				await MobileTitlebarService.Refresh(App.language.GetString("Home_session"));
		}

		
	}
}