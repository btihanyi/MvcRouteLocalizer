using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.Routing;

namespace MvcRouteLocalizer
{
    internal class LocalizedRoute : Route
    {
        private readonly Func<string, RouteValueDictionary, RouteValueDictionary> matchRoute;
        private readonly Func<RouteValueDictionary, RouteValueDictionary, RouteValueDictionary, RouteValueDictionary, BoundUrl> bindUrl;

        public LocalizedRoute(Route route, IRouteDictionaryProvider dictionary)
            : base(route.Url, WrapSpecialDefaultValues(route.Defaults), route.Constraints, route.DataTokens,
                   new LocalizedRouteHandler(route.RouteHandler))
        {
            this.Dictionary = dictionary;

            var parsedRoute = typeof(Route).GetField("_parsedRoute", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
            matchRoute = CreateMatchRouteDelegate(parsedRoute);
            bindUrl = CreateBindUrlDelegate(parsedRoute);
        }

        private IRouteDictionaryProvider Dictionary { get; }

        private static Func<string, RouteValueDictionary, RouteValueDictionary> CreateMatchRouteDelegate(object parsedRoute)
        {
            var virtualPath = Expression.Parameter(typeof(string), "virtualPath");
            var defaultValues = Expression.Parameter(typeof(RouteValueDictionary), "defaultValues");

            // (virtualPath, defaultValues) => parsedRoute.Match(virtualPath, defaultValues)
            return Expression.Lambda<Func<string, RouteValueDictionary, RouteValueDictionary>>(
                       Expression.Call(
                           Expression.Constant(parsedRoute),
                           parsedRoute.GetType().GetMethod("Match"),
                           virtualPath, defaultValues
                       ),
                       virtualPath, defaultValues
                   ).Compile();
        }

        private static Func<RouteValueDictionary, RouteValueDictionary, RouteValueDictionary, RouteValueDictionary, BoundUrl>
            CreateBindUrlDelegate(object parsedRoute)
        {
            var currentValues = Expression.Parameter(typeof(RouteValueDictionary), "currentValues");
            var values = Expression.Parameter(typeof(RouteValueDictionary), "values");
            var defaultValues = Expression.Parameter(typeof(RouteValueDictionary), "currentValues");
            var constraints = Expression.Parameter(typeof(RouteValueDictionary), "constraints");
            var bindMethod = parsedRoute.GetType().GetMethod("Bind");
            var result = Expression.Variable(bindMethod.ReturnType, "result");

            // (currentValues, values, defaultValues, constraints) =>
            // {
            //     var result = parsedRoute.Bind(currentValues, values, defaultValues, constraints);
            //     return (result != null ? new BoundUrl() { Url = result.Url, Values = result.Values } : null);
            // }
            return Expression.Lambda<Func<RouteValueDictionary, RouteValueDictionary, RouteValueDictionary, RouteValueDictionary, BoundUrl>>(
                       Expression.Block(
                           new[] { result },
                           Expression.Assign(
                               result,
                               Expression.Call(
                                   Expression.Constant(parsedRoute),
                                   bindMethod,
                                   currentValues, values, defaultValues, constraints
                               )
                           ),
                           Expression.Condition(
                               Expression.NotEqual(
                                   result,
                                   Expression.Constant(null, result.Type)
                                ),
                               Expression.MemberInit(
                                   Expression.New(typeof(BoundUrl)),
                                   Expression.Bind(
                                       typeof(BoundUrl).GetProperty(nameof(BoundUrl.Url)),
                                       Expression.Property(result, "Url")
                                   ),
                                   Expression.Bind(
                                       typeof(BoundUrl).GetProperty(nameof(BoundUrl.Values)),
                                       Expression.Property(result, "Values")
                                   )
                               ),
                               Expression.Constant(null, typeof(BoundUrl))
                            )
                       ),
                       currentValues, values, defaultValues, constraints
                   ).Compile();
        }

        private static RouteValueDictionary WrapSpecialDefaultValues(RouteValueDictionary values)
        {
            var result = new RouteValueDictionary();

            if (values != null)
            {
                foreach (var item in values)
                {
                    if (item.Value is string value &&
                        (string.Equals(item.Key, RouteDataKeys.Controller, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(item.Key, RouteDataKeys.Action, StringComparison.OrdinalIgnoreCase)))
                    {
                        result.Add(item.Key, new DefaultValueWrapper(value));
                    }
                    else
                    {
                        result.Add(item.Key, item.Value);
                    }
                }
            }

            return result;
        }

        private static RouteValueDictionary UnwrapSpecialDefaultValues(RouteValueDictionary values)
        {
            if (values != null)
            {
                var result = new RouteValueDictionary();

                foreach (var item in values)
                {
                    if (item.Value is DefaultValueWrapper wrapper)
                    {
                        result.Add(item.Key, wrapper.Value);
                    }
                    else
                    {
                        result.Add(item.Key, item.Value);
                    }
                }
                return result;
            }
            else
            {
                return null;
            }
        }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            // Parse incoming URL (we trim off the first two chars since they're always "~/")
            string requestPath = httpContext.Request.AppRelativeCurrentExecutionFilePath.Substring(2) + httpContext.Request.PathInfo;

            var values = matchRoute(requestPath, Defaults);
            if (values == null)
            {
                // If we got back a null value set, that means the URL did not match
                return null;
            }

            if (values.ContainsKey(RouteDataKeys.Controller) && values.ContainsKey(RouteDataKeys.Action))
            {
                string controller = GetControllerNameFromLocalized(values);
                if (controller == null)
                {
                    return null;
                }

                string action = GetActionNameFromLocalized(values, controller);
                if (action == null)
                {
                    return null;
                }

                values[RouteDataKeys.Controller] = controller;
                values[RouteDataKeys.Action] = action;
            }
            else
            {
                values = UnwrapSpecialDefaultValues(values);
            }

            // Validate the values
            if (!ProcessConstraints(httpContext, values, RouteDirection.IncomingRequest))
            {
                return null;
            }

            var routeData = new RouteData(this, RouteHandler);

            // Copy the matched values
            foreach (var value in values)
            {
                routeData.Values.Add(value.Key, value.Value);
            }

            // Copy the DataTokens from the Route to the RouteData
            if (DataTokens != null)
            {
                foreach (var token in DataTokens)
                {
                    routeData.DataTokens.Add(token.Key, token.Value);
                }
            }

            return routeData;
        }

        private string GetControllerNameFromLocalized(RouteValueDictionary values)
        {
            switch (values[RouteDataKeys.Controller])
            {
                case null:
                default:
                    return null;

                case string localizedController:
                    return Dictionary.GetOriginalControllerName(localizedController);

                case DefaultValueWrapper wrapper:
                    return wrapper.Value;
            }
        }

        private string GetActionNameFromLocalized(RouteValueDictionary values, string controller)
        {
            switch (values[RouteDataKeys.Action])
            {
                case null:
                default:
                    return null;

                case string localizedAction:
                    return Dictionary.GetOriginalActionName(controller, localizedAction);

                case DefaultValueWrapper wrapper:
                    return wrapper.Value;
            }
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary explicitValues)
        {
            var requestValues = requestContext.RouteData.Values;
            var defaultValues = UnwrapSpecialDefaultValues(Defaults);

            if ((explicitValues[RouteDataKeys.Controller] ?? requestValues[RouteDataKeys.Controller] ?? defaultValues[RouteDataKeys.Controller]) is string controller &&
                (explicitValues[RouteDataKeys.Action] ?? requestValues[RouteDataKeys.Action] ?? defaultValues[RouteDataKeys.Action]) is string action)
            {
                string localizedController = Dictionary.GetLocalizedControllerName(controller);
                string localizedAction = Dictionary.GetLocalizedActionName(controller, action);

                if (localizedController != null || localizedAction != null)
                {
                    explicitValues = new RouteValueDictionary(explicitValues);
                    requestValues = new RouteValueDictionary(requestValues);

                    if (localizedController != null)
                    {
                        var segment = new LocalizedPathSegment(controller, localizedController);
                        UpdateValueIfExists(explicitValues, RouteDataKeys.Controller, segment);
                        UpdateValueIfExists(requestValues, RouteDataKeys.Controller, segment);
                        UpdateValueIfExists(defaultValues, RouteDataKeys.Controller, segment);
                    }

                    if (localizedAction != null)
                    {
                        var segment = new LocalizedPathSegment(action, localizedAction);
                        UpdateValueIfExists(explicitValues, RouteDataKeys.Action, segment);
                        UpdateValueIfExists(requestValues, RouteDataKeys.Action, segment);
                        UpdateValueIfExists(defaultValues, RouteDataKeys.Action, segment);
                    }

                    void UpdateValueIfExists(RouteValueDictionary values, string key, LocalizedPathSegment segment)
                    {
                        if (values.TryGetValue(key, out object result) && (result is string value) && (value == segment.OriginalText))
                        {
                            values[key] = segment;
                        }
                    }
                }
            }

            // Try to generate a URL that represents the values passed in based on current
            // values from the RouteData and new values using the specified Route.
            var boundUrl = bindUrl(requestValues, explicitValues, defaultValues, Constraints);
            if (boundUrl == null)
            {
                return null;
            }

            // Convert back localized bound values to their originals
            var boundValues = new RouteValueDictionary(boundUrl.Values);
            foreach (var item in boundUrl.Values)
            {
                if (item.Value is LocalizedPathSegment segment)
                {
                    boundValues[item.Key] = segment.OriginalText;
                }
            }

            // Verify that the route matches the validation rules
            if (!ProcessConstraints(requestContext.HttpContext, boundValues, RouteDirection.UrlGeneration))
            {
                return null;
            }

            var virtualPathdata = new VirtualPathData(this, boundUrl.Url);

            // Add the DataTokens from the Route to the VirtualPathData
            if (DataTokens != null)
            {
                foreach (var token in DataTokens)
                {
                    virtualPathdata.DataTokens.Add(token.Key, token.Value);
                }
            }

            return virtualPathdata;
        }

        private bool ProcessConstraints(HttpContextBase httpContext, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (Constraints != null)
            {
                foreach (var constraint in Constraints)
                {
                    if (!ProcessConstraint(httpContext, constraint.Value, constraint.Key, values, routeDirection))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private sealed class BoundUrl
        {
            public string Url { get; set; }

            public RouteValueDictionary Values { get; set; }
        }

        private sealed class LocalizedPathSegment
        {
            public LocalizedPathSegment(string original, string localized)
            {
                this.OriginalText = original;
                this.LocalizedText = localized;
            }

            public string OriginalText { get; }

            public string LocalizedText { get; }

            public override string ToString()
            {
                return LocalizedText;
            }
        }

        private sealed class DefaultValueWrapper
        {
            public DefaultValueWrapper(string value)
            {
                this.Value = value;
            }

            public string Value { get; }

            public override string ToString()
            {
                return Value;
            }
        }
    }
}
