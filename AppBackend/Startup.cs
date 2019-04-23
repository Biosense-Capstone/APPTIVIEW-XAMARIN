using Microsoft.Owin;
using Owin;

[assembly: OwinStartup("backend", typeof(AppBackend.Startup))]

namespace AppBackend
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMobileApp(app);
        }
    }
}