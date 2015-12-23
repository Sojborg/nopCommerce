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
        private readonly ILogger _logger;
        private readonly PaymentSettings _paymentSettings;
        private readonly ILocalizationService _localizationService;

        public PaymentDibsController(IWebHelper webHelper, IPaymentService paymentService, PaymentSettings paymentSettings
            , IOrderService orderService, IOrderProcessingService orderProcessingService)
        {
            _paymentService = paymentService;
            _paymentSettings = paymentSettings;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {

            var model = new ConfigurationModel();
            

            return View("~/Plugins/Payments.Dibs/Views/PaymentDibs/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();
            

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