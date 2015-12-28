using Nop.Web.Framework;

namespace Nop.Plugin.Shipping.Gls.Models
{
    public class GlsShippingModel
    {
        [NopResourceDisplayName("Plugins.Shipping.Gls.Fields.GatewayUrl")]
        public string GatewayUrl { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Gls.Fields.Rate")]
        public decimal Rate { get; set; }
        [NopResourceDisplayName("Plugins.Shipping.Gls.Fields.NumberOfSearchResults")]
        public int NumberOfSearchResults { get; set; }
    }
}