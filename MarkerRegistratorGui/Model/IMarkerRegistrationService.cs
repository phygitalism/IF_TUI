using System.Numerics;

namespace MarkerRegistratorGui.Model
{
	public interface IMarkerRegistrationService
	{
		int IdsCount { get; }

		(Vector2 position, Vector2 size) RegistrationField { get; }
	}
}
