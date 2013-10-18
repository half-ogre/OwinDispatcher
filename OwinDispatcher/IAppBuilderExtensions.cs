using System;
using System.IO;
using Owin;

namespace OwinDispatcher
{
    public static class IAppBuilderExtensions
    {
        public static IAppBuilder UseDispatcher(this IAppBuilder appBuilder, Action<Dispatcher> dispatch)
        {
            return appBuilder.Use(typeof(Dispatcher), dispatch);
        }
    }
}
