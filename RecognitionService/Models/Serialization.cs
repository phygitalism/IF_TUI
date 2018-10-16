using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
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

		public static string SerializeConfig(MarkerConfig config)
		{
			var list = new JArray(config.registredTangibles.Select(SerializeMarker));
			return JsonConvert.SerializeObject(list);
		}

		public static MarkerConfig DeserializeConfig(string json)
		{
			var tangibles = JToken.Parse(json).Value<JArray>();
			var registredTangibles = tangibles
				.Select(token => token.ToObject<JObject>())
				.Select(DeserializeMarker)
				.ToList();
			return new MarkerConfig(registredTangibles);
		}
	}
}
