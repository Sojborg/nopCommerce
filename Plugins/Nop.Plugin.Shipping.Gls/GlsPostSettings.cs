
using Nop.Core.Configuration;

namespace Nop.Plugin.Shipping.Gls
{
    public class GlsPostSettings : ISettings
    {
        public string GatewayUrl { get; set; }

        public decimal AdditionalHandlingCharge { get; set; }

        public bool HideDeliveryInformation { get; set; }
    }
}