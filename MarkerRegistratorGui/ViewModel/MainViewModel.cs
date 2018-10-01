using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading;
using MarkerRegistratorGui.Model;

namespace MarkerRegistratorGui.ViewModel
{
	public class MainViewModel : IDisposable
	{
		private readonly Dictionary<int, TrackedMarkerViewModel> _markers = new Dictionary<int, TrackedMarkerViewModel>();
		private readonly ModelRoot _modelRoot = new ModelRoot()
		{
			TrackingService = new TuioTrackingService(),
			RegistrationService = new DummyRegistrationService()
		};

		public ObservableCollection<TrackedMarkerViewModel> TrackedMarkers { get; }
			= new ObservableCollection<TrackedMarkerViewModel>();

		public ScaleAdapter ScaleAdapter { get; }
		public MarkerRegistrationViewModel MarkerRegistration { get; }

		private readonly List<IDisposable> _disposables;

		public MainViewModel()
		{
			ScaleAdapter = new ScaleAdapter();
			MarkerRegistration = new MarkerRegistrationViewModel(_modelRoot.RegistrationService, ScaleAdapter);


			_disposables = new List<IDisposable>()
			{
				Observable.FromEvent<int>(
					handler => _modelRoot.TrackingService.OnMarkerDown += handler,
					handler => _modelRoot.TrackingService.OnMarkerDown -= handler
				)
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(HandleMarkerDown),

				Observable.FromEvent<MarkerState>(
					handler => _modelRoot.TrackingService.OnMarkerStateUpdate += handler,
					handler => _modelRoot.TrackingService.OnMarkerStateUpdate -= handler
				)
				.Subscribe(HandleMarkerUpdate),

				Observable.FromEvent<int>(
					handler => _modelRoot.TrackingService.OnMarkerUp += handler,
					handler => _modelRoot.TrackingService.OnMarkerUp -= handler
				)
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(HandleMarkerUp),
			};

			_modelRoot.TrackingService.Start();
		}

		private void HandleMarkerDown(int id)
		{
			if (_markers.ContainsKey(id))
				HandleMarkerUp(id);

			var marker = new TrackedMarkerViewModel(id, ScaleAdapter);

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

		public void Dispose()
		{
			_disposables.ForEach(d => d.Dispose());
			_modelRoot.TrackingService.Stop();
		}
	}
}
