using Nop.Web.Framework;

namespace Nop.Plugin.Shipping.GLs.Models
{
    public class GlsPostShippingModel
    {
        [NopResourceDisplayName("Plugins.Shipping.AustraliaPost.Fields.GatewayUrl")]
        public string GatewayUrl { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.AustraliaPost.Fields.AdditionalHandlingCharge")]
        public decimal AdditionalHandlingCharge { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.AustraliaPost.Fields.HideDeliveryInformation")]
        public bool HideDeliveryInformation { get; set; }
    }
}