using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;

namespace MvcRouteLocalizer
{
    /// <summary>
    /// Provides multilingual translations for the names of controllers and actions.
    /// </summary>
    public class ResxRouteDictionaryProvider : MultilingualRouteDictionaryProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResxRouteDictionaryProvider"/> class
        /// with the provided <paramref name="resourceManager"/> instance.
        /// </summary>
        /// <param name="resourceManager">The resource manager that provides access to embedded resource (.resx)
        /// files in multiple cultures. Sets the value of <see cref="ResourceManager"/>.</param>
        public ResxRouteDictionaryProvider(ResourceManager resourceManager)
        {
            this.ResourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
        }

        /// <summary>
        /// Gets the resource manager that provides access to embedded resource (.resx) files in multiple cultures.
        /// </summary>
        protected ResourceManager ResourceManager { get; }

        /// <summary>
        /// Creates a <see cref="Localization"/> instance for the specified <paramref name="culture"/>
        /// by reading the localized values from the <see cref="ResourceManager"/>.
        /// </summary>
        /// <param name="culture">The culture from which the <see cref="Localization"/> should be created.</param>
        /// <returns>A new <see cref="Localization"/> instance based on the <paramref name="culture"/>.</returns>
        protected override Localization InitForCulture(CultureInfo culture)
        {
            var localization = new Localization();
            var resourceSet = ResourceManager.GetResourceSet(culture, true, true);

            // Loop through the records in the resource
            foreach (DictionaryEntry entry in resourceSet)
            {
                if ((entry.Key is string key) && (entry.Value is string localizedValue))
                {
                    string[] segments = key.Split('/');

                    if (segments.Length == 0 || segments.Any(segment => segment.Length == 0))
                    {
                        throw new InvalidOperationException($"Empty path segments are not allowed.");
                    }

                    if (segments.Length == 1)
                    {
                        // Only the controller is specified
                        string controller = segments[0];

                        if (MvcTopologyCache.Controllers.TryGetValue(controller, out ControllerInfo controllerInfo))
                        {
                            if (!localization.ControllersByOriginalName.TryGetValue(controller, out ControllerLocalization controllerLocalization))
                            {
                                controllerLocalization = new ControllerLocalization()
                                {
                                    OriginalName = controllerInfo.Name
                                };
                                localization.ControllersByOriginalName.Add(controller, controllerLocalization);
                            }
                            controllerLocalization.LocalizedName = localizedValue;
                        }
                    }
                    else if (segments.Length == 2)
                    {
                        // Controller and action are both specified
                        string controller = segments[0];
                        string action = segments[1];

                        if (MvcTopologyCache.Controllers.TryGetValue(controller, out ControllerInfo controllerInfo) &&
                            controllerInfo.Actions.TryGetValue(action, out ActionInfo actionInfo))
                        {
                            if (!localization.ControllersByOriginalName.TryGetValue(controller, out ControllerLocalization controllerLocalization))
                            {
                                controllerLocalization = new ControllerLocalization()
                                {
                                    OriginalName = controllerInfo.Name
                                };
                                localization.ControllersByOriginalName.Add(controller, controllerLocalization);
                            }

                            if (!controllerLocalization.ActionsByOriginalName.TryGetValue(action, out ActionLocalization actionLocalization))
                            {
                                actionLocalization = new ActionLocalization()
                                {
                                    OriginalName = actionInfo.Name
                                };
                                controllerLocalization.ActionsByOriginalName.Add(action, actionLocalization);
                            }
                            actionLocalization.LocalizedName = localizedValue;
                        }
                    }
                }
            }

            // Add the remaining controllers and actions not mapped in the resource
            foreach (var controllerInfo in MvcTopologyCache.Controllers.Values)
            {
                if (!localization.ControllersByOriginalName.TryGetValue(controllerInfo.Name, out ControllerLocalization controllerLocalization))
                {
                    controllerLocalization = new ControllerLocalization()
                    {
                        OriginalName = controllerInfo.Name
                    };
                    localization.ControllersByOriginalName.Add(controllerInfo.Name, controllerLocalization);
                }

                foreach (var actionInfo in controllerInfo.Actions.Values)
                {
                    if (!controllerLocalization.ActionsByOriginalName.TryGetValue(actionInfo.Name, out ActionLocalization actionLocalization))
                    {
                        actionLocalization = new ActionLocalization()
                        {
                            OriginalName = actionInfo.Name
                        };
                        controllerLocalization.ActionsByOriginalName.Add(actionInfo.Name, actionLocalization);
                    }
                }
            }

            // Add the localizations accessible by their localized names
            foreach (var controller in localization.ControllersByOriginalName.Values)
            {
                if (localization.ControllersByLocalizedName.ContainsKey(controller.LocalizedName))
                {
                    throw new InvalidOperationException(
                        $"The localization '{controller.LocalizedName}' is ambiguous between the controllers.");
                }
                localization.ControllersByLocalizedName.Add(controller.LocalizedName, controller);

                foreach (var action in controller.ActionsByOriginalName.Values)
                {
                    if (controller.ActionsByLocalizedName.ContainsKey(action.LocalizedName))
                    {
                        throw new InvalidOperationException(
                            $"The action localization '{action.LocalizedName}' is ambiguous in the {controller.OriginalName} controller.");
                    }
                    controller.ActionsByLocalizedName.Add(action.LocalizedName, action);
                }
            }

            return localization;
        }
    }
}
