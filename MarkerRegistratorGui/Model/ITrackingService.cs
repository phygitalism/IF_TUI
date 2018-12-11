using System;

namespace MarkerRegistratorGui.Model
{
	public readonly struct TrackerEvents
	{
		public readonly TrackerEvent<MarkerState>[] markerEvents;
		public readonly TrackerEvent<PointerState>[] pointerEvents;

		public TrackerEvents(
			TrackerEvent<MarkerState>[] markerEvents,
			TrackerEvent<PointerState>[] pointerEvents
		)
		{
			this.markerEvents = markerEvents;
			this.pointerEvents = pointerEvents;
		}
	}

	public interface ITrackingService
	{
		event Action<TrackerEvents> OnEvents;
	}
}
