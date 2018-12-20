using System;
using System.Linq;
using MarkerRegistratorGui.Model;

namespace MarkerRegistratorGui.ViewModel
{
	public class PointersViewModel
	{
		public event Action<TrackerEvent<PointerState>[]> OnPointersUpdate;

		public PointersViewModel(ITrackingService trackingService)
		{
			trackingService.OnEvents += HandleEvents;
		}

		private void HandleEvents(TrackerEvents events)
		{
			OnPointersUpdate?.Invoke(events.pointerEvents.ToArray());
		}
	}
}