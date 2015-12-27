using System.Web.Mvc;
using Nop.Plugin.Shipping.GLs.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Shipping.Gls.Controllers
{
    [AdminAuthorize]
    public class ShippingGlsPostController : BasePluginController
    {
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;

        public ShippingGlsPostController(
            ISettingService settingService,
            ILocalizationService localizationService)
        {
            this._settingService = settingService;
            this._localizationService = localizationService;
        }

        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new GlsPostShippingModel();
            //model.GatewayUrl = _australiaPostSettings.GatewayUrl;
            //model.AdditionalHandlingCharge = _australiaPostSettings.AdditionalHandlingCharge;
            //model.HideDeliveryInformation = _australiaPostSettings.HideDeliveryInformation;

            return View("~/Plugins/Shipping.AustraliaPost/Views/ShippingGlsPost/Configure.cshtml", model);
        }

        [HttpPost]
        [ChildActionOnly]
        public ActionResult Configure(GlsPostShippingModel model)
        {
            if (!ModelState.IsValid)
            {
                return Configure();
            }
            
            //save settings
            //_australiaPostSettings.GatewayUrl = model.GatewayUrl;
            //_australiaPostSettings.AdditionalHandlingCharge = model.AdditionalHandlingCharge;
            //_australiaPostSettings.HideDeliveryInformation = model.HideDeliveryInformation;
            //_settingService.SaveSetting(_australiaPostSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

    }
}
