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

		public Vector2 FieldPosition { get; } = new Vector2(0.1f, 0.1f);
		public Vector2 FiledSize { get; } = new Vector2(0.2f, 0.3f);

		public event Action OnMarkerCandidatePlaced;
		public event Action OnMarkerCandidateRemoved;
		public event Action OnRegisteredIdsChanged;

		public DummyRegistrationService() => EmulateMarkerCandidate();

		private async void EmulateMarkerCandidate()
		{
			var time = TimeSpan.FromSeconds(5);

			Debug.WriteLine($"Emulating marker candidate after {time}");
			await Task.Delay(time);

			Debug.WriteLine($"Emulating marker candidate");

			OnMarkerCandidatePlaced?.Invoke();
		}

		public void RegisterId(int id)
		{
			Debug.WriteLine($"Register id {id}");

			if (!AvailableIds.Contains(id) || RegisteredIds.Contains(id))
				throw new InvalidOperationException();

			OnMarkerCandidateRemoved?.Invoke();

			_registeredIds.Add(id);
			OnRegisteredIdsChanged?.Invoke();

			EmulateMarkerCandidate();
		}

		public void UnregisterId(int id)
		{
			Debug.WriteLine($"Unregister id {id}");

			if (!RegisteredIds.Contains(id))
				throw new InvalidOperationException();

			_registeredIds.Remove(id);
			OnRegisteredIdsChanged?.Invoke();
		}
	}
}
