using System;
using System.Numerics;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RecognitionService.Models
{
	public class RegistredTangibleMarker
	{
		public enum MarkerState
		{
			Passive, 
			Active
		}
		public int Id;
		public Triangle triangle;
		public float initialAngle;
		public MarkerState State { get; set; } = MarkerState.Passive;
		[JsonIgnore]
		public List<Segment> Sides
		{
			get { return triangle.sides; }
		}

		public RegistredTangibleMarker(int id, Triangle triangle)
		{
			this.Id = id;
			this.triangle = triangle;
			this.initialAngle = triangle.LargeSide.CalculateAngleBetweenY();
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
		public float initialAngle;
		//public int ShortVertexId;
		//public int MiddleVertexId;
		//public int LargeVertexId;
		public float rotationAngle
		{
			get { return ClockwiseDifferenceBetweenAngles(initialAngle, triangle.LargeSide.CalculateAngleBetweenY()); }
		}

		public Vector2 relativeCenter = new Vector2();

		public Vector2 Center
		{
			get { return FindCenter(); }
		}

		public List<Segment> Sides
		{
			get { return triangle.sides; }
		}

		public RecognizedTangibleMarker(int id, Triangle triangle, float initialAngle)
		{
			this.Id = id;
			this.Type = ActionType.Added;
			this.triangle = triangle;
			this.initialAngle = initialAngle;
		}

		public void UpdatePosition(TouchPoint newTouch)
		{
			if (newTouch.id == triangle.posA.id)
			{
				triangle.posA = newTouch;
			}
			else if (newTouch.id == triangle.posB.id)
			{
				triangle.posB = newTouch;
			}
			else
			{
				triangle.posC = newTouch;
			}
		}

		private Vector2 FindCenter()
		{

			if (triangle.posB.Position.X - triangle.posA.Position.X < 1e-3)
			{
				return find_center(triangle.posB.Position, triangle.posC.Position, triangle.posA.Position);
			}

			if (triangle.posC.Position.X - triangle.posB.Position.X < 1e-3)
			{
				return find_center(triangle.posC.Position, triangle.posA.Position, triangle.posB.Position);
			}

			return find_center(triangle.posA.Position, triangle.posB.Position, triangle.posC.Position);
		}

		private Vector2 find_center(Vector2 v1, Vector2 v2, Vector2 v3)
		{
			var m_a = (v2.Y - v1.Y) / (v2.X - v1.X);
			var m_b = (v3.Y - v2.Y) / (v3.X - v2.X);
			var x_center = (m_a * m_b * (v1.Y - v3.Y) + m_b * (v1.X + v2.X) - m_a * (v2.X + v3.X)) / (2 * (m_b - m_a));
			var y_center = -1 / m_a * (x_center - (v1.X + v2.X) / 2) + (v1.Y + v2.Y) / 2;
			return new Vector2(x_center, y_center);
		}
		
		private float ClockwiseDifferenceBetweenAngles(float initialAngle, float newAngle)
		{
			if (initialAngle > newAngle)
			{
				newAngle += 2 * (float)Math.PI;
			}

			return newAngle - initialAngle;
		}
	}
}
