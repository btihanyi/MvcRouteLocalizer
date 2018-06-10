using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace MvcRouteLocalizer
{
    /// <summary>
    /// Provides the required basic information about the available controllers and actions in the specified assemblies.
    /// </summary>
    public class ReflectionBasedMvcTopologyProvider : IMvcTopologyProvider
    {
        /// <summary>
        /// The suffix that must be present at the end of every controller class' name.
        /// </summary>
        protected const string ControllerSuffix = "Controller";

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionBasedMvcTopologyProvider"/> class by assigning
        /// the <see cref="Assemblies"/> collection with the specified <paramref name="assemblies"/> or all assemblies
        /// in the current <see cref="AppDomain"/>.
        /// </summary>
        /// <param name="assemblies">The assemblies that contain the controller types in the ASP.NET MVC application.
        /// If no parameters are specified (the array is empty), it will be filled with all available assemblies
        /// in <see cref="AppDomain.CurrentDomain"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assemblies"/> is null.</exception>
        public ReflectionBasedMvcTopologyProvider(params Assembly[] assemblies)
        {
            this.Assemblies = assemblies ?? throw new ArgumentNullException(nameof(assemblies));

            if (Assemblies.Count == 0)
            {
                Assemblies = AppDomain.CurrentDomain.GetAssemblies();
            }
        }

        /// <summary>
        /// Gets the assemblies where the controller types are defined.
        /// </summary>
        public IReadOnlyCollection<Assembly> Assemblies { get; }

        /// <summary>
        /// Creates an <see cref="MvcTopology"/> of the available controllers and their actions found in the
        /// specified <see cref="Assemblies"/>.
        /// </summary>
        /// <returns>The <see cref="MvcTopology"/> containing all the available controllers and their actions
        /// found in the specified <see cref="Assemblies"/>, including their actions.</returns>
        /// <exception cref="InvalidOperationException">Multiple actions with the same name have different URL names.</exception>
        public virtual MvcTopology GetMvcTopology()
        {
            var controllers = new List<ControllerInfo>();

            foreach (var type in Assemblies.SelectMany(assembly => assembly.GetTypes()))
            {
                if (IsControllerClass(type))
                {
                    string controllerName = GetControllerName(type);
                    string controllerUrlName = GetUrlName(type);
                    var actions = GetActionInfos(type);
                    var controllerInfo = new ControllerInfo(type.Namespace, controllerName, controllerUrlName, actions);
                    controllers.Add(controllerInfo);
                }
            }

            return new MvcTopology(controllers);
        }

        private IReadOnlyCollection<ActionInfo> GetActionInfos(Type controllerType)
        {
            var actions = new Dictionary<string, ActionInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (var method in controllerType.GetMethods(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                if (IsActionMethod(method))
                {
                    string actionName = GetActionName(method);
                    string actionUrlName = GetUrlName(method);
                    var actionInfo = new ActionInfo(actionName, actionUrlName);

                    // Check if there is already an action with the same name
                    if (actions.TryGetValue(actionName, out ActionInfo action))
                    {
                        if (action.UrlName == null)
                        {
                            actions[actionName] = action;
                        }
                        else if (!string.Equals(action.UrlName, actionName, StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidOperationException(
                                $"The '{controllerType}' type has multiple actions with the name of '{actionName}', " +
                                $"but they have different URL names specified.");
                        }
                    }
                    else
                    {
                        actions.Add(actionName, actionInfo);
                    }
                }
            }

            return actions.Values;
        }

        /// <summary>
        /// Determines whether the <paramref name="type"/> is a controller class based on MVC's built-in conventions.
        /// </summary>
        /// <param name="type">The CLR type that is a possible candidate for a controller class.</param>
        /// <returns><paramref langword="true" /> if the <paramref name="type"/> is a controller;
        /// otherwise <paramref langword="false" />.</returns>
        protected virtual bool IsControllerClass(Type type)
        {
            return (type.IsPublic &&
                    type.IsClass &&
                    !type.IsAbstract &&
                    !type.ContainsGenericParameters &&
                    type.Name.EndsWith(ControllerSuffix, StringComparison.OrdinalIgnoreCase) &&
                    !type.Name.Equals(ControllerSuffix, StringComparison.OrdinalIgnoreCase) &&
                    typeof(IController).IsAssignableFrom(type));
        }

        /// <summary>
        /// Determines whether the <paramref name="method"/> is an action method.
        /// </summary>
        /// <param name="method">The method inside a controller that is a is a possible candidate for an action method.</param>
        /// <returns><paramref langword="true" /> if the <paramref name="method"/> is an action method;
        /// otherwise <paramref langword="false" />.</returns>
        protected virtual bool IsActionMethod(MethodInfo method)
        {
            return (method.IsPublic &&
                    !method.IsStatic &&
                    !method.IsAbstract &&
                    !method.IsConstructor &&
                    !method.IsGenericMethod &&
                    !method.IsSpecialName &&
                    !method.IsDefined(typeof(NonActionAttribute)) &&
                    !method.GetBaseDefinition().DeclaringType.IsAssignableFrom(typeof(Controller)));
        }

        /// <summary>
        /// Gets the name of the controller from its type's name, based on MVC's built-in conventions.
        /// </summary>
        /// <param name="controllerType">The CLR type of the controller.</param>
        /// <returns>The controller's name.</returns>
        protected virtual string GetControllerName(Type controllerType)
        {
            return controllerType.Name.Substring(0, controllerType.Name.Length - ControllerSuffix.Length);
        }

        /// <summary>
        /// Gets the name of the action from its method's name, or from the name specified in the
        /// <see cref="ActionNameAttribute"/>, if present.
        /// </summary>
        /// <param name="actionMethod">The method of the action.</param>
        /// <returns>The action's name.</returns>
        protected virtual string GetActionName(MethodInfo actionMethod)
        {
            return (actionMethod.GetCustomAttribute<ActionNameAttribute>()?.Name ?? actionMethod.Name);
        }

        /// <summary>
        /// Gets the URL name of a controller or action from the <see cref="UrlNameAttribute.Name"/>, if present.
        /// </summary>
        /// <param name="memberInfo">The controller type or action method.</param>
        /// <returns>The URL name specified in the <see cref="UrlNameAttribute"/>, if present.</returns>
        protected virtual string GetUrlName(MemberInfo memberInfo)
        {
            return memberInfo.GetCustomAttribute<UrlNameAttribute>()?.Name;
        }
    }
}
