
using Nop.Core.Configuration;

namespace Nop.Plugin.Shipping.Gls
{
    public class GlsSettings : ISettings
    {
        public string GatewayUrl { get; set; }

        public decimal Rate { get; set; }

        public int NumberOfSearchResults { get; set; }
    }
}