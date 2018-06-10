using System;
using System.Collections.Generic;

namespace MvcRouteLocalizer
{
    /// <summary>
    /// Provides the bijection logic between the original route values and their corresponding translations
    /// in only one culture.
    /// </summary>
    public abstract class MonolingualRouteDictionaryProvider : RouteDictionaryProviderBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MonolingualRouteDictionaryProvider"/> class.
        /// </summary>
        protected MonolingualRouteDictionaryProvider()
        {
            SingleLocalization = new Localization();
        }

        /// <summary>
        /// Gets the sole <see cref="Localization"/> instance to be used in the application.
        /// </summary>
        protected Localization SingleLocalization { get; }

        /// <summary>
        /// Gets the sole <see cref="Localization"/> instance to be used in the application.
        /// </summary>
        /// <returns>The value of the <see cref="SingleLocalization"/> property.</returns>
        protected override Localization GetLocalization() => SingleLocalization;

        /// <summary>
        /// Initializes the provider instance with data from <see cref="SingleLocalization"/>.
        /// </summary>
        /// <param name="topologyProvider">The topology provider containing the required information
        /// about the localizable controllers and actions.</param>
        /// <exception cref="InvalidOperationException">
        /// More than one controller's name matches with the localization.-or-
        /// More than one action's name matches with the localization within the same controller.
        /// </exception>
        public override void Init(IMvcTopologyProvider topologyProvider)
        {
            var topology = topologyProvider.GetMvcTopology();

            foreach (var controllerInfo in topology.Controllers.Values)
            {
                string localizedControllerName = GetLocalizationForController(controllerInfo);

                if (SingleLocalization.ControllersByLocalizedName.ContainsKey(localizedControllerName))
                {
                    throw new InvalidOperationException(
                        $"There are more than one controller with localized the name of '{localizedControllerName}'.");
                }

                var controllerLocalization = new ControllerLocalization()
                {
                    OriginalName = controllerInfo.Name,
                    LocalizedName = localizedControllerName
                };
                SingleLocalization.ControllersByOriginalName.Add(controllerInfo.Name, controllerLocalization);
                SingleLocalization.ControllersByLocalizedName.Add(localizedControllerName, controllerLocalization);

                AddActionLocalizations(controllerInfo, controllerLocalization);
            }
        }

        private void AddActionLocalizations(ControllerInfo controllerInfo, ControllerLocalization controllerLocalization)
        {
            foreach (var actionInfo in controllerInfo.Actions.Values)
            {
                string localizedActionName = GetLocalizationForAction(controllerInfo, actionInfo);

                if (controllerLocalization.ActionsByLocalizedName.ContainsKey(localizedActionName))
                {
                    throw new InvalidOperationException($"There are more than one action in the " +
                        $"{controllerInfo.Name} controller with localized the name of '{localizedActionName}'.");
                }

                var actionLocalization = new ActionLocalization()
                {
                    OriginalName = actionInfo.Name,
                    LocalizedName = localizedActionName
                };
                controllerLocalization.ActionsByOriginalName.Add(actionInfo.Name, actionLocalization);
                controllerLocalization.ActionsByLocalizedName.Add(localizedActionName, actionLocalization);
            }
        }

        /// <summary>
        /// Gets the localized name of the given <paramref name="controller"/>.
        /// </summary>
        /// <param name="controller">The controller to be localized.</param>
        /// <returns>The localized name of the <paramref name="controller"/>.</returns>
        protected abstract string GetLocalizationForController(ControllerInfo controller);

        /// <summary>
        /// Gets the localized name of the given <paramref name="action"/>.
        /// </summary>
        /// <param name="controller">The controller of the <paramref name="action"/>.</param>
        /// <param name="action">The action to be localized.</param>
        /// <returns>The localized name of the <paramref name="action"/>.</returns>
        protected abstract string GetLocalizationForAction(ControllerInfo controller, ActionInfo action);
    }
}
