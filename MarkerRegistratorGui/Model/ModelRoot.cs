namespace MarkerRegistratorGui.Model
{
	public class ModelRoot
	{
		public IMarkerTrackingService TrackingService { get; set; }
		public IMarkerRegistrationService RegistrationService { get; set; }
	}
}
