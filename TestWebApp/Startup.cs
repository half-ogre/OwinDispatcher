using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using OwinDispatcher;

[assembly: OwinStartup(typeof(TestWebApp.Startup))]

namespace TestWebApp
{
    public class Thing
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Startup
    {
        static int _lastThingId;
        static readonly IDictionary<int, Thing> _things = new Dictionary<int, Thing>();

        public void Configuration(IAppBuilder app)
        {
            app.UseDispatcher(dispatcher =>
            {
                // list all the things:
                dispatcher.Get("/things", (environment, next, @params) =>
                {
                    var response = new OwinResponse(environment)
                    {
                        StatusCode = 200,
                        ContentType = "text/plain"
                    };

                    response.Write("# All the things:");
                    response.Write(Environment.NewLine);
                    response.Write(Environment.NewLine);

                    foreach (var thing in _things.Values)
                    {
                        response.Write(String.Concat("- Thing #", thing.Id, ": ", thing.Name));
                        response.Write(Environment.NewLine);
                    }

                    return Task.FromResult((object)null);
                });

                // create a new thing:
                dispatcher.Post("/things", async (environment, next, @params) =>
                {
                    var request = new OwinRequest(environment);
                    var form = await request.ReadFormAsync();
                    
                    var response = new OwinResponse(environment);

                    var thingName = form["name"];

                    if (thingName == null)
                    {
                        response.StatusCode = 400;
                        await response.WriteAsync("The thing to POST is missing a name.");
                        return;
                    }
                    
                    _things.Add(++_lastThingId, new Thing
                    {
                        Id = _lastThingId,
                        Name = thingName
                    });
                    var uri = String.Concat("/things/", _lastThingId);

                    response.StatusCode = 201;
                    response.Headers["Location"] = uri;
                    response.ContentType = "text/plain";
                    await response.WriteAsync(uri);
                });

                // list all the things:
                dispatcher.Get("/things/{id}", (environment, next, @params) =>
                {
                    var response = new OwinResponse(environment);

                    int id;
                    if (!int.TryParse(@params.id, out id))
                    {
                        response.StatusCode = 404;
                        return Task.FromResult((object)null);
                    }

                    var thing = _things[id];

                    response.StatusCode = 200;
                    response.ContentType = "text/plain";

                    return response.WriteAsync(String.Concat("Thing #", thing.Id, " is ", thing.Name, "."));
                });
            });
        }
    }
}