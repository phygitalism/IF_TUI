using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading;
using MarkerRegistratorGui.Model;

namespace MarkerRegistratorGui.ViewModel
{
	public class MarkerTrackingViewModel : IDisposable
	{
		private readonly Dictionary<int, TrackedMarkerViewModel> _markers = new Dictionary<int, TrackedMarkerViewModel>();
		private readonly List<IDisposable> _disposables;
		private readonly ScaleAdapter _scaleAdapter;

		public ObservableCollection<TrackedMarkerViewModel> TrackedMarkers { get; }
			= new ObservableCollection<TrackedMarkerViewModel>();

		public MarkerTrackingViewModel(ITrackingService markerTracking, ScaleAdapter scaleAdapter)
		{
			_scaleAdapter = scaleAdapter;

			_disposables = new List<IDisposable>()
			{
				Observable.FromEvent<TrackerEvent<MarkerState>>(
					handler => markerTracking.OnMarkerEvent += handler,
					handler => markerTracking.OnMarkerEvent -= handler
				)
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(HandleMarkerEvent)
			};
		}

		private void HandleMarkerEvent(TrackerEvent<MarkerState> e)
		{
			if (e.type == TrackerEventType.Down)
			{
				if (_markers.TryGetValue(e.id, out var marker))
				{
					_markers.Remove(e.id);
					TrackedMarkers.Remove(marker);
				}

				marker = new TrackedMarkerViewModel(e.id, _scaleAdapter);
				marker.UpdateValues(e.state);

				_markers.Add(e.id, marker);
				TrackedMarkers.Add(marker);
			}
			else if (e.type == TrackerEventType.Up)
			{
				if (_markers.TryGetValue(e.id, out var marker))
				{
					_markers.Remove(e.id);
					TrackedMarkers.Remove(marker);
				}
			}
			else if (e.type == TrackerEventType.Update)
			{
				if (_markers.TryGetValue(e.id, out var marker))
					marker.UpdateValues(e.state);
			}
		}

		public void Dispose() => _disposables.ForEach(d => d.Dispose());
	}
}
