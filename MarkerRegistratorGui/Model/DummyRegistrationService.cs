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
		private readonly TimeSpan _delay = TimeSpan.FromSeconds(1);

		private readonly HashSet<int> _availableIds = new HashSet<int>(Enumerable.Range(0, 10));
		private readonly HashSet<int> _registeredIds = new HashSet<int>();

		public IEnumerable<int> AvailableIds => _availableIds;
		public IEnumerable<int> RegisteredIds => _registeredIds;

		public Vector2 FieldPosition { get; } = new Vector2(0.4f, 0.4f);
		public Vector2 FieldSize { get; } = new Vector2(0.2f, 0.2f);
		public IEnumerable<Vector2> Pointers { get; } = Array.Empty<Vector2>();

		public event Action<MarkerCandidateState> OnMarkerCandidateUpdated;
		public event Action<int> OnPointersCountChanged;

		public DummyRegistrationService() => ResetMarkerCandidateAsync();

		private async void ResetMarkerCandidateAsync()
		{
			await DoWithDelay(
				"Removing marker candidate",
				() => OnMarkerCandidateUpdated?.Invoke(MarkerCandidateState.NotDetected)
			);

			await Task.Delay(5000);
			await DoWithDelay(
				"Emulating marker candidate",
				() => OnMarkerCandidateUpdated?.Invoke(MarkerCandidateState.Detected)
			);
		}

		public void RegisterCandidate(int id)
		{
			Debug.WriteLine($"Register id {id}");

			if (!AvailableIds.Contains(id))
				throw new InvalidOperationException();

			_registeredIds.Add(id);

			ResetMarkerCandidateAsync();
		}

		public void UnregisterId(int id)
		{
			Debug.WriteLine($"Unregister id {id}");

			if (!RegisteredIds.Contains(id))
				throw new InvalidOperationException();

			_registeredIds.Remove(id);
		}

		public Task<IEnumerable<int>> GetRegisteredIdsAsync()
			=> DoWithDelay("Requesting ids", () => Task.FromResult((IEnumerable<int>)_registeredIds));


		private Task DoWithDelay(string actionText, Action action)
			=> DoWithDelay(actionText, () =>
			{
				action();
				return Task.FromResult(false);
			});

		private Task DoWithDelay(string actionText, Func<Task> action)
			=> DoWithDelay(actionText, async () => {
				await action();
				return false;
			});

		private async Task<T> DoWithDelay<T>(string actionText, Func<Task<T>> action)
		{
			Debug.WriteLine($"{actionText} after {_delay}");
			await Task.Delay(_delay);

			Debug.WriteLine(actionText);
			return await action();
		}
	}
}
