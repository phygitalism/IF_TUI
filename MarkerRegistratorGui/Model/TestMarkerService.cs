using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace MarkerRegistratorGui.Model
{
	public class TestMarkerService : ITrackingService
	{
		private readonly List<int> _registeredMarkers = new List<int>();

		public event Action<TrackerEvent<PointerState>> OnPointerEvent;
		public event Action<TrackerEvent<MarkerState>> OnMarkerEvent;

		public void Start()
		{
			AddTestMarkerAsync(new Vector2(0.20f, 0.20f), 0.05f, -0.75f);
			AddTestMarkerAsync(new Vector2(0.20f, 0.80f), 0.05f, 0.25f);
			AddTestMarkerAsync(new Vector2(0.80f, 0.20f), 0.05f, 0.75f);
			AddTestMarkerAsync(new Vector2(0.80f, 0.80f), 0.05f, -0.25f);
		}

		public void Stop()
		{

		}

		public async void AddTestMarkerAsync(Vector2 position, float radius, float rotation)
		{
			var id = _registeredMarkers.Count;
			_registeredMarkers.Add(id);

			OnMarkerEvent?.Invoke(
				new TrackerEvent<MarkerState>(id, TrackerEventType.Down, new MarkerState())
			);

			await Task.Run(async () =>
			{
				float i = 0;
				while (true)
					unchecked
					{
						i += 0.1f;

						var newRotation = rotation + (float)Math.Sin(i) * 0.25f;
						var newPosition = position + new Vector2((float)Math.Cos(i), (float)Math.Sin(i)) * radius;

						OnMarkerEvent?.Invoke(new TrackerEvent<MarkerState>(
							id,
							TrackerEventType.Update,
							new MarkerState(newPosition, newRotation, radius)
						));
						await Task.Delay(10);
					}
			});

			OnMarkerEvent?.Invoke(
				new TrackerEvent<MarkerState>(id, TrackerEventType.Up, new MarkerState())
			);
		}
	}
}
