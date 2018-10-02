using System.Numerics;

namespace MarkerRegistratorGui.Model
{
	public readonly struct TrackerEvent<TState>
	{
		public readonly int id;
		public readonly TrackerEventType type;
		public readonly TState state;

		public TrackerEvent(int id, TrackerEventType type, TState state)
		{
			this.id = id;
			this.type = type;
			this.state = state;
		}
	}

	public enum TrackerEventType
	{
		Up,
		Down,
		Update
	}

	public readonly struct PointerState
	{
		public readonly Vector2 position;

		public PointerState(Vector2 position)
		{
			this.position = position;
		}
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
