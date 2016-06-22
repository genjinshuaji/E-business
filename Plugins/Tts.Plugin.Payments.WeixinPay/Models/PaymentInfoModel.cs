using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Tts.Plugin.Payments.WeixinPay.Models
{
    public class PaymentInfoModel : BaseNopModel
    {
        public PaymentInfoModel()
        {
           
        }

        [NopResourceDisplayName("Payment.PayMethod")]
        [AllowHtml]
        public string PayMethod { get; set; }



        public bool EnableBankPay { get; set; }
    }
}