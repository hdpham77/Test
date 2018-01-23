using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CERS.EDT.Web.Services
{
	public static class Extensions
	{
		public static void MapRoute(this RouteCollection routes, EDTServiceRoute route, string url, object defaults)
		{
			routes.MapRoute(GetRouteName(route), url, defaults);
		}

		public static string GetRouteName(EDTServiceRoute route)
		{
			return typeof(EDTServiceRoute).Name + route.ToString();
		}

		public static string RouteUrl(this UrlHelper url, EDTServiceRoute route, object routeValues)
		{
			return url.RouteUrl(GetRouteName(route), routeValues);
		}

		public static string EDTServiceRouteUrl(this UrlHelper url, EDTServiceRoute route, object routeValues)
		{
			HttpRequest request = HttpContext.Current.Request;
			string schemeHostCombo = request.Url.Scheme + "://" + request.Url.DnsSafeHost;
			return schemeHostCombo + url.RouteUrl(route, routeValues);
		}

		public static string EDTServiceRouteUrl(this UrlHelper url, EDTServiceRoute route)
		{
			HttpRequest request = HttpContext.Current.Request;
			string schemeHostCombo = request.Url.Scheme + "://" + request.Url.DnsSafeHost;
			return schemeHostCombo + url.RouteUrl(route, null);
		}
	}
}