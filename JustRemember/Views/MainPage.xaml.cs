using JustRemember.Services;
using JustRemember.ViewModels;

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
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.Initialize();
            ViewModel.wr = this;
            ViewModel2.Initialize();
			ViewModel2.view = this;
			MobileTitlebarService.Refresh();
            base.OnNavigatedTo(e);
        }

        public ListView WAR
        {
            get { return workAround; }
            set
            {
                workAround = value;
            }
        }

		public ListView SSL
		{
			get { return sessionList; }
			set { sessionList = value; }
		}

		private void sessionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ViewModel2.UpdateSelection(e);
		}

		private void sessionList_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
		{
			ViewModel2.DoubleClickSession(e);
		}

		private void RefreshSession(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			ViewModel2.RefrehListAsync(e);
		}

		private void DeleteSelectedSesion(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			ViewModel2.DeleteSelectedSession(e);
		}

		private void DeSelectSession(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			ViewModel2.DeSelectSession(e);
		}

		private void OpenSession(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			ViewModel2.OpenSessionMenu(e);
		}

		private void GotoSettings(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			NavigationService.Navigate<AppConfigView>();
		}

		private void GoToPrenote(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			NavigationService.Navigate<PrenoteView>();
		}

		private void GoToNoteEditor(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			NavigationService.Navigate<NoteEditorView>();
		}

		private async void changePage(Pivot sender, PivotItemEventArgs args)
		{
			if (sender.SelectedIndex == 0)
				await MobileTitlebarService.Refresh(App.language.GetString("Home_note"));
			else if (sender.SelectedIndex == 1)
				await MobileTitlebarService.Refresh(App.language.GetString("Home_session"));
		}
	}
}
