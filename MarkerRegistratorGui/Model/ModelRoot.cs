namespace MarkerRegistratorGui.Model
{
	public class ModelRoot
	{
		private readonly TuioTrackingService _trackingService = new TuioTrackingService();

		public ITrackingService TrackingService => _trackingService;
		public IMarkerRegistrationService RegistrationService { get; }
		public IMarkerRegistrationField RegistrationField { get; }

		public ModelRoot()
		{
			RegistrationField = new RegistrationField(_trackingService);
			RegistrationService = new RegistrationService(RegistrationField);
		}

		public void Start() => _trackingService.Start();

		public void Stop() => _trackingService.Stop();
	}
}
