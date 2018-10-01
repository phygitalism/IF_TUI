using System.Diagnostics;
using System.Windows.Threading;
using MarkerRegistratorGui.Model;

namespace MarkerRegistratorGui.ViewModel
{
	public class MainViewModel
	{
		private readonly ModelRoot _modelRoot = new ModelRoot()
		{
			TrackingService = new TuioTrackingService(),
			RegistrationService = new DummyRegistrationService()
		};

		public ScaleAdapter ScaleAdapter { get; }
		public MarkerRegistrationViewModel MarkerRegistration { get; }
		public MarkerTrackingViewModel MarkerTracking { get; }

		public MainViewModel()
		{
			ScaleAdapter = new ScaleAdapter();
			MarkerRegistration = new MarkerRegistrationViewModel(_modelRoot.RegistrationService, ScaleAdapter);
			MarkerTracking = new MarkerTrackingViewModel(_modelRoot.TrackingService, ScaleAdapter);

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
