using Windows.ApplicationModel;
using Windows.UI.Xaml.Controls;

namespace JustRemember.Views
{
    public sealed partial class WhatsNewDialog : ContentDialog
    {
        public WhatsNewDialog()
        {
            this.InitializeComponent();
			this.PrimaryButtonText = App.language.GetString("Match_dialog_ok");
        }

		PackageId app { get => Package.Current.Id; }
		public string version { get => $"{app.Version.Major}.{app.Version.Minor}.{app.Version.Revision} build {app.Version.Build}"; }
	}
}
