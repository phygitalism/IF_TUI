using System;

namespace MarkerRegistratorGui.Model
{
	public interface IMarkerService
	{
		IMarkerRegistrationService RegistrationService { get; }

		event Action<int> OnMarkerDown;
		event Action<int> OnMarkerUp;
		event Action<MarkerState> OnMarkerStateUpdate;

		void Start();
		void Stop();
	}
}
