using JustRemember.Models;
using JustRemember.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
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
    public sealed partial class AudioDescription : Page
    {
		public AudioSplitViewModel vm { get; } = new AudioSplitViewModel();
        public AudioDescription()
        {
            this.InitializeComponent();
			needNote.Opacity = 1;
			ApplicationView.GetForCurrentView().VisibleBoundsChanged += AudioDescription_VisibleBoundsChanged;
			mdControl.Width = ApplicationView.GetForCurrentView().VisibleBounds.Width - 40;
			mdControl.MediaOpened += MdControl_MediaOpened;
			customcontrol.Backward100Millisec += Customcontrol_Backward100Millisec;
			customcontrol.Forward100Millisec += Customcontrol_Forward100Millisec;
		}

		private void Customcontrol_Forward100Millisec(object sender, EventArgs e)
		{
			if (mdControl.NaturalDuration.TimeSpan > TimeSpan.FromSeconds(1))
				mdControl.Position -= TimeSpan.FromMilliseconds(100);
		}

		private void Customcontrol_Backward100Millisec(object sender, EventArgs e)
		{
			if (mdControl.NaturalDuration.TimeSpan > TimeSpan.FromSeconds(1))
				mdControl.Position -= TimeSpan.FromMilliseconds(100);
		}

		private void MdControl_MediaOpened(object sender, RoutedEventArgs e)
		{
			FrameworkElement transportControlsTemplateRoot = (FrameworkElement)VisualTreeHelper.GetChild(mdControl.TransportControls, 0);
			Slider sliderControl = (Slider)transportControlsTemplateRoot.FindName("ProgressSlider");
			if (sliderControl != null && mdControl.NaturalDuration.TimeSpan.TotalMinutes > 120)
			{
				// Default is 1%. Change to 0.1% for more granular seeking.
				sliderControl.StepFrequency = 0.1;
			}
		}

		private void AudioDescription_VisibleBoundsChanged(ApplicationView sender, object args)
		{
			mdControl.Width = sender.VisibleBounds.Width - 40;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			vm.Initialize(e.Parameter);
			base.OnNavigatedTo(e);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			mdControl.MediaOpened -= MdControl_MediaOpened;
			base.OnNavigatedFrom(e);
		}

		public MediaElement mdControl { get => bgControl; }
		public Storyboard RotateIcon { get => rotateIcon; }
		public CompositeTransform AngleIcon { get => angleIcon; }

		private void SetTime(object sender, RoutedEventArgs e)
		{
			string id = (sender as Button).Tag.ToString();
			int index = AudioSplitItem.Hunt(vm.Splits, id);
			if (index > -1)
				vm.Splits[index].Time = mdControl.Position;
			Refresh(index);
		}

		private void ResetTime(object sender, RoutedEventArgs e)
		{
			string id = (sender as Button).Tag.ToString();
			int index = AudioSplitItem.Hunt(vm.Splits, id);
			if (index > -1)
				vm.Splits[index].Time = TimeSpan.FromSeconds(-1);
			Refresh(index);
		}

		private void SeekTo(object sender, RoutedEventArgs e)
		{
			string id = (sender as Button).Tag.ToString();
			int index = AudioSplitItem.Hunt(vm.Splits, id);
			if (index > -1)
				mdControl.Position = vm.Splits[index].Time;
		}

		void Refresh(int index)
		{
			var item = vm.Splits[index];
			vm.Splits.RemoveAt(index);
			vm.Splits.Insert(index, item);
		}
	}

	public class CustomAudiControl : MediaTransportControls
	{
		public event EventHandler<EventArgs> Forward100Millisec;
		public event EventHandler<EventArgs> Backward100Millisec;

		public CustomAudiControl()
		{
			DefaultStyleKey = typeof(CustomAudiControl);
			IsPlaybackRateButtonVisible = false;
			IsFullWindowButtonVisible = false;
			IsFullWindowButtonVisible = false;
			IsNextTrackButtonVisible = false;
			IsSeekBarVisible = true;
			IsSeekEnabled = true;
			IsSkipBackwardButtonVisible = false;
			IsSkipForwardButtonVisible = false;
			IsZoomButtonVisible = false;
			IsVolumeButtonVisible = true;
			IsCompact = true;
		}

		protected override void OnApplyTemplate()
		{
			// Find the custom button and create an event handler for its Click event.
			var fw1mil = GetTemplateChild("ForwardOneMil") as Button;
			var bw1mil = GetTemplateChild("BackwardOneMil") as Button;
			//var likeButton = GetTemplateChild("LikeButton") as Button;
			fw1mil.Click += Fw1mil_Click;
			bw1mil.Click += Bw1mil_Click;
			//likeButton.Click += LikeButton_Click;
			base.OnApplyTemplate();
		}

		private void Bw1mil_Click(object sender, RoutedEventArgs e)
		{
			Backward100Millisec?.Invoke(sender, null);
		}

		private void Fw1mil_Click(object sender, RoutedEventArgs e)
		{
			Forward100Millisec?.Invoke(sender, null);
		}
	}
}