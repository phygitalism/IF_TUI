using System;
using System.Collections.Generic;

namespace MarkerRegistratorGui.Model
{
	public readonly struct TrackerEvents
	{
		public readonly IEnumerable<TrackerEvent<MarkerState>> markerEvents;
		public readonly IEnumerable<TrackerEvent<PointerState>> pointerEvents;

		public TrackerEvents(
			IEnumerable<TrackerEvent<MarkerState>> markerEvents,
			IEnumerable<TrackerEvent<PointerState>> pointerEvents
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
