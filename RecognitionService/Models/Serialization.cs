using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RecognitionService.Models
{
	public static class Serialization
	{
		public static JObject SerializeMarker(RegistredTangibleMarker marker)
		{
			var triangleObj = marker.triangle.Serialize();
			var obj = new JObject()
			{
				["id"] = marker.Id,
				["triangle"] = triangleObj,
				["initialAngle"] = marker.initialAngle,
				["angleToCenter"] = marker.angleToCenter
			};
			return obj;

		}

		public static RegistredTangibleMarker DeserializeMarker(JObject obj)
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
}
