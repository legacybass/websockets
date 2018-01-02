using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebSockets.Sockets;

namespace WebSockets
{
	public abstract class Hub : IDisposable
	{
		private Dictionary<Guid, Client> _Clients { get; } = new Dictionary<Guid, Client>();
		public IEnumerable<Client> Clients { get => _Clients.Values; }
		private bool Disposed { get; set; }

		public virtual Task OnConnected(Client client) => Task.CompletedTask;

		public virtual Task OnDisconnected(Client client) => Task.CompletedTask;

		public virtual Task OnMessageReceived(Client client, string message) => Task.CompletedTask;

		public void AddClient(Client socket) => _Clients.Add(socket.Id, socket);
		public void RemoveClient(Client client) => _Clients.Remove(client.Id);

		public void Broadcast(string content)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			if (Disposed)
				return;

			Dispose(true);
			Disposed = true;
		}

		protected virtual void Dispose(bool disposing) { }
	}
}
