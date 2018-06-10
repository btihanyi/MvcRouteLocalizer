using System;
using System.Collections.Generic;
using System.Linq;

namespace MvcRouteLocalizer
{
    /// <summary>
    /// Represents an ASP.NET MVC application's hierarchical structure, particularly the available controllers and their actions.
    /// </summary>
    public class MvcTopology
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MvcTopology"/> class.
        /// </summary>
        /// <param name="controllers">The available controllers in the application.</param>
        /// <exception cref="ArgumentNullException"><paramref name="controllers"/> is null.</exception>
        /// <exception cref="ArgumentException">More than one controller has the same name.</exception>
        public MvcTopology(IReadOnlyCollection<ControllerInfo> controllers)
        {
            if (controllers == null)
            {
                throw new ArgumentNullException(nameof(controllers));
            }

            var dictionary = new Dictionary<string, ControllerInfo>(controllers.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var controller in controllers)
            {
                if (!dictionary.ContainsKey(controller.Name))
                {
                    dictionary.Add(controller.Name, controller);
                }
                else
                {
                    throw new ArgumentException($"There is more than one controller defined with the same name of '{controller.Name}'.");
                }
            }
            this.Controllers = dictionary;
        }

        /// <summary>
        /// Gets the available controllers in the application.
        /// </summary>
        public IReadOnlyDictionary<string, ControllerInfo> Controllers { get; }
    }
}
