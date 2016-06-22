using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Catalog;
using Tts.Plugin.Payments.WeixinPay.Models;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework.Controllers;
using ThoughtWorks.QRCode.Codec;
using WxPayAPI;

namespace Tts.Plugin.Payments.WeixinPay.Controllers
{
    public class PaymentWeixinController : BasePaymentController
    {
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly WeixinPaymentSettings _weixinPaymentSettings;
        private readonly PaymentSettings _paymentSettings;
        private HttpContextBase _httpContext;
        private ICacheManager _cacheManager;
        private IStoreContext _storeContext;
        private IWorkContext _workContext;
        private JsApiPay _jsApiPay;
        private ResultNotify _resultNotify;
        private NativePay _nativePay;
        private IPriceFormatter _priceFormatter;

        public PaymentWeixinController(ISettingService settingService,
            IPaymentService paymentService, IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ILogger logger, IWebHelper webHelper,
            WeixinPaymentSettings aliPayPaymentSettings,
            PaymentSettings paymentSettings,
            HttpContextBase httpContext,
            ICacheManager cacheManager,
            IStoreContext storeContext,
            IWorkContext workContext,
            JsApiPay apiPay, ResultNotify resultNotify, NativePay nativePay, IPriceFormatter priceFormatter)
        {
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._logger = logger;
            this._webHelper = webHelper;
            this._weixinPaymentSettings = aliPayPaymentSettings;
            this._paymentSettings = paymentSettings;
            _httpContext = httpContext;
            _cacheManager = cacheManager;
            _storeContext = storeContext;
            _workContext = workContext;
            _jsApiPay = apiPay;
            _resultNotify = resultNotify;
            _nativePay = nativePay;
            _priceFormatter = priceFormatter;
        }


        #region 配置
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel()
            {
                Email = _weixinPaymentSettings.Email,
                Password = _weixinPaymentSettings.Password,
                AppId = _weixinPaymentSettings.AppId,
                AppSecret = _weixinPaymentSettings.AppSecret,
                Mchid = _weixinPaymentSettings.Mchid,
                ApiSecret = _weixinPaymentSettings.ApiSecret,
                AdditionalFee = _weixinPaymentSettings.AdditionalFee
            };

            return View("~/Plugins/Payments.WeixinPay/Views/PaymentWeixin/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _weixinPaymentSettings.Email = model.Email;
            _weixinPaymentSettings.Password = model.Password;
            _weixinPaymentSettings.AppId = model.AppId;
            _weixinPaymentSettings.AppSecret = model.AppSecret;
            _weixinPaymentSettings.Mchid = model.Mchid;
            _weixinPaymentSettings.ApiSecret = model.ApiSecret;
            _weixinPaymentSettings.AdditionalFee = model.AdditionalFee;

            _settingService.SaveSetting(_weixinPaymentSettings);
            WxPayConfig.WeixinPaymentSetting = _weixinPaymentSettings;

            return Configure();
        }
        #endregion

        #region 支付
        /// <summary>
        /// 微信公众号支付
        /// </summary>
        /// <returns></returns>
        public ActionResult JsPay()
        {
            _jsApiPay.GetOpenidAndAccessToken();
            return RedirectToRoute("payment.weixinpay", new
            {
                openId = _jsApiPay.openid
            });
        }

        /// <summary>
        /// 扫码支付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult NativePay(Guid orderGuid)
        {
            var order = _orderService.GetOrderByGuid(orderGuid);
            if (order == null || order.OrderStatus == OrderStatus.Complete)
                return RedirectToRoute("HomePage");
            //扫码支付
            _nativePay.body = GetOrderBody(order); ;
            _nativePay.total_fee = Convert.ToInt32(order.OrderTotal * 100);
            //_nativePay.openid = openId;
            _nativePay.orderId = order.Id.ToString();
            var url = _nativePay.GetPayUrl();
           // var data = HttpUtility.UrlEncode(url);
            //开始生成二维码
            //初始化二维码生成工具
            QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
            qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
            qrCodeEncoder.QRCodeVersion = 0;
            qrCodeEncoder.QRCodeScale = 4;

            //将字符串生成二维码图片
            Bitmap image = qrCodeEncoder.Encode(url, Encoding.Default);

            ;
            var model = new NativePayModel()
            {
                OrderNo = order.Id.ToString(),
                OrderTotal = _priceFormatter.FormatPrice(order.OrderTotal)
            };
            //保存为PNG到内存流  
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                model.QrCodeBase64String = Convert.ToBase64String(ms.GetBuffer());
            }

            return View("~/Plugins/Payments.WeixinPay/Views/PaymentWeixin/NativePay.cshtml", model);
        }


        public ActionResult WeixinPay(string openId)
        {
             Guid orderGuid=new Guid();
             try
             {
                 orderGuid = (Guid)_httpContext.Session["WeixinPay.JsPay.OrderGuid"];
                 _httpContext.Session.Remove("WeixinPay.JsPay.OrderGuid");//移除session
             }
             catch (Exception)
             {
                  return Content("订单失效");
             }
            if (string.IsNullOrEmpty(openId))
            {
                return Content("用户验证失败");
            }
            var order = _orderService.GetOrderByGuid(orderGuid);
            if (order == null || order.OrderStatus == OrderStatus.Complete)
                return RedirectToRoute("HomePage");

            var body = GetOrderBody(order);

            _jsApiPay.body = body;
            _jsApiPay.total_fee = Convert.ToInt32(order.OrderTotal * 100);
            _jsApiPay.openid = openId;
            _jsApiPay.orderId = order.Id.ToString();
            try
            {
                _jsApiPay.GetUnifiedOrderResult();
                string wxJsApiParam = _jsApiPay.GetJsApiParameters();//获取H5调起JS API参数                    
                ViewBag.orderId = order.Id;
                return View("~/Plugins/Payments.WeixinPay/Views/PaymentWeixin/JsPay.cshtml", (object)wxJsApiParam);

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return Content("<span style='color:#FF0000;font-size:20px'>" + "下单失败，请返回重试：错误信息:" + ex.ToString() + "</span>");

            }
        }

        private static string GetOrderBody(Order order)
        {
            string body = "";
            foreach (var item in order.OrderItems)
            {
                if (item.Product.Name.Length > 100)
                    body += item.Product.Name.Substring(0, 100) + "...,";
                else
                    body += item.Product.Name + ",";
            }
            body = body.Substring(0, body.Length - 1);
            if (body.Length > 999)
            {
                body = body.Substring(0, 996) + "...";
            }
            return body;
        }

        #endregion

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            return View("~/Plugins/Payments.WeixinPay/Views/PaymentWeixin/PaymentInfo.cshtml");
        }




        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            paymentInfo.CustomValues.Add(WeixinPaymentSystemNames.SelectedWeixinPayMethod, form["payModel"]);
            return paymentInfo;
        }

        #region CallBack
        [ValidateInput(false)]
        public ActionResult Notify(FormCollection form)
        {
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.WeixinPay") as WeixinPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("微信支付模块没有安装");


            _resultNotify.ProcessNotify();
            return Content("");
        }

        #endregion
    }
}