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
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace JustRemember_.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Match : Page
    {
        public Match()
        {
            this.InitializeComponent();
        }
        public SessionViewModel ViewModel { get; } = new SessionViewModel();
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter.GetType() == typeof(NoteModel))
            {
                ViewModel.InitializeNew((NoteModel)e.Parameter);
            }
            else if (e.Parameter.GetType() == typeof(SessionModel))
            {
                ViewModel.RestoreSession((SessionModel)e.Parameter);
            }
            else
            {
                //Unknow??
            }
            ViewModel.view = this;
            base.OnNavigatedTo(e);
        }

        public ScrollViewer displayTexts
        {
            get => displayTextScroll; set => displayTextScroll = value;
        }
    }
}
