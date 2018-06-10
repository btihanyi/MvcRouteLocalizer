using System;

namespace MvcRouteLocalizer
{
    /// <summary>
    /// Provides the bijection logic between the original route values and their corresponding translations
    /// using the dictionary-based <see cref="Localization"/> class.
    /// </summary>
    public abstract partial class RouteDictionaryProviderBase : IRouteDictionaryProvider
    {
        /// <summary>
        /// Initializes the provider instance after its configuration has been set.
        /// </summary>
        /// <param name="topologyProvider">The topology provider containing the required information
        /// about the localizable controllers and actions.</param>
        public abstract void Init(IMvcTopologyProvider topologyProvider);

        /// <summary>
        /// Gets the currently used <see cref="Localization"/> instance.
        /// </summary>
        /// <returns>The currently used <see cref="Localization"/> instance.</returns>
        protected abstract Localization GetLocalization();

        /// <summary>
        /// Gets the localized name of the controller.
        /// </summary>
        /// <param name="controller">The controller's name to be localized.</param>
        /// <returns>The localized name of the controller.</returns>
        public virtual string GetLocalizedControllerName(string controller)
        {
            if (GetLocalization().ControllersByOriginalName.TryGetValue(controller, out ControllerLocalization controllerLocalization))
            {
                return controllerLocalization.LocalizedName;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the localized name of the action.
        /// </summary>
        /// <param name="controller">The controller's name.</param>
        /// <param name="action">The action's name to be localized.</param>
        /// <returns>The localized name of the action.</returns>
        public virtual string GetLocalizedActionName(string controller, string action)
        {
            if (GetLocalization().ControllersByOriginalName.TryGetValue(controller, out ControllerLocalization controllerLocalization) &&
                controllerLocalization.ActionsByOriginalName.TryGetValue(action, out ActionLocalization actionLocalization))
            {
                return actionLocalization.LocalizedName;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the controller's original name from its localization.
        /// </summary>
        /// <param name="localizedController">The controller's localized name.</param>
        /// <returns>The original name of the controller.</returns>
        public virtual string GetOriginalControllerName(string localizedController)
        {
            if (GetLocalization().ControllersByLocalizedName.TryGetValue(localizedController, out ControllerLocalization controllerLocalization))
            {
                return controllerLocalization.OriginalName;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the action's original name from its localization.
        /// </summary>
        /// <param name="controller">The controller's original name.</param>
        /// <param name="localizedAction">The action's localized name.</param>
        /// <returns>The original name of the action.</returns>
        public virtual string GetOriginalActionName(string controller, string localizedAction)
        {
            if (GetLocalization().ControllersByOriginalName.TryGetValue(controller, out ControllerLocalization controllerLocalization) &&
                controllerLocalization.ActionsByLocalizedName.TryGetValue(localizedAction, out ActionLocalization actionLocalization))
            {
                return actionLocalization.OriginalName;
            }
            else
            {
                return null;
            }
        }
    }
}
