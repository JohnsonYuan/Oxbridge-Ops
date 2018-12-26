using Autofac;
using Autofac.Core;
using Nop.Core.Caching;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Services.BonusApp.Authentication;
using Nop.Services.BonusApp.Configuration;
using Nop.Services.BonusApp.Customers;
using Nop.Services.BonusApp.Logging;
using Nop.Services.ZhiXiao.BonusApp;

namespace Nop.Services.BonusApp
{
    public class BonusAppDependencyRegister : IDependencyRegistrar
    {
        public int Order => 1;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            // bonus app service
            builder.RegisterType<BonusAppService>()
                .As<IBonusAppService>().InstancePerLifetimeScope();

            builder.RegisterType<BonusApp_CustomerActivityService>()
                .As<IBonusApp_CustomerActivityService>()
                .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static")).InstancePerLifetimeScope();

            builder.RegisterType<BonusApp_CustomerService>()
                .As<IBonusApp_CustomerService>()
                .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static")).InstancePerLifetimeScope();

            builder.RegisterType<BonusApp_SettingService>()
                .As<IBonusApp_SettingService>()
                .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static")).InstancePerLifetimeScope();

            builder.RegisterType<BonusAppFormsAuthenticationService>()
                .As<IBonusAppFormsAuthenticationService>().InstancePerLifetimeScope();
        }
    }
}
