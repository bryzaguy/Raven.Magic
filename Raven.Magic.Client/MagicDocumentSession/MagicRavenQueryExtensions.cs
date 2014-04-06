namespace Raven.Magic.Client.MagicDocumentSession
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Raven.Client.Linq;

    public static class MagicRavenQueryExtensions
    {
        public static IList<T> ToList<T>(this IRavenQueryable<T> query)
        {
            return Execute(query, (query as IQueryable<T>).ToList(), (a, b) => b.ConvertAll(a.LoadWithIncludes));
        }

        public static T First<T>(this IRavenQueryable<T> query)
        {
            return Execute(query, (query as IQueryable<T>).First(), (a, b) => a.LoadWithIncludes(b));
        }

        public static TResult Execute<TResult, T>(IRavenQueryable<T> query, TResult result, Func<MagicRavenQueryInspector<T>, TResult, TResult> loader)
        {
            var magicRavenQueryInspector = query as MagicRavenQueryInspector<T>;
            if (magicRavenQueryInspector != null)
                return loader(magicRavenQueryInspector, result);
            return result;
        }

        public static T FirstOrDefault<T>(this IRavenQueryable<T> query)
        {
            return (query as IQueryable<T>).FirstOrDefault();
        }
    }
}