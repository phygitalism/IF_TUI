using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarkerRegistratorGui.Model
{
	public interface IMarkerRegistrationService
	{
		IEnumerable<int> AvailableIds { get; }

		event Action<MarkerCandidateState> OnMarkerCandidateUpdated;

		void RegisterCandidate(int id);
		Task<IEnumerable<int>> GetRegisteredIdsAsync();
	}

	public enum MarkerCandidateState
	{
		NotDetected,
		Detected
	}
}
