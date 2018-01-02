using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSockets.Sockets
{
	public class Client
	{
		internal WebSocket Socket { get; private set; }
		public Guid Id { get; }

		public Client(WebSocket socket)
		{
			Socket = socket;
			Id = Guid.NewGuid();
		}

		public Task Send(string type, string content) => Send(new TypedMessage { Type = type, Message = content });

		public Task Send<T>(string type, T content) => Send(new TypedMessage { Type = type, Message = JsonConvert.SerializeObject(content) });

		private Task Send(TypedMessage typedMessage) => Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(typedMessage)));

		private Task Send(byte[] content) => 
			Socket.SendAsync(new ArraySegment<byte>(content, 0, content.Length), WebSocketMessageType.Text, true, CancellationToken.None);
	}
}
