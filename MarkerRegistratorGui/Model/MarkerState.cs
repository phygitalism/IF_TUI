using System.Numerics;

namespace MarkerRegistratorGui.Model
{
	public readonly struct MarkerState
	{
		public readonly int id;
		public readonly Vector2 position;
		public readonly float rotation;
		public readonly float radius;

		public MarkerState(int id, Vector2 position, float rotation, float radius)
		{
			this.id = id;
			this.position = position;
			this.rotation = rotation;
			this.radius = radius;
		}
	}
}
