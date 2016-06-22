using Nop.Core.Configuration;

namespace Tts.Plugin.Payments.WeixinPay
{
    public class WeixinPaymentSettings : ISettings
    {
        /// <summary>
        /// �˺�
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AppId { get; set; }

        public string AppSecret { get; set; }

        /// <summary>
        /// �̻���
        /// </summary>
        public string Mchid { get; set; }

        /// <summary>
        /// ǩ����Կ
        /// </summary>
        public string ApiSecret { get; set; }


        /// <summary>
        /// �����������
        /// </summary>
        public decimal  AdditionalFee { get; set; }

    }


    public static class WeixinPaymentSystemNames
    {
        public static string SelectedWeixinPayMethod { get { return "֧��ģʽ"; } }

        /// <summary>
        /// ���ں�֧��
        /// </summary>
        public const string JsPay = "���ں�֧��";

        /// <summary>
        /// ɨ��֧��
        /// </summary>
        public const string NativePay ="ɨ��֧��";


        /// <summary>
        /// ɨ��֧��
        /// </summary>
        public const string AppPay = "΢��App֧��"; 
    }
}
