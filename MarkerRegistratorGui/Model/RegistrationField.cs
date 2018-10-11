using System;
using System.Collections.Generic;
using System.Numerics;

namespace MarkerRegistratorGui.Model
{
	public class RegistrationField : IMarkerRegistrationField
	{
		private const int _pointersPerMarker = 3;

		private readonly ITrackingService _trackingService;
		private readonly Dictionary<int, Vector2> _pointersInside
			= new Dictionary<int, Vector2>();

		private readonly bool _hasCandidate;

		private Vector2 _center = new Vector2(0.5f, 0.5f);

		public Vector2 FieldPosition => _center - FieldSize / 2;
		public Vector2 FieldSize { get; } = new Vector2(0.2f, 0.3f);

		public IEnumerable<Vector2> PointersInside => _pointersInside.Values;

		public event Action OnMarkerCandidatePlaced;
		public event Action OnMarkerCandidateRemoved;

		public RegistrationField(ITrackingService trackingService)
		{
			_trackingService = trackingService;

			_trackingService.OnEvents += HandleTrackerEvents;
		}

		private void HandleTrackerEvents(TrackerEvents events)
		{
			foreach (var e in events.pointerEvents)
			{
				if (e.type == TrackerEventType.Up || IsInside(e.state.position))
					_pointersInside[e.id] = e.state.position;
				else
					_pointersInside.Remove(e.id);
			}

			HasCandidate = _pointersInside.Count == _pointersPerMarker;
		}

		private bool HasCandidate
		{
			get => _hasCandidate;
			set
			{
				if (!_hasCandidate && value)
					OnMarkerCandidatePlaced?.Invoke();
				if (_hasCandidate && !value)
					OnMarkerCandidateRemoved?.Invoke();
			}
		}

		private bool IsInside(Vector2 position)
			=> position.X >= FieldPosition.X
			&& position.Y >= FieldPosition.Y
			&& position.X <= FieldPosition.X + FieldSize.X
			&& position.Y <= FieldPosition.Y + FieldSize.Y;
	}
}
