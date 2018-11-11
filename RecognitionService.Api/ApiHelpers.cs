using Newtonsoft.Json.Linq;

namespace RecognitionService.Api
{
	public static class ApiHelpers
	{
		public static JObject CreateMessage(ApiEvent apiEvent, object payload = null)
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

		public static class ApiKeys
		{
			public const string EventType = "event";
			public const string EventPayload = "payload";
		}

		public readonly struct RegisterMarkerRequestArgs
		{
			public readonly int id;
			public readonly Triangle triangle;

			public RegisterMarkerRequestArgs(int id, Triangle triangle)
			{
				this.id = id;
				this.triangle = triangle;
			}
		}
	}
}
