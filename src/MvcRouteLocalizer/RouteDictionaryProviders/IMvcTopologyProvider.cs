using System;

namespace MvcRouteLocalizer
{
    /// <summary>
    /// Provides the required information about the available controllers and actions.
    /// </summary>
    public interface IMvcTopologyProvider
    {
        /// <summary>
        /// Creates an <see cref="MvcTopology"/> of the available controllers and their actions.
        /// </summary>
        /// <returns>The <see cref="MvcTopology"/> containing all the available controllers, including their actions.</returns>
        MvcTopology GetMvcTopology();
    }
}
