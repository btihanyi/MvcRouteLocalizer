using System;

namespace MvcRouteLocalizer
{
    /// <summary>
    /// Provides localization information in a single culture for a controller or action.
    /// </summary>
    public abstract class SegmentLocalization
    {
        private string localizedName;

        /// <summary>
        /// Gets or sets the original name of the segment.
        /// </summary>
        public string OriginalName { get; set; }

        /// <summary>
        /// Gets or sets the localized name of the segment.
        /// If it has not been specified explicitly, the <see cref="OriginalName"/> will be returned instead.
        /// </summary>
        public string LocalizedName
        {
            get => localizedName ?? OriginalName;
            set => localizedName = value;
        }
    }
}
