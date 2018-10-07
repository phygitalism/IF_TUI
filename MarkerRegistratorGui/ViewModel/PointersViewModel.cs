using System;
using System.Reactive.Linq;
using System.Threading;
using MarkerRegistratorGui.Model;

namespace MarkerRegistratorGui.ViewModel
{
	public class PointersViewModel
	{
		public IObservable<TrackerEvent<PointerState>> WhenPointerEvent { get; }

		public PointersViewModel(ITrackingService trackingService)
		{
			WhenPointerEvent = Observable.FromEvent<TrackerEvent<PointerState>>(
				h => trackingService.OnPointerEvent += h,
				h => trackingService.OnPointerEvent -= h
			)
			.ObserveOn(SynchronizationContext.Current);
		}
	}
}