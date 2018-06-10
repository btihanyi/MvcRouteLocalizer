using System;

namespace MvcRouteLocalizer
{
    /// <summary>
    /// Provides simple translations based on the names specified in <see cref="UrlNameAttribute"/>s.
    /// </summary>
    public class DefaultRouteDictionaryProvider : MonolingualRouteDictionaryProvider
    {
        /// <summary>
        /// Gets the localized name of the given <paramref name="controller"/> based on the <see cref="UrlNameAttribute.Name"/> value.
        /// </summary>
        /// <param name="controller">The controller to be localized.</param>
        /// <returns>The <see cref="UrlNameAttribute.Name"/>'s value, if the attribute is present;
        /// otherwise the <see cref="ControllerInfo.Name"/>'s value.</returns>
        protected override string GetLocalizationForController(ControllerInfo controller)
        {
            return (controller.UrlName ?? controller.Name);
        }

        /// <summary>
        /// Gets the localized name of the given <paramref name="action"/> based on the <see cref="UrlNameAttribute.Name"/> value.
        /// </summary>
        /// <param name="controller">The controller of the <paramref name="action"/>.</param>
        /// <param name="action">The action to be localized.</param>
        /// <returns>The <see cref="UrlNameAttribute.Name"/>'s value, if the attribute is present;
        /// otherwise the <see cref="ActionInfo.Name"/>'s value.</returns>
        protected override string GetLocalizationForAction(ControllerInfo controller, ActionInfo action)
        {
            return (action.UrlName ?? action.Name);
        }
    }
}
