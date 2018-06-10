using System;
using System.Web;
using System.Web.Routing;

namespace MvcRouteLocalizer
{
    internal class LocalizedRouteHandler : IRouteHandler
    {
        public LocalizedRouteHandler(IRouteHandler originalRouteHandler)
        {
            this.OriginalRouteHandler = originalRouteHandler ?? throw new ArgumentNullException(nameof(originalRouteHandler));
        }

        private IRouteHandler OriginalRouteHandler { get; }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            ////TODO: custom LocalizedHttpHandler for URL redirections
            return OriginalRouteHandler.GetHttpHandler(requestContext);
        }
    }
}
