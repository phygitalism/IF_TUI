﻿using System;
using System.Collections.Generic;
using System.Numerics;

namespace MarkerRegistratorGui.Model
{
	public interface IMarkerRegistrationField
	{
		Vector2 FieldPosition { get; }
		Vector2 FieldSize { get; }

		IEnumerable<Vector2> PointersInside { get; }

		event Action OnMarkerCandidatePlaced;
		event Action OnMarkerCandidateRemoved;
	}
}
