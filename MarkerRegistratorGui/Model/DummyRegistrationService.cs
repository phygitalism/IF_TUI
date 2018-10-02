using System.Numerics;

namespace MarkerRegistratorGui.Model
{
	public class DummyRegistrationService : IMarkerRegistrationService
	{
		public int IdsCount => 10;

		public (Vector2 position, Vector2 size) RegistrationField
			=> (new Vector2(0.1f, 0.1f), new Vector2(0.2f, 0.3f));
	}
}
