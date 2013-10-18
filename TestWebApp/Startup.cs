using Microsoft.Owin;
using Owin;
using OwinDispatcher;

[assembly: OwinStartup(typeof(TestWebApp.Startup))]

namespace TestWebApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseDispatcher(dispatcher =>
                dispatcher.Get("/", (environment, next) =>
                {
                    var response = new OwinResponse(environment)
                    {
                        StatusCode = 200
                    };

                    return response.WriteAsync("Home");
                }));
        }
    }
}