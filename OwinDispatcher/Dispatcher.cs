using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Owin.Types;

namespace OwinDispatcher
{
    public class Dispatcher
    {
        private readonly Func<IDictionary<string, object>, Task> _next;
        readonly IDictionary<string, List<Tuple<Regex, Func<IDictionary<string, object>, dynamic, Func<IDictionary<string, object>, Task>, Task>>>> _handlers;
        static readonly Regex _tokenRegex = new Regex(@"\{([a-z]+)\}", RegexOptions.IgnoreCase);

        public Dispatcher(Func<IDictionary<string, object>, Task> next, Action<Dispatcher> dispatch)
        {
            _next = next;
            _handlers = new Dictionary<string, List<Tuple<Regex, Func<IDictionary<string, object>, dynamic, Func<IDictionary<string, object>, Task>, Task>>>>();

            dispatch(this);
        }

        private void AddHandler(string method, Tuple<Regex, Func<IDictionary<string, object>, dynamic, Func<IDictionary<string, object>, Task>, Task>> handler)
        {
            var key = method.ToLowerInvariant();

            EnsureHandlersHaveMethodKey(key);

            _handlers[key].Add(handler);
        }

        static Regex CreateRegexForUrlPattern(string urlPattern)
        {
            var regexString = _tokenRegex.Replace(urlPattern, @"(?<$1>[^/]+)");

            return new Regex(String.Concat("^", regexString, "$"));
        }

        public void Delete(
            string urlPattern,
            Func<IDictionary<string, object>, dynamic, Func<IDictionary<string, object>, Task>, Task> handler)
        {
            AddHandler(
                "DELETE",
                new Tuple<Regex, Func<IDictionary<string, object>, dynamic, Func<IDictionary<string, object>, Task>, Task>>(
                    CreateRegexForUrlPattern(urlPattern),
                    handler));
        }

        void EnsureHandlersHaveMethodKey(string method)
        {
            var key = method.ToLowerInvariant();

            if (!_handlers.ContainsKey(key))
                _handlers.Add(key, new List<Tuple<Regex, Func<IDictionary<string, object>, dynamic, Func<IDictionary<string, object>, Task>, Task>>>());
        }

        Func<IDictionary<string, object>, Func<IDictionary<string, object>, Task>, Task> FindHandler(string method, string path)
        {
            var key = method.ToLowerInvariant();
            
            EnsureHandlersHaveMethodKey(key);

            var @params = new ExpandoObject() as IDictionary<string, object>;

            Match matches = null;

            var matchingHandler = _handlers[key]
                .FirstOrDefault(x =>
                {
                    matches = x.Item1.Match(path);
                    return matches.Success;
                });

            if (matchingHandler == null)
                return null;

            foreach (var groupName in matchingHandler.Item1.GetGroupNames())
                @params.Add(groupName, matches.Groups[groupName].Value);

            return (environement, next) => matchingHandler.Item2(environement, @params, next);
        }

        public void Get(
            string urlPattern,
            Func<IDictionary<string, object>, dynamic, Func<IDictionary<string, object>, Task>, Task> handler)
        {
            AddHandler(
                "GET",
                new Tuple<Regex, Func<IDictionary<string, object>, dynamic, Func<IDictionary<string, object>, Task>, Task>>(
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

        public void Patch(
            string urlPattern,
            Func<IDictionary<string, object>, dynamic, Func<IDictionary<string, object>, Task>, Task> handler)
        {
            AddHandler(
                "PATCH",
                new Tuple<Regex, Func<IDictionary<string, object>, dynamic, Func<IDictionary<string, object>, Task>, Task>>(
                    CreateRegexForUrlPattern(urlPattern),
                    handler));
        }

        public void Post(
            string urlPattern,
            Func<IDictionary<string, object>, dynamic, Func<IDictionary<string, object>, Task>, Task> handler)
        {
            AddHandler(
                "POST",
                new Tuple<Regex, Func<IDictionary<string, object>, dynamic, Func<IDictionary<string, object>, Task>, Task>>(
                    CreateRegexForUrlPattern(urlPattern),
                    handler));
        }

        public void Put(
            string urlPattern,
            Func<IDictionary<string, object>, dynamic, Func<IDictionary<string, object>, Task>, Task> handler)
        {
            AddHandler(
                "PUT",
                new Tuple<Regex, Func<IDictionary<string, object>, dynamic, Func<IDictionary<string, object>, Task>, Task>>(
                    CreateRegexForUrlPattern(urlPattern),
                    handler));
        }
    }
}
