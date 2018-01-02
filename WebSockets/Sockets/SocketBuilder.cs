using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace WebSockets.Sockets
{
	public class SocketBuilder
	{
		protected RouteBuilder Routes { get; }
		public ConnectionDispatcher Dispatcher { get; }

		public SocketBuilder(RouteBuilder routes, ConnectionDispatcher dispatcher)
		{
			Routes = routes;
			Dispatcher = dispatcher;
		}

		public void MapSocket<THandler>(string path, THandler handler) where THandler : Hub
		{
			MapSocket(path, () => handler);
		}

		public void MapSocket<THandler>(string path, Func<THandler> factory) where THandler : Hub
		{
			if (path.StartsWith("/") || path.StartsWith("~"))
				path = path.Substring(1);

			var authorizeAttributes = typeof(THandler).GetCustomAttributes<AuthorizeAttribute>(true);

			Routes.MapRoute(path, c => Dispatcher.ExecuteAsync(c, authorizeAttributes, factory()));
		}
	}
}
