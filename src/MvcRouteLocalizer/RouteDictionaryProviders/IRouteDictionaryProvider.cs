using System;

namespace MvcRouteLocalizer
{
    /// <summary>
    /// Provides the bijection logic between the original route values and their corresponding translations.
    /// </summary>
    public interface IRouteDictionaryProvider
    {
        /// <summary>
        /// Initializes the provider instance after its configuration has been set.
        /// </summary>
        /// <param name="topologyProvider">The topology provider containing the required information
        /// about the localizable controllers and actions.</param>
        void Init(IMvcTopologyProvider topologyProvider);

        /// <summary>
        /// Gets the localized name of the controller.
        /// </summary>
        /// <param name="controller">The controller's name to be localized.</param>
        /// <returns>The localized name of the controller.</returns>
        string GetLocalizedControllerName(string controller);

        /// <summary>
        /// Gets the localized name of the action.
        /// </summary>
        /// <param name="controller">The controller's name.</param>
        /// <param name="action">The action's name to be localized.</param>
        /// <returns>The localized name of the action.</returns>
        string GetLocalizedActionName(string controller, string action);

        /// <summary>
        /// Gets the controller's original name from its localization.
        /// </summary>
        /// <param name="localizedController">The controller's localized name.</param>
        /// <returns>The original name of the controller.</returns>
        string GetOriginalControllerName(string localizedController);

        /// <summary>
        /// Gets the action's original name from its localization.
        /// </summary>
        /// <param name="controller">The controller's original name.</param>
        /// <param name="localizedAction">The action's localized name.</param>
        /// <returns>The original name of the action.</returns>
        string GetOriginalActionName(string controller, string localizedAction);
    }
}
