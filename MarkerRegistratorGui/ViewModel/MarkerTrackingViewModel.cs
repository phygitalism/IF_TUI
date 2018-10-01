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
				Observable.FromEvent<int>(
					handler => markerTracking.OnMarkerDown += handler,
					handler => markerTracking.OnMarkerDown -= handler
				)
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(HandleMarkerDown),

				Observable.FromEvent<MarkerState>(
					handler => markerTracking.OnMarkerStateUpdate += handler,
					handler => markerTracking.OnMarkerStateUpdate -= handler
				)
				.Subscribe(HandleMarkerUpdate),

				Observable.FromEvent<int>(
					handler => markerTracking.OnMarkerUp += handler,
					handler => markerTracking.OnMarkerUp -= handler
				)
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(HandleMarkerUp),
			};
		}

		private void HandleMarkerDown(int id)
		{
			if (_markers.ContainsKey(id))
				HandleMarkerUp(id);

			var marker = new TrackedMarkerViewModel(id, _scaleAdapter);

			_markers.Add(id, marker);
			TrackedMarkers.Add(marker);
		}

		private void HandleMarkerUp(int id)
		{
			if (_markers.TryGetValue(id, out var marker))
			{
				_markers.Remove(id);
				TrackedMarkers.Remove(marker);
			}
		}

		private void HandleMarkerUpdate(MarkerState state)
		{
			if (_markers.TryGetValue(state.id, out var marker))
				marker.UpdateValues(state.position, state.rotation, state.radius);
		}

		public void Dispose() => _disposables.ForEach(d => d.Dispose());
	}
}
