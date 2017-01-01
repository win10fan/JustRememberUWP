﻿using System;
using System.IO;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Globalization;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace JustRemember_UWP
{
	sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {			
            InitializeComponent();
#if DEBUG
            if (Directory.Exists(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\Prenote"))
            {
                Directory.Delete(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\Prenote", true);
            }
#endif
            ApplicationView.PreferredLaunchViewSize = new Size(320, 240);
            if (!PrenoteLoader.isDeployed)
            {
                PrenoteLoader.DeployPrenote();
            }
			if (!Utilities.initialize)
			{
				//Utilities.systemAccent = (Color)Resources["SystemAccentColor"];
				Utilities.savedPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "settings.xml");
				Utilities.currentSettings = Settings.Load(Utilities.savedPath);
				Utilities.initialize = true;
			}
			Current.RequestedTheme = Utilities.currentSettings.theme;
            ApplicationLanguages.PrimaryLanguageOverride = Utilities.lang[Utilities.currentSettings.language];
            Suspending += OnSuspending;
            //TODO:Load language
            //if (config == Settings.Default || config.selectedLanguage == 2)
            //{
            //    //Let it be
            //}
            //else if (config.selectedLanguage != 2)
            //{
            //    ApplicationLanguages.PrimaryLanguageOverride = SettingHelper.GetSelectedLanguage(config.selectedLanguage);
            //}
            language = ResourceLoader.GetForViewIndependentUse();
        }
        public static ResourceLoader language;
        
        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                DebugSettings.EnableFrameRateCounter = false;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {

                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            deferral.Complete();
        }
    }
}