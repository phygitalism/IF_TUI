using System;
using System.Numerics;

namespace MarkerRegistratorGui.Model
{
	public class DummyRegistrationField : IMarkerRegistrationField
	{
		public Vector2 FieldPosition { get; } = new Vector2(0.1f, 0.1f);
		public Vector2 FiledSize { get; } = new Vector2(0.2f, 0.3f);

		public event Action OnMarkerCandidatePlaced;
		public event Action OnMarkerCandidateRemoved;
	}
}
