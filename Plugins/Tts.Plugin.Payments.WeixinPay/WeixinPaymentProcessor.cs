using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Customers;
using Nop.Core.Plugins;
using Tts.Plugin.Payments.WeixinPay.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Web.Framework;


namespace Tts.Plugin.Payments.WeixinPay
{
    /// <summary>
    /// AliPay payment processor
    /// </summary>
    public class WeixinPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly WeixinPaymentSettings _aliPayPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IStoreContext _storeContext;

        private HttpContextBase _httpContext;
        private ICacheManager _cacheManager;
        #endregion

        #region Ctor

        public WeixinPaymentProcessor(WeixinPaymentSettings aliPayPaymentSettings,
            ISettingService settingService, IWebHelper webHelper,
            IStoreContext storeContext, HttpContextBase httpContext, ICacheManager cacheManager)
        {
            this._aliPayPaymentSettings = aliPayPaymentSettings;
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._storeContext = storeContext;
            _httpContext = httpContext;
            _cacheManager = cacheManager;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;
            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var customInfo = postProcessPaymentRequest.Order.DeserializeCustomValues();
            if (customInfo != null && customInfo.Count > 0)
            {
                var post = new RemotePost();
                post.FormName = "weixinpaysubmit";
                post.Method = "POST";
                post.Add("orderGuid", postProcessPaymentRequest.Order.OrderGuid.ToString());
                switch (customInfo[WeixinPaymentSystemNames.SelectedWeixinPayMethod].ToString())
                {
                    case WeixinPaymentSystemNames.JsPay:
                        _httpContext.Session["WeixinPay.JsPay.OrderGuid"] = postProcessPaymentRequest.Order.OrderGuid; //保存orderguid
                        _httpContext.Response.RedirectToRoute("Weixin.Js.Pay");
                        break;
                    case WeixinPaymentSystemNames.NativePay:
                         post.Url = _webHelper.GetStoreLocation(false) + "WeixinNativePay";
                         post.Post();
                        break;
                }
            }
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _aliPayPaymentSettings.AdditionalFee;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //AliPay is the redirection payment method
            //It also validates whether order is also paid (after redirection) so customers will not be able to pay twice
            
            //payment status should be Pending
            if (order.PaymentStatus != PaymentStatus.Pending)
                return false;

            //一分钟以内不允许重复支付
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 30)
                return false;

            return true;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentWeixin";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Tts.Plugin.Payments.WeixinPay.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentWeixin";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Tts.Plugin.Payments.WeixinPay.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentWeixinController);
        }

        public override void Install()
        {
            //settings
            var settings = new WeixinPaymentSettings()
            {
                Email = "",
                Password = "",
                AppId = "wxb16ef7626015e6de",
                AppSecret = "",
                Mchid = "",
                ApiSecret = "",
                AdditionalFee = 0
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.WeixinPay.Email", "账户（可空）");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.WeixinPay.Password", "密码（可空）");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.WeixinPay.AppId", "Appid");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.WeixinPay.AppSecret", "AppSecret");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.WeixinPay.Mchid", "商户号");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.WeixinPay.ApiSecret", "秘钥KEY（ApiSecret）");

            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.WeixinPay.AdditionalFee", "微信支付额外的费用.");
   
            base.Install();
        }


        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.WeixinPay.Email");
            this.DeletePluginLocaleResource("Plugins.Payments.WeixinPay.Password");
            this.DeletePluginLocaleResource("Plugins.Payments.WeixinPay.AppId");
            this.DeletePluginLocaleResource("Plugins.Payments.WeixinPay.AppSecret");
            this.DeletePluginLocaleResource("Plugins.Payments.WeixinPay.Mchid");
            this.DeletePluginLocaleResource("Plugins.Payments.WeixinPay.ApiSecret");
            this.DeletePluginLocaleResource("Plugins.Payments.WeixinPay.AdditionalFee");
            base.Uninstall();
        }
        #endregion

        #region Properies

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
                return RecurringPaymentType.NotSupported;
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
            get { return false; }
        }

        #endregion
    }
}
