using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;
using System.Threading.Tasks;
using RecognitionService.Api;
using Xunit;

namespace Tests
{
	public class ApiTest
	{
		private const int _serverPort = 12350;
		private static readonly string _serverUri = $@"ws:\\localhost:{_serverPort}";

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
		public void Reconnection()
		{
			using (var server = CreateServer())
			{
				using (var client = CreateClient())
				{
					Assert.True(client.IsConnected);
					Assert.True(server.HasConnections);
				}

				using (var client = CreateClient())
				{
					Assert.True(client.IsConnected);
					Assert.True(server.HasConnections);
				}
			}
		}

		[Fact]
		public async Task ListRequest()
		{
			using (var server = CreateServer())
			using (var client = CreateClient())
			{
				var list = new[] { 1, 2, 3 };

				server.OnMarkerListRequested += () => list;

				var receivedList = await client.GetMarkerListAsync();

				Assert.True(receivedList.SequenceEqual(list));
			}
		}

		[Fact]
		public async Task RegisterRequest()
		{
			using (var server = CreateServer())
			using (var client = CreateClient())
			{
				var registerList = new[]
				{
					(1, new Triangle(
						new Vector2(0),
						new Vector2(0),
						new Vector2(0)
					)),
					(2, new Triangle(
						new Vector2(1),
						new Vector2(2),
						new Vector2(3)
					)),
					(3, new Triangle(
						new Vector2(3),
						new Vector2(2),
						new Vector2(1)
					))
				};
				var registeredList = new List<(int, Triangle)>();

				var serverObservable = Observable.FromEvent<Action<int, Triangle>, (int, Triangle)>(
					handler => (arg1, arg2) => handler((arg1, arg2)),
					handler => server.OnRegisterMarkerRequested += handler,
					handler => server.OnRegisterMarkerRequested -= handler
				);

				var subscription = serverObservable
					.Subscribe(args => registeredList.Add(args));

				var resultAwaitable = serverObservable
					.Take(registerList.Length)
					.ToArray();

				foreach ((var id, var triangle) in registerList)
					client.RegisterMarker(id, triangle);

				await resultAwaitable;

				Assert.True(
					registeredList.SequenceEqual(registerList)
				);
			}
		}

		[Fact]
		public async Task UnregisterRequest()
		{
			using (var server = CreateServer())
			using (var client = CreateClient())
			{
				var idsToUnregister = 2;

				var initialData = new[] { 1, 2, 3 };
				var registered = initialData.ToList();

				var serverObservable = Observable.FromEvent<int>(
					h => server.OnUnregisterMarkerRequested += h,
					h => server.OnUnregisterMarkerRequested -= h
				);

				var subscription = serverObservable
					.Subscribe(id => registered.Remove(id));

				var resultAwaitable = serverObservable
					.Take(idsToUnregister)
					.ToArray();

				foreach (var id in initialData.Take(idsToUnregister))
					client.UnregisterMarker(id);

				await resultAwaitable;

				Assert.True(
					registered.SequenceEqual(initialData.Skip(idsToUnregister))
				);
			}
		}

		private RecognitionServiceServer CreateServer()
			=> new RecognitionServiceServer(_serverPort);

		private RecognitionServiceClient CreateClient()
			=> new RecognitionServiceClient(_serverUri);
	}
}
