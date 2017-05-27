using JustRemember_.ViewModels;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace JustRemember_.Views
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
    }
}
