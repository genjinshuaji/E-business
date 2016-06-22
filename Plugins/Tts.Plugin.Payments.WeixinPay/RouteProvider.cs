using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc.Routes;

namespace Tts.Plugin.Payments.WeixinPay
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            //Notify
            routes.MapRoute("Plugin.Payments.WeixinPay.Notify",
                 "Plugins/PaymentWeixin/Notify",
                 new { controller = "PaymentWeixin", action = "Notify" },
                 new[] { "Tts.Plugin.Payments.WeixinPay.Controllers" }
            );


            //JsPay
            routes.MapRoute("Weixin.Js.Pay",
                 "WeixinJsPay",
                 new { controller = "PaymentWeixin", action = "JsPay" },
                 new[] { "Tts.Plugin.Payments.WeixinPay.Controllers" }
            );

            //Native 扫码支付
            routes.MapRoute("Weixin.Native.Pay",
               "WeixinNativePay",
               new { controller = "PaymentWeixin", action = "NativePay" },
               new[] { "Tts.Plugin.Payments.WeixinPay.Controllers" }
            );

            //Js pay 跳转
            routes.MapRoute("payment.weixinpay",
                 "payment/weixinpay/{openid}",
                 new { controller = "PaymentWeixin", action = "WeixinPay" },
                 new[] { "Tts.Plugin.Payments.WeixinPay.Controllers" }
            );
        }
        public int Priority
        {
            get
            {
                return 100;
            }
        }
    }
}
