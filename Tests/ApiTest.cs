using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using RecognitionService.Api;
using Xunit;

namespace Tests
{
	public class ApiTest
	{
		private const int _serverPort = 12350;
		private const string _clientUri = @"ws:\\localhost:12350";

		[Fact]
		public void Connection()
		{
			using (var server = CreateServer())
			using (var client = CreateClient())
			{
				Assert.True(client.IsConnected);
				Assert.True(server.HasConnections);
			}
		}

		[Fact]
		public async Task ListRequest()
		{
			using (var server = CreateServer())
			using (var client = CreateClient())
			{
				var list = new[] { 1, 2, 3 };

				server.OnListRequested += () => list;

				var receivedList = await client.GetIdListAsync();

				Assert.True(receivedList.SequenceEqual(list));
			}
		}

		[Fact]
		public void RegisterRequest()
		{
			using (var server = CreateServer())
			using (var client = CreateClient())
			{
				var registerList = new []
				{
					(1, new Rectangle(
						new Vector2(0),
						new Vector2(0),
						new Vector2(0)
					)),
					(2, new Rectangle(
						new Vector2(1),
						new Vector2(2),
						new Vector2(3)
					)),
					(3, new Rectangle(
						new Vector2(3),
						new Vector2(2),
						new Vector2(1)
					))
				};
				var registeredList = new List<(int, Rectangle)>();

				server.OnRegisterRequested += (id, rect) => registeredList.Add((id, rect));

				foreach ((var id, var rectangle) in registerList)
					client.RegisterId(id, rectangle);

				Task.Delay(TimeSpan.FromSeconds(5)).Wait();

				Assert.True(registeredList.SequenceEqual(registerList));
			}
		}

		[Fact]
		public void UnregisterRequest()
		{
			using (var server = CreateServer())
			using (var client = CreateClient())
			{
				var idsToUnregister = 2;

				var initialData = new [] { 1, 2, 3 };
				var registered = initialData.ToList();

				server.OnUnregisterRequested += id => registered.Remove(id);

				foreach (var id in initialData.Take(idsToUnregister))
					client.UnregisterId(id);

				Task.Delay(TimeSpan.FromSeconds(5)).Wait();

				Assert.True(
					registered.SequenceEqual(initialData.Skip(idsToUnregister))
				);
			}
		}

		private RecognitionServiceServer CreateServer()
			=> new RecognitionServiceServer(_serverPort);

		private RecognitionServiceClient CreateClient()
			=> new RecognitionServiceClient(_clientUri);
	}
}
