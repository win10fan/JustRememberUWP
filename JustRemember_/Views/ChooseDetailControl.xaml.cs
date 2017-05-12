using JustRemember_.Models;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace JustRemember_.Views
{
    public sealed partial class ChooseDetailControl : UserControl
    {
        public SampleModel MasterMenuItem
        {
            get { return GetValue(MasterMenuItemProperty) as SampleModel; }
            set { SetValue(MasterMenuItemProperty, value); }
        }

        public static DependencyProperty MasterMenuItemProperty = DependencyProperty.Register("MasterMenuItem",typeof(SampleModel),typeof(ChooseDetailControl),new PropertyMetadata(null));

        public ChooseDetailControl()
        {
            InitializeComponent();
        }
    }
}
