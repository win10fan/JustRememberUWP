using System;

using JustRemember.Services;

using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using JustRemember.Models;
using System.Collections.ObjectModel;
using Windows.Globalization;
using Windows.ApplicationModel.Resources;
using Windows.UI.ViewManagement;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;

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
			_cfg = ApplicationData.Current.LocalSettings;
			if (_cfg.Values.Count < 3)
			{
				Config = new AppConfigModel();
			}
			Config = AppConfigModel.GetSettings();
			Config.isInitialize = true;
			InitializeComponent();
			ApplicationLanguages.PrimaryLanguageOverride = AppConfigModel.languages[Config.language];
			language = ResourceLoader.GetForViewIndependentUse();
			//Deferred execution until used. Check https://msdn.microsoft.com/library/dd642331(v=vs.110).aspx for further info on Lazy<T> class.
			_activationService = new Lazy<ActivationService>(CreateActivationService);
		}
		
		public static AppConfigModel Config;
		public static ObservableCollection<StatModel> Stats;
		public static ResourceLoader language;
		public static bool isDeploying;
		public static SessionModel cachedSession;
		public static NoteModel selectedNote;
		public static ApplicationDataContainer _cfg;
		//TODO:Make time limit longer

		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used such as when the application is launched to open a specific file.
		/// </summary>
		/// <param name="e">Details about the launch request and process.</param>
		protected override async void OnLaunched(LaunchActivatedEventArgs e)
		{
			if (!Config.isOpenBefore)
			{
				if (MobileTitlebarService.isMobile)
					DetectResolution();
				else
					Config.halfResolution = 700;
				Config.isOpenBefore = false;
			}
			await ThemeSelectorService.SetThemeAsync(Config.isItLightTheme ? ElementTheme.Light : ElementTheme.Dark);
			MobileTitlebarService.Refresh();
			Stats = await StatModel.Get();
			if (!e.PrelaunchActivated)
			{
				await ActivationService.ActivateAsync(e);
			}
			AnnoyPlayer.Initialize();
		}

		private void DetectResolution()
		{
			var viw = ApplicationView.GetForCurrentView();
			var res = new List<double>() { viw.VisibleBounds.Width, viw.VisibleBounds.Height };
			var max = res.Max() / 2;
			if (max < res.Min())
			{
				Config.halfResolution = max + (res.Min() / 2);
			}
			else
			{
				Config.halfResolution = max;
			}
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