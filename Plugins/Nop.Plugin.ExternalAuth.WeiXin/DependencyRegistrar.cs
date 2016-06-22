using Autofac;
using Nop.Plugin.ExternalAuth.WeiXin.Core;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using System;

namespace Nop.Plugin.ExternalAuth.WeiXin
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, Nop.Core.Infrastructure.ITypeFinder typeFinder)
        {
            builder.RegisterType<WeiXinProviderAuthorizer>().As<IOAuthProviderWeiXinAuthorizer>().InstancePerLifetimeScope();
        }

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            throw new NotImplementedException();
        }

        public int Order
        {
            get { return 0; }
        }
    }
}
