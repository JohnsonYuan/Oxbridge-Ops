using Autofac;
using Nop.Admin.Helpers;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Web.ZhiXiao.Factories;

namespace Web.ZhiXiao.Infrastructure
{/// <summary>
 /// Dependency registrar
 /// </summary>
    public class DependencyRegistrar : IDependencyRegistrar
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<RegisterZhiXiaoUserHelper>().As<IRegisterZhiXiaoUserHelper>()
                .InstancePerDependency();

            builder.RegisterType<CustomerModelFactory>().As<ICustomerModelFactory>()
                .InstancePerLifetimeScope();
        }

        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        public int Order
        {
            get { return 2; }
        }
    }
}