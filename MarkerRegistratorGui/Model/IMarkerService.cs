using System;
using System.Numerics;

namespace MarkerRegistratorGui.Model
{
	public interface IMarkerService
	{
		int IdsCount { get; }

		(Vector2 position, Vector2 size) RegistrationField { get; }

		event Action<int> OnMarkerDown;
		event Action<int> OnMarkerUp;
		event Action<MarkerState> OnMarkerStateUpdate;

		void Start();
		void Stop();
	}
}
