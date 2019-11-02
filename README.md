_**NOTICE! I am no longer maintaining this repository. It has been archived.**_

# OwinDispatcher

_OwinDispatcher is a simple OWIN middleware that dispatches requests by associating URL patterns to handler functions._

## Installing

Install OwinDispatcher via NuGet: `install-package OwinDispatcher`. Or, download the latest binary distribution from the [available releases](https://github.com/half-ogre/OwinDispatcher/releases).

## Configuration

To configure OwinDispatcher, invoke the `UseDispatcher` extension method on the `IAppBuilder` instance in your OWIN startup configuration, like so:

```
public class Startup
{
  public void Configuration(IAppBuilder app)
  {
    app.UseDispatcher(dispatcher =>
    {
       dispatcher.Get("/", (environment, next) =>
       {
           // ... get the home page
       });
    });
  }
}
```

The `Dispatcher` middleware should typically be at or near the end of your configured OWIN middlewares, as the handler functions will often write to the response body.

## Handling Requests by Method and URL

Use methods on the `Dispatcher` class to associate [handler functions](#the-handler-function) to a specified URL. There is a `Dispatcher` method for each of the most common HTTP methods: `Get`, `Post`, `Put`, `Patch`, and `Delete`. For example:

```
app.UseDispatcher(dispatcher =>
{
   dispatcher.Get("/user", (environment, next) =>
   {
       // ... get the current user
   });
   
   dispatcher.Post("/users", (environment, next) =>
   {
       // ... create a new user
   });

   dispatcher.Put("/user/email", (environment, next) =>
   {
       // ... set the current user's email address
   });
   
   dispatcher.Patch("/user", (environment, next) =>
   {
       // ... update the current user
   });
   
   dispatcher.Delete("/user", (environment, next) =>
   {
       // ... delete the current user
   });
});
```

## URL Patterns

You may use a URL pattern instead of a static URL when registering a handler function. To do so, insert a token name surrounded by curly braces in the URL segment you want to capture by name. For example:

```
dispatcher.Get("/users/{id}", (environment, @params, next) =>
{
  var user = Users.find(@params.id);
  // ... build the response
});
```

## The Handler Function

A handler fucntion's type is: `Func<IDictionary<string, object>,, Func<IDictionary<string, object>, Task>, Task>`. 

The first argumenent, `IDictionary<string, object>`, is the [OWIN environment](http://owin.org/spec/owin-1.0.0.html#_3.2._Environment) for the current request.

The second argument, `dynamic`, is a dynamic object with a property for each token in the [URL pattern](#url-patterns). For example. the URL pattern `/users/{id}` would have a dynamic object with an `id` property.

The third argument, `Func<IDictionary<string, object>, Task>` is a function that returns a `Task` for the next OWIN app or middleware. You can invoke this and return that task to pass execution to the next OWIN app or middleware (see example below).

The handler function must return a `Task`. This task must eventually be completed. When writing to the response body's stream, it is common to return the task from an invocation of `environment["owin.ResponseBody"].WriteAsync(...)`. See the OWIN specification section about the [response body](http://owin.org/spec/owin-1.0.0.html#_3.5._Response_Body) for more information.

Here's an example of a simple "Hello!" response:

```
dispatcher.Get("/", (environment, next) =>
{
  environment["owin.ResponseStatusCode"] = 200;
  return environment["owin.ResponseBody"].WriteAsync("Hello!");
});
```

And here's an example of passing execution to the next OWIN app:

```
dispatcher.Get("/", (environment, next) =>
{
  // .... determine that we shouldn't handle this
  return next(environment);
});
```
