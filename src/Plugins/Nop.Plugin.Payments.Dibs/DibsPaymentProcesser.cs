using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.Dibs.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Web.Framework;

namespace Nop.Plugin.Payments.Dibs
{
    public class DibsPaymentProcesser : BasePlugin, IPaymentMethod
    {
        private readonly HttpContextBase _httpContext;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly DibsPaymentSettings _dibsPaymentSettings;
        private const int DanisCurrencyCode = 208;

        public DibsPaymentProcesser(HttpContextBase httpContext, IWebHelper webHelper, ISettingService settingService, DibsPaymentSettings dibsPaymentSettings)
        {
            _httpContext = httpContext;
            _webHelper = webHelper;
            _settingService = settingService;
            _dibsPaymentSettings = dibsPaymentSettings;
        }

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;
            return result;
        }

        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var serverUrl = "http://localhost:15536";
            var dibsPaymentWindowUrl = _dibsPaymentSettings.DibsWinFlexUrl;

            RemotePost post = new RemotePost();
            post.FormName = "FlexWin";
            post.Url = dibsPaymentWindowUrl;
            //if (_DIBSPaymentSettings.usesandbox)
            //{
                post.Add("test", "yes");
            //}
            post.Add("uniqueoid", "yes");
            var orderTotal = Math.Round(postProcessPaymentRequest.Order.OrderTotal, 2);
            var ordertotal2 = orderTotal * 100;
            int amount = Convert.ToInt32(ordertotal2);
            int currencyCode = DanisCurrencyCode;
            int merhcantId = Convert.ToInt32(_dibsPaymentSettings.MerchantId);
            int ordernumber = Convert.ToInt32("1000"+postProcessPaymentRequest.Order.Id.ToString("D2"));
            string continueurl = _webHelper.GetStoreLocation(false) + "Plugins/PaymentDibs/PDTHandler";
            string cancelurl = _webHelper.GetStoreLocation(false) + "/Plugins/PaymentDibs/CancelOrder";
            string md5secret = _dibsPaymentSettings.Md5Secret;
            string md5secret2 = _dibsPaymentSettings.Md5Secret2;
            string stringToMd5 = string.Concat(md5secret, md5secret2, merhcantId,
                 ordernumber, currencyCode, amount);
            //string md5check = CalcMD5Key(merhcantID, ordernumber, currencyCode, amount);
            post.Add("lang", "da");
            post.Add("currency", currencyCode.ToString());
            post.Add("color", "blue");
            post.Add("decorator", "default");
            post.Add("merchant", merhcantId.ToString());
            post.Add("orderid", ordernumber.ToString());
            post.Add("amount", amount.ToString());
            //post.Add("md5key", md5check);
            post.Add("accepturl", continueurl);
            post.Add("cancelurl", cancelurl);

            post.Post();
        }

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            return false;
        }

        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return 0;
        }

        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return result;
        }

        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }

        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring pay ment method not supported");
            return result;
        }

        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("CancelRecurringPayment method not supported");
            return result;
        }

        public bool CanRePostProcessPayment(Order order)
        {
            throw new NotImplementedException();
        }

        public Type GetControllerType()
        {
            return typeof(PaymentDibsController);
        }

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.Manual;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get
            {
                return false;
            }
        }

        public override void Install()
        {
            //settings
            var settings = new DibsPaymentSettings
            {
                UseSandbox = true,
                DibsWinFlexUrl = "https://payment.architrade.com/paymentweb/start.action"
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Dibs.Fields.RedirectionTip", "Du vil blive sendt videre til dibs betalingsvindue for at færdiggøre din ordre.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Dibs.Fields.UseSandbox", "Brug test mode");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Dibs.Fields.MerchantId", "Merchant id");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Dibs.Fields.Md5Secret", "Md5 secret");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Dibs.Fields.Md5Secret2", "Md5 secret 2");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Dibs.Fields.DibsWinFlexUrl", "Dibs win flex url");

            base.Install();
        }

        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentDibs";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.Dibs.Controllers" }, { "area", null } };
        }

        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentDibs";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.Dibs.Controllers" }, { "area", null } };
        }
    }
}
