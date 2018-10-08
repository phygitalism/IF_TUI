namespace MarkerRegistratorGui.Model
{
	public class ModelRoot
	{
		public ITrackingService TrackingService { get; set; }
		public IMarkerRegistrationService RegistrationService { get; set; }
		public IMarkerRegistrationField RegistrationField { get; set; }
	}
}
