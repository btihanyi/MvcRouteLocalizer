using System;

namespace MvcRouteLocalizer
{
    /// <summary>
    /// Provides programmatic configuration for the route localization.
    /// </summary>
    public class LocalizationOptions
    {
        private IRouteDictionaryProvider dictionary;
        private IMvcTopologyProvider topologyProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizationOptions"/> class with default values.
        /// </summary>
        internal LocalizationOptions()
        {
            Dictionary = new DefaultRouteDictionaryProvider();
            MvcTopologyProvider = new ReflectionBasedMvcTopologyProvider();
        }

        /// <summary>
        /// Gets or sets the <see cref="IRouteDictionaryProvider"/> for the localization configuration.
        /// The default value is a <see cref="DefaultRouteDictionaryProvider"/> instance.
        /// </summary>
        /// <exception cref="ArgumentNullException">The specified value is null.</exception>
        public IRouteDictionaryProvider Dictionary
        {
            get => dictionary;
            set => dictionary = value ?? throw new ArgumentNullException();
        }

        /// <summary>
        /// Gets or sets the <see cref="IMvcTopologyProvider"/> for the localization configuration.
        /// The default value is a <see cref="ReflectionBasedMvcTopologyProvider"/> instance.
        /// </summary>
        /// <exception cref="ArgumentNullException">The specified value is null.</exception>
        public IMvcTopologyProvider MvcTopologyProvider
        {
            get => topologyProvider;
            set => topologyProvider = value ?? throw new ArgumentNullException();
        }
    }
}
