using System;
using System.Collections.Generic;

namespace MvcRouteLocalizer
{
    /// <summary>
    /// Provides localization information in a single culture for the a controller.
    /// </summary>
    public class ControllerLocalization : SegmentLocalization
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerLocalization"/> class.
        /// </summary>
        public ControllerLocalization()
        {
            ActionsByOriginalName = new Dictionary<string, ActionLocalization>(StringComparer.OrdinalIgnoreCase);
            ActionsByLocalizedName = new Dictionary<string, ActionLocalization>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets a dictionary based on the actions and their original names as key.
        /// </summary>
        public Dictionary<string, ActionLocalization> ActionsByOriginalName { get; }

        /// <summary>
        /// Gets a dictionary based on the actions and their localized names as key.
        /// </summary>
        public Dictionary<string, ActionLocalization> ActionsByLocalizedName { get; }
    }
}
