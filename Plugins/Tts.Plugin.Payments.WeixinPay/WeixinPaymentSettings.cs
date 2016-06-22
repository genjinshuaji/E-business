using Nop.Core.Configuration;

namespace Tts.Plugin.Payments.WeixinPay
{
    public class WeixinPaymentSettings : ISettings
    {
        /// <summary>
        /// 账号
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AppId { get; set; }

        public string AppSecret { get; set; }

        /// <summary>
        /// 商户号
        /// </summary>
        public string Mchid { get; set; }

        /// <summary>
        /// 签名秘钥
        /// </summary>
        public string ApiSecret { get; set; }


        /// <summary>
        /// 额外的手续费
        /// </summary>
        public decimal  AdditionalFee { get; set; }

    }


    public static class WeixinPaymentSystemNames
    {
        public static string SelectedWeixinPayMethod { get { return "支付模式"; } }

        /// <summary>
        /// 公众号支付
        /// </summary>
        public const string JsPay = "公众号支付";

        /// <summary>
        /// 扫码支付
        /// </summary>
        public const string NativePay ="扫码支付";


        /// <summary>
        /// 扫码支付
        /// </summary>
        public const string AppPay = "微信App支付"; 
    }
}
