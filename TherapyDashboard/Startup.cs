using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TherapyDashboard.Startup))]
namespace TherapyDashboard
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
