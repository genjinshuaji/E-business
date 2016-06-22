using Autofac;
using Nop.Core.Infrastructure.DependencyManagement;
using WxPayAPI;

namespace Tts.Plugin.Payments.WeixinPay
{
    public class DependencyRegistrar : IDependencyRegistrar
    {

        public void Register(Autofac.ContainerBuilder builder, Nop.Core.Infrastructure.ITypeFinder typeFinder, Nop.Core.Configuration.NopConfig config)
        {
            builder.RegisterType(typeof (JsApiPay));
            builder.RegisterType(typeof(ResultNotify));
            builder.RegisterType(typeof(NativePay));
        }

        public int Order
        {
            get { return 2; }
        }
    }
}
