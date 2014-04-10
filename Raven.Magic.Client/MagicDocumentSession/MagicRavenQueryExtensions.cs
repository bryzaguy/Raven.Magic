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
            return Execute(query, (query as IQueryable<T>).ToList(), (q, list) => list.ConvertAll(q.LoadWithIncludes));
        }

        public static T First<T>(this IRavenQueryable<T> query)
        {
            return query.OneResult((query as IQueryable<T>).First());
        }

        public static T FirstOrDefault<T>(this IRavenQueryable<T> query)
        {
            return query.OneResult((query as IQueryable<T>).FirstOrDefault());
        }

        public static T Single<T>(this IRavenQueryable<T> query)
        {
            return query.OneResult((query as IQueryable<T>).Single());
        }

        public static T SingleOrDefault<T>(this IRavenQueryable<T> query)
        {
            return query.OneResult((query as IQueryable<T>).SingleOrDefault());
        }

        private static T OneResult<T>(this IRavenQueryable<T> query, T result)
        {
            return Execute(query, result, (q, item) => q.LoadWithIncludes(item));
        }

        public static TResult Execute<TResult, T>(IRavenQueryable<T> query, TResult result, Func<MagicRavenQueryInspector<T>, TResult, TResult> loader)
        {
            var magicRavenQueryInspector = query as MagicRavenQueryInspector<T>;
            if (magicRavenQueryInspector != null)
                return loader(magicRavenQueryInspector, result);
            return result;
        }
    }
}