using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace MarkerRegistratorGui.Model
{
	public class RegistrationField : IMarkerRegistrationField
	{
		private readonly ITrackingService _trackingService;
		private readonly Dictionary<int, Vector2> _pointersInside
			= new Dictionary<int, Vector2>();

		private Vector2 _center = new Vector2(0.5f, 0.5f);

		public Vector2 FieldPosition => _center - FieldSize / 2;
		public Vector2 FieldSize { get; } = new Vector2(0.2f, 0.3f);

		public IEnumerable<Vector2> Pointers => _pointersInside.Values;

		public event Action<int> OnPointersCountChanged;

		public RegistrationField(ITrackingService trackingService)
		{
			_trackingService = trackingService;

			_trackingService.OnEvents += HandleTrackerEvents;
		}

		private void HandleTrackerEvents(TrackerEvents events)
		{
			var pointersCount = _pointersInside.Count;

			foreach (var e in events.pointerEvents)
			{
				if (IsInside(e.state.position) && e.type != TrackerEventType.Up)
					_pointersInside[e.id] = e.state.position;
				else
					_pointersInside.Remove(e.id);
			}

			if (pointersCount != _pointersInside.Count)
				OnPointersCountChanged?.Invoke(_pointersInside.Count);
		}

		private bool IsInside(Vector2 position)
			=> position.X >= FieldPosition.X
			&& position.Y >= FieldPosition.Y
			&& position.X <= FieldPosition.X + FieldSize.X
			&& position.Y <= FieldPosition.Y + FieldSize.Y;
	}
}
