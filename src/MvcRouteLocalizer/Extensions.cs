using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MvcRouteLocalizer;

namespace System.Web.Routing
{
    /// <summary>
    /// Extension methods for <see cref="RouteCollection"/>.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Localizes all convention-based routes in the <paramref name="routeCollection"/> with a default
        /// <see cref="LocalizationOptions"/> instance. This method should be called after all routes have been configured.
        /// </summary>
        /// <param name="routeCollection">The collection containing all the routes of the ASP.NET MVC application.</param>
        /// <exception cref="ArgumentException"><paramref name="routeCollection"/> is null.</exception>
        public static void Localize(this RouteCollection routeCollection)
        {
            Localize(routeCollection, options: null);
        }

        /// <summary>
        /// Localizes all convention-based routes in the <paramref name="routeCollection"/> with a custom configured
        /// <see cref="LocalizationOptions"/> instance. This method should be called after all routes have been configured.
        /// </summary>
        /// <param name="routeCollection">The collection containing all the routes of the ASP.NET MVC application.</param>
        /// <param name="options">A callback to configure the default <see cref="LocalizationOptions"/> instance.</param>
        /// <exception cref="ArgumentException"><paramref name="routeCollection"/> is null.</exception>
        public static void Localize(this RouteCollection routeCollection, Action<LocalizationOptions> options)
        {
            if (routeCollection == null)
            {
                throw new ArgumentNullException(nameof(routeCollection));
            }

            // Create and initialize a new LocalizationOptions instance
            var localizationOptions = new LocalizationOptions();
            options?.Invoke(localizationOptions);
            localizationOptions.Dictionary.Init(localizationOptions.MvcTopologyProvider);

            // Create separate collection for all named and unnamed routes
            var namedMap = typeof(RouteCollection).GetField("_namedMap", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(routeCollection)
                           as Dictionary<string, RouteBase>
                           ?? throw new InvalidOperationException($"There is no _namedMap field in the {nameof(RouteCollection)} class.");
            var unnamedRoutes = routeCollection.Except(namedMap.Values);

            // And combine them to a common list with their nullable names as key
            var routes = namedMap.Concat(unnamedRoutes.Select(route => new KeyValuePair<string, RouteBase>(null, route))).ToList();

            // Iterate through all available routes
            foreach (var routeItem in routes)
            {
                // Which has the type Route
                if ((routeItem.Value is Route route) && (route.GetType() == typeof(Route)))
                {
                    // Create a new LocalizedRoute proxy
                    string routeName = routeItem.Key;
                    var localizedRoute = new LocalizedRoute(route, localizationOptions.Dictionary);

                    // Replace the original route with this new one
                    int index = routeCollection.IndexOf(route);
                    routeCollection[index] = localizedRoute;
                    if (!string.IsNullOrEmpty(routeName))
                    {
                        namedMap[routeName] = localizedRoute;
                    }
                }
            }
        }
    }
}
