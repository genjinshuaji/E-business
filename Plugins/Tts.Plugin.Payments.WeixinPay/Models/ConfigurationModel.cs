using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Tts.Plugin.Payments.WeixinPay.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        /// <summary>
        /// 账号
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.WeixinPay.Email")]
        public string Email { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.WeixinPay.Password")]
        public string Password { get; set; }


        /// <summary>
        /// 
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.WeixinPay.AppId")]
        public string AppId { get; set; }

        [NopResourceDisplayName("Plugins.Payments.WeixinPay.AppSecret")]
        public string AppSecret { get; set; }

        /// <summary>
        /// 商户号
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.WeixinPay.Mchid")]
        public string Mchid { get; set; }

        /// <summary>
        /// 签名秘钥
        /// </summary>
        [NopResourceDisplayName("Plugins.Payments.WeixinPay.ApiSecret")]
        public string ApiSecret { get; set; }

        /// <summary>
        /// 额外的费用
        /// </summary>
         [NopResourceDisplayName("Plugins.Payments.WeixinPay.AdditionalFee")]
        public decimal AdditionalFee { get; set; }


    }
}