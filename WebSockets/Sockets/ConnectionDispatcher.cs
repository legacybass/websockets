using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSockets.Sockets
{
	public class ConnectionDispatcher
	{
		public async Task ExecuteAsync<T>(HttpContext context, IEnumerable<IAuthorizeData> authorizeAttributes, T hub) where T : Hub
		{
			if(!context.WebSockets.IsWebSocketRequest)
				return;

			if (!await AuthorizeHelper.AuthorizeAsync(context, authorizeAttributes))
				return;
			WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();
			var client = new Client(socket);
			hub.AddClient(client);
			await hub.OnConnected(client);

			Debug.WriteLine($"Client '{client.Id}' connected.");

			await StartReceiving(client, hub);

			Debug.WriteLine($"Client '{client.Id}' disconnected.");

			hub.RemoveClient(client);
			await hub.OnDisconnected(client);
		}

		protected async Task<WebSocketReceiveResult> StartReceiving<T>(Client client, T hub) where T : Hub
		{
			var incomingMessage = new List<ArraySegment<byte>>();
			while(true)
			{
				const int bufferSize = 0x1000;
				int totalBytes = 0;
				WebSocketReceiveResult result;

				do
				{
					var buffer = new ArraySegment<byte>(new byte[bufferSize]);
					result = await client.Socket.ReceiveAsync(buffer, CancellationToken.None);

					if (result.MessageType == WebSocketMessageType.Close)
					{
						return result;
					}

					var truncBuffer = new ArraySegment<byte>(buffer.Array, 0, result.Count);
					incomingMessage.Add(truncBuffer);
					totalBytes += result.Count;
				} while (!result.EndOfMessage);

				byte[] messageBuffer = null;

				if(incomingMessage.Count > 1)
				{
					messageBuffer = new byte[totalBytes];
					int offset = 0;

					for(int i = 0; i < incomingMessage.Count; ++i)
					{
						Buffer.BlockCopy(incomingMessage[i].Array, 0, messageBuffer, offset, incomingMessage[i].Count);
						offset += incomingMessage[i].Count;
					}
				}
				else
				{
					messageBuffer = new byte[incomingMessage[0].Count];
					Buffer.BlockCopy(incomingMessage[0].Array, incomingMessage[0].Offset, messageBuffer, 0, incomingMessage[0].Count);
				}

				string messageString = Encoding.UTF8.GetString(messageBuffer);

				try
				{
					// Try to deserialize it as a typed message
					var typedMessage = JsonConvert.DeserializeObject<TypedMessage>(messageString);
					var method = hub.GetType().GetMethod(typedMessage.Type, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic
																			| BindingFlags.Public | BindingFlags.Static);

					if (method != null)
					{
						if (method.ReturnType == typeof(Task) || method.ReturnType.IsSubclassOf(typeof(Task)))
						{
							await (Task)method.Invoke(hub, new object[] { client, typedMessage.Message });
						}
						else
							method.Invoke(hub, new object[] { client, typedMessage.Message });
					}
					else
						await hub.OnMessageReceived(client, messageString);
				}
				catch (JsonReaderException)
				{
					// This means that either it couldn't deserialize, or that it wasn't the right type.
					// Just send the whole thing through the virtual hub method and call it good enough.

					await hub.OnMessageReceived(client, messageString);
				}

				incomingMessage.Clear();
			}
		}
	}
}
