namespace Raven.Magic.Client.MagicDocumentSession
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Raven.Client.Linq;

    public static class MagicRavenQueryExtensions
    {
        public static IList<T> ToList<T>(this IQueryable<T> query)
        {
            return Execute(query, (query as IEnumerable<T>).ToList(), (q, list) => list.ConvertAll(q.LoadWithIncludes));
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

        private static IRavenQueryable<T> AsMagicQuery<T>(this IRavenQueryable<T> query, Func<IQueryable<T>, IEnumerable<T>> queryAddition)
        {
            var magicQuery = query as MagicRavenQueryInspector<T>;
            var queryResult = queryAddition(query) as IRavenQueryable<T>;
            if (magicQuery != null)
            {
                queryResult = new MagicRavenQueryInspector<T>(queryAddition(query) as IRavenQueryable<T>, magicQuery.LoadHelper, magicQuery.Parser);
            }
            return queryResult;
        }

        public static IRavenQueryable<T> Where<T>(this IRavenQueryable<T> query, Expression<Func<T, bool>> predicate)
        {
            return query.AsMagicQuery(a => a.Where(predicate));
        }

        public static IRavenQueryable<T> Where<T>(this IRavenQueryable<T> query, Expression<Func<T, int, bool>> predicate)
        {
            return query.AsMagicQuery(a => a.Where(predicate));
        }

        /*
        public static IRavenQueryable<T> Select<T, TResult>(this IRavenQueryable<T> query, Func<T, TResult> selector)
        {
            return (query as IQueryable<T>).Select(selector) as IRavenQueryable<T>;
            return query.AsMagicQuery(a => a.Where(predicate));
        }

        public static IRavenQueryable<T> Select<T, TResult>(this IRavenQueryable<T> query, Func<T, int, TResult> selector)
        {
            return query.AsMagicQuery(a => a.Select(selector));
        }

        public static IRavenQueryable<T> SelectMany<T, TResult>(this IRavenQueryable<T> query, Func<T, IEnumerable<TResult>> selector)
        {
            return (query as IQueryable<T>).SelectMany(selector) as IRavenQueryable<T>;
        }

        public static IRavenQueryable<T> SelectMany<T, TResult>(this IRavenQueryable<T> query, Func<T, int, IEnumerable<TResult>> selector)
        {
            return (query as IQueryable<T>).SelectMany(selector) as IRavenQueryable<T>;
        }

        public static IRavenQueryable<T> SelectMany<T, TResult>(this IRavenQueryable<T> query, Func<T, int, IEnumerable<TResult>> collectionSelector, Func<T, TResult, TResult> resultSelector)
        {
            return (query as IQueryable<T>).SelectMany(collectionSelector, resultSelector) as IRavenQueryable<T>;
        } 
         */
        
        //public static IRavenQueryable<T> Take<T>(this IRavenQueryable<T> query, int count)
        //{
        //    return query.AsMagicQuery(a => a.Take(count));
        //}

        //public static IRavenQueryable<T> Skip<T>(this IRavenQueryable<T> query, int count)
        //{
        //    return query.AsMagicQuery(a => a.Skip(count));
        //}

        private static T OneResult<T>(this IEnumerable<T> query, T result)
        {
            return Execute(query, result, (q, item) => q.LoadWithIncludes(item));
        }

        public static TResult Execute<TResult, T>(IEnumerable<T> query, TResult result, Func<MagicRavenQueryInspector<T>, TResult, TResult> loader)
        {
            var magicRavenQueryInspector = query as MagicRavenQueryInspector<T>;
            if (magicRavenQueryInspector != null)
                return loader(magicRavenQueryInspector, result);
            return result;
        }
    }
}