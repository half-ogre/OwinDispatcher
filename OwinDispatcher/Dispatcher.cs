using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Owin.Types;

namespace OwinDispatcher
{
    public class Dispatcher
    {
        private readonly Func<IDictionary<string, object>, Task> _next;
        readonly IDictionary<string, List<Tuple<Regex, Func<IDictionary<string, object>, Func<IDictionary<string, object>, Task>, Task>>>> _handlers;

        public Dispatcher(Func<IDictionary<string, object>, Task> next, Action<Dispatcher> dispatch)
        {
            _next = next;
            _handlers = new Dictionary<string, List<Tuple<Regex, Func<IDictionary<string, object>, Func<IDictionary<string, object>, Task>, Task>>>>();

            dispatch(this);
        }

        private void AddHandler(string method, Tuple<Regex, Func<IDictionary<string, object>, Func<IDictionary<string, object>, Task>, Task>> handler)
        {
            var key = method.ToLowerInvariant();

            EnsureHandlersHaveMethodKey(key);

            _handlers[key].Add(handler);
        }

        static Regex CreateRegexForUrlPattern(string urlPattern)
        {
            return new Regex(String.Concat("^", urlPattern, "$"));
        }

        void EnsureHandlersHaveMethodKey(string method)
        {
            var key = method.ToLowerInvariant();

            if (!_handlers.ContainsKey(key))
                _handlers.Add(key, new List<Tuple<Regex, Func<IDictionary<string, object>, Func<IDictionary<string, object>, Task>, Task>>>());
        }

        Func<IDictionary<string, object>, Func<IDictionary<string, object>, Task>, Task> FindHandler(string method, string path)
        {
            var key = method.ToLowerInvariant();
            
            EnsureHandlersHaveMethodKey(key);

            var match = _handlers[key]
                .FirstOrDefault(x => x.Item1.IsMatch(path));

            if (match == null)
                return null;

            return match.Item2;
        }

        public void Get(
            string urlPattern,
            Func<IDictionary<string, object>, Func<IDictionary<string, object>, Task>, Task> handler)
        {
            AddHandler(
                "GET", 
                new Tuple<Regex, Func<IDictionary<string, object>, Func<IDictionary<string, object>, Task>, Task>>(
                    CreateRegexForUrlPattern(urlPattern), 
                    handler));
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            var request = new OwinRequest(environment);

            var handler = FindHandler(request.Method, request.Path);

            if (handler == null)
                return _next(environment);

            return handler(environment, _next);
        }

        public void Post(
            string urlPattern,
            Func<IDictionary<string, object>, Func<IDictionary<string, object>, Task>, Task> handler)
        {
            AddHandler(
                "POST",
                new Tuple<Regex, Func<IDictionary<string, object>, Func<IDictionary<string, object>, Task>, Task>>(
                    CreateRegexForUrlPattern(urlPattern),
                    handler));
        }
    }
}
