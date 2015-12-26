using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.Dibs.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.Dibs.Controllers
{
    public class PaymentDibsController : BasePaymentController
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;
        private readonly PaymentSettings _paymentSettings;
        private readonly ILocalizationService _localizationService;

        public PaymentDibsController(IWebHelper webHelper, IPaymentService paymentService, PaymentSettings paymentSettings
            , IOrderService orderService, IOrderProcessingService orderProcessingService, ISettingService settingService,
            IStoreService storeService, IWorkContext workContext, ILocalizationService localizationService) 
        {
            _paymentService = paymentService;
            _paymentSettings = paymentSettings;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _settingService = settingService;
            _storeService = storeService;
            _workContext = workContext;
            _localizationService = localizationService;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var dibsPaymentSettings = _settingService.LoadSetting<DibsPaymentSettings>(storeScope);

            var model = new ConfigurationModel();
            model.UseSandbox = dibsPaymentSettings.UseSandbox;
            model.MerchantId = dibsPaymentSettings.MerchantId;
            model.Md5Secret = dibsPaymentSettings.Md5Secret;
            model.Md5Secret2 = dibsPaymentSettings.Md5Secret2;
            model.DibsWinFlexUrl = dibsPaymentSettings.DibsWinFlexUrl;

            return View("~/Plugins/Payments.Dibs/Views/PaymentDibs/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var dibsPaymentSettings = _settingService.LoadSetting<DibsPaymentSettings>(storeScope);

            dibsPaymentSettings.UseSandbox = model.UseSandbox;
            dibsPaymentSettings.MerchantId = model.MerchantId;
            dibsPaymentSettings.Md5Secret = model.Md5Secret;
            dibsPaymentSettings.Md5Secret2 = model.Md5Secret2;
            dibsPaymentSettings.DibsWinFlexUrl = model.DibsWinFlexUrl;

            _settingService.SaveSetting(dibsPaymentSettings, x => x.UseSandbox, storeScope, false);
            _settingService.SaveSetting(dibsPaymentSettings, x => x.MerchantId, storeScope, false);
            _settingService.SaveSetting(dibsPaymentSettings, x => x.Md5Secret, storeScope, false);
            _settingService.SaveSetting(dibsPaymentSettings, x => x.Md5Secret2, storeScope, false);
            _settingService.SaveSetting(dibsPaymentSettings, x => x.DibsWinFlexUrl, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            return View("~/Plugins/Payments.Dibs/Views/PaymentDibs/PaymentInfo.cshtml");
        }

        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }

        private PaymentStatus GetPaymentStatus(string paymentStatus)
        {
            switch (paymentStatus)
            {
                case "2":
                    return PaymentStatus.Paid;
                default:
                    throw new InvalidOperationException("Payment was not successfull.");
            }
        }

        private string ConvertToCardType(string paytype)
        {
            switch (paytype)
            {
                case "MC":
                    return "Mastercard";
                case "MC(DK)":
                    return "Mastercard (DK)";
                case "MC(SE)":
                    return "Mastercard (SE)";
                case "V-DK":
                    return "VISA-Dankort";
                case "VISA":
                    return "VISA";
                case "MPO_Nets":
                    return "MobilePay Online";
                default:
                    return "Ukendt";
            }
        }

        [ValidateInput(false)]
        public ActionResult PDTHandler(FormCollection form)
        {
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.Dibs") as DibsPaymentProcesser;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("PayPal Standard module cannot be loaded");

            var orderId = 0;
            var orderIdStr = form["orderid"].Replace("1000", "");
            var orderIdParsed = int.TryParse(orderIdStr, out orderId);

            if (orderIdParsed)
            {
                var paytype = form["paytype"];
                var transactionsNumber = form["transact"];
                var approvalcode = form["approvalcode"];
                var statuscode = form["statuscode"];
                var amountStr = form["amount"];

                var order = _orderService.GetOrderById(orderId);
                order.CardType = ConvertToCardType(paytype);
                order.OrderNotes.Add(new OrderNote
                {
                    Note = $"Dibs transaktions nummer: {transactionsNumber}",
                    DisplayToCustomer = true,
                    CreatedOnUtc = DateTime.UtcNow
                });
                order.OrderNotes.Add(new OrderNote
                {
                    Note = $"Dibs approvalcode code: {approvalcode}",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);

                //validate order total
                decimal amount;
                var amountParsed = decimal.TryParse(amountStr, out amount);
                if (amountParsed)
                {
                    if (!Math.Round(amount/100, 2).Equals(Math.Round(order.OrderTotal, 2)))
                    {
                        string errorStr = $"Dibs PDT. Returned order total {amount} doesn't equal order total {order.OrderTotal}";
                        _logger.Error(errorStr);

                        return RedirectToAction("Index", "Home", new {area = ""});
                    }
                }

                //mark order as paid
                try
                {
                    var newPaymentStatus = GetPaymentStatus(statuscode);

                    if (newPaymentStatus == PaymentStatus.Paid)
                    {
                        if (_orderProcessingService.CanMarkOrderAsPaid(order))
                        {
                            order.AuthorizationTransactionId = transactionsNumber;
                            _orderService.UpdateOrder(order);

                            _orderProcessingService.MarkOrderAsPaid(order);
                        }
                    }

                }
                catch (Exception ex)
                {
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = $"Could not parse status code: {statuscode}",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                }
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
            }

            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}