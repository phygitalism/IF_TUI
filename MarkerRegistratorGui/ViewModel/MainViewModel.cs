﻿using System.Diagnostics;
using System.Windows.Threading;
using MarkerRegistratorGui.Model;

namespace MarkerRegistratorGui.ViewModel
{
	public class MainViewModel
	{
		private readonly ModelRoot _modelRoot = new ModelRoot()
		{
			TrackingService = new TuioTrackingService()
		};

		public ScaleAdapter ScaleAdapter { get; }
		public MarkerRegistrationViewModel MarkerRegistration { get; }
		public MarkerTrackingViewModel MarkerTracking { get; }
		public PointersViewModel Pointers { get; }

		public MainViewModel()
		{
			var dummyRegistration = new DummyRegistrationService();
			_modelRoot.RegistrationField = dummyRegistration;
			_modelRoot.RegistrationService = dummyRegistration;

			ScaleAdapter = new ScaleAdapter();
			MarkerRegistration = new MarkerRegistrationViewModel(
				_modelRoot.RegistrationService,
				_modelRoot.RegistrationField,
				ScaleAdapter
			);
			MarkerTracking = new MarkerTrackingViewModel(_modelRoot.TrackingService, ScaleAdapter);
			Pointers = new PointersViewModel(_modelRoot.TrackingService);

			Dispatcher.CurrentDispatcher.ShutdownStarted += (sender, e) => Dispose();

			_modelRoot.TrackingService.Start();
		}

		public void Dispose()
		{
			Debug.WriteLine("Disposing");
			_modelRoot.TrackingService.Stop();
		}
	}
}
