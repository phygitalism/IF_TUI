using System.Numerics;

namespace MarkerRegistratorGui.Model
{
	public readonly struct MarkerEvent
	{
		public readonly int id;
		public readonly MarkerEventType type;
		public readonly MarkerState state;

		public MarkerEvent(int id, MarkerEventType type, MarkerState state)
		{
			this.id = id;
			this.type = type;
			this.state = state;
		}
	}

	public enum MarkerEventType
	{
		Up,
		Down,
		Update
	}

	public readonly struct MarkerState
	{
		public readonly Vector2 position;
		public readonly float rotation;
		public readonly float radius;

		public MarkerState(Vector2 position, float rotation, float radius)
		{
			this.position = position;
			this.rotation = rotation;
			this.radius = radius;
		}
	}
}
