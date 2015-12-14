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
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;
        private readonly PaymentSettings _paymentSettings;
        private readonly ILocalizationService _localizationService;

        public PaymentDibsController(IWebHelper webHelper)
        {
            _webHelper = webHelper;
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

        [ValidateInput(false)]
        public ActionResult PDTHandler(FormCollection form)
        {
            var tx = _webHelper.QueryString<string>("tx");
            Dictionary<string, string> values;
            string response;

            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.Dibs") as DibsPaymentProcesser;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("PayPal Standard module cannot be loaded");

            return RedirectToRoute("CheckoutCompleted", new { orderId = 123 });
            //if (processor.GetPdtDetails(tx, out values, out response))
            //{
            //    string orderNumber = string.Empty;
            //    values.TryGetValue("custom", out orderNumber);
            //    Guid orderNumberGuid = Guid.Empty;
            //    try
            //    {
            //        orderNumberGuid = new Guid(orderNumber);
            //    }
            //    catch { }
            //    Order order = _orderService.GetOrderByGuid(orderNumberGuid);
            //    if (order != null)
            //    {
            //        decimal mc_gross = decimal.Zero;
            //        try
            //        {
            //            mc_gross = decimal.Parse(values["mc_gross"], new CultureInfo("en-US"));
            //        }
            //        catch (Exception exc)
            //        {
            //            _logger.Error("PayPal PDT. Error getting mc_gross", exc);
            //        }

            //        string payer_status = string.Empty;
            //        values.TryGetValue("payer_status", out payer_status);
            //        string payment_status = string.Empty;
            //        values.TryGetValue("payment_status", out payment_status);
            //        string pending_reason = string.Empty;
            //        values.TryGetValue("pending_reason", out pending_reason);
            //        string mc_currency = string.Empty;
            //        values.TryGetValue("mc_currency", out mc_currency);
            //        string txn_id = string.Empty;
            //        values.TryGetValue("txn_id", out txn_id);
            //        string payment_type = string.Empty;
            //        values.TryGetValue("payment_type", out payment_type);
            //        string payer_id = string.Empty;
            //        values.TryGetValue("payer_id", out payer_id);
            //        string receiver_id = string.Empty;
            //        values.TryGetValue("receiver_id", out receiver_id);
            //        string invoice = string.Empty;
            //        values.TryGetValue("invoice", out invoice);
            //        string payment_fee = string.Empty;
            //        values.TryGetValue("payment_fee", out payment_fee);

            //        var sb = new StringBuilder();
            //        sb.AppendLine("Paypal PDT:");
            //        sb.AppendLine("mc_gross: " + mc_gross);
            //        sb.AppendLine("Payer status: " + payer_status);
            //        sb.AppendLine("Payment status: " + payment_status);
            //        sb.AppendLine("Pending reason: " + pending_reason);
            //        sb.AppendLine("mc_currency: " + mc_currency);
            //        sb.AppendLine("txn_id: " + txn_id);
            //        sb.AppendLine("payment_type: " + payment_type);
            //        sb.AppendLine("payer_id: " + payer_id);
            //        sb.AppendLine("receiver_id: " + receiver_id);
            //        sb.AppendLine("invoice: " + invoice);
            //        sb.AppendLine("payment_fee: " + payment_fee);

            //        var newPaymentStatus = DibsHelper.GetPaymentStatus(payment_status, pending_reason);
            //        sb.AppendLine("New payment status: " + newPaymentStatus);

            //        //order note
            //        order.OrderNotes.Add(new OrderNote
            //        {
            //            Note = sb.ToString(),
            //            DisplayToCustomer = false,
            //            CreatedOnUtc = DateTime.UtcNow
            //        });
            //        _orderService.UpdateOrder(order);

            //        //load settings for a chosen store scope
            //        //var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            //        //var payPalStandardPaymentSettings = _settingService.LoadSetting<PayPalStandardPaymentSettings>(storeScope);

            //        //validate order total
            //        //if (payPalStandardPaymentSettings.PdtValidateOrderTotal && !Math.Round(mc_gross, 2).Equals(Math.Round(order.OrderTotal, 2)))
            //        //{
            //        //    string errorStr = string.Format("PayPal PDT. Returned order total {0} doesn't equal order total {1}", mc_gross, order.OrderTotal);
            //        //    _logger.Error(errorStr);

            //        //    return RedirectToAction("Index", "Home", new { area = "" });
            //        //}

            //        //mark order as paid
            //        if (newPaymentStatus == PaymentStatus.Paid)
            //        {
            //            if (_orderProcessingService.CanMarkOrderAsPaid(order))
            //            {
            //                order.AuthorizationTransactionId = txn_id;
            //                _orderService.UpdateOrder(order);

            //                _orderProcessingService.MarkOrderAsPaid(order);
            //            }
            //        }
            //    }

            //    return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
            //}
            //else
            //{
            //    string orderNumber = string.Empty;
            //    values.TryGetValue("custom", out orderNumber);
            //    Guid orderNumberGuid = Guid.Empty;
            //    try
            //    {
            //        orderNumberGuid = new Guid(orderNumber);
            //    }
            //    catch { }
            //    Order order = _orderService.GetOrderByGuid(orderNumberGuid);
            //    if (order != null)
            //    {
            //        //order note
            //        order.OrderNotes.Add(new OrderNote
            //        {
            //            Note = "PayPal PDT failed. " + response,
            //            DisplayToCustomer = false,
            //            CreatedOnUtc = DateTime.UtcNow
            //        });
            //        _orderService.UpdateOrder(order);
            //    }
            //    return RedirectToAction("Index", "Home", new { area = "" });
            //}
        }
    }
}