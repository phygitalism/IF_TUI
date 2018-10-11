using System;
using System.Collections.Generic;

namespace MarkerRegistratorGui.Model
{
	public interface IMarkerRegistrationService
	{
		IEnumerable<int> AvailableIds { get; }
		IEnumerable<int> RegisteredIds { get; }

		event Action OnRegisteredIdsChanged;
		event Action<MarkerCandidateState> OnMarkerCandidateUpdated;

		void RegisterCandidate(int id);
	}

	public enum MarkerCandidateState
	{
		NotDetected,
		Detected
	}
}
