using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.Dibs
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            //PDT
            routes.MapRoute("Plugin.Payments.Dibs.PDTHandler",
                 "Plugins/PaymentDibs/PDTHandler",
                 new { controller = "PaymentDibs", action = "PDTHandler" },
                 new[] { "Nop.Plugin.Payments.Dibs.Controllers" }
            );
            //IPN
            //routes.MapRoute("Plugin.Payments.PayPalStandard.IPNHandler",
            //     "Plugins/Dibs/IPNHandler",
            //     new { controller = "Dibs", action = "IPNHandler" },
            //     new[] { "Nop.Plugin.Payments.PayPalStandard.Controllers" }
            //);
            ////Cancel
            //routes.MapRoute("Plugin.Payments.PayPalStandard.CancelOrder",
            //     "Plugins/Dibs/CancelOrder",
            //     new { controller = "Dibs", action = "CancelOrder" },
            //     new[] { "Nop.Plugin.Payments.PayPalStandard.Controllers" }
            //);
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
