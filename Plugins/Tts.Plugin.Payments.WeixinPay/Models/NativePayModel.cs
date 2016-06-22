namespace Tts.Plugin.Payments.WeixinPay.Models
{
    public class NativePayModel
    {
        public string OrderNo { get; set; }

        public string OrderTotal { get; set; }

        public string QrCodeBase64String { get; set; }
    }
}