using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.Dibs
{
    public class DibsPaymentSettings : ISettings
    {
        public bool UseSandbox { get; set; }

        public string MerchantId { get; set; }

        public string Md5Secret { get; set; }

        public string Md5Secret2 { get; set; }
        public TransactMode TransactMode { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }
        /// <summary>
        /// Additional fee
        /// </summary>
        public decimal AdditionalFee { get; set; }

        public string DibsWinFlexUrl { get; set; }
    }
}
