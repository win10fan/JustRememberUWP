using JustRemember.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace JustRemember
{
	public sealed partial class Selection : UserControl, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
		{
			if (Equals(storage, value))
			{
				return;
			}

			storage = value;
			OnPropertyChanged(propertyName);
		}

		void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		public Selection()
		{
			this.InitializeComponent();
		}

		bool chk;
		public bool chkd
		{
			get => chk;
			set
			{
				Set(ref chk, value);
				OnPropertyChanged(nameof(isCheck));
			}
		}

		Visibility isCheck
		{
			get => chkd ? Visibility.Visible : Visibility.Collapsed;
		}

		public DependencyProperty isChecked = DependencyProperty.RegisterAttached("isChecked", typeof(bool), typeof(Selection), new PropertyMetadata(false, CheckedPropertyChanged));

		private static void CheckedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{

		}
	}
}
