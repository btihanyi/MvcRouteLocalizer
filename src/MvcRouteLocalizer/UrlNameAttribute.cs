using System;

namespace MvcRouteLocalizer
{
    /// <summary>
    /// Specifies the localized name for the target controller or action.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class UrlNameAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UrlNameAttribute"/> class by assigning the <paramref name="name"/> parameter
        /// to the <see cref="Name"/> property.
        /// </summary>
        /// <param name="name">The localized name of the controller or action.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="name"/> is empty.</exception>
        public UrlNameAttribute(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The name cannot be empty.", nameof(name));
            }

            this.Name = name;
        }

        /// <summary>
        /// Gets the localized name.
        /// </summary>
        public string Name { get; }
    }
}
