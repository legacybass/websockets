using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Net.WebSockets;
using WebSockets.Sockets;
using System.Reflection;

namespace WebSockets
{
	public static class ApplicationConfigurationExtensions
	{
		public static IServiceCollection AddMindfireWebSockets(this IServiceCollection services)
		{
			services.AddRouting();
			services.AddAuthorizationPolicyEvaluator();
			services.TryAddSingleton<ConnectionDispatcher>();
			services.AddAuthorization();
			return services;
		}

		public static IApplicationBuilder UseMindfireWebSockets(this IApplicationBuilder app, Action<SocketBuilder> callback)
		{
			var dispatcher = app.ApplicationServices.GetRequiredService<ConnectionDispatcher>();
			var routes = new RouteBuilder(app);
			callback(new SocketBuilder(routes, dispatcher));
			app.UseWebSockets();
			app.UseRouter(routes.Build());
			return app;
		}
	}
}
