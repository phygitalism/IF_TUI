using System;
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
		public float initialAngle;

		public float rotationAngle
		{
			get { return FindRotationAngle(); }
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

		private Vector2 FindCenter()
		{

			if (triangle.posB.X - triangle.posA.X == 0f)
			{
				return find_center(triangle.posB, triangle.posC, triangle.posA);
			}

			if (triangle.posC.X - triangle.posB.X == 0f)
			{
				return find_center(triangle.posC, triangle.posA, triangle.posB);
			}

			return find_center(triangle.posA, triangle.posB, triangle.posC);
		}

		private Vector2 find_center(Vector2 v1, Vector2 v2, Vector2 v3)
		{
			var m_a = (v2.Y - v1.Y) / (v2.X - v1.X);
			var m_b = (v3.Y - v2.Y) / (v3.X - v2.X);
			var x_center = (m_a * m_b * (v1.Y - v3.Y) + m_b * (v1.X + v2.X) - m_a * (v2.X + v3.X)) / (2 * (m_b - m_a));
			var y_center = -1 / m_a * (x_center - (v1.X + v2.X) / 2) + (v1.Y + v2.Y) / 2;
			return new Vector2(x_center, y_center);
		}

		private float FindRotationAngle()
		{
			float newAngle = CalculateAngle(Vector2.UnitY, triangle.LargeSide.destination - triangle.LargeSide.origin);
			return ClockwiseDifferenceBetweenAngles(initialAngle, newAngle);
		}
		
		private float ClockwiseDifferenceBetweenAngles(float initialAngle, float newAngle)
		{
			if (initialAngle > newAngle)
			{
				newAngle += 2 * (float)Math.PI;
			}

			return newAngle - initialAngle;
		}
		
		private float CalculateAngle(Vector2 v1, Vector2 v2, bool degree=false)
		{
			float cosine = Vector2.Dot(Vector2.Normalize(v1), Vector2.Normalize(v2));
			cosine = (cosine < -1) ? -1 : ((cosine > 1) ? 1 : cosine);
			float radians = (float)Math.Acos(cosine);
			if (degree)
			{
				return radToDeg(radians);
			}
			return radians;
		}
		
		private float radToDeg(float rad)
		{
			return (float)((rad * 180)/Math.PI);
		}
	}
}
