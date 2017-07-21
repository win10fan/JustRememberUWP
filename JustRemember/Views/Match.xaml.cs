using JustRemember.Models;
using JustRemember.Services;
using JustRemember.ViewModels;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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
			dbg = App.Config.showDebugging ? Visibility.Visible : Visibility.Collapsed;
			this.InitializeComponent();
			this.KeyDown += Match_KeyDown;
#if DEBUG
#else
			dhr.Opacity = 1;			
#endif
		}

		string mode = "M";
		public string modeDetection
		{
			get => mode;
			set
			{
				mode = value;
				if (ViewModel.isPausing)
				{
					ViewModel.UnPauseFunc.Execute(null);
				}
			}
		}

		private void Match_KeyDown(object sender, KeyRoutedEventArgs e)
		{
			if (App.Config.choiceStyle != choiceDisplayMode.Write && !ViewModel.isPausing)
			{
				if (e.Key == Windows.System.VirtualKey.Number1)
					ViewModel.Choose(0);
				if (e.Key == Windows.System.VirtualKey.Number2)
					ViewModel.Choose(1);
				if (e.Key == Windows.System.VirtualKey.Number3)
					ViewModel.Choose(2);
				if (e.Key == Windows.System.VirtualKey.Number4 && ViewModel.totalChoice >= 4)
					ViewModel.Choose(3);
				if (e.Key == Windows.System.VirtualKey.Number5 && ViewModel.totalChoice >= 5)
					ViewModel.Choose(4);
			}
		}

		public SessionViewModel ViewModel { get; } = new SessionViewModel();
		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			while (NavigationService.Frame.BackStack.Last().SourcePageType != typeof(MainPage))
			{
				NavigationService.Frame.BackStack.RemoveAt(NavigationService.Frame.BackStack.Count - 1);
				Debug.Write(NavigationService.Frame.BackStack.Count);
			}
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
			//
			if (ViewModel.current.StatInfo.correctedChoice.Count < 1)
			{
				foreach (var item in ViewModel.current.choices)
				{
					ViewModel.current.StatInfo.correctedChoice.Add(item.corrected);
				}
			}
			base.OnNavigatedTo(e);
		}

		private void Writing(object sender, KeyRoutedEventArgs e)
		{
			if (e.Key == Windows.System.VirtualKey.Space)
			{
				ViewModel.writeUp = writeBox.Text.Trim();
			}
		}

		private void SubmitText(object sender, RoutedEventArgs e)
		{
			ViewModel.writeUp = writeBox.Text.Trim();
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

		public TextBox writebx
		{
			get => writeBox;
			set => writeBox = value;
		}

		Visibility dbg;
		public Visibility debugShow
		{
			get => dbg;
			set => dbg = value;
		}
	}
}