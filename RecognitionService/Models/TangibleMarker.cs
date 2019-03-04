using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RecognitionService.Models
{
	public class RegistredTangibleMarker
	{
		public int Id { get; set; }
		public float InitialAngle { get; set; }
		public bool IsClockwiseRotated { get; set; }

		public Dictionary<string, Vector2> vertexes { get; set; }
		public List<Segment> sides { get; set; }

		[JsonIgnore]
		public List<Segment> Sides
		{
			get { return sides; }
		}

		[JsonIgnore]
		public Triangle Triangle
		{
			get { return new Triangle(vertexes["v1"], vertexes["v2"], vertexes["v3"]); }
		}

		public RegistredTangibleMarker()
		{ }

		public RegistredTangibleMarker(int id, (Vector2 v1, Vector2 v2, Vector2 v3) vertexes)
		{
			this.Id = id;
			this.vertexes = new Dictionary<string, Vector2>
			{
				["v1"] = vertexes.v1,
				["v2"] = vertexes.v2,
				["v3"] = vertexes.v3
			};
			var triangle = Triangle;
			this.sides = triangle.SortedSides;
			this.InitialAngle = triangle.LargeSide.CalculateAngleBetweenY();
			this.IsClockwiseRotated = triangle.IsClockwiseRotated;
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
		public ActionType Type { get; set; } = ActionType.Added;
		public Triangle Triangle { get; private set; }
		public float InitialAngle { get; private set; }
		public Dictionary<int, TouchPoint> ActiveTouchPoints { get; private set; }

		// связывает тач с конкретной вершиной треугольника (хранит по id тача - индекс вершины треугольника в списке)
		public Dictionary<int, int> TouchPointMap { get; private set; }

		public Vector2 RelativeCenter { get; set; } = new Vector2();

		public Vector2 Center
		{
			get { return FindCenter(); }
		}

		public float RotationAngle
		{
			get
			{
				return ClockwiseDifferenceBetweenAngles(InitialAngle, CurrentAngleForLargeSide);
			}
		}

		private float CurrentAngleForLargeSide
		{
			get
			{
				var v1 = ActiveTouchPoints[LargeSideTouchIds.Item1].Position;
				var v2 = ActiveTouchPoints[LargeSideTouchIds.Item2].Position;
				return new Segment(v1, v2).CalculateAngleBetweenY();
			}
		}

		public List<Segment> Sides
		{
			get { return Triangle.SortedSides; }
		}

		public (int, int) LargeSideTouchIds;

		public RecognizedTangibleMarker(int id, (TouchPoint, TouchPoint, TouchPoint) vertexes, float initialAngle = 0.0f)
		{
			this.Id = id;
			this.Triangle = new Triangle(vertexes.Item1.Position, vertexes.Item2.Position, vertexes.Item3.Position);
			this.InitialAngle = Triangle.LargeSide.CalculateAngleBetweenY();

			this.ActiveTouchPoints = new Dictionary<int, TouchPoint>
			{
				[vertexes.Item1.Id] = vertexes.Item1,
				[vertexes.Item2.Id] = vertexes.Item2,
				[vertexes.Item3.Id] = vertexes.Item3
			};

			// TODO: позднее будет возможность изменять id тачпоинта, если он исчез и появился с другим id
			this.TouchPointMap = new Dictionary<int, int>()
			{
				[vertexes.Item1.Id] = 0,
				[vertexes.Item2.Id] = 1,
				[vertexes.Item3.Id] = 2
			};
			this.LargeSideTouchIds = GetTouchIdsForLargeSide(vertexes);
		}

		private (int, int) GetTouchIdsForLargeSide((TouchPoint, TouchPoint, TouchPoint) vertexes)
		{
			var sortedSidesWithId = new List<(Segment, (int, int))>()
			{
				(new Segment(vertexes.Item1.Position, vertexes.Item2.Position), (vertexes.Item1.Id, vertexes.Item2.Id)),
				(new Segment(vertexes.Item2.Position, vertexes.Item3.Position), (vertexes.Item2.Id, vertexes.Item3.Id)),
				(new Segment(vertexes.Item3.Position, vertexes.Item1.Position), (vertexes.Item3.Id, vertexes.Item1.Id))
			};
			sortedSidesWithId.Sort((v1, v2) => v1.Item1.Length >= v2.Item1.Length ? 1 : -1);
			return sortedSidesWithId[2].Item2;
		}

		public void UpdateVertexes(List<TouchPoint> newTouches)
		{
			if (newTouches.Count != 3)
			{
				Console.WriteLine("WARNING: Invalid amount of touches");
				return;
			}

			Type = ActionType.Updated;

			foreach (var touch in newTouches)
			{
				ActiveTouchPoints[touch.Id] = touch;
				if (touch.Type == TouchPoint.ActionType.Up)
				{
					Type = ActionType.Removed;
				}

				int vertexIndex;
				if (TouchPointMap.TryGetValue(touch.Id, out vertexIndex))
				{
					switch (vertexIndex)
					{
						case 0:
							Triangle.posA = touch.Position;
							break;
						case 1:
							Triangle.posB = touch.Position;
							break;
						case 2:
							Triangle.posC = touch.Position;
							break;
						default:
							break;
					}
				}
				else
				{
					// TODO: handle unmapped touch id
					Console.WriteLine($"ERROR: unmapped touch id {touch.Id} for tangible marker {Id}");
				}
			}
		}
		
		// how to calculate center http://algolist.manual.ru/maths/geom/equation/circle.php
		private Vector2 FindCenter()
		{
			var combinationsOfVertecies = new List<Vector2>(){Triangle.posA, Triangle.posB, Triangle.posC}.GetPermutations(3);
			foreach (var combinationOfVertecies in combinationsOfVertecies)
			{
				var listOfVertecies = combinationOfVertecies.ToList();
				var firstSide = new Segment(listOfVertecies[0], listOfVertecies[1]);
				var secondSide = new Segment(listOfVertecies[1], listOfVertecies[2]);
				if ( (!firstSide.isPerpendicularToX() && !firstSide.isPerpendicularToY() &&
					  !secondSide.isPerpendicularToX() && !secondSide.isPerpendicularToY()) ||
					 (firstSide.isPerpendicularToX() && secondSide.isPerpendicularToY()))
				{
					return CalculateCenter(listOfVertecies[0], listOfVertecies[1], listOfVertecies[2]);
				}
			}
			//окружности не существует
			return Vector2.Zero;
		}


		private Vector2 CalculateCenter(Vector2 v1, Vector2 v2, Vector2 v3)
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
