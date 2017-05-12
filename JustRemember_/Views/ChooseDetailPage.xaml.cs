using JustRemember_.Models;
using JustRemember_.ViewModels;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace JustRemember_.Views
{
    public sealed partial class ChooseDetailPage : Page
    {
        public ChooseDetailViewModel ViewModel { get; } = new ChooseDetailViewModel();
        public ChooseDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.Item = e.Parameter as SampleModel;
        }
    }
}
