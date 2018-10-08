using System;
using System.Numerics;

namespace MarkerRegistratorGui.Model
{
	public interface IMarkerRegistrationField
	{
		Vector2 FieldPosition { get; }
		Vector2 FiledSize { get; }

		event Action OnMarkerCandidatePlaced;
		event Action OnMarkerCandidateRemoved;
	}
}
