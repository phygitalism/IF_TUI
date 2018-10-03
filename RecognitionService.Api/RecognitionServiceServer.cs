using System;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace RecognitionService.Api
{
	public class RecognitionServiceServer : IDisposable
	{
		private readonly WebSocketServer _webSocketServer;
		private readonly MessageResponder _messageResponder;

		public event Func<int[]> OnListRequested;
		public event Action<int, Rectangle> OnRegisterRequested;
		public event Action<int> OnUnregisterRequested;

		public bool HasConnections => _webSocketServer.WebSocketServices.SessionCount > 0;

		public RecognitionServiceServer(int port)
		{
			_webSocketServer = new WebSocketServer(port);
			_messageResponder = new MessageResponder(this);

			_webSocketServer.AddWebSocketService("/", () => _messageResponder);

			_webSocketServer.Start();
		}

		public void Dispose() => _webSocketServer.Stop();

		private class MessageResponder : WebSocketBehavior
		{
			private readonly RecognitionServiceServer _server;

			public MessageResponder(RecognitionServiceServer server)
			{
				_server = server;
			}

			protected override void OnMessage(MessageEventArgs e)
			{
				try
				{
					Debug.WriteLine("Received message");

					if (e.IsText)
					{
						(var type, var payload) = ApiHelpers.ParseMessage(e.Data);

						switch (type)
						{
							case ApiEvent.RequestList:
								HandleListRequest();
								break;
							case ApiEvent.RegisterMarker:
								HandleRegisterRequest(payload);
								break;
							case ApiEvent.UnregisterMarker:
								HandleUnregisterRequest(payload);
								break;
							default:
								throw new NotSupportedException();
						}
					}
					else
						throw new NotSupportedException();
				}
				catch (Exception exception)
				{
					Debug.WriteLine($"Exception:{exception.Message}");
				}
			}

			private void HandleRegisterRequest(JToken payload)
			{
				Debug.WriteLine("Register requested");

				var request = payload.ToObject<RegisterRequestArgs>();

				_server?.OnRegisterRequested(request.id, request.rectangle);
			}

			private void HandleUnregisterRequest(JToken payload)
			{
				Debug.WriteLine("Unregister requested");

				var id = payload.ToObject<int>();

				_server.OnUnregisterRequested?.Invoke(id);
			}

			private void HandleListRequest()
			{
				Debug.WriteLine("List requested");

				var list = _server.OnListRequested?.Invoke();

				var message = ApiHelpers.CreateMessage(ApiEvent.ResponseList, list);

				Send(message.ToString());
			}
		}
	}
}
