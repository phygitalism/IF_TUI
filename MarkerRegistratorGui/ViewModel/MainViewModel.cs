﻿using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using MarkerRegistratorGui.Model;
using Reactive.Bindings;

namespace MarkerRegistratorGui.ViewModel
{
	public class MainViewModel
	{
		private readonly ModelRoot _modelRoot = new ModelRoot();

		public ScaleAdapter ScaleAdapter { get; }
		public MarkerRegistrationViewModel MarkerRegistration { get; }
		public MarkerTrackingViewModel MarkerTracking { get; }
		public PointersViewModel Pointers { get; }

		public ICommand ExitCommand { get; }
			= new ReactiveCommand<Window>()
				.WithSubscribe(window => window.Close());

		public MainViewModel()
		{
			ScaleAdapter = new ScaleAdapter();
			MarkerRegistration = new MarkerRegistrationViewModel(
				_modelRoot.RegistrationService,
				_modelRoot.RegistrationField,
				ScaleAdapter
			);
			MarkerTracking = new MarkerTrackingViewModel(_modelRoot.TrackingService, ScaleAdapter);
			Pointers = new PointersViewModel(_modelRoot.TrackingService);

			Dispatcher.CurrentDispatcher.ShutdownStarted += (sender, e) => Dispose();

			_modelRoot.Start();
		}

		private void Dispose()
		{
			Debug.WriteLine("Disposing");
			_modelRoot.Stop();

			MarkerRegistration.Dispose();
			MarkerTracking.Dispose();
		}
	}
}
