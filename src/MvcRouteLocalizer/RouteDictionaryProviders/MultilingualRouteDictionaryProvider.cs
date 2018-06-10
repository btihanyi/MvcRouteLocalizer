using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;

namespace MvcRouteLocalizer
{
    /// <summary>
    /// Provides the bijection logic between the original route values and their corresponding translations
    /// in multiple cultures.
    /// </summary>
    public abstract class MultilingualRouteDictionaryProvider : RouteDictionaryProviderBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultilingualRouteDictionaryProvider"/> class.
        /// </summary>
        protected MultilingualRouteDictionaryProvider()
        {
            this.Localizations = new ConcurrentDictionary<CultureInfo, Localization>();
        }

        /// <summary>
        /// Gets the localizations for each culture used in the application.
        /// </summary>
        protected ConcurrentDictionary<CultureInfo, Localization> Localizations { get; }

        /// <summary>
        /// Gets a cached instance of the application's <see cref="MvcTopology"/> structure.
        /// </summary>
        protected MvcTopology MvcTopologyCache { get; private set; }

        /// <summary>
        /// Caches the <see cref="MvcTopology"/> instance created by <paramref name="topologyProvider"/>
        /// into the <see cref="MvcTopologyCache"/> property.
        /// </summary>
        /// <param name="topologyProvider">The topology provider containing the required information
        /// about the localizable controllers and actions.</param>
        public override void Init(IMvcTopologyProvider topologyProvider)
        {
            MvcTopologyCache = topologyProvider.GetMvcTopology();
        }

        /// <summary>
        /// Gets the localization for the current culture <see cref="CultureInfo.CurrentUICulture"/>.
        /// </summary>
        /// <returns>The <see cref="Localization"/> instance for the current culture from <see cref="Localizations"/>.</returns>
        protected override Localization GetLocalization()
        {
            return Localizations.GetOrAdd(CultureInfo.CurrentUICulture, InitForCulture);
        }

        /// <summary>
        /// Creates a <see cref="Localization"/> instance for the specified <paramref name="culture"/>.
        /// </summary>
        /// <param name="culture">The culture from which the <see cref="Localization"/> should be created.</param>
        /// <returns>A new <see cref="Localization"/> instance based on the <paramref name="culture"/>.</returns>
        protected abstract Localization InitForCulture(CultureInfo culture);
    }
}
