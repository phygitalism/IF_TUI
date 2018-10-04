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
		public async Task RegisterRequest()
		{
			using (var server = CreateServer())
			using (var client = CreateClient())
			{
				var registerList = new[]
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

				var serverObservable = Observable.FromEvent<Action<int, Rectangle>, (int, Rectangle)>(
					handler => (arg1, arg2) => handler((arg1, arg2)),
					handler => server.OnRegisterRequested += handler,
					handler => server.OnRegisterRequested -= handler
				);

				var subscription = serverObservable
					.Subscribe(args => registeredList.Add(args));

				var resultAwaitable = serverObservable
					.Take(registerList.Length)
					.ToArray();

				foreach ((var id, var rectangle) in registerList)
					client.RegisterId(id, rectangle);

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
					h => server.OnUnregisterRequested += h,
					h => server.OnUnregisterRequested -= h
				);

				var subscription = serverObservable
					.Subscribe(id => registered.Remove(id));

				var resultAwaitable = serverObservable
					.Take(idsToUnregister)
					.ToArray();

				foreach (var id in initialData.Take(idsToUnregister))
					client.UnregisterId(id);

				await resultAwaitable;

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
