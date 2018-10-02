using System;

namespace MarkerRegistratorGui.Model
{
	public interface ITrackingService
	{
		event Action<TrackerEvent<PointerState>> OnPointerEvent;
		event Action<TrackerEvent<MarkerState>> OnMarkerEvent;

		void Start();
		void Stop();
	}
}
