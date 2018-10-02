namespace MarkerRegistratorGui.Model
{
	public class ModelRoot
	{
		public ITrackingService TrackingService { get; set; }
		public IMarkerRegistrationService RegistrationService { get; set; }
	}
}
