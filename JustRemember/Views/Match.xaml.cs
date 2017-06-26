using JustRemember.Models;
using JustRemember.Services;
using JustRemember.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace JustRemember.Views
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class Match : Page
	{
		public static SessionModel transfer;
		public Match()
		{
			dbg = Visibility.Collapsed;
#if DEBUG
			dbg = Visibility.Visible;
#endif
			this.InitializeComponent();
		}
		public SessionViewModel ViewModel { get; } = new SessionViewModel();
		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			ViewModel.current = (SessionModel)e.Parameter;
			ViewModel.view = this;
			ViewModel.RestoreSession();
			//ViewModel.isPausing = false;
			if (MobileTitlebarService.isMobile)
			{
				await MobileTitlebarService.Refresh(ViewModel.current.StatInfo.noteTitle, Resources["SystemControlPageBackgroundBaseLowBrush"], Resources["SystemControlForegroundBaseLowBrush"]);
			}
			else
			{
				await MobileTitlebarService.Refresh(ViewModel.current.StatInfo.noteTitle);
			}
			base.OnNavigatedTo(e);
		}
		
		/// <summary>
		/// The scroll area that use to place display textblock selected choice
		/// </summary>
		public ScrollViewer displayTexts
		{
			get => displayTextScroll; set => displayTextScroll = value;
		}

		/// <summary>
		/// textblock that use to display selected choice
		/// </summary>
		public TextBlock displayTXT
		{
			get { return dpTxt; }
			set { dpTxt = value; }
		}

		/// <summary>
		/// Animation from normal to paused grid
		/// </summary>
		public Storyboard Pause
		{
			get { return startPause; }
			set { startPause = value; }
		}

		/// <summary>
		/// Animation from paused to normal
		/// </summary>
		public Storyboard UnPause
		{
			get { return stopPause; }
			set { stopPause = value; }
		}

		Visibility dbg;
		public Visibility debugShow
		{
			get => dbg;
			set => dbg = value;
		}
	}
}