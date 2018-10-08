using System.Collections.Generic;

namespace MarkerRegistratorGui.Model
{
	public interface IMarkerRegistrationService
	{
		IEnumerable<int> AvailableIds { get; }
		IEnumerable<int> RegisteredIds { get; }

		void RegisterId(int id);
		void UnregisterId(int id);
	}
}
