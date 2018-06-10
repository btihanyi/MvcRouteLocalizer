using System;
using System.Collections.Generic;

namespace MvcRouteLocalizer
{
    /// <summary>
    /// Provides localization information in a single culture for the whole application.
    /// </summary>
    public class Localization
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Localization"/> class.
        /// </summary>
        public Localization()
        {
            ControllersByOriginalName = new Dictionary<string, ControllerLocalization>(StringComparer.OrdinalIgnoreCase);
            ControllersByLocalizedName = new Dictionary<string, ControllerLocalization>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets a dictionary based on the controllers and their original names as key.
        /// </summary>
        public Dictionary<string, ControllerLocalization> ControllersByOriginalName { get; }

        /// <summary>
        /// Gets a dictionary based on the controller and their localized names as key.
        /// </summary>
        public Dictionary<string, ControllerLocalization> ControllersByLocalizedName { get; }
    }
}
