using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace MarkerRegistratorGui.Model
{
	public class DummyRegistrationService : IMarkerRegistrationService, IMarkerRegistrationField
	{
		private readonly HashSet<int> _availableIds = new HashSet<int>(Enumerable.Range(0, 10));
		private readonly HashSet<int> _registeredIds = new HashSet<int>();

		public IEnumerable<int> AvailableIds => _availableIds;
		public IEnumerable<int> RegisteredIds => _registeredIds;

		public Vector2 FieldPosition { get; } = new Vector2(0.4f, 0.4f);
		public Vector2 FieldSize { get; } = new Vector2(0.2f, 0.2f);
		public IEnumerable<Vector2> Pointers { get; } = Array.Empty<Vector2>();

		public event Action OnRegisteredIdsChanged;
		public event Action<MarkerCandidateState> OnMarkerCandidateUpdated;
		public event Action<int> OnPointersCountChanged;

		public DummyRegistrationService() => EmulateMarkerCandidate();

		private async void EmulateMarkerCandidate()
		{
			var time = TimeSpan.FromSeconds(2);

			Debug.WriteLine($"Emulating marker candidate after {time}");
			await Task.Delay(time);

			Debug.WriteLine($"Emulating marker candidate");

			OnMarkerCandidateUpdated?.Invoke(MarkerCandidateState.Detected);
		}

		public void RegisterCandidate(int id)
		{
			Debug.WriteLine($"Register id {id}");

			if (!AvailableIds.Contains(id))
				throw new InvalidOperationException();

			OnMarkerCandidateUpdated?.Invoke(MarkerCandidateState.NotDetected);

			if (_registeredIds.Add(id))
				OnRegisteredIdsChanged?.Invoke();

			EmulateMarkerCandidate();
		}

		public void UnregisterId(int id)
		{
			Debug.WriteLine($"Unregister id {id}");

			if (!RegisteredIds.Contains(id))
				throw new InvalidOperationException();

			if (_registeredIds.Remove(id))
				OnRegisteredIdsChanged?.Invoke();
		}
	}
}
