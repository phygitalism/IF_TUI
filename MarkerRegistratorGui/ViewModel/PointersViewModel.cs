using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using MarkerRegistratorGui.Model;

namespace MarkerRegistratorGui.ViewModel
{
	public class PointersViewModel
	{
		public IObservable<IEnumerable<TrackerEvent<PointerState>>> WhenPointerEvent { get; }

		public PointersViewModel(ITrackingService trackingService)
		{
			WhenPointerEvent = Observable.FromEvent<TrackerEvents>(
				h => trackingService.OnEvents += h,
				h => trackingService.OnEvents -= h
			)
			.Select(events => events.pointerEvents)
			.ObserveOn(SynchronizationContext.Current);
		}
	}
}