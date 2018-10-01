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

		public MarkerTrackingViewModel(IMarkerTrackingService markerTracking, ScaleAdapter scaleAdapter)
		{
			_scaleAdapter = scaleAdapter;

			_disposables = new List<IDisposable>()
			{
				Observable.FromEvent<MarkerEvent>(
					handler => markerTracking.OnMarkerEvent += handler,
					handler => markerTracking.OnMarkerEvent -= handler
				)
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(HandleMarkerEvent)
			};
		}

		private void HandleMarkerEvent(MarkerEvent e)
		{
			if (e.type == MarkerEventType.Down)
			{
				if (_markers.TryGetValue(e.id, out var marker))
				{
					_markers.Remove(e.id);
					TrackedMarkers.Remove(marker);
				}

				marker = new TrackedMarkerViewModel(e.id, _scaleAdapter);

				_markers.Add(e.id, marker);
				TrackedMarkers.Add(marker);
			}

			if (e.type == MarkerEventType.Up)
			{
				if (_markers.TryGetValue(e.id, out var marker))
				{
					_markers.Remove(e.id);
					TrackedMarkers.Remove(marker);
				}
			}

			if (e.type == MarkerEventType.Update)
			{
				if (_markers.TryGetValue(e.id, out var marker))
					marker.UpdateValues(e.state);
			}
		}

		public void Dispose() => _disposables.ForEach(d => d.Dispose());
	}
}
