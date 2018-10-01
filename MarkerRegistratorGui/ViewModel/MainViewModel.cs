using System.Collections.Generic;
using System.Collections.ObjectModel;
using MarkerRegistratorGui.Model;

namespace MarkerRegistratorGui.ViewModel
{
	public class MainViewModel
	{
		private readonly Dictionary<int, TrackedMarkerViewModel> _markers = new Dictionary<int, TrackedMarkerViewModel>();
		private readonly ModelRoot _modelRoot = new ModelRoot()
		{
			TrackingService = new TestMarkerService(),
			RegistrationService = new DummyRegistrationService()
		};

		public ObservableCollection<TrackedMarkerViewModel> TrackedMarkers { get; }
			= new ObservableCollection<TrackedMarkerViewModel>();

		public ScaleAdapter ScaleAdapter { get; }
		public MarkerRegistrationViewModel MarkerRegistration { get; }

		public MainViewModel()
		{
			ScaleAdapter = new ScaleAdapter();
			MarkerRegistration = new MarkerRegistrationViewModel(_modelRoot.RegistrationService, ScaleAdapter);

			_modelRoot.TrackingService.OnMarkerDown += HandleMarkerDown;
			_modelRoot.TrackingService.OnMarkerUp += HandleMarkerUp;
			_modelRoot.TrackingService.OnMarkerStateUpdate += HandleMarkerUpdate;

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
	}
}
