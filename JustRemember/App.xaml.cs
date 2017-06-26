using System;

using JustRemember.Services;

using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using JustRemember.Models;
using JustRemember.Helpers;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Media;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace JustRemember
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	sealed partial class App : Application
	{
		private Lazy<ActivationService> _activationService;
		private ActivationService ActivationService { get { return _activationService.Value; } }

		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App()
		{
			InitializeComponent();

			//Deferred execution until used. Check https://msdn.microsoft.com/library/dd642331(v=vs.110).aspx for further info on Lazy<T> class.
			_activationService = new Lazy<ActivationService>(CreateActivationService);
		}

		public static AppConfigModel Config;
		public static ObservableCollection<StatModel> Stats;

		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used such as when the application is launched to open a specific file.
		/// </summary>
		/// <param name="e">Details about the launch request and process.</param>
		protected override async void OnLaunched(LaunchActivatedEventArgs e)
		{
			Config = await AppConfigModel.Load2();
			await ThemeSelectorService.SetThemeAsync(Config.isItLightTheme ? ElementTheme.Light : ElementTheme.Dark);
			await MobileTitlebarService.Refresh("", Resources["SystemControlBackgroundAccentBrush"], Resources["ApplicationForegroundThemeBrush"]);
			Stats = await StatModel.Get();
			if (!e.PrelaunchActivated)
			{
				await ActivationService.ActivateAsync(e);
			}
			AnnoyPlayer.Initialize();
			await Task.Run(async () => { while (true) { await Config.Save(); await Task.Delay(TimeSpan.FromSeconds(3)); } });
		}
		
		/// <summary>
		/// Invoked when the application is activated by some means other than normal launching.
		/// </summary>
		/// <param name="args">Event data for the event.</param>
		protected override async void OnActivated(IActivatedEventArgs args)
		{
			await ActivationService.ActivateAsync(args);
		}

		private ActivationService CreateActivationService()
		{
			return new ActivationService(this, typeof(Views.MainPage));
		}
	}
}