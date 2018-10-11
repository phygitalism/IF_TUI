using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using RecognitionService.Api;

namespace MarkerRegistratorGui.Model
{
	public class RegistrationService : IMarkerRegistrationService
	{
		private readonly HashSet<int> _registeredIds = new HashSet<int>();

		private readonly RecognitionServiceClient _serviceClient
			= new RecognitionServiceClient("ws://localhost:8080");

		private MarkerCandidateState _candidateState = MarkerCandidateState.NotDetected;

		private readonly IMarkerRegistrationField _registrationField;

		public IEnumerable<int> AvailableIds { get; } = Enumerable.Range(0, 20);
		public IEnumerable<int> RegisteredIds => _registeredIds;

		public event Action OnRegisteredIdsChanged;
		public event Action<MarkerCandidateState> OnMarkerCandidateUpdated;

		public RegistrationService(IMarkerRegistrationField registrationField)
		{
			_registrationField = registrationField;

			_registrationField.OnPointersCountChanged += UpdateCandidateState;
		}

		private void UpdateCandidateState(int count)
		{
			var newState = count == 3
				? MarkerCandidateState.Detected
				: MarkerCandidateState.NotDetected;

			if (_candidateState != newState)
			{
				Debug.WriteLine($"{nameof(MarkerCandidateState)} - {newState}");

				_candidateState = newState;
				OnMarkerCandidateUpdated?.Invoke(newState);
			}
		}

		public async Task UpdateRegisteredAsync()
		{
			var registered = await _serviceClient.GetMarkerListAsync();

			_registeredIds.Clear();

			foreach (var id in registered)
				_registeredIds.Add(id);

			OnRegisteredIdsChanged?.Invoke();
		}

		public void RegisterCandidate(int id)
		{
			var pointers = _registrationField.Pointers.ToArray();

			if (pointers.Length != 3)
				throw new InvalidOperationException("Not 3 pointers");

			var triangle = new Triangle(pointers[0], pointers[1], pointers[2]);

			_serviceClient.RegisterMarker(id, triangle);

			if (_registeredIds.Add(id))
				OnRegisteredIdsChanged?.Invoke();
		}
	}
}
