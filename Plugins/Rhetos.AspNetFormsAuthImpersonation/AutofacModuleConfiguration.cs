using Autofac;
using Rhetos.AspNetFormsAuth;
using Rhetos.Extensibility;
using Rhetos.Security;
using Rhetos.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace Rhetos.AspNetFormsAuthImpersonation
{
    [Export(typeof(Module))]
    [ExportMetadata(MefProvider.DependsOn, typeof(Rhetos.AspNetFormsAuth.AutofacModuleConfiguration))]
    public class AutofacModuleConfiguration : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ImpersonationService>().InstancePerLifetimeScope();

            Plugins.CheckOverride<IUserInfo, AspNetImpersonationUserInfo>(builder, typeof(AspNetUserInfo), typeof(WcfWindowsUserInfo));
            builder.RegisterType<AspNetImpersonationUserInfo>().As<IUserInfo>().InstancePerLifetimeScope();

            base.Load(builder);
        }
    }
}
