using System.Numerics;

namespace RecognitionService.Api
{
	public readonly struct Rectangle
	{
		public readonly Vector2 posA;
		public readonly Vector2 posB;
		public readonly Vector2 posC;

		public Rectangle(Vector2 posA, Vector2 posB, Vector2 posC)
		{
			this.posA = posA;
			this.posB = posB;
			this.posC = posC;
		}
	}
}
