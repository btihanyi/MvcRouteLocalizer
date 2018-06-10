using System;
using System.Collections.Generic;

namespace MvcRouteLocalizer
{
    /// <summary>
    /// Provides basic information about a controller and its actions.
    /// </summary>
    public class ControllerInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerInfo"/> class.
        /// </summary>
        /// <param name="name">The name of the controller (without the Controller suffix).</param>
        /// <param name="actions">The action methods in the controller.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is null.-or-<paramref name="actions"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="name"/> is empty.-or-More than one action has the same name.</exception>
        public ControllerInfo(string name, IReadOnlyCollection<ActionInfo> actions)
            : this(null, name, null, actions)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerInfo"/> class.
        /// </summary>
        /// <param name="name">The name of the controller (without the Controller suffix).</param>
        /// <param name="urlName">The URL name of the controller.</param>
        /// <param name="actions">The action methods in the controller.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is null.-or-<paramref name="actions"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="name"/> is empty.-or-More than one action has the same name.</exception>
        public ControllerInfo(string name, string urlName, IReadOnlyCollection<ActionInfo> actions)
            : this(null, name, urlName, actions)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerInfo"/> class.
        /// </summary>
        /// <param name="namespace">The namespace of the controller type.</param>
        /// <param name="name">The name of the controller (without the Controller suffix).</param>
        /// <param name="urlName">The URL name of the controller.</param>
        /// <param name="actions">The action methods in the controller.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is null.-or-<paramref name="actions"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="name"/> is empty.-or-More than one action has the same name.</exception>
        public ControllerInfo(string @namespace, string name, string urlName, IReadOnlyCollection<ActionInfo> actions)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Namespace = @namespace;
            this.UrlName = (!string.IsNullOrWhiteSpace(urlName) ? urlName : null);

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The name cannot be empty.", nameof(name));
            }
            if (actions == null)
            {
                throw new ArgumentNullException(nameof(actions));
            }

            // Add the actions to the controller
            var dictionary = new Dictionary<string, ActionInfo>(actions.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var action in actions)
            {
                if (!dictionary.ContainsKey(action.Name))
                {
                    dictionary.Add(action.Name, action);
                }
                else
                {
                    throw new ArgumentException($"There is more than one action defined with the same name of '{action.Name}' " +
                        $"in the controller ${name}.");
                }
            }
            this.Actions = dictionary;
        }

        /// <summary>
        /// Gets the namespace of the controller.
        /// </summary>
        public string Namespace { get; }

        /// <summary>
        /// Gets the name of the controller (without the Controller suffix).
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the URL name of the controller, or null if it's not specified.
        /// </summary>
        public string UrlName { get; }

        /// <summary>
        /// Gets the action methods of the controller.
        /// </summary>
        public IReadOnlyDictionary<string, ActionInfo> Actions { get; }
    }
}
