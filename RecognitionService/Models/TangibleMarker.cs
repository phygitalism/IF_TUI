using System.Numerics;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RecognitionService.Models
{
	public struct RegistredTangibleMarker
	{
		public int Id;
		public Triangle triangle;
		public float initialAngle;
		public float angleToCenter;

		[JsonIgnore]
		public List<Segment> Sides
		{
			get { return triangle.sides; }
		}

		public RegistredTangibleMarker(int id, Triangle triangle, float initialAngle, float angleToCenter)
		{
			this.Id = id;
			this.triangle = triangle;
			this.initialAngle = initialAngle;
			this.angleToCenter = angleToCenter;
		}
	}

	public class RecognizedTangibleMarker
	{
		public enum ActionType
		{
			Added = 0,
			Updated = 1,
			Removed = 2
		}

		public int Id;
		public ActionType Type;
		public Triangle triangle;
		public float rotationAngle;
		public Vector2 center;

		public List<Segment> Sides
		{
			get { return triangle.sides; }
		}

		public RecognizedTangibleMarker(int id, Triangle triangle, float rotationAngle, Vector2 center)
		{
			this.Id = id;
			this.Type = ActionType.Added;
			this.triangle = triangle;
			this.rotationAngle = rotationAngle;
			this.center = center;
		}
	}
}
