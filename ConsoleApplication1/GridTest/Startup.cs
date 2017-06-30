using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GridTest.Startup))]
namespace GridTest
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
