# OwinDispatcher

_OwinDispatcher is a simple OWIN middleware that dispatches requests by associating URL patterns to handler functions._

## Installing

Install OwinDispatcher via NuGet: `install-package OwinDispatcher -pre`. Or, download the latest binary distribution from the [available releases](https://github.com/half-ogre/OwinDispatcher/releases).

## Getting Started

To use OwinDispatcher, invoke the `UseDispatcher` extension method on the `IAppBuilder` instance in your OWIN startup configuration, like so:

```
public class Startup
{
  public void Configuration(IAppBuilder app)
  {
    app.UseDispatcher(dispatcher =>
    {
       dispatcher.Get("/", (environment, next) =>
       {
           // ...
       });
    });
  }
}
```

## Handling Requests by Method and URL

Use methods on the `Dispatcher` class to register a handler function for a specified URL. There is a `Dispatcher` method for the most common HTTP methods: `Get`, `Post`, `Put`, `Patch`, and `Delete`. For example:

```
app.UseDispatcher(dispatcher =>
{
   // get the current user:
   dispatcher.Get("/user", (environment, next) =>
   {
       // ...
   });
   
   // create a new user:
   dispatcher.Post("/users", (environment, next) =>
   {
       // ...
   });
   
   // update the current user
   dispatcher.Patch("/user", (environment, next) =>
   {
       // ...
   });
   
   // delete the current user
   dispatcher.Delete("/user", (environment, next) =>
   {
       // ...
   });
});
```

## The Handler Function

A handler fucntion's type is: `Func<IDictionary<string, object>,, Func<IDictionary<string, object>, Task>, Task>`. The first argumenent, `IDictionary<string, object>`, is the OWIN environment for the current request. The second argument, `Func<IDictionary<string, object>, Task>` is a funtion to pass execution to the next OWIN app. The return type is a   `Task` that must eventually complete. 

Example of a simple response:

```
dispatcher.Get("/", (environment, next) =>
 {
     environment["owin.ResponseStatusCode"] = 200;
     return environment["owin.ResponseBody"].WriteAsync("Hello!");
 });
```

Example of logging some request details and then passing execution to the next app:

```
dispatcher.Get("/", (environment, next) =>
 {
     log.Write(String.Concat("GET ", environment["owin.RequestPath"]);
     return next(environment);
 });
```
