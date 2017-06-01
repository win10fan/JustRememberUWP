using JustRemember_.Models;
using JustRemember_.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace JustRemember_.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Match : Page
    {
        public static SessionModel transfer;
        public Match()
        {
            this.InitializeComponent();
        }
        public SessionViewModel ViewModel { get; } = new SessionViewModel();
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.current = new SessionModel();
            ViewModel.current = (SessionModel)e.Parameter;
            ViewModel.RestoreSession();
            ViewModel.view = this;
			ViewModel.isPausing = false;
			if (!ViewModel.current.isNew)
			{
				//Reload text display
				for (int i = 0; i < ViewModel.currentChoice; i++)
				{
					ViewModel.AddTextDisplay(i);
				}
			}
            base.OnNavigatedTo(e);
        }

        public ScrollViewer displayTexts
        {
            get => displayTextScroll; set => displayTextScroll = value;
        }

        private void Choice0(object sender, RoutedEventArgs e)
        {
            ViewModel.Choose(0);
        }

        private void Choice1(object sender, RoutedEventArgs e)
        {
            ViewModel.Choose(1);
        }

        private void Choice2(object sender, RoutedEventArgs e)
        {
            ViewModel.Choose(2);
        }

        private void Choice3(object sender, RoutedEventArgs e)
        {
            ViewModel.Choose(3);
        }

        private void Choice4(object sender, RoutedEventArgs e)
        {
            ViewModel.Choose(4);
        }

        public TextBlock displayTXT
        {
            get { return dpTxt; }
            set { dpTxt = value; }
        }

        public Storyboard Pause
        {
            get { return startPause; }
            set { startPause = value; }
        }
        
        public Storyboard UnPause
        {
            get { return stopPause; }
            set { stopPause = value; }
        }

        private void TryPause(object sender, TappedRoutedEventArgs e)
        {
            ViewModel.isPausing = true;
        }

        private void BackToMainMenu(object sender, RoutedEventArgs e)
        {
            ViewModel.KickToMainPage();
        }

        private void SaveSession(object sender, RoutedEventArgs e)
        {
			if (ViewModel.currentChoice < 1)
			{
				return;
			}
            ViewModel.SaveToSessionList();
        }

        private void TryUnPause(object sender, RoutedEventArgs e)
        {
            ViewModel.isPausing = false;
        }
    }
}
