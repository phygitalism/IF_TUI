using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WebSocketSharp;

namespace RecognitionService.Api
{
	public class RecognitionServiceClient : IDisposable
	{
		private readonly WebSocket _webSocket;

		private Action<int[]> _onResponseList;

		public bool IsConnected => _webSocket.IsAlive;

		public RecognitionServiceClient(string uri)
		{
			_webSocket = new WebSocket(uri);

			_webSocket.OnMessage += HandleMessage;

			_webSocket.Connect();
		}

		private void HandleMessage(object sender, MessageEventArgs e)
		{
			try
			{
				Debug.WriteLine("Received message");

				if (e.IsText)
				{
					(var type, var payload) = ApiHelpers.ParseMessage(e.Data);
					switch (type)
					{
						case ApiEvent.ResponseList:
							var result = payload.ToObject<int[]>();
							_onResponseList?.Invoke(result);
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

		public void Dispose() => _webSocket.Close();

		public async Task<int[]> GetMarkerListAsync()
		{
			var tcs = new TaskCompletionSource<int[]>();
			_onResponseList += tcs.SetResult;

			Debug.WriteLine("Sending list request");

			var message = ApiHelpers.CreateMessage(ApiEvent.RequestList);
			_webSocket.Send(message.ToString());

			Debug.WriteLine("Waiting for list response");

			var result = await tcs.Task;
			_onResponseList -= tcs.SetResult;

			Debug.WriteLine("Received list response");

			return result;
		}

		public void RegisterMarker(int id, Triangle triangle)
		{
			Debug.WriteLine("Sending register request");

			var message = ApiHelpers.CreateMessage(
				ApiEvent.RegisterMarker,
				new RegisterMarkerRequestArgs(id, triangle)
			);

			_webSocket.Send(message.ToString());
		}

		public void UnregisterMarker(int id)
		{
			Debug.WriteLine("Sending unregister request");

			var message = ApiHelpers.CreateMessage(ApiEvent.UnregisterMarker, id);

			_webSocket.Send(message.ToString());
		}
	}
}
