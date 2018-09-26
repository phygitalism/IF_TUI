using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace MarkerRegistratorGui.Model
{
	public class TestMarkerService : IMarkerService
	{
		private readonly List<int> _registeredMarkers = new List<int>();

		public IEnumerable<int> RegisteredMarkers { get; }

		public event Action<int> OnMarkerDown;
		public event Action<int> OnMarkerUp;
		public event Action<MarkerState> OnMarkerStateUpdate;

		public void Start()
		{
			AddTestMarkerAsync(new Vector2(1.0f, 0.0f), 0.1f, -0.75f);
			AddTestMarkerAsync(new Vector2(0.0f, 1.0f), 0.1f, 0.25f);
			AddTestMarkerAsync(new Vector2(0.0f, 0.0f), 0.1f, 0.75f);
			AddTestMarkerAsync(new Vector2(1.0f, 1.0f), 0.1f, -0.25f);
		}

		public void Stop()
		{

		}

		public async void AddTestMarkerAsync(Vector2 position, float radius, float rotation)
		{
			var id = _registeredMarkers.Count;
			_registeredMarkers.Add(id);

			OnMarkerDown?.Invoke(id);

			await Task.Run(async () =>
			{
				float i = 0;
				while (true)
					unchecked
					{
						i += 0.1f;

						var newRotation = rotation + (float)Math.Sin(i) * 0.25f;
						var newPosition = position;// + new Vector2((float)Math.Cos(i), (float)Math.Sin(i)) * radius;

						OnMarkerStateUpdate?.Invoke(new MarkerState(id, newPosition, newRotation, radius));
						await Task.Delay(10);
					}
			});

			OnMarkerUp?.Invoke(id);
		}
	}
}
