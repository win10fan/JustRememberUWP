using JustRemember.Helpers;
using JustRemember.Models;
using JustRemember.Services;
using System;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.AppExtensions;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace JustRemember.ViewModels
{
	public class ExtensionViewModel : Observable
    {
		public AppExtensionCatalog Notecatalog;
		private CoreDispatcher _dispatcher;
		ObservableCollection<Extension> _ext;
		public ObservableCollection<Extension> Extensions
		{
			get => _ext;
			set => Set(ref _ext, value);
		}
		
		public void InitializeDispatch()
		{

			#region Error Checking & Dispatcher Setup
			// check that we haven't already been initialized
			if (_dispatcher != null)
			{
				//throw new ExtensionManagerException("Extension Manager for " + this.Contract + " is already initialized.");
			}

			// thread that initializes the extension manager has the dispatcher
			_dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;
			#endregion

			Notecatalog = AppExtensionCatalog.Open("rememberit.notes");
			Notecatalog.PackageInstalled += NewExtensionInstalled;
			Notecatalog.PackageUninstalling += UpdateExt;
			GetAllExtensions();
		}

		private async void UpdateExt(AppExtensionCatalog sender, AppExtensionPackageUninstallingEventArgs args)
		{
			await _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{
				GetAllExtensions();
			});
		}

		private async void NewExtensionInstalled(AppExtensionCatalog sender, AppExtensionPackageInstalledEventArgs args)
		{
			await _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{
				GetAllExtensions();
			});
		}

		public Visibility noExt
		{
			get
			{
				if (Extensions == null)
				{
					return Visibility.Visible;
				}
				if (Extensions?.Count < 1)
				{
					return Visibility.Visible;
				}
				return Visibility.Collapsed;
			}
		}

		public async void GetAllExtensions()
		{
			Notecatalog = AppExtensionCatalog.Open("rememberit.notes");
			//Get all note extension
			var allext = await Notecatalog.FindAllAsync();
			if (allext?.Count > 0)
			{
				Extensions = await ExtensionService.GetExtension(allext, ExtensionType.Notes);
			}
			OnPropertyChanged(nameof(noExt));
		}
	}
}