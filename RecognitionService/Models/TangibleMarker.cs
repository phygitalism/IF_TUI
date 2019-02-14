using System;
using System.Numerics;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RecognitionService.Models
{
	public class RegistredTangibleMarker
	{
		public int Id;
		public Triangle triangle;
		public float initialAngle;
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
		public List<TouchPoint> vertecies;
		
		//я знаю что очень криво но по-другому тоже будет криво пока так чтолы поткстить
		public void UpdatePosition(Dictionary<int, TouchPoint> newTouches)
		{
			foreach (var touchId in newTouches.Keys)
			{
				if (touchId == vertecies[0].id)
				{
					triangle.posA = newTouches[touchId].Position;
				}
				else if (touchId == vertecies[1].id)
				{
					triangle.posB = newTouches[touchId].Position;
				}
				else if (touchId == vertecies[2].id)
				{
					triangle.posC = newTouches[touchId].Position;
				}
			}
			UpdateSides();
		}

		//TODO rewrite update methods
		private void UpdateSides()
		{
			triangle.sides = new List<Segment>()
			{
				new Segment(triangle.posA, triangle.posB),
				new Segment(triangle.posB, triangle.posC),
				new Segment(triangle.posC, triangle.posA)
			};
		}
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

		public RecognizedTangibleMarker(int id, Triangle triangle, float initialAngle, List<TouchPoint> vertecies) 
		{
			this.Id = id;
			this.Type = ActionType.Added;
			this.triangle = triangle;
			this.initialAngle = initialAngle;
			this.vertecies = vertecies;
		}

		private Vector2 FindCenter()
		{

			if (triangle.posB.X - triangle.posA.X < 1e-3)
			{
				return find_center(triangle.posB, triangle.posC, triangle.posA);
			}

			if (triangle.posC.X - triangle.posB.X < 1e-3)
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
