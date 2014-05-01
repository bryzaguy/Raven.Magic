namespace Raven.Magic.Client.MagicDocumentSession
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Raven.Client.Linq;

    public static class MagicRavenQueryExtensions
    {
        public static IList<T> ToList<T>(this IQueryable<T> query)
        {
            return Execute(query, (query as IEnumerable<T>).ToList(), (q, list) => list.ConvertAll(a => (T)q.LoadWithIncludes(a)));
        }

        public static T First<T>(this IRavenQueryable<T> query)
        {
            return query.OneResult((query as IQueryable<T>).First());
        }

        public static T First<T>(this IRavenQueryable<T> query, Func<T, bool> predicate)
        {
            return query.OneResult((query as IQueryable<T>).First(predicate));
        }

        public static T Last<T>(this IRavenQueryable<T> query)
        {
            return query.OneResult((query as IQueryable<T>).Last());
        }

        public static T Last<T>(this IRavenQueryable<T> query, Func<T, bool> predicate)
        {
            return query.OneResult((query as IQueryable<T>).Last(predicate));
        }

        public static T LastOrDefault<T>(this IRavenQueryable<T> query)
        {
            return query.OneResult((query as IQueryable<T>).LastOrDefault());
        }

        public static T LastOrDefault<T>(this IRavenQueryable<T> query, Func<T, bool> predicate)
        {
            return query.OneResult((query as IQueryable<T>).LastOrDefault(predicate));
        }

        public static T FirstOrDefault<T>(this IRavenQueryable<T> query)
        {
            return query.OneResult((query as IQueryable<T>).FirstOrDefault());
        }

        public static T FirstOrDefault<T>(this IRavenQueryable<T> query, Func<T, bool> predicate)
        {
            return query.OneResult((query as IQueryable<T>).FirstOrDefault(predicate));
        }

        public static T Single<T>(this IRavenQueryable<T> query)
        {
            return query.OneResult((query as IQueryable<T>).Single());
        }

        public static T Single<T>(this IRavenQueryable<T> query, Func<T, bool> predicate)
        {
            return query.OneResult((query as IQueryable<T>).Single(predicate));
        }

        public static T SingleOrDefault<T>(this IRavenQueryable<T> query)
        {
            return query.OneResult((query as IQueryable<T>).SingleOrDefault());
        }

        public static T SingleOrDefault<T>(this IRavenQueryable<T> query, Func<T, bool> predicate)
        {
            return query.OneResult((query as IQueryable<T>).SingleOrDefault(predicate));
        }

        private static T OneResult<T>(this IEnumerable<T> query, T result)
        {
            return Execute(query, result, (q, item) => (T)q.LoadWithIncludes(item));
        }

        public static TResult Execute<TResult, T>(IEnumerable<T> query, TResult result, Func<MagicRavenQueryProvider<T>, TResult, TResult> loader)
        {
            var queryable = query as IQueryable;
            if (queryable != null) 
            {
                var magicRavenQueryInspector = queryable.Provider as MagicRavenQueryProvider<T>;
                if (magicRavenQueryInspector != null)
                    return loader(magicRavenQueryInspector, result);
            }
            return result;
        }
    }
}