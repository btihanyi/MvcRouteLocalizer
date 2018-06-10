using System;

namespace MvcRouteLocalizer
{
    /// <summary>
    /// Provides basic information about a controller action.
    /// </summary>
    public class ActionInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionInfo"/> class.
        /// </summary>
        /// <param name="name">The name of the action.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="name"/> is empty.</exception>
        public ActionInfo(string name)
            : this(name, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionInfo"/> class.
        /// </summary>
        /// <param name="name">The name of the action.</param>
        /// <param name="urlName">The URL name of the action.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="name"/> is empty.</exception>
        public ActionInfo(string name, string urlName)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.UrlName = (!string.IsNullOrWhiteSpace(urlName) ? urlName : null);

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The name cannot be empty.", nameof(name));
            }
        }

        /// <summary>
        /// Gets the name of the action.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the URL name of the action, or null if it's not specified.
        /// </summary>
        public string UrlName { get; }
    }
}
