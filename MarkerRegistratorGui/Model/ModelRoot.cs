namespace MarkerRegistratorGui.Model
{
	public class ModelRoot
	{
		private readonly TuioTrackingService _trackingService = new TuioTrackingService();
		private readonly DummyRegistrationService _registrationService = new DummyRegistrationService();

		public ITrackingService TrackingService => _trackingService;
		public IMarkerRegistrationService RegistrationService => _registrationService;
		public IMarkerRegistrationField RegistrationField => _registrationService;

		public void Start() => _trackingService.Start();

		public void Stop() => _trackingService.Stop();
	}
}
