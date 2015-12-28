using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Shipping;
using Nop.Core.Plugins;
using Nop.Plugin.Shipping.Gls.GlsWebService;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;

namespace Nop.Plugin.Shipping.Gls
{
    /// <summary>
    /// Australia post computation method
    /// </summary>
    public class GlsComputationMethod : BasePlugin, IShippingRateComputationMethod
    {
        #region Constants

        private const int MIN_LENGTH = 50; // 5 cm
        private const int MIN_WEIGHT = 1; // 1 g
        private const int MAX_LENGTH = 1050; // 105 cm
        private const int MAX_WEIGHT = 20000; // 20 Kg
        private const int MIN_GIRTH = 160; // 16 cm
        private const int MAX_GIRTH = 1050; // 105 cm

        #endregion

        #region Fields

        private readonly IMeasureService _measureService;
        private readonly IShippingService _shippingService;
        private readonly ISettingService _settingService;
        private readonly GlsSettings _glsSettings;

        #endregion

        #region Ctor
        public GlsComputationMethod(IMeasureService measureService,
            IShippingService shippingService, ISettingService settingService, GlsSettings glsSettings)
        {
            this._measureService = measureService;
            this._shippingService = shippingService;
            this._settingService = settingService;
            this._glsSettings = glsSettings;
        }
        #endregion

        #region Utilities

        private string GetGatewayUrl()
        {
            return "";
            //return _australiaPostSettings.GatewayUrl;
        }

        private string ConvertToDanishDayName(Weekday day)
        {
            switch (day.day)
            {
                case Day.Monday:
                    return "Mandag";
                case Day.Tuesday:
                    return "Tirsdag";
                case Day.Wednesday:
                    return "Onsdag";
                case Day.Thursday:
                    return "Torsdag";
                case Day.Friday:
                    return "Fredag";
                case Day.Saturday:
                    return "Lørdag";
                case Day.Sunday:
                    return "Søndag";
                default:
                    throw  new FormatException("Weekday not supported - " + day.ToString());
            }
        }

        private string GeneratePickupStoreOpeningHours(Weekday[] pakkeshopdata)
        {
            var stringBuilder = new StringBuilder();
            foreach (var weekday in pakkeshopdata)
            {
                stringBuilder.Append("<br />");
                stringBuilder.Append("<b>" + ConvertToDanishDayName(weekday) + "</b>: ");
                stringBuilder.Append(weekday.openAt.From + " - " + weekday.openAt.To);
            }
            return stringBuilder.ToString();
        }

        private string GenerateStoreLocationLocation(PakkeshopData data)
        {
            return $"{data.Streetname} {data.CityName} - Åbningstider {GeneratePickupStoreOpeningHours(data.OpeningHours)}";
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Gets available shipping options
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Represents a response of getting shipping rate options</returns>
        public GetShippingOptionResponse GetShippingOptions(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException("getShippingOptionRequest");

            var response = new GetShippingOptionResponse();

            // <endpoint address="http://www.gls.dk/webservices_v3/wsShopFinder.asmx" binding="basicHttpBinding" bindingConfiguration="wsShopFinderSoap" contract="GlsWebService.wsShopFinderSoap" name="wsShopFinderSoap"/>
            var endpoint = new EndpointAddress(_glsSettings.GatewayUrl);
            var binding = new BasicHttpBinding();

            var glsService = new wsShopFinderSoapClient(binding, endpoint);

            var street = getShippingOptionRequest.ShippingAddress.Address1;
            var postalCode = getShippingOptionRequest.ShippingAddress.ZipPostalCode;
            var countryCode = "DK";
            var numberOfResults = _glsSettings.NumberOfSearchResults;

            var nearestShops = glsService.SearchNearestParcelShops(street, postalCode, countryCode, numberOfResults);
            if (nearestShops.parcelshops == null || nearestShops.parcelshops.Length < 1) return response;

            foreach (var pakkeshopData in nearestShops.parcelshops)
            {
                var shippingOption = new ShippingOption();
                shippingOption.Name = pakkeshopData.CompanyName;
                shippingOption.Description = GenerateStoreLocationLocation(pakkeshopData);
                shippingOption.Rate = _glsSettings.Rate;
                response.ShippingOptions.Add(shippingOption);
            }

            return response;
        }

        /// <summary>
        /// Gets fixed shipping rate (if shipping rate computation method allows it and the rate can be calculated before checkout).
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Fixed shipping rate; or null in case there's no fixed shipping rate</returns>
        public decimal? GetFixedRate(GetShippingOptionRequest getShippingOptionRequest)
        {
            return null;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "ShippingGls";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Shipping.Gls.Controllers" }, { "area", null } };
        }
        
        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //settings
            var settings = new GlsSettings
            {
                GatewayUrl = "http://www.gls.dk/webservices_v3/wsShopFinder.asmx",
                Rate = 65,
                NumberOfSearchResults = 4
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Gls.Fields.GatewayUrl", "Gateway URL");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Gls.Fields.GatewayUrl.Hint", "Specify gateway URL");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Gls.Fields.Rate", "Shipping cost");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Gls.Fields.Rate.Hint", "Enter the cost of shipping.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Gls.Fields.NumberOfSearchResults", "Number of search results");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Gls.Fields.NumberOfSearchResults.Hint", "Number of search results.");

            base.Install();
        }

        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<GlsSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.Shipping.Gls.Fields.GatewayUrl");
            this.DeletePluginLocaleResource("Plugins.Shipping.Gls.Fields.GatewayUrl.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.Gls.Fields.Rate");
            this.DeletePluginLocaleResource("Plugins.Shipping.Gls.Fields.Rate.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.Gls.Fields.NumberOfSearchResults");
            this.DeletePluginLocaleResource("Plugins.Shipping.Gls.Fields.NumberOfSearchResults.Hint");

            base.Uninstall();
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets a shipping rate computation method type
        /// </summary>
        public ShippingRateComputationMethodType ShippingRateComputationMethodType
        {
            get
            {
                return ShippingRateComputationMethodType.Realtime;
            }
        }

        /// <summary>
        /// Gets a shipment tracker
        /// </summary>
        public IShipmentTracker ShipmentTracker 
        { 
            get { return null; }
        }

        #endregion
    }
}