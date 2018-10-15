using System;
using System.Numerics;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RecognitionService.Models
{
	public struct RegistredTangibleMarker
	{
		public int Id;
		public Triangle triangle;
		public float initialAngle;
		public float angleToCenter;

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

		public JObject Serialize()
		{
			var triangleObj = triangle.Serialize();
			var obj = new JObject()
			{
				["id"] = Id,
				["triangle"] = triangleObj,
				["initialAngle"] = initialAngle,
				["angleToCenter"] = angleToCenter
			};
			return obj;

		}

		public static RegistredTangibleMarker Deserialize(JObject obj)
		{
			var tangible = new RegistredTangibleMarker(
				obj.Value<int>("id"),
				Triangle.Deserialize(obj.Value<JObject>("triangle")),
				obj.Value<float>("initialAngle"),
				obj.Value<float>("angleToCenter")
			);

			return tangible;
		}
	}

	public struct RecognizedTangibleMarker
	{
		public int Id;
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
			this.triangle = triangle;
			this.rotationAngle = rotationAngle;
			this.center = center;
		}
	}
}
