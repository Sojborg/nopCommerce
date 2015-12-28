using System.Web.Mvc;
using Nop.Plugin.Shipping.Gls.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Shipping.Gls.Controllers
{
    [AdminAuthorize]
    public class ShippingGlsController : BasePluginController
    {
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly GlsSettings _glsSettings;

        public ShippingGlsController(
            ISettingService settingService,
            ILocalizationService localizationService,
            GlsSettings glsSettings)
        {
            this._settingService = settingService;
            this._localizationService = localizationService;
            _glsSettings = glsSettings;
        }

        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new GlsShippingModel();
            model.GatewayUrl = _glsSettings.GatewayUrl;
            model.Rate = _glsSettings.Rate;
            model.NumberOfSearchResults = _glsSettings.NumberOfSearchResults;

            return View("~/Plugins/Shipping.Gls/Views/ShippingGls/Configure.cshtml", model);
        }

        [HttpPost]
        [ChildActionOnly]
        public ActionResult Configure(GlsShippingModel model)
        {
            if (!ModelState.IsValid)
            {
                return Configure();
            }
            
            //save settings
            _glsSettings.GatewayUrl = model.GatewayUrl;
            _glsSettings.Rate = model.Rate;
            _glsSettings.NumberOfSearchResults = model.NumberOfSearchResults;

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

    }
}
