using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using MarkerRegistratorGui.Model;
using Reactive.Bindings;

namespace MarkerRegistratorGui.ViewModel
{
	public class MainViewModel
	{
		private readonly Dictionary<int, TrackedMarkerViewModel> _markers = new Dictionary<int, TrackedMarkerViewModel>();
		private readonly IMarkerService _markerService = new TestMarkerService();

		public ObservableCollection<TrackedMarkerViewModel> TrackedMarkers { get; }
			= new ObservableCollection<TrackedMarkerViewModel>();

		public ReactiveProperty<IdSelectionViewModel> IdSelection { get; }

		public ReactiveProperty<float> WindowWidth { get; }
		public ReactiveProperty<float> WindowHeight { get; }

		public MainViewModel()
		{
			WindowWidth = new ReactiveProperty<float>();
			WindowHeight = new ReactiveProperty<float>();

			IdSelection = new ReactiveProperty<IdSelectionViewModel>();

			_markerService.OnMarkerDown += HandleMarkerDown;
			_markerService.OnMarkerUp += HandleMarkerUp;
			_markerService.OnMarkerStateUpdate += HandleMarkerUpdate;

			_markerService.Start();

			SelectIdAsync();
		}

		private async void SelectIdAsync()
		{
			IdSelection.Value = new IdSelectionViewModel(_markerService.IdsCount);

			var selectedId = await IdSelection.Value.SelectedId.FirstAsync();
			Debug.Print($"Selected id {selectedId}");

			IdSelection.Value = null;
		}

		private void HandleMarkerDown(int id)
		{
			if (_markers.ContainsKey(id))
				HandleMarkerUp(id);

			var marker = new TrackedMarkerViewModel(id, this);

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
