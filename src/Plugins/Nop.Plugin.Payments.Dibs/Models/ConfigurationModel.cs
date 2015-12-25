using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.Dibs.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Payments.Dibs.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Dibs.Fields.MerchantId")]
        public string MerchantId { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Dibs.Fields.Md5Secret")]
        public string Md5Secret { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Dibs.Fields.Md5Secret2")]
        public string Md5Secret2 { get; set; }
        
        [NopResourceDisplayName("Plugins.Payments.Dibs.Fields.DibsWinFlexUrl")]
        public string DibsWinFlexUrl { get; set; }
        
    }
}