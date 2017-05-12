using JustRemember_.ViewModels;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace JustRemember_.Views
{
    public sealed partial class ChoosePage : Page
    {
        public ChooseViewModel ViewModel { get; } = new ChooseViewModel();
        public ChoosePage()
        {
            InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.LoadDataAsync(WindowStates.CurrentState);
        }
    }
}
