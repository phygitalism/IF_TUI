using System;

namespace MarkerRegistratorGui.Model
{
	public interface IMarkerTrackingService
	{
		event Action<MarkerEvent> OnMarkerEvent;

		void Start();
		void Stop();
	}
}
