using Newtonsoft.Json.Linq;

namespace RecognitionService.Api
{
	public static class ApiHelpers
	{
		public static JObject CreateMessage(ApiEvent apiEvent, object payload)
			=> new JObject()
			{
				[ApiKeys.EventType] = apiEvent.ToString(),
				[ApiKeys.EventPayload] = payload != null ? JToken.FromObject(payload) : null
			};

		public static (ApiEvent, JToken) ParseMessage(string json)
		{
			var message = JObject.Parse(json);
			return (
				message[ApiKeys.EventType].ToObject<ApiEvent>(),
				message[ApiKeys.EventPayload]
			);
		}
	}
}
