﻿using JustRemember.Helpers;
using JustRemember.Models;
using JustRemember.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppExtensions;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace JustRemember.ViewModels
{
    public class ExtensionViewModel : Observable
    {
		AppExtensionCatalog Notecatalog;
		private CoreDispatcher _dispatcher;
		ObservableCollection<Extension> _ext;
		public ObservableCollection<Extension> Extensions
		{
			get => _ext;
			set => Set(ref _ext, value);
		}

		public void Initialize()
		{
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
			GetAllExtensions();
		}

		private async void NewExtensionInstalled(AppExtensionCatalog sender, AppExtensionPackageInstalledEventArgs args)
		{
			await _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{
				GetAllExtensions();
			});
		}

		public async void GetAllExtensions()
		{
			Notecatalog = AppExtensionCatalog.Open("rememberit.notes");
			//Get all note extension
			var allext = await Notecatalog.FindAllAsync();
			Extensions = await ExtensionService.GetExtension(allext, ExtensionType.Notes);
			OnPropertyChanged(nameof(noExt));
		}
		
		public Visibility noExt { get => Extensions?.Count < 1 ? Visibility.Visible : Visibility.Collapsed; }

		int _selec = -1;
		public int extensionListSelection
		{
			get => _selec;
			set
			{
				Set(ref _selec, value);
				OnPropertyChanged(nameof(isSelected));
				OnPropertyChanged(nameof(SelectedExt));
			}
		}

		public Visibility isSelected
		{
			get => extensionListSelection == -1 ? Visibility.Collapsed : Visibility.Visible;
		}

		public Extension SelectedExt
		{
			get
			{
				if (extensionListSelection != -1)
				{
					return Extensions[extensionListSelection];
				}
				return new Extension();
			}
		}

		public async void RequestUninstallSelected()
		{
			if (extensionListSelection > -1)
			{
				var packName = SelectedExt.Core.DisplayName;
				var un = await Notecatalog.RequestRemovePackageAsync(SelectedExt.Core.Package.Id.FullName);
				if (un)
				{
					if (SelectedExt.extensionType == ExtensionType.Notes)
					{
						PrenoteService.RequestRemovePrenoteExtension(packName);
					}
					extensionListSelection = -1;
					GetAllExtensions();
				}
			}
		}
	}
}